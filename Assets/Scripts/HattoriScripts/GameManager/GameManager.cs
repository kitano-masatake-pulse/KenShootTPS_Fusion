using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;



//ゲーム時間、キルデス、スコア管理を行うクラス
public class GameManager : NetworkBehaviour,IAfterSpawned
{
    // シングルトンインスタンス
    public static GameManager Instance { get; private set; }
    //初期化のためのフラグ
    private bool _afterSpawned = false;
    private bool _sceneLoaded = false;
    
    //生成時イベント
    public static event Action OnGameManagerSpawned;
    
    //チーム分け辞書
    public Dictionary<PlayerRef, TeamType> PlayerTeams { get; private set; }

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
    private Dictionary<PlayerRef, PlayerScore> PlayerScores { get; set;}
    private Dictionary<PlayerRef, PlayerScore> PlayerScoresRed { get; set; }
    private Dictionary<PlayerRef, PlayerScore> PlayerScoresBlue { get; set; }
    public event Action OnScoreChanged;

    //============================================
    //時間関係
    //============================================
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
    //===========================================
    //初期化処理
    //===========================================
    private void Awake()
    {
        Debug.Log("GameManager Awake called.");
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void Spawned()
    {
        Debug.Log("GameManager Spawned called.");
       
    }

    public void SceneLoaded()
    {
        _sceneLoaded = true;
        TryInitialize();
    }

    public void AfterSpawned()
    {
        _afterSpawned = true;
        TryInitialize();
    }

    private void TryInitialize()
    {
        if (_afterSpawned && _sceneLoaded)
        {
            InitializeGameManager();
        }
    }
    private void InitializeGameManager()
    {
        if (Runner == null)
        {
            Debug.LogError("GameManager: Spawned > Runner is null. ");
            return;
        }

        PlayerScores = new Dictionary<PlayerRef, PlayerScore>();
        PlayerScoresRed = new Dictionary<PlayerRef, PlayerScore>();
        PlayerScoresBlue = new Dictionary<PlayerRef, PlayerScore>();
        PlayerTeams = new Dictionary<PlayerRef, TeamType>();

        foreach (var playerRef in Runner.ActivePlayers)
        {
            PlayerScore playerScore = new PlayerScore(0, 0);
            PlayerScores.Add(playerRef, playerScore);
            if (Runner.TryGetPlayerObject(playerRef, out NetworkObject playerObject))
            {
                //プレイヤーのチームに応じてスコアを分ける
                //同時にチーム分け辞書にも登録
                if (playerObject.TryGetComponent(out PlayerNetworkState playerState))
                {
                    if (playerState.Team == TeamType.Red)
                    {
                        PlayerScoresRed.Add(playerRef, playerScore);
                        PlayerTeams.Add(playerRef, TeamType.Red);

                    }
                    else if (playerState.Team == TeamType.Blue)
                    {
                        PlayerScoresBlue.Add(playerRef, playerScore);
                        PlayerTeams.Add(playerRef, TeamType.Blue);
                    }
                    else
                    {
                        PlayerTeams.Add(playerRef, TeamType.None);
                    }
                }
            }
            else
            {
                Debug.LogWarning($"GameManager: Player {playerRef} has no associated NetworkObject.");
            }

        }
        //PrintAllScore();

        if (Object.HasStateAuthority)
        {
            RemainingSeconds = initialTimeSec;
            startSimTime = Runner.SimulationTime;
            TimerStart();
        }
        OnGameManagerSpawned?.Invoke();
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

        OnAnyPlayerDied?.Invoke(timeStamp, victim, killer);
        if (IsMyPlayer(victim))
        {
            OnMyPlayerDied?.Invoke(timeStamp, killer);
        }

    }

    //時間更新
    public override void FixedUpdateNetwork()
    {
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

        

        // タイマー更新処理
        if (!Object.HasStateAuthority) return;
        if(!IsTimerRunning) return;

        double elapsed = Runner.SimulationTime - startSimTime;
        int elapsedSeconds = Mathf.FloorToInt((float)elapsed);

        int newRemainingSeconds = Mathf.Max(initialTimeSec - elapsedSeconds);

        if (newRemainingSeconds != RemainingSeconds)
        {
            RemainingSeconds = newRemainingSeconds;
            if (RemainingSeconds <= 0)
            {
                IsTimerRunning = false;
                Debug.Log("Game Over! Time's up!");
            }
        }
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

    //===========================================
    //全ローカル環境で呼ばれるメソッド群
    //===========================================
    //プレイヤーのスコアを更新するメソッド
    public void AddScore(PlayerRef victim, PlayerRef killer)
    {
        if (PlayerScores.TryGetValue(victim, out PlayerScore victimScore))
        {
            victimScore.Deaths++;
            PlayerScores[victim] = victimScore;
            if(PlayerScoresRed.ContainsKey(victim))
            {
                PlayerScoresRed[victim] = victimScore;
            }
            else if(PlayerScoresBlue.ContainsKey(victim))
            {
                PlayerScoresBlue[victim] = victimScore;
            }

        }
        
        if (PlayerScores.TryGetValue(killer, out PlayerScore killerScore))
        {
            killerScore.Kills++;
            PlayerScores[killer] = killerScore;
            if (PlayerScoresRed.ContainsKey(killer))
            {
                PlayerScoresRed[killer] = killerScore;
            }
            else if (PlayerScoresBlue.ContainsKey(victim))
            {
                PlayerScoresBlue[killer] = killerScore;
            }
        }
        //キルプレイヤーがいない(None)ならスコアは加算しない

        _scoreDirty = true;
        // スコア変更イベントを発火
        //OnScoreChanged?.Invoke();
    }

    //時間更新時のコールバックなど
    static void TimeChangedCallback(Changed<GameManager> changed)
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
        if (PlayerScores == null)
        {
            Debug.LogError("PlayerScores is null!");
        }
        //ログ
        Debug.Log($"TryGetPlayerScore： {PlayerScores[playerRef].Kills}, {PlayerScores[playerRef].Deaths}");
        return PlayerScores.TryGetValue(playerRef, out score);
    }
    
    public PlayerScore GetPlayerScore(PlayerRef playerRef)
    {
        if (PlayerScores.TryGetValue(playerRef, out PlayerScore score))
        {
            return score;
        }
        else
        {
            throw new KeyNotFoundException($"PlayerRef {playerRef} not found in PlayerScores.");
        }
    }
    /// <summary>
    /// 自分のプレイヤースコアを取得するメソッド
    /// </summary> 
    public bool TryGetMyScore(out PlayerScore score)
    {
        return TryGetPlayerScore(Runner.LocalPlayer, out score);
    }
    public PlayerScore GetMyScore()
    {
        if (TryGetMyScore(out PlayerScore score))
        {
            return score;
        }
        else
        {
            throw new KeyNotFoundException($"Local player score not found in PlayerScores.");
        }
    }

    /// <summary>
    ///キル数降順でソートされたスコアデータ(keyとvalueを持つリスト)を返すメソッド
    /// </summary>
    public IReadOnlyList<KeyValuePair<PlayerRef, PlayerScore>> GetSortedScores()
    {
        var sortedScores = PlayerScores
         .OrderByDescending(kvp => kvp.Value.Kills)     // キル数が多い順
         .ThenBy(kvp => kvp.Value.Deaths)               // デス数が少ない順
         .ThenBy(kvp => kvp.Key.RawEncoded)             // PlayerRefの数値が小さい順
         .ToList();

        return sortedScores;
    }

    ///<summary>
    ///全プレイヤー分のスコアデータ(辞書)を返すメソッド
    ///</summary>
    public IReadOnlyDictionary<PlayerRef, PlayerScore> GetAllScores()
    {
        return PlayerScores;
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
        if (PlayerTeams.TryGetValue(playerRef, out TeamType team))
        {
            outTeam = team;
            return true;
        }
        else
        {
            outTeam = TeamType.None; // チームが見つからない場合はNoneを返す
            return false;
        }
    }

    /// <summary>
    /// 全ローカル環境でプレイヤーのチームを設定するメソッド
    /// </summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetPlayerTeam(PlayerRef playerRef, TeamType newTeam)
    {
        
        TeamType oldTeam = TeamType.None;
        if (TryGetPlayerTeam(playerRef, out TeamType currentTeam))
        {
            // 既に同じチームに所属している場合は何もしない
            if (currentTeam == newTeam) return;
            else             {
                oldTeam = currentTeam; // 古いチームを保存
            }
        }

        //チーム辞書を更新
        if (PlayerTeams.ContainsKey(playerRef))
        {
            PlayerTeams[playerRef] = newTeam;
        }
        else
        {
            PlayerTeams.Add(playerRef, newTeam);
        }

        //既にスコアが存在するか検索し、あればPlayerScoreを取得
        PlayerScore currentScore = new PlayerScore(0, 0);
        if (PlayerScores.TryGetValue(playerRef, out PlayerScore score))
        {
            currentScore = score; 
        }

        // 古いチームのスコアを削除
        if (oldTeam == TeamType.Red && PlayerScoresRed.ContainsKey(playerRef))
        {
            PlayerScoresRed.Remove(playerRef);
        }
        else if (oldTeam == TeamType.Blue && PlayerScoresBlue.ContainsKey(playerRef))
        {
            PlayerScoresBlue.Remove(playerRef);
        }

        // 新しいチームのスコアを追加または更新
        if (newTeam == TeamType.Red && !PlayerScoresRed.ContainsKey(playerRef))
        {
            PlayerScoresRed.Add(playerRef, currentScore);
        }
        else if (newTeam == TeamType.Blue && !PlayerScoresBlue.ContainsKey(playerRef))
        {
             PlayerScoresBlue.Add(playerRef, currentScore);
        }
    }

    //===========================================
    //-----------------デバッグ用----------------
    //===========================================

    //全プレイヤーのスコアをデバッグログに出力するメソッド
    public void PrintAllScore()
    {
        if (!Object.HasStateAuthority) return;
        foreach (var kvp in PlayerScores)
        {
            Debug.Log($"Player: {kvp.Key}, Kills: {kvp.Value.Kills}, Deaths: {kvp.Value.Deaths}");
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
    //===========================================
    /*
    private void Update()
    {
        //デバッグ用：キーを押すとランダムなキルを発生させる
        if (Input.GetKeyDown(KeyCode.L))
        {
            DebugRandomKill();
        }
    }
    */
}


