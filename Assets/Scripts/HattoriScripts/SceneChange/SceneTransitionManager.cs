using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Collections.Unicode;


public class SceneTransitionManager : MonoBehaviour,INetworkRunnerCallbacks
{
    //�V���O���g��
    public static SceneTransitionManager Instance { get; private set; }
    [SerializeField] private SceneChangeFade sceneTrasitionFade;
    [SerializeField] private float fadeInDuration = 2f; 
    [SerializeField] private float fadeOutDuration = 1f; 
    [SerializeField] private float timeoutSeconds = 10f; 
    [SerializeField] private NetworkPrefabRef sceneChangeAnchorPrefab; // �V�[���ύX�A���J�[�̃v���n�u

    private SceneTransitionAnchor anchor;
    private NetworkRunner runner;
    private Coroutine sceneCoroutine;
    private Coroutine timeoutCoroutine;
    private event Action<PlayerRef> OnPlayerLeftRoom;

    private HashSet<PlayerRef> transitionProcessFinishedPlayers = new HashSet<PlayerRef>();

    private bool isTransitioning = false; // �V�[���J�ڒ����ǂ����̃t���O
    private bool hasTransitionExecuted = false; // �V�[���J�ڂ����s���ꂽ���ǂ����̃t���O

    //======================
    //����������
    //======================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject); 
    }
    public void OnEnable()
    {
        SceneManager.sceneLoaded -= StartScene; 
        SceneManager.sceneLoaded += StartScene; 
        ConnectionManager.OnNetworkRunnerGenerated -= SubscribeRunner;
        ConnectionManager.OnNetworkRunnerGenerated += SubscribeRunner;
    }
    private void OnDisable()
    {
        StopAllCoroutines(); // �S�ẴR���[�`�����~
        SceneManager.sceneLoaded -= StartScene; // �V�[�����[�h���̃C�x���g������
        ConnectionManager.OnNetworkRunnerGenerated -= SubscribeRunner;
    }

    private void SubscribeRunner(NetworkRunner newRunner)
    {
        runner = newRunner;
        runner.AddCallbacks(this);
    }


    //======================
    //�V�[���ύX����
    //======================


    //�V�[���J�n���̏���
    private void StartScene(Scene scene, LoadSceneMode mode)
    {
        //�V�[�����[�h��Ƀt�F�[�h�C�����J�n
        if (sceneCoroutine != null) StopCoroutine(sceneCoroutine);
        sceneCoroutine = StartCoroutine(sceneTrasitionFade.FadeRoutine(1f, 0f, fadeInDuration));
    }


    //�V�[���ύX���s�����\�b�h
    public void ChangeScene(SceneType nextScene,bool leaveRoom = false)
    {
        if (runner != null && runner.IsRunning)
        {
            Debug.Log("SceneChange: Online");
            ChangeSceneOnline(nextScene, leaveRoom);
        }
        else
        {
            Debug.Log("SceneChange: Offline");
            ChangeSceneOffline(nextScene);
        }
    }

    public void ChangeSceneOffline(SceneType nextScene)
    {
        sceneCoroutine = StartCoroutine(TransitionProcessCoroutine(nextScene));
        Debug.Log($"�V�[����{nextScene.ToSceneName()}�ɕύX���܂����B");
    }
    
    public void ChangeSceneOnline(SceneType nextScene, bool leaveRoom = false)
    {
        if (runner == null || !runner.IsRunning) return;

        //�A���J�[��������ΐ���
        if (anchor == null&&runner.IsServer)
        {
            anchor = runner.Spawn(sceneChangeAnchorPrefab).GetComponent<SceneTransitionAnchor>();
            DontDestroyOnLoad(anchor.gameObject); // �V�[���ύX�A���J�[��ێ�
            Debug.Log("SceneChange: AnchorSpawned");
        }

        //���������ꍇ�̏���
        if (leaveRoom)
        {
            //�����i�[���V���b�g�_�E����I�t���C���ŃV�[���ύX

            runner.Shutdown(true, ShutdownReason.Ok);
            ChangeSceneOffline(nextScene);
            return;
        }

        ChangeSceneFromHost(nextScene);
    }


    private void ChangeSceneFromHost(SceneType nextScene)
    {
        if(!runner.IsServer) return;

        hasTransitionExecuted = false; // �J�ڂ����s���ꂽ�t���O�����Z�b�g
        isTransitioning = true; // �V�[���J�ڒ��t���O�𗧂Ă�
        timeoutCoroutine = StartCoroutine(TimeoutCoroutine(timeoutSeconds, nextScene));// �^�C���A�E�g�������J�n

        //�A���J�[��ʂ��ăV�[���ύX��ʒm
        anchor.RPC_RequestSceneTransition(nextScene);
    }

    public void RaisePlayerLeftRoom(PlayerRef leftPlayer)
    {
        OnPlayerLeftRoom?.Invoke(leftPlayer); // �v���C���[�����[���𗣂ꂽ���Ƃ�ʒm
    }

    public void OnTransitionProcessFinished(SceneType nextScene, PlayerRef player)
    {
        Debug.Log($"SceneChange : �V�[���J�ڏ������������܂����B�\�[�X: {player}");
        transitionProcessFinishedPlayers.Add(player); 
        //�S�ẴA�N�e�B�u�v���C���[��sceneChangeCompletePlayers�Ɋ܂܂�Ă��邩�m�F
        if (runner != null && runner.ActivePlayers != null)
        {
            foreach (var activePlayer in runner.ActivePlayers)
            {
                if (!transitionProcessFinishedPlayers.Contains(activePlayer))
                {
                    return; // �܂��S�����������Ă��Ȃ��̂ŏI��
                }
            }
        }
        //�S�Ẵv���C���[���V�[���ύX�����������ꍇ�̏���
        Debug.Log("SceneChange:�S���̃V�[���J�ڎ��������m�F�A�J�ڎ��s");
        ExecuteTransition(nextScene); 
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
        Debug.Log($"SceneChange:Player{player} �̑ޏo���m�F");
        //�V�[���J�ڒ��Ƀv���C���[���ޏo�����ꍇ�A�V�[���J�ڏ��������Ƃ��Ĉ���
        if (isTransitioning)
        { 
            OnTransitionProcessFinished(SceneType.Lobby, player); 
        }
    }

    //�I�����C���ŃV�[���J�ڎ��̂����s���郁�\�b�h
    private void ExecuteTransition(SceneType nextScene)
    {
        if (hasTransitionExecuted) return; // ���ɑJ�ڂ����s����Ă���ꍇ�͉������Ȃ�
        hasTransitionExecuted = true;
        if (timeoutCoroutine != null)
            StopCoroutine(timeoutCoroutine);
        isTransitioning = false;
        transitionProcessFinishedPlayers.Clear();
        //�V�[���J�ڂ̎��s
        runner.SetActiveScene(nextScene.ToSceneName()); 
    }

    //====================
    //�R���[�`����
    //====================
    public IEnumerator TransitionProcessCoroutine(SceneType nextScene, bool isOnline = false)
    {
        yield return StartCoroutine(sceneTrasitionFade.FadeRoutine(0f, 1f, fadeOutDuration)); 
        yield return StartCoroutine(GCCoroutine());
        //�I�t���C���Ȃ炻�̂܂܃V�[���J��
        if (!isOnline)
        {
            Debug.Log($"SceneChange:Change {nextScene} in Coroutine");
            SceneManager.LoadScene(nextScene.ToSceneName());
        }
    }

    //�����������s���񓯊�����
    private IEnumerator GCCoroutine()
    {     
        yield return Resources.UnloadUnusedAssets();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        yield return null; // �t���[���҂�

    }

    private IEnumerator TimeoutCoroutine(float timeoutDuration, SceneType nextScene)
    {
        yield return new WaitForSeconds(timeoutDuration);
        // �^�C���A�E�g�����������ꍇ�A�V�[���J�ڂ������I�Ɏ��s
        ExecuteTransition(nextScene);
    }

    //====================
    //INetworkRunnerCallbacks�̎���
    //====================
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {
        runner.RemoveCallbacks(this);
        this.runner = null;
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player){ }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
}
