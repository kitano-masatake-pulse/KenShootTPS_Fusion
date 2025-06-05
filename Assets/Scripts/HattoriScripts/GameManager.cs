using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;



//�Q�[�����ԁA�L���f�X�A�X�R�A�Ǘ����s���N���X
public class GameManager : NetworkBehaviour
{
    // �V���O���g���C���X�^���X
    public static GameManager Instance { get; private set; }
    //�������C�x���g
    public static event Action OnGameManagerSpawned;

    //============================================
    //�L���f�X�֌W
    //============================================

    //�L�����O�\����
    public struct KillLog
    {
        public PlayerRef Victim;
        public PlayerRef Killer;
        public float Timestamp; // �L����������������
        public KillLog(PlayerRef victim, PlayerRef killer,float timeStamp)
        {
            Victim = victim;
            Killer = killer;
            Timestamp = timeStamp;
        }
    }

    //�L�����O�i�[���X�g
    private const int KillLogLimit = 100;
    private List<KillLog> killLogs = new List<KillLog>(KillLogLimit);

    /// <summary>
    /// �v���C���[�̒N�������S�����Ƃ��ɔ��΂���C�x���g(victime, killer, timeStamp)
    /// </summary>
    public event Action<PlayerRef, PlayerRef, float> OnAnyPlayerDied;

    /// <summary>
    /// �����̃v���C���[�����S�����Ƃ��ɔ��΂���C�x���g(killer, timeStamp)
    /// </summary>
    public event Action<PlayerRef, float> OnMyPlayerDied;
    //���ΐ�ꗗ
    //PlayerAvatar��PlayerDeathHandler
    //HUDManager��RespawnPanel

    //============================================
    //�X�R�A�֌W
    //============================================

    /// <summary>
    /// �v���C���[�̃X�R�A���Ǘ����鎫���ikey: PlayerRef,value: PlayerScore)
    /// </summary>
    private Dictionary<PlayerRef, PlayerScore> PlayerScores { get; set;}

    /// <summary>
    /// �X�R�A�ύX�ʒm�C�x���g
    /// </summary>
    public event Action OnManagerScoreChanged;

    //============================================
    //���Ԋ֌W
    //============================================
    // �c�莞��(�b)
    [Networked(OnChanged = nameof(TimeChangedCallback))]
    public int RemainingSeconds { get; private set; }
    // ���ԕύX���̃C�x���g
    public event Action<int> OnTimeChanged;
    // �c�莞�ԏ����l�i3�� = 180 �b�j
    public int initialTimeSec = 180;
    // �^�C�}�[�J�n���� SimulationTime ���T�[�o�[�^�z�X�g�ŃL���b�V��
    [Networked]
    private double startSimTime { get; set; } = 0.0;
    // �^�C�}�[�����쒆���ǂ���

    [Networked]
    public bool IsTimerRunning { get; private set; } = false;


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
        //�X�R�A�����̏�����
        PlayerScores = new Dictionary<PlayerRef, PlayerScore>();
    }
    //�񓯊��ȏ�����
    public override void Spawned()
    {
        
        Debug.Log($"ScoreManager Spawned. Current Player Count: {Runner.ActivePlayers.Count()}");
        //�e���ŃX�R�A��������������
        foreach (var playerRef in Runner.ActivePlayers)
        {
            PlayerScore playerScore = new PlayerScore(0, 0);
            PlayerScores.Add(playerRef, playerScore);

        }
        PrintAllScore();

        //�^�C�}�[������
        if(Object.HasStateAuthority)
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
    public void NotifyDeath(PlayerRef victim, PlayerRef killer, float killTime)
    {
        
        if (!Object.HasStateAuthority) return;
        //killTime���Ɋ��Ɏ������Ԃ��I�����Ă���Ύ��s���Ȃ�
        if(startSimTime + initialTimeSec <= Runner.SimulationTime)
        {
            Debug.LogWarning("Game time has already ended. Not processing death notification.");
            return;
        }
        //RPC�ŃL�����O�̑��M�@���Ԃ̓z�X�g�
        RPC_SendDeathData(victim, killer, Runner.SimulationTime);

    }

    //�L�����O��S���[�J�����ɑ��M���A���łɐF�X���郁�\�b�h
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SendDeathData(PlayerRef victim, PlayerRef killer, float timeStamp)
    {
        //�L�����O��ǉ�
        var killLog = new KillLog(victim, killer, timeStamp);
        killLogs.Add(killLog);
        //�K�v�ɉ����āA�L�����O�̃T�C�Y�����Ȃǂ��s��
        if (killLogs.Count >= KillLogLimit) 
        {
            killLogs.RemoveAt(0); 
        }

        //�X�R�A�v�Z
        AddScore(victim, killer);

        //���S�C�x���g�𔭉�
        OnAnyPlayerDied?.Invoke(victim, killer,timeStamp);
        Debug.Log($"Event���Ό���{Runner.LocalPlayer}");
        //�����̃v���C���[�ɑ΂��ẮAOnMyPlayerDied�C�x���g������
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
                // �^�C�}�[���I�������Ƃ��̏����͂���
            }
        }
    }

    // �^�C�}�[�X�^�[�g
    public void TimerStart()
    {
        if (!Object.HasStateAuthority) return;

        // ���܉��b�ڂ��� startSimTime �ɋL�^
        startSimTime = Runner.SimulationTime;
        IsTimerRunning = true;

        // �uTimerStart �����u�Ԃ� RemainingSeconds�v�������l�ɂ��Ă����i�O�̂��߁j
        RemainingSeconds = initialTimeSec;
    }

    // �^�C�}�[���Z�b�g�i�c�莞�Ԃ������l�ɖ߂��A��~����j
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
        //�L���v���C���[�����Ȃ�(None)�Ȃ�X�R�A�͉��Z���Ȃ�

        // �X�R�A�ύX�C�x���g�𔭉�
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
    public IReadOnlyCollection<KeyValuePair<PlayerRef, PlayerScore>> GetSortedScores()
    {
        var SortedScores = PlayerScores
            .OrderByDescending(kvp => kvp.Value.Kills)
            .ToList();
        return SortedScores;
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
    /// <summary>
    /// ���݂̃T�[�o�[���Ԃł̃^�C���X�^���v���擾���郁�\�b�h
    /// </summary>
    public float GetCurrentTime()
    {
        return Runner.SimulationTime;
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
        NotifyDeath(victim, killer, Runner.SimulationTime);
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


