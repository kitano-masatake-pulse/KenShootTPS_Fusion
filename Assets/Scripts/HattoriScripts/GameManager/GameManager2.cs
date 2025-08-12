using Fusion;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;



//�Q�[�����ԁA�L���f�X�A�X�R�A�Ǘ����s���N���X
public class GameManager2 : NetworkBehaviour,IAfterSpawned
{
    // �V���O���g���C���X�^���X
    public static GameManager2 Instance { get; private set; }
    //�������̂��߂̃t���O
    private bool _afterSpawned = false;
    private bool _sceneLoaded = false;

    // �������������v���C���[�̃J�E���g
    private int readyPlayerCount = 0;

    //�������C�x���g
    public static event Action OnManagerInitialized;
    //�����I���C�x���g
    public event Action OnTimeUp;

    
    //============================================
    //���[�U�[�f�[�^�֌W
    //============================================
    public UserData[] UserDataArray { get; private set; } = new UserData[50];
    bool isUserDataArrayDirty = false; // �ŐV��UserDataArray���������܂�����Ă��Ȃ����ǂ����̃t���O


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
    [Header("�Q�[���X�^�[�g�܂ł̃J�E���g(3�b�ȏ�)")]
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

    //============================================
    //�f�o�b�O�p����
    //============================================
    public DebugInputData debugInput = DebugInputData.Default();

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
        Debug.Log("GameManager2: InitializeGameManager called.");


        if (Runner.IsServer)
        {
            RemainingSeconds = initialTimeSec;
            startSimTime = Runner.SimulationTime;
        }


        //������UserData�̏�����������ǉ�����
        if (Runner.IsServer)
        {
            RPC_RequestId(); // �z�X�g����S�N���C�A���g��ID�v���𑗐M
            Debug.Log("GameManager2: InitializeGameManager called on server. Requesting IDs from clients.");
        }
        else 
        {
            RPC_SubmitIdToHost(ConnectionManager.Instance.GetLocalID()); // �N���C�A���g�͎�����ID���z�X�g�ɑ��M
            Debug.Log("GameManager2: InitializeGameManager called on client. Waiting for RPC_RequestId.");
            // �N���C�A���g���ł�RPC_RequestId��҂�
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


    // �@ �z�X�g���S�N���C�A���g �v��
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_RequestId()
    {
        Debug.Log("GameManager2: RPC_RequestId called. Requesting IDs from clients.");
        // �N���C�A���g�͎����� ID �𑗐M����
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
        PlayerRef sender = info.Source;            // �ǂ� PlayerRef ���痈����

        if (GameManager2.Instance != null)
        {
            GameManager2.Instance.RegisterUserID(userID, sender); // GameManager2�̃��\�b�h���Ăяo���ă��[�U�[ID��o�^

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
        //UserDataArray�̒�����player�Ɉ�v����v�f��T��
        int index = Array.FindIndex(UserDataArray, u => u.userGuid == userID);
        if (index >= 0)
        {
            //���������ꍇ�͍X�V
            UserDataArray[index].playerRef = player;
            UserDataArray[index].userConnectionState = ConnectionState.Connected; // �ڑ���Ԃ��X�V


            // �f�o�b�O�p�ɏ���\��
            UserDataArray[index].DisplayUserInfo();

            //���łɓo�^����Ă���̂ɍēx�o�^���悤�Ƃ����Ƃ������Ƃ́A�Đڑ��҂ƍl������
            Debug.Log($"UserData for player {player} already exists. Updated connection state and player reference. UserID: {userID}");
            // �����ŕK�v�Ȃ�Đڑ����̏�����ǉ����邱�Ƃ��\


            // �Ⴆ�΁A�X�R�A��`�[�����̍Đݒ�Ȃ�
            ShareUserDataArray(UserDataArray); // �Đڑ����ɑS�N���C�A���g�ɍŐV��UserDataArray�����L����





        }
        else
        {

            var newUserData = new UserData
            {
                userGuid = userID,
                playerRef = player,
                userScore = new PlayerScore(), // �X�R�A�͏�����
                userTeam = TeamType.None, // �`�[���͏�����
                userConnectionState = ConnectionState.Connected, // �ڑ���Ԃ͐ڑ����ɐݒ�
                //userName = $"Player {userID}" // ���[�U�[���̓f�t�H���g�ݒ�
            };

            AddUserData(newUserData);
            // �f�o�b�O�p�ɏ���\��
            newUserData.DisplayUserInfo();
        }

        isUserDataArrayDirty = true; // UserDataArray�̓������K�v�ł��邱�Ƃ������t���O�𗧂Ă�

    }



    void AddUserData(UserData userData)
    {
        //������Ȃ������ꍇ�͒ǉ�
        for (int i = 0; i < UserDataArray.Length; i++)
        {
            if (UserDataArray[i].Equals(default(UserData)))
            {
                UserDataArray[i] = userData;
                break;
            }
        }



    }


    //���[�U�[�f�[�^�̍X�V���s�����\�b�h
    void UpdateUserDataArray(UserData userData)
    {

        int index = Array.FindIndex(UserDataArray, u => u.userGuid==userData.userGuid);
        if (index >= 0)
        {
            //���������ꍇ�͍X�V
            UserDataArray[index] = userData;
            
        }
        else
        {

            //var newUserData = new UserData
            //{
            //    userID = userID,
            //    playerRef = player,
            //    userScore = new PlayerScore(), // �X�R�A�͏�����
            //    userTeam = TeamType.None, // �`�[���͏�����
            //    userConnectionState = ConnectionState.Connected, // �ڑ���Ԃ͐ڑ����ɐݒ�
            //    //userName = $"Player {userID}" // ���[�U�[���̓f�t�H���g�ݒ�
            //};

            AddUserData(userData);
            // �f�o�b�O�p�ɏ���\��
            //newUserData.DisplayUserInfo();
        }

        isUserDataArrayDirty = true; // UserDataArray�̓������K�v�ł��邱�Ƃ������t���O�𗧂Ă�

    }

    public void UpdateConnectionState(PlayerRef player, ConnectionState state)
    { 
        if (!Runner.IsServer) { return; }
        //UserDataArray�̒�����player�Ɉ�v����v�f��T��
        int index = Array.FindIndex(UserDataArray, u => u.playerRef == player);
        if (index >= 0)
        {
            //���������ꍇ�͍X�V
            UserDataArray[index].userConnectionState = state; // �ڑ���Ԃ��X�V
            isUserDataArrayDirty = true; // UserDataArray�̓������K�v�ł��邱�Ƃ������t���O�𗧂Ă�
        }
        else
        {
            Debug.LogWarning($"Player {player} not found in UserDataArray.");
        }

    }



    //���[�U�[�f�[�^�̃`�F�b�N���s�����\�b�h
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
                // �e�v���C���[��UserData�����݂��邩�`�F�b�N
                //PlayerRef�Ń`�F�b�N���Ă邪�A�ق�Ƃ��ɐ��m�ɂ��Ȃ�UserID���ƍ������ق��������H
                if (!userDataArray.Any(d => d.playerRef == activePlayer ))
                {
                    Debug.LogWarning($"UserData for player {activePlayer} is missing or invalid.");
                    return false;
                }
                

            }

            return true; // �S�Ẵv���C���[��UserData�����݂���ꍇ��true��Ԃ�
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
            if (userData.Equals(default(UserData))) continue; // ��v�f�����O
            RPC_ShareUserData(userData);
        }



        RPC_InitializeCompleted();// �����������C�x���g�𔭉�
        _scoreDirty = true; // �X�R�A���X�V���ꂽ���Ƃ������t���O�𗧂Ă�



    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_ShareUserData(UserData sharedUserData)
    {

        int elementSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(UserData));
       
        Debug.Log($"RPC_ShareUserData called. element size {elementSize} bytes.");


        // UserDataArray�̒�����player�Ɉ�v����v�f��T��
        int index = Array.FindIndex(UserDataArray, u => u.playerRef == sharedUserData.playerRef);
        if (index >= 0)
        {
            //���������ꍇ�͍X�V
            UserDataArray[index] = sharedUserData;
        }
        else
        {
            //������Ȃ������ꍇ�͒ǉ�
            AddUserData(sharedUserData);
        }
        isUserDataArrayDirty = true; // UserDataArray�̓������K�v�ł��邱�Ƃ������t���O�𗧂Ă�
        Debug.Log($"UserData for player {sharedUserData.playerRef} updated or added. UserID {sharedUserData.userGuid}");


        _scoreDirty = true; // �X�R�A���X�V���ꂽ���Ƃ������t���O�𗧂Ă�



    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_InitializeCompleted()
    { 
        OnManagerInitialized?.Invoke(); // �����������C�x���g�𔭉�

    }


    //��������_��(�e���ŁAGameManager�̏��������e�X�I��������ǂ������m�F����)
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
            StopCoroutine(countdownCoroutine); // �����̃J�E���g�_�E�����~
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
                RPC_InvokeCountDown(countdown); // �S�N���C�A���g�ɃJ�E���g�_�E���J�n��ʒm
            }
        }
        // �J�E���g�_�E���I����ɃQ�[�����J�n
        TimerStart();

        //PlayerAvatar�̍s���s�\������
        foreach (var player in Runner.ActivePlayers)
        {
            var playerObject = Runner.GetPlayerObject(player);
            if (playerObject != null)
            {
                var avatar = playerObject.GetComponent<PlayerAvatar>();
                if (avatar != null)
                {
                    //RPC�ōs���s�\������
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
        // �^�C�}�[�X�V����
        if (!Object.HasStateAuthority) return;
        if (!IsTimerRunning) return;
        //���݂̎��Ԃ���X�^�[�g���Ԃ����������Ԃ��o�ߎ���
        double elapsed = Runner.SimulationTime - startSimTime;
        //�o�ߎ��Ԃ�b�ɕϊ�
        int elapsedSeconds = Mathf.FloorToInt((float)elapsed);

        //�c�莞�Ԃ��v�Z
        int newRemainingSeconds = Mathf.Max(initialTimeSec - elapsedSeconds);

        // �c�莞�Ԃ��ς�����ꍇ�̂ݍX�V
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

    //���ԍX�V
    public override void FixedUpdateNetwork()
    {
        if (isUserDataArrayDirty && Runner.IsServer && CheckUserDataIsAbleToShare(UserDataArray))
        {
            {
                int elementSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(UserData));
                int totalSize = UserDataArray.Length * elementSize;
                Debug.Log($"ShareUserData. element size {elementSize} bytes. total size {totalSize} bytes. ");
                ShareUserDataArray(UserDataArray);
                isUserDataArrayDirty = false; // ���������������̂Ńt���O�����Z�b�g
            }


        }

        //�X�R�A�X�V�^�C�~���O
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

        UpdateTimer(); // �^�C�}�[�̍X�V�������Ăяo��


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

    private void Update()
    {
         debugInput = LocalInputHandler.CollectDebugInput();
        if (debugInput.BattleEndPressedDown)
        {
            OnTimeUp?.Invoke(); // �f�o�b�O�p�Ɏ����I���C�x���g�𔭉�
        }
    }
   
}


