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
    //シングルトン
    public static SceneTransitionManager Instance { get; private set; }
    [SerializeField] private SceneChangeFade sceneTrasitionFade;
    [SerializeField] private float fadeInDuration = 2f; 
    [SerializeField] private float fadeOutDuration = 1f; 
    [SerializeField] private float timeoutSeconds = 10f; 
    [SerializeField] private NetworkPrefabRef sceneChangeAnchorPrefab; // シーン変更アンカーのプレハブ

    private SceneTransitionAnchor anchor;
    private NetworkRunner runner;
    private Coroutine sceneCoroutine;
    private Coroutine timeoutCoroutine;
    private event Action<PlayerRef> OnPlayerLeftRoom;

    private HashSet<PlayerRef> transitionProcessFinishedPlayers = new HashSet<PlayerRef>();

    private bool isTransitioning = false; // シーン遷移中かどうかのフラグ
    private bool hasTransitionExecuted = false; // シーン遷移が実行されたかどうかのフラグ

    //======================
    //初期化処理
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
        StopAllCoroutines(); // 全てのコルーチンを停止
        SceneManager.sceneLoaded -= StartScene; // シーンロード時のイベントを解除
        ConnectionManager.OnNetworkRunnerGenerated -= SubscribeRunner;
    }

    private void SubscribeRunner(NetworkRunner newRunner)
    {
        runner = newRunner;
        runner.AddCallbacks(this);
    }


    //======================
    //シーン変更処理
    //======================


    //シーン開始時の処理
    private void StartScene(Scene scene, LoadSceneMode mode)
    {
        //シーンロード後にフェードインを開始
        if (sceneCoroutine != null) StopCoroutine(sceneCoroutine);
        sceneCoroutine = StartCoroutine(sceneTrasitionFade.FadeRoutine(1f, 0f, fadeInDuration));
    }


    //シーン変更を行うメソッド
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
        Debug.Log($"シーンを{nextScene.ToSceneName()}に変更しました。");
    }
    
    public void ChangeSceneOnline(SceneType nextScene, bool leaveRoom = false)
    {
        if (runner == null || !runner.IsRunning) return;

        //アンカーが無ければ生成
        if (anchor == null&&runner.IsServer)
        {
            anchor = runner.Spawn(sceneChangeAnchorPrefab).GetComponent<SceneTransitionAnchor>();
            DontDestroyOnLoad(anchor.gameObject); // シーン変更アンカーを保持
            Debug.Log("SceneChange: AnchorSpawned");
        }

        //部屋を閉じる場合の処理
        if (leaveRoom)
        {
            //ランナーをシャットダウン後オフラインでシーン変更

            runner.Shutdown(true, ShutdownReason.Ok);
            ChangeSceneOffline(nextScene);
            return;
        }

        ChangeSceneFromHost(nextScene);
    }


    private void ChangeSceneFromHost(SceneType nextScene)
    {
        if(!runner.IsServer) return;

        hasTransitionExecuted = false; // 遷移が実行されたフラグをリセット
        isTransitioning = true; // シーン遷移中フラグを立てる
        timeoutCoroutine = StartCoroutine(TimeoutCoroutine(timeoutSeconds, nextScene));// タイムアウト処理を開始

        //アンカーを通じてシーン変更を通知
        anchor.RPC_RequestSceneTransition(nextScene);
    }

    public void RaisePlayerLeftRoom(PlayerRef leftPlayer)
    {
        OnPlayerLeftRoom?.Invoke(leftPlayer); // プレイヤーがルームを離れたことを通知
    }

    public void OnTransitionProcessFinished(SceneType nextScene, PlayerRef player)
    {
        Debug.Log($"SceneChange : シーン遷移処理が完了しました。ソース: {player}");
        transitionProcessFinishedPlayers.Add(player); 
        //全てのアクティブプレイヤーがsceneChangeCompletePlayersに含まれているか確認
        if (runner != null && runner.ActivePlayers != null)
        {
            foreach (var activePlayer in runner.ActivePlayers)
            {
                if (!transitionProcessFinishedPlayers.Contains(activePlayer))
                {
                    return; // まだ全員が完了していないので終了
                }
            }
        }
        //全てのプレイヤーがシーン変更を完了した場合の処理
        Debug.Log("SceneChange:全環境のシーン遷移時処理を確認、遷移実行");
        ExecuteTransition(nextScene); 
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
        Debug.Log($"SceneChange:Player{player} の退出を確認");
        //シーン遷移宙にプレイヤーが退出した場合、シーン遷移処理完了として扱う
        if (isTransitioning)
        { 
            OnTransitionProcessFinished(SceneType.Lobby, player); 
        }
    }

    //オンラインでシーン遷移自体を実行するメソッド
    private void ExecuteTransition(SceneType nextScene)
    {
        if (hasTransitionExecuted) return; // 既に遷移が実行されている場合は何もしない
        hasTransitionExecuted = true;
        if (timeoutCoroutine != null)
            StopCoroutine(timeoutCoroutine);
        isTransitioning = false;
        transitionProcessFinishedPlayers.Clear();
        //シーン遷移の実行
        runner.SetActiveScene(nextScene.ToSceneName()); 
    }

    //====================
    //コルーチン類
    //====================
    public IEnumerator TransitionProcessCoroutine(SceneType nextScene, bool isOnline = false)
    {
        yield return StartCoroutine(sceneTrasitionFade.FadeRoutine(0f, 1f, fadeOutDuration)); 
        yield return StartCoroutine(GCCoroutine());
        //オフラインならそのままシーン遷移
        if (!isOnline)
        {
            Debug.Log($"SceneChange:Change {nextScene} in Coroutine");
            SceneManager.LoadScene(nextScene.ToSceneName());
        }
    }

    //内部処理を行う非同期処理
    private IEnumerator GCCoroutine()
    {     
        yield return Resources.UnloadUnusedAssets();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        yield return null; // フレーム待ち

    }

    private IEnumerator TimeoutCoroutine(float timeoutDuration, SceneType nextScene)
    {
        yield return new WaitForSeconds(timeoutDuration);
        // タイムアウトが発生した場合、シーン遷移を強制的に実行
        ExecuteTransition(nextScene);
    }

    //====================
    //INetworkRunnerCallbacksの実装
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
