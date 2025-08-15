using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;



//ゲーム時間、キルデス、スコア管理を行うクラス
public class GameManager2 : NetworkBehaviour,IAfterSpawned
{
    // シングルトンインスタンス
    public static GameManager2 Instance { get; private set; }
    //初期化のためのフラグ
    private bool _afterSpawned = false;
    private bool _sceneLoaded = false;

    // 準備完了したプレイヤーのカウント
    private int readyPlayerCount = 0;

    //生成時イベント
    public static event Action OnManagerInitialized;
    //試合終了イベント
    public event Action OnTimeUp;

    
    //============================================
    //ユーザーデータ関係
    //============================================
    public UserData[] UserDataArray { get; private set; } = new UserData[50];
    bool isUserDataArrayDirty = false; // 最新のUserDataArrayが同期がまだされていないかどうかのフラグ


    //============================================
    //キルデス関係
    //============================================
    public struct KillLog
    {
        public float Timestamp; 
        public PlayerRef Victim;
        public PlayerRef Killer;
        
        public KillLog(float timeStamp,PlayerRef victim, PlayerRef killer)
        {
            Timestamp = timeStamp;
            Victim = victim;
            Killer = killer;
        }
    }
    private const int KillLogLimit = 100;
    private List<KillLog> killLogs = new List<KillLog>(KillLogLimit);
    public event Action<float,PlayerRef, PlayerRef> OnAnyPlayerDied;
    public event Action<float,PlayerRef> OnMyPlayerDied;
    //発火先一覧
    //PlayerAvatarのPlayerDeathHandler
    //HUDManagerのRespawnPanel

    //============================================
    //スコア関係
    //============================================
    public event Action OnScoreChanged;

    //============================================
    //時間関係
    //============================================
    [Header("ゲームスタートまでのカウント(3秒以上)")]
    private const float countdownTime = 5f;
    private Coroutine countdownCoroutine;
    public event Action<float> OnCountDownBattleStart;
    [Networked(OnChanged = nameof(TimeChangedCallback))]
    public int RemainingSeconds { get; private set; }
    public int initialTimeSec = 180;
    [Networked]
    private double startSimTime { get; set; } = 0.0;
    [Networked]
    public bool IsTimerRunning { get; private set; } = false;
    public event Action<int> OnTimeChanged;

    //============================================
    // イベント発火用フラグ
    //=============================================
    private bool _scoreDirty = false;
    private struct DeathEventData
    {
        public float timeStamp;
        public PlayerRef victim;
        public PlayerRef killer;
        public bool isMyPlayer;
    }
    private Queue<DeathEventData> deathEventQueue = new Queue<DeathEventData>();
    [Networked] TickTimer NextTickTimer { get; set; }

    //============================================
    //デバッグ用入力
    //============================================
    public DebugInputData debugInput = DebugInputData.Default();

    //===========================================
    //初期化処理
    //===========================================
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AfterSpawned()
    {
        if (!Runner.IsServer)
        {
            InitializeGameManager(); // クライアント側で初期化を行う
        }

    }

    public void InitializeGameManager()
    {
        Debug.Log("GameManager2: InitializeGameManager called.");


        if (Runner.IsServer)
        {
            RemainingSeconds = initialTimeSec;
            startSimTime = Runner.SimulationTime;
        }


        //ここにUserDataの初期化処理を追加する
        if (Runner.IsServer)
        {
            RPC_RequestId(); // ホストから全クライアントにID要求を送信
            Debug.Log("GameManager2: InitializeGameManager called on server. Requesting IDs from clients.");
        }
        else 
        {
            RPC_SubmitIdToHost(ConnectionManager.Instance.GetLocalID()); // クライアントは自分のIDをホストに送信
            Debug.Log("GameManager2: InitializeGameManager called on client. Waiting for RPC_RequestId.");
            // クライアント側ではRPC_RequestIdを待つ
        }

        if(Runner.TryGetPlayerObject(Runner.LocalPlayer, out NetworkObject playerObject))
        {
            PlayerAvatar avatar = playerObject.GetComponent<PlayerAvatar>();
            avatar.StunPlayer();
        }
        else
        {
            Debug.LogError("Player object not found for local player.");
        }
        SceneTransitionManager.Instance.StartScene();
        RPC_ReadyGame();
    }


    // ① ホスト→全クライアント 要求
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_RequestId()
    {
        Debug.Log("GameManager2: RPC_RequestId called. Requesting IDs from clients.");
        // クライアントは自分の ID を送信する
        if (Runner.IsServer)
        {
            if (ConnectionManager.Instance != null)
            {
                RPC_SubmitIdToHost(ConnectionManager.Instance.GetLocalID());
            }
            else
            {
                Debug.LogError($"GameManager2: RPC_RequestId called but not valid.  Runner.IsServer={Runner.IsServer}" +
                    $"Exist  ConnectionManager.Instance:{ConnectionManager.Instance != null}");

            }
    }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]

    public void RPC_SubmitIdToHost(Guid userID,RpcInfo info = default)
    {

        Debug.Log("RPC_SubmitIdToHost called.");
        PlayerRef sender = info.Source;            // どの PlayerRef から来たか

        if (GameManager2.Instance != null)
        {
            GameManager2.Instance.RegisterUserID(userID, sender); // GameManager2のメソッドを呼び出してユーザーIDを登録

        }
        else
        {
            Debug.LogError("GameManager2 instance is not available.");
        }

    }

    public void RegisterUserID(Guid userID, PlayerRef player)
    {
        Debug.Log($"RegisterUserID called with userID: {userID}, player: {player}");
        if (!Runner.IsServer) { return; }
        //UserDataArrayの中からplayerに一致する要素を探す
        int index = Array.FindIndex(UserDataArray, u => u.userGuid == userID);
        if (index >= 0)
        {
            //見つかった場合は更新
            UserDataArray[index].playerRef = player;
            UserDataArray[index].userConnectionState = ConnectionState.Connected; // 接続状態を更新


            // デバッグ用に情報を表示
            UserDataArray[index].DisplayUserInfo();

            //すでに登録されているのに再度登録しようとしたということは、再接続者と考えられる
            Debug.Log($"UserData for player {player} already exists. Updated connection state and player reference. UserID: {userID}");
            // ここで必要なら再接続時の処理を追加することも可能


            // 例えば、スコアやチーム情報の再設定など
            ShareUserDataArray(UserDataArray); // 再接続時に全クライアントに最新のUserDataArrayを共有する





        }
        else
        {

            var newUserData = new UserData
            {
                userGuid = userID,
                playerRef = player,
                userScore = new PlayerScore(), // スコアは初期化
                userTeam = TeamType.None, // チームは初期化
                userConnectionState = ConnectionState.Connected, // 接続状態は接続中に設定
                //userName = $"Player {userID}" // ユーザー名はデフォルト設定
            };

            AddUserData(newUserData);
            // デバッグ用に情報を表示
            newUserData.DisplayUserInfo();
        }

        isUserDataArrayDirty = true; // UserDataArrayの同期が必要であることを示すフラグを立てる

    }



    void AddUserData(UserData userData)
    {
        //見つからなかった場合は追加
        for (int i = 0; i < UserDataArray.Length; i++)
        {
            if (UserDataArray[i].Equals(default(UserData)))
            {
                UserDataArray[i] = userData;
                break;
            }
        }



    }


    //ユーザーデータの更新を行うメソッド
    void UpdateUserDataArray(UserData userData)
    {

        int index = Array.FindIndex(UserDataArray, u => u.userGuid==userData.userGuid);
        if (index >= 0)
        {
            //見つかった場合は更新
            UserDataArray[index] = userData;
            
        }
        else
        {

            //var newUserData = new UserData
            //{
            //    userID = userID,
            //    playerRef = player,
            //    userScore = new PlayerScore(), // スコアは初期化
            //    userTeam = TeamType.None, // チームは初期化
            //    userConnectionState = ConnectionState.Connected, // 接続状態は接続中に設定
            //    //userName = $"Player {userID}" // ユーザー名はデフォルト設定
            //};

            AddUserData(userData);
            // デバッグ用に情報を表示
            //newUserData.DisplayUserInfo();
        }

        isUserDataArrayDirty = true; // UserDataArrayの同期が必要であることを示すフラグを立てる

    }

    public void UpdateConnectionState(PlayerRef player, ConnectionState state)
    { 
        if (!Runner.IsServer) { return; }
        //UserDataArrayの中からplayerに一致する要素を探す
        int index = Array.FindIndex(UserDataArray, u => u.playerRef == player);
        if (index >= 0)
        {
            //見つかった場合は更新
            UserDataArray[index].userConnectionState = state; // 接続状態を更新
            isUserDataArrayDirty = true; // UserDataArrayの同期が必要であることを示すフラグを立てる
        }
        else
        {
            Debug.LogWarning($"Player {player} not found in UserDataArray.");
        }

    }



    //ユーザーデータのチェックを行うメソッド
    private bool CheckUserDataIsAbleToShare(UserData[] userDataArray)
    {
        if (!Runner.IsServer) { return  false; }

        int validCount = userDataArray.Count(d => d.userGuid != Guid.Empty); 
        if (validCount < Runner.ActivePlayers.Count() )
        {
            
            return false;
        }
        else 
        {
            foreach (PlayerRef activePlayer in Runner.ActivePlayers)
            {
                // 各プレイヤーのUserDataが存在するかチェック
                //PlayerRefでチェックしてるが、ほんとうに正確にやるならUserIDを照合したほうがいい？
                if (!userDataArray.Any(d => d.playerRef == activePlayer ))
                {
                    Debug.LogWarning($"UserData for player {activePlayer} is missing or invalid.");
                    return false;
                }
                

            }

            return true; // 全てのプレイヤーのUserDataが存在する場合はtrueを返す
        }


    }


    
    public void ShareUserDataArray(UserData[] updatedUserDataArray)
    {

        int elementSize= System.Runtime.InteropServices.Marshal.SizeOf(typeof(UserData));
        int totalSize = updatedUserDataArray.Length*elementSize ;
        Debug.Log($"RPC_ShareUserData called. element size {elementSize} bytes. total size {totalSize} bytes. ");
        UserDataArray = updatedUserDataArray;

        foreach (var userData in UserDataArray)
        {
            if (userData.Equals(default(UserData))) continue; // 空要素を除外
            RPC_ShareUserData(userData);
        }



        RPC_InitializeCompleted();// 初期化完了イベントを発火
        _scoreDirty = true; // スコアが更新されたことを示すフラグを立てる



    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_ShareUserData(UserData sharedUserData)
    {

        int elementSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(UserData));
       
        Debug.Log($"RPC_ShareUserData called. element size {elementSize} bytes.");


        // UserDataArrayの中からplayerに一致する要素を探す
        int index = Array.FindIndex(UserDataArray, u => u.playerRef == sharedUserData.playerRef);
        if (index >= 0)
        {
            //見つかった場合は更新
            UserDataArray[index] = sharedUserData;
        }
        else
        {
            //見つからなかった場合は追加
            AddUserData(sharedUserData);
        }
        isUserDataArrayDirty = true; // UserDataArrayの同期が必要であることを示すフラグを立てる
        Debug.Log($"UserData for player {sharedUserData.playerRef} updated or added. UserID {sharedUserData.userGuid}");


        _scoreDirty = true; // スコアが更新されたことを示すフラグを立てる



    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_InitializeCompleted()
    { 
        OnManagerInitialized?.Invoke(); // 初期化完了イベントを発火

    }


    //こっから点呼(各環境で、GameManagerの初期化が各々終わったかどうかを確認する)
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ReadyGame()
    {
        readyPlayerCount++;
        if( readyPlayerCount >= Runner.ActivePlayers.Count())
        {
            CountdownBattleStart();
        }
    }


    //===========================================
    //ホスト環境でのみ呼ばれるメソッド群
    //===========================================
    //プレイヤーが死亡したときに呼ばれ、色々な処理を行うメソッド
    public void NotifyDeath(float killTime,PlayerRef victim, PlayerRef killer)
    {
        Debug.Log($"Object :{Object}");
        Debug.Log($"Object.HasStateAuthority :{Object.HasStateAuthority}");
        if (!Object.HasStateAuthority) return;
        //killTime時に既に試合時間が終了していれば実行しない
        if(startSimTime + initialTimeSec <= Runner.SimulationTime)
        {
            Debug.LogWarning("Game time has already ended. Not processing death notification.");
            return;
        }
        RPC_SendDeathData(Runner.SimulationTime, victim, killer);

    }

    //キルログを全ローカル環境に送信し、ついでに色々するメソッド
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SendDeathData(float timeStamp,PlayerRef victim, PlayerRef killer)
    {
        var killLog = new KillLog(timeStamp, victim, killer);
        killLogs.Add(killLog);

        //上限を超えたら最古のログを削除
        if (killLogs.Count >= KillLogLimit) 
        {
            killLogs.RemoveAt(0); 
        }
        NextTickTimer =    // 次ティック後にExpiredになる
            TickTimer.CreateFromTicks(Runner, 2);

        AddScore(victim, killer);

        //OnAnyPlayerDied?.Invoke(timeStamp, victim, killer);
        foreach(var player in Runner.ActivePlayers)
        {
            var playerObject = Runner.GetPlayerObject(player);
            if (playerObject != null)
            {
                var deathHandler = playerObject.GetComponent<PlayerDeathHandler>();
                if (deathHandler != null)
                {
                    deathHandler.HandleDeath(timeStamp, victim, killer);
                }
            }
        }

        if (IsMyPlayer(victim))
        {
            OnMyPlayerDied?.Invoke(timeStamp, killer);
        }

    }

    private void CountdownBattleStart()
    {
        if (!Object.HasStateAuthority) return;
        if (countdownTime < 3)
        {
            Debug.LogError("Countdown time is less than 3 seconds. Cannot start countdown.");
            return;
        }
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine); // 既存のカウントダウンを停止
        }
        StartCoroutine(CountdownCoroutine());
    }


    private System.Collections.IEnumerator CountdownCoroutine()
    {
        float countdown = countdownTime;
        while (countdown > 0)
        {
            yield return new WaitForSeconds(1f);
            countdown--;
            //
            if(countdown == 3)
            {
                RPC_InvokeCountDown(countdown); // 全クライアントにカウントダウン開始を通知
            }
        }
        // カウントダウン終了後にゲームを開始
        TimerStart();

        //PlayerAvatarの行動不能を解除
        foreach (var player in Runner.ActivePlayers)
        {
            var playerObject = Runner.GetPlayerObject(player);
            if (playerObject != null)
            {
                var avatar = playerObject.GetComponent<PlayerAvatar>();
                if (avatar != null)
                {
                    //RPCで行動不能を解除
                    avatar.RPC_ClearStun();
                }
            }
        }
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_InvokeCountDown(float countdown)
    {
        OnCountDownBattleStart?.Invoke(countdown);
    }


    private void UpdateTimer()
    {
        // タイマー更新処理
        if (!Object.HasStateAuthority) return;
        if (!IsTimerRunning) return;
        //現在の時間からスタート時間を引いた時間が経過時間
        double elapsed = Runner.SimulationTime - startSimTime;
        //経過時間を秒に変換
        int elapsedSeconds = Mathf.FloorToInt((float)elapsed);

        //残り時間を計算
        int newRemainingSeconds = Mathf.Max(initialTimeSec - elapsedSeconds);

        // 残り時間が変わった場合のみ更新
        if (newRemainingSeconds != RemainingSeconds)
        {
            RemainingSeconds = newRemainingSeconds;
            if (RemainingSeconds <= 0)
            {
                IsTimerRunning = false;
                //試合終了処理を開始
                EndGame();
            }
        }
    }

    //時間更新
    public override void FixedUpdateNetwork()
    {
        if (isUserDataArrayDirty && Runner.IsServer && CheckUserDataIsAbleToShare(UserDataArray))
        {
            {
                int elementSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(UserData));
                int totalSize = UserDataArray.Length * elementSize;
                Debug.Log($"ShareUserData. element size {elementSize} bytes. total size {totalSize} bytes. ");
                ShareUserDataArray(UserDataArray);
                isUserDataArrayDirty = false; // 同期が完了したのでフラグをリセット
            }


        }

        //スコア更新タイミング
        if (NextTickTimer.Expired(Runner))
        {
            NextTickTimer = TickTimer.None;   // リセット
            if (_scoreDirty)
            {
                //スコア変更イベントを発火
                OnScoreChanged?.Invoke();
                _scoreDirty = false;
            }

        }

        UpdateTimer(); // タイマーの更新処理を呼び出す


    }

    public void TimerStart()
    {
        if (!Object.HasStateAuthority) return;

        startSimTime = Runner.SimulationTime;
        IsTimerRunning = true;

        RemainingSeconds = initialTimeSec;
    }

    public void TimerReset()
    {
        if (!Object.HasStateAuthority) return;
        IsTimerRunning = false;
        RemainingSeconds = initialTimeSec;

        startSimTime = Runner.SimulationTime;
    }

    private void EndGame()
    {
        if (!Object.HasStateAuthority) return;
        Debug.Log("GameManager2: EndGame called. Game has ended.");
        //試合終了イベントを発火
        OnTimeUp?.Invoke();

    }

    //===========================================
    //全ローカル環境で呼ばれるメソッド群
    //===========================================
    //プレイヤーのスコアを更新するメソッド
    public void AddScore(PlayerRef victim, PlayerRef killer)
    {
        //デススコア加算
        //FirstOrDefaultを使うとワンチャンnullではなく
        UserData? foundV = UserDataArray.FirstOrDefault(u => u.playerRef==victim);
        if (!foundV.Equals(default(UserData)))
        {
            UserData updatedUserData = foundV.Value;
            updatedUserData.userScore.Deaths++;
            int index = Array.IndexOf(UserDataArray, foundV.Value);
            if (index >= 0)
            {
                UserDataArray[index] = updatedUserData;
            }
        }

        if (killer != PlayerRef.None&&killer != victim)
        {
            //キルスコア加算
            UserData? foundK = UserDataArray.FirstOrDefault(u => u.playerRef == killer);
            if (!foundV.Equals(default(UserData)))
            {
                UserData updatedUserData = foundK.Value;
                updatedUserData.userScore.Kills++;
                int index = Array.IndexOf(UserDataArray, foundK.Value);
                if (index >= 0)
                {
                    UserDataArray[index] = updatedUserData;
                }
            }
        }

        _scoreDirty = true;

        // スコア変更イベントを発火
        //OnScoreChanged?.Invoke();
    }

    //時間更新時のコールバックなど
    static void TimeChangedCallback(Changed<GameManager2> changed)
    {
        changed.Behaviour.RaiseTimeChanged();
    }
    private void RaiseTimeChanged()
    {
        OnTimeChanged?.Invoke(RemainingSeconds);
    }

    //===========================================
    //アクセサメソッド群
    //===========================================


    /// <summary>
    /// 指定されたプレイヤーのスコアを取得するメソッド
    /// </summary>
    public bool TryGetPlayerScore(PlayerRef playerRef, out PlayerScore score)
    {
        UserData? found = UserDataArray.FirstOrDefault(u => u.playerRef == playerRef);
        score = found.GetValueOrDefault().userScore;
        return found.HasValue;
    }
    
    /// <summary>
    /// 自分のプレイヤースコアを取得するメソッド
    /// </summary> 
    public bool TryGetMyScore(out PlayerScore score)
    {
        return TryGetPlayerScore(Runner.LocalPlayer, out score);
    }

    /// <summary>
    ///キル数降順でソートされたスコアデータ(keyとvalueを持つリスト)を返すメソッド
    /// </summary>
    // UserDataArrayを使ってキル数降順・デス数昇順・PlayerRef順でソートしたリストを返すメソッド
    public IReadOnlyList<UserData> GetSortedUserData()
    {
        var sortedUserData = UserDataArray
            .Where(u => !u.Equals(default(UserData))) // 空要素を除外
            .OrderByDescending(u => u.userScore.Kills) // キル数が多い順
            .ThenBy(u => u.userScore.Deaths)           // デス数が少ない順
            .ThenBy(u => u.playerRef.RawEncoded)       // PlayerRefの数値が小さい順
            .ToList();

        return sortedUserData;
    }

    /// <summary>
    /// 自分のプレイヤーが指定されたPlayerRefと一致するかどうかを確認するメソッド
    /// </summary>
    public bool IsMyPlayer(PlayerRef playerRef)
    {
        return Runner.LocalPlayer == playerRef;
    }

    /// <summary>
    ///  自分のプレイヤーのPlayerRefを取得するメソッド
    /// </summary> 
    public PlayerRef GetMyPlayerRef()
    {
        return Runner.LocalPlayer;
    }
    public NetworkObject GetMyPlayer()
    {
        return Runner.GetPlayerObject(Runner.LocalPlayer);
    }
    /// <summary>
    /// 現在のサーバー時間でのタイムスタンプを取得するメソッド
    /// </summary>
    public float GetCurrentTime()
    {
        return Runner.SimulationTime;
    }

    /// <summary>
    /// プレイヤーのチームを取得するメソッド
    /// </summary>
    public bool TryGetPlayerTeam(PlayerRef playerRef, out TeamType outTeam)
    {
        UserData? found = UserDataArray.FirstOrDefault(u => u.playerRef == playerRef);
        if (found.HasValue)
        {
            outTeam = found.Value.userTeam;
            return true;
        }
        else
        {
            outTeam = TeamType.None; // チームが見つからない場合はNoneを返す
            return false;
        }
    }

    //===========================================
    //-----------------デバッグ用----------------
    //===========================================

    //全プレイヤーのスコアをデバッグログに出力するメソッド
    public void PrintAllScore()
    {
        if (!Object.HasStateAuthority) return;
        foreach (var userData in UserDataArray)
        {
            if (userData.Equals(default(UserData))) continue;
            Debug.Log($"Player: {userData.playerRef}, Kills: {userData.userScore.Kills}, Deaths: {userData.userScore.Deaths}");
        }
    }

    //ランダムなプレイヤーのキルを生み出しNotifyDeathを呼び出すメソッド
    public void DebugRandomKill()
    {
        if (!Object.HasStateAuthority) return;
        var players = Runner.ActivePlayers.ToList();
        if (players.Count < 2) return; // 2人以上のプレイヤーが必要
        var victim = players[UnityEngine.Random.Range(0, players.Count)];
        PlayerRef killer;
        do
        {
            killer = players[UnityEngine.Random.Range(0, players.Count)];
        } while (killer == victim); // 自分をキルすることはできない
        NotifyDeath(Runner.SimulationTime, victim, killer);
    }

    private void Update()
    {
         debugInput = LocalInputHandler.CollectDebugInput();
        if (debugInput.BattleEndPressedDown)
        {
            OnTimeUp?.Invoke(); // デバッグ用に試合終了イベントを発火
        }
    }
   
}


