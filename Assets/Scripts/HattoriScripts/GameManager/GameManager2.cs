using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;



//�Q�[�����ԁA�L���f�X�A�X�R�A�Ǘ����s���N���X
public class GameManager2 : NetworkBehaviour,IAfterSpawned
{
    // �V���O���g���C���X�^���X
    public static GameManager2 Instance { get; private set; }
    //�������̂��߂̃t���O
    private bool _afterSpawned = false;
    private bool _sceneLoaded = false;
    
    //�������C�x���g
    public static event Action OnManagerInitialized;
    //�����I���C�x���g
    public event Action OnTimeUp;

    
    //============================================
    //���[�U�[�f�[�^�֌W
    //============================================
    public UserData[] UserDataArray { get; private set; } = new UserData[50];


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
            InitializeGameManager(); // �N���C�A���g���ŏ��������s��
        }

    }

    public void InitializeGameManager()
    {
        if (Runner == null)
        {
            Debug.LogError("GameManager2: Spawned > Runner is null. ");
            return;
        }

        //������UserData�̏�����������ǉ�����

        if (Object.HasStateAuthority)
        {
            RemainingSeconds = initialTimeSec;
            startSimTime = Runner.SimulationTime;
            TimerStart();
        }
        OnManagerInitialized?.Invoke();
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
                //�����I���������J�n
                EndGame();
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

    private void EndGame()
    {
        if (!Object.HasStateAuthority) return;
        Debug.Log("GameManager2: EndGame called. Game has ended.");
        //�����I���C�x���g�𔭉�
        OnTimeUp?.Invoke();

    }

    //===========================================
    //�S���[�J�����ŌĂ΂�郁�\�b�h�Q
    //===========================================
    //�v���C���[�̃X�R�A���X�V���郁�\�b�h
    public void AddScore(PlayerRef victim, PlayerRef killer)
    {
        //�f�X�X�R�A���Z
        //FirstOrDefault���g���ƃ����`����null�ł͂Ȃ�
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

        if (killer != PlayerRef.None)
        {
            //�L���X�R�A���Z
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

        // �X�R�A�ύX�C�x���g�𔭉�
        //OnScoreChanged?.Invoke();
    }

    //���ԍX�V���̃R�[���o�b�N�Ȃ�
    static void TimeChangedCallback(Changed<GameManager2> changed)
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
        UserData? found = UserDataArray.FirstOrDefault(u => u.playerRef == playerRef);
        score = found.GetValueOrDefault().userScore;
        return found.HasValue;
    }
    
    /// <summary>
    /// �����̃v���C���[�X�R�A���擾���郁�\�b�h
    /// </summary> 
    public bool TryGetMyScore(out PlayerScore score)
    {
        return TryGetPlayerScore(Runner.LocalPlayer, out score);
    }

    /// <summary>
    ///�L�����~���Ń\�[�g���ꂽ�X�R�A�f�[�^(key��value�������X�g)��Ԃ����\�b�h
    /// </summary>
    // UserDataArray���g���ăL�����~���E�f�X�������EPlayerRef���Ń\�[�g�������X�g��Ԃ����\�b�h
    public IReadOnlyList<UserData> GetSortedUserData()
    {
        var sortedUserData = UserDataArray
            .Where(u => !u.Equals(default(UserData))) // ��v�f�����O
            .OrderByDescending(u => u.userScore.Kills) // �L������������
            .ThenBy(u => u.userScore.Deaths)           // �f�X�������Ȃ���
            .ThenBy(u => u.playerRef.RawEncoded)       // PlayerRef�̐��l����������
            .ToList();

        return sortedUserData;
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
        UserData? found = UserDataArray.FirstOrDefault(u => u.playerRef == playerRef);
        if (found.HasValue)
        {
            outTeam = found.Value.userTeam;
            return true;
        }
        else
        {
            outTeam = TeamType.None; // �`�[����������Ȃ��ꍇ��None��Ԃ�
            return false;
        }
    }

    //===========================================
    //-----------------�f�o�b�O�p----------------
    //===========================================

    //�S�v���C���[�̃X�R�A���f�o�b�O���O�ɏo�͂��郁�\�b�h
    public void PrintAllScore()
    {
        if (!Object.HasStateAuthority) return;
        foreach (var userData in UserDataArray)
        {
            if (userData.Equals(default(UserData))) continue;
            Debug.Log($"Player: {userData.playerRef}, Kills: {userData.userScore.Kills}, Deaths: {userData.userScore.Deaths}");
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
    
    private void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.N))
        {
            OnTimeUp?.Invoke();
        }
    }
   
}


