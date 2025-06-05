using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;



//ゲーム時間、キルデス、スコア管理を行うクラス
public class GameManager : NetworkBehaviour
{
    // シングルトンインスタンス
    public static GameManager Instance { get; private set; }
    //生成時イベント
    public static event Action OnGameManagerSpawned;

    //============================================
    //キルデス関係
    //============================================

    //キルログ構造体
    public struct KillLog
    {
        public PlayerRef Victim;
        public PlayerRef Killer;
        public float Timestamp; // キルが発生した時間
        public KillLog(PlayerRef victim, PlayerRef killer,float timeStamp)
        {
            Victim = victim;
            Killer = killer;
            Timestamp = timeStamp;
        }
    }

    //キルログ格納リスト
    private const int KillLogLimit = 100;
    private List<KillLog> killLogs = new List<KillLog>(KillLogLimit);

    /// <summary>
    /// プレイヤーの誰かが死亡したときに発火するイベント(victime, killer, timeStamp)
    /// </summary>
    public event Action<PlayerRef, PlayerRef, float> OnAnyPlayerDied;

    /// <summary>
    /// 自分のプレイヤーが死亡したときに発火するイベント(killer, timeStamp)
    /// </summary>
    public event Action<PlayerRef, float> OnMyPlayerDied;
    //発火先一覧
    //PlayerAvatarのPlayerDeathHandler
    //HUDManagerのRespawnPanel

    //============================================
    //スコア関係
    //============================================

    /// <summary>
    /// プレイヤーのスコアを管理する辞書（key: PlayerRef,value: PlayerScore)
    /// </summary>
    private Dictionary<PlayerRef, PlayerScore> PlayerScores { get; set;}

    /// <summary>
    /// スコア変更通知イベント
    /// </summary>
    public event Action OnManagerScoreChanged;

    //============================================
    //時間関係
    //============================================
    // 残り時間(秒)
    [Networked(OnChanged = nameof(TimeChangedCallback))]
    public int RemainingSeconds { get; private set; }
    // 時間変更時のイベント
    public event Action<int> OnTimeChanged;
    // 残り時間初期値（3分 = 180 秒）
    public int initialTimeSec = 180;
    // タイマー開始時の SimulationTime をサーバー／ホストでキャッシュ
    [Networked]
    private double startSimTime { get; set; } = 0.0;
    // タイマーが動作中かどうか

    [Networked]
    public bool IsTimerRunning { get; private set; } = false;


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
        //スコア辞書の初期化
        PlayerScores = new Dictionary<PlayerRef, PlayerScore>();
    }
    //非同期な初期化
    public override void Spawned()
    {
        
        Debug.Log($"ScoreManager Spawned. Current Player Count: {Runner.ActivePlayers.Count()}");
        //各環境でスコア辞書内を初期化
        foreach (var playerRef in Runner.ActivePlayers)
        {
            PlayerScore playerScore = new PlayerScore(0, 0);
            PlayerScores.Add(playerRef, playerScore);

        }
        PrintAllScore();

        //タイマー初期化
        if(Object.HasStateAuthority)
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
    public void NotifyDeath(PlayerRef victim, PlayerRef killer, float killTime)
    {
        
        if (!Object.HasStateAuthority) return;
        //killTime時に既に試合時間が終了していれば実行しない
        if(startSimTime + initialTimeSec <= Runner.SimulationTime)
        {
            Debug.LogWarning("Game time has already ended. Not processing death notification.");
            return;
        }
        //RPCでキルログの送信　時間はホスト基準
        RPC_SendDeathData(victim, killer, Runner.SimulationTime);

    }

    //キルログを全ローカル環境に送信し、ついでに色々するメソッド
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SendDeathData(PlayerRef victim, PlayerRef killer, float timeStamp)
    {
        //キルログを追加
        var killLog = new KillLog(victim, killer, timeStamp);
        killLogs.Add(killLog);
        //必要に応じて、キルログのサイズ制限などを行う
        if (killLogs.Count >= KillLogLimit) 
        {
            killLogs.RemoveAt(0); 
        }

        //スコア計算
        AddScore(victim, killer);

        //死亡イベントを発火
        OnAnyPlayerDied?.Invoke(victim, killer,timeStamp);
        Debug.Log($"Event発火元は{Runner.LocalPlayer}");
        //自分のプレイヤーに対しては、OnMyPlayerDiedイベントも発火
        if (Runner.LocalPlayer == victim)
        {
            OnMyPlayerDied?.Invoke(killer,timeStamp);
        }

    }

    public override void FixedUpdateNetwork()
    {
        if(!Object.HasStateAuthority) return;
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
                // タイマーが終了したときの処理はここ
            }
        }
    }

    // タイマースタート
    public void TimerStart()
    {
        if (!Object.HasStateAuthority) return;

        // いま何秒目かを startSimTime に記録
        startSimTime = Runner.SimulationTime;
        IsTimerRunning = true;

        // 「TimerStart した瞬間に RemainingSeconds」を初期値にしておく（念のため）
        RemainingSeconds = initialTimeSec;
    }

    // タイマーリセット（残り時間を初期値に戻し、停止する）
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
    public void AddScore(PlayerRef victim, PlayerRef killer = default)
    {
        if (PlayerScores.TryGetValue(victim, out PlayerScore victimScore))
        {
            victimScore.Deaths++;
            PlayerScores[victim] = victimScore;
        }
        
        if (PlayerScores.TryGetValue(killer, out PlayerScore killerScore))
        {
            killerScore.Kills++;
            PlayerScores[killer] = killerScore;
        }
        //キルプレイヤーがいない(None)ならスコアは加算しない

        // スコア変更イベントを発火
        OnManagerScoreChanged?.Invoke();
    }


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
    public IReadOnlyCollection<KeyValuePair<PlayerRef, PlayerScore>> GetSortedScores()
    {
        var SortedScores = PlayerScores
            .OrderByDescending(kvp => kvp.Value.Kills)
            .ToList();
        return SortedScores;
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
    /// <summary>
    /// 現在のサーバー時間でのタイムスタンプを取得するメソッド
    /// </summary>
    public float GetCurrentTime()
    {
        return Runner.SimulationTime;
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
        NotifyDeath(victim, killer, Runner.SimulationTime);
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


