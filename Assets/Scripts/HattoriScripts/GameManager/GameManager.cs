using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;



//�Q�[�����ԁA�L���f�X�A�X�R�A�Ǘ����s���N���X
public class GameManager : NetworkBehaviour,IAfterSpawned
{
    // �V���O���g���C���X�^���X
    public static GameManager Instance { get; private set; }
    //�������̂��߂̃t���O
    private bool _afterSpawned = false;
    private bool _sceneLoaded = false;
    
    //�������C�x���g
    public static event Action OnGameManagerSpawned;
    
    //�`�[����������
    public Dictionary<PlayerRef, TeamType> PlayerTeams { get; private set; }

    //============================================
    //�L���f�X�֌W
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
    //���ΐ�ꗗ
    //PlayerAvatar��PlayerDeathHandler
    //HUDManager��RespawnPanel

    //============================================
    //�X�R�A�֌W
    //============================================
    private Dictionary<PlayerRef, PlayerScore> PlayerScores { get; set;}
    private Dictionary<PlayerRef, PlayerScore> PlayerScoresRed { get; set; }
    private Dictionary<PlayerRef, PlayerScore> PlayerScoresBlue { get; set; }
    public event Action OnScoreChanged;

    //============================================
    //���Ԋ֌W
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
    // �C�x���g���Ηp�t���O
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
    //����������
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
                //�v���C���[�̃`�[���ɉ����ăX�R�A�𕪂���
                //�����Ƀ`�[�����������ɂ��o�^
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
    //�z�X�g���ł̂݌Ă΂�郁�\�b�h�Q
    //===========================================
    //�v���C���[�����S�����Ƃ��ɌĂ΂�A�F�X�ȏ������s�����\�b�h
    public void NotifyDeath(float killTime,PlayerRef victim, PlayerRef killer)
    {
        Debug.Log($"Object :{Object}");
        Debug.Log($"Object.HasStateAuthority :{Object.HasStateAuthority}");
        if (!Object.HasStateAuthority) return;
        //killTime���Ɋ��Ɏ������Ԃ��I�����Ă���Ύ��s���Ȃ�
        if(startSimTime + initialTimeSec <= Runner.SimulationTime)
        {
            Debug.LogWarning("Game time has already ended. Not processing death notification.");
            return;
        }
        RPC_SendDeathData(Runner.SimulationTime, victim, killer);

    }

    //�L�����O��S���[�J�����ɑ��M���A���łɐF�X���郁�\�b�h
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SendDeathData(float timeStamp,PlayerRef victim, PlayerRef killer)
    {
        var killLog = new KillLog(timeStamp, victim, killer);
        killLogs.Add(killLog);

        //����𒴂�����ŌẪ��O���폜
        if (killLogs.Count >= KillLogLimit) 
        {
            killLogs.RemoveAt(0); 
        }
        NextTickTimer =    // ���e�B�b�N���Expired�ɂȂ�
            TickTimer.CreateFromTicks(Runner, 2);

        AddScore(victim, killer);

        OnAnyPlayerDied?.Invoke(timeStamp, victim, killer);
        if (IsMyPlayer(victim))
        {
            OnMyPlayerDied?.Invoke(timeStamp, killer);
        }

    }

    //���ԍX�V
    public override void FixedUpdateNetwork()
    {
        if (NextTickTimer.Expired(Runner))
        {
            NextTickTimer = TickTimer.None;   // ���Z�b�g
            if (_scoreDirty)
            {
                //�X�R�A�ύX�C�x���g�𔭉�
                OnScoreChanged?.Invoke();
                _scoreDirty = false;
            }

        }

        

        // �^�C�}�[�X�V����
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
    //�S���[�J�����ŌĂ΂�郁�\�b�h�Q
    //===========================================
    //�v���C���[�̃X�R�A���X�V���郁�\�b�h
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
        //�L���v���C���[�����Ȃ�(None)�Ȃ�X�R�A�͉��Z���Ȃ�

        _scoreDirty = true;
        // �X�R�A�ύX�C�x���g�𔭉�
        //OnScoreChanged?.Invoke();
    }

    //���ԍX�V���̃R�[���o�b�N�Ȃ�
    static void TimeChangedCallback(Changed<GameManager> changed)
    {
        changed.Behaviour.RaiseTimeChanged();
    }
    private void RaiseTimeChanged()
    {
        OnTimeChanged?.Invoke(RemainingSeconds);
    }

    //===========================================
    //�A�N�Z�T���\�b�h�Q
    //===========================================
    /// <summary>
    /// �w�肳�ꂽ�v���C���[�̃X�R�A���擾���郁�\�b�h
    /// </summary>
    public bool TryGetPlayerScore(PlayerRef playerRef, out PlayerScore score)
    {
        if (PlayerScores == null)
        {
            Debug.LogError("PlayerScores is null!");
        }
        //���O
        Debug.Log($"TryGetPlayerScore�F {PlayerScores[playerRef].Kills}, {PlayerScores[playerRef].Deaths}");
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
    /// �����̃v���C���[�X�R�A���擾���郁�\�b�h
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
    ///�L�����~���Ń\�[�g���ꂽ�X�R�A�f�[�^(key��value�������X�g)��Ԃ����\�b�h
    /// </summary>
    public IReadOnlyList<KeyValuePair<PlayerRef, PlayerScore>> GetSortedScores()
    {
        var sortedScores = PlayerScores
         .OrderByDescending(kvp => kvp.Value.Kills)     // �L������������
         .ThenBy(kvp => kvp.Value.Deaths)               // �f�X�������Ȃ���
         .ThenBy(kvp => kvp.Key.RawEncoded)             // PlayerRef�̐��l����������
         .ToList();

        return sortedScores;
    }

    ///<summary>
    ///�S�v���C���[���̃X�R�A�f�[�^(����)��Ԃ����\�b�h
    ///</summary>
    public IReadOnlyDictionary<PlayerRef, PlayerScore> GetAllScores()
    {
        return PlayerScores;
    }
    /// <summary>
    /// �����̃v���C���[���w�肳�ꂽPlayerRef�ƈ�v���邩�ǂ������m�F���郁�\�b�h
    /// </summary>
    public bool IsMyPlayer(PlayerRef playerRef)
    {
        return Runner.LocalPlayer == playerRef;
    }

    /// <summary>
    ///  �����̃v���C���[��PlayerRef���擾���郁�\�b�h
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
    /// ���݂̃T�[�o�[���Ԃł̃^�C���X�^���v���擾���郁�\�b�h
    /// </summary>
    public float GetCurrentTime()
    {
        return Runner.SimulationTime;
    }

    /// <summary>
    /// �v���C���[�̃`�[�����擾���郁�\�b�h
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
            outTeam = TeamType.None; // �`�[����������Ȃ��ꍇ��None��Ԃ�
            return false;
        }
    }

    /// <summary>
    /// �S���[�J�����Ńv���C���[�̃`�[����ݒ肷�郁�\�b�h
    /// </summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetPlayerTeam(PlayerRef playerRef, TeamType newTeam)
    {
        
        TeamType oldTeam = TeamType.None;
        if (TryGetPlayerTeam(playerRef, out TeamType currentTeam))
        {
            // ���ɓ����`�[���ɏ������Ă���ꍇ�͉������Ȃ�
            if (currentTeam == newTeam) return;
            else             {
                oldTeam = currentTeam; // �Â��`�[����ۑ�
            }
        }

        //�`�[���������X�V
        if (PlayerTeams.ContainsKey(playerRef))
        {
            PlayerTeams[playerRef] = newTeam;
        }
        else
        {
            PlayerTeams.Add(playerRef, newTeam);
        }

        //���ɃX�R�A�����݂��邩�������A�����PlayerScore���擾
        PlayerScore currentScore = new PlayerScore(0, 0);
        if (PlayerScores.TryGetValue(playerRef, out PlayerScore score))
        {
            currentScore = score; 
        }

        // �Â��`�[���̃X�R�A���폜
        if (oldTeam == TeamType.Red && PlayerScoresRed.ContainsKey(playerRef))
        {
            PlayerScoresRed.Remove(playerRef);
        }
        else if (oldTeam == TeamType.Blue && PlayerScoresBlue.ContainsKey(playerRef))
        {
            PlayerScoresBlue.Remove(playerRef);
        }

        // �V�����`�[���̃X�R�A��ǉ��܂��͍X�V
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
    //-----------------�f�o�b�O�p----------------
    //===========================================

    //�S�v���C���[�̃X�R�A���f�o�b�O���O�ɏo�͂��郁�\�b�h
    public void PrintAllScore()
    {
        if (!Object.HasStateAuthority) return;
        foreach (var kvp in PlayerScores)
        {
            Debug.Log($"Player: {kvp.Key}, Kills: {kvp.Value.Kills}, Deaths: {kvp.Value.Deaths}");
        }
    }

    //�����_���ȃv���C���[�̃L���𐶂ݏo��NotifyDeath���Ăяo�����\�b�h
    public void DebugRandomKill()
    {
        if (!Object.HasStateAuthority) return;
        var players = Runner.ActivePlayers.ToList();
        if (players.Count < 2) return; // 2�l�ȏ�̃v���C���[���K�v
        var victim = players[UnityEngine.Random.Range(0, players.Count)];
        PlayerRef killer;
        do
        {
            killer = players[UnityEngine.Random.Range(0, players.Count)];
        } while (killer == victim); // �������L�����邱�Ƃ͂ł��Ȃ�
        NotifyDeath(Runner.SimulationTime, victim, killer);
    }
    //===========================================
    /*
    private void Update()
    {
        //�f�o�b�O�p�F�L�[�������ƃ����_���ȃL���𔭐�������
        if (Input.GetKeyDown(KeyCode.L))
        {
            DebugRandomKill();
        }
    }
    */
}


