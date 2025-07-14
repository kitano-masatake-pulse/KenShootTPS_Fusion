using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Collections.Unicode;


public class ConnectionManager : MonoBehaviour, INetworkRunnerCallbacks
{
    // Start is called before the first frame update  

    public static ConnectionManager Instance;

    Guid localUserGuid ;
    [SerializeField] string guidValue;
    private bool _isFirstTime=true;
    NetworkRunner networkRunner;
    private StartGameArgs startGameArgs;
    private Dictionary<string, SessionProperty> customProps;
    private int? maxPlayerNum=3;
    private float  reconnectTimeout=30f;
    [SerializeField]private NetworkRunner networkRunnerPrefab;


    public static Action<NetworkRunner> OnNetworkRunnerGenerated;// Runnerが生成されたときのイベント、StartGame前
    public static Action<NetworkRunner> OnSessionConnected;// Runnerが接続されたときのイベント、StartGame後



    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject); // シーンをまたいで保持
        }
        else
        {
            Destroy(gameObject); // 複数生成されないように
        }

    }


    void OnEnable()
    {

        OnNetworkRunnerGenerated -= AddCallbackMe;
        OnNetworkRunnerGenerated += AddCallbackMe;
    }

    void OnDisable()
    {
        OnNetworkRunnerGenerated -= AddCallbackMe;
        networkRunner.RemoveCallbacks(this);
    }

    void AddCallbackMe(NetworkRunner runner)
    {
        // NetworkRunnerのコールバック対象に、このスクリプト（GameLauncher）を登録する
        if (runner != null)
        {
            runner.AddCallbacks(this);
            networkRunner = runner; // 現在のRunnerを保持

        }
    }



    private async void Start()
    {
        Debug.Log("GameLauncher: Start Called");
        //ランナーが場になければ
        // NetworkRunnerを生成する
        networkRunner = FindObjectOfType<NetworkRunner>();
        if (networkRunner != null)
        {
            _isFirstTime = false;
            Debug.Log("GameLauncher: Found existing NetworkRunner in the scene.");
            OnNetworkRunnerGenerated?.Invoke(networkRunner);


        }
        else if (networkRunner == null)
        {
            _isFirstTime = true;
            //シーンにNetworkRunnerが存在しない場合は、Prefabから生成
            networkRunner = Instantiate(networkRunnerPrefab);
            OnNetworkRunnerGenerated?.Invoke(networkRunner);

            StartSession(); // セッションを開始する


            OnSessionConnected?.Invoke(networkRunner);


        }
    }

    async void StartSession()
    {
        var customProps = new Dictionary<string, SessionProperty>();

        if (GameRuleSettings.Instance != null)
        {
            customProps["GameRule"] = (int)GameRuleSettings.Instance.selectedRule;
        }
        else
        {
            customProps["GameRule"] = (int)GameRule.DeathMatch;
        }

        startGameArgs =
        new StartGameArgs
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionProperties = customProps,
            PlayerCount = maxPlayerNum,
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneManager = networkRunner.GetComponent<NetworkSceneManagerDefault>()
        };

        // StartGameArgsに渡した設定で、セッションに参加する
        var result = await networkRunner.StartGame(startGameArgs);

        Debug.Log($"GameLauncher.PreStartGame called. {Time.time} {networkRunner.Tick},{networkRunner.SimulationTime} ");


        if (result.Ok)
        {
            Debug.Log("成功！");
        }
        else
        {
            Debug.Log("失敗！");
        }
    }


    // Update is called once per frame  
    void Update()
    {
    }


    // INetworkRunnerCallbacksの実装

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("Scene load done.");

        if (GameManager2.Instance != null)
        {

           // InquireGameManager(GameManager2.Instance);


        }
    }
    //退出処理
    public async void LeaveRoom()
    {
        // すべてのコールバックを削除
        networkRunner.RemoveCallbacks(this);
        // Runnerを停止
        await networkRunner.Shutdown();
        // シーンをタイトルシーンに戻す
        SceneManager.LoadScene("TitleScene");
    }


    public Guid GetLocalID()
    { 
    
        return localUserGuid;
    }

    //NetworkRunner.UserIdを16bytesで表す
    Guid SerializeUserIDToGUID(string userId)
    {

        

        return Guid.ParseExact(userId, "D");
    
    }



   

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player} joined.");


        if (player==runner.LocalPlayer && localUserGuid == Guid.Empty) 
        { 
            localUserGuid = SerializeUserIDToGUID(runner.UserId); 
            guidValue = localUserGuid.ToString("N"); // GUIDを文字列に変換して保持
        }

        if(GameManager2.Instance != null)
        {
            if (player == runner.LocalPlayer)
            {
                // GameManager2のメソッドを呼び出してユーザーIDを登録
                GameManager2.Instance.RPC_SubmitIdToHost(localUserGuid);

            }
            
        }
        else
        {
            Debug.Log("GameManager2 instance is not available.");
        }



    }
    




    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player} left.");
        if (runner.IsServer)
        { 
            if (GameManager2.Instance != null)
            {
                // GameManager2のメソッドを呼び出してユーザーIDを削除
                GameManager2.Instance.UpdateConnectionState(player,ConnectionState.Disconnected);
            }
            else
            {
                Debug.Log("GameManager2 instance is not available.");
            }

        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        //Debug.Log("Input received.");
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        Debug.Log($"Input missing for player {player}.");
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"Runner shutdown due to {shutdownReason}.");
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("Connected to server.");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        Debug.Log($"GameLauncher runner Disconnected . OnDisconnectedFromServer called. Runner: {runner}");
        StartCoroutine(TryReconnectCoroutine()); // 再接続を試みる
    }
    IEnumerator TryReconnectCoroutine()
    {

        float startTime = Time.time;

        //いったんシャットダウンしておく
        //networkRunner.Shutdown();


        while (Time.time - startTime < reconnectTimeout)
        {
            // 少し待ってから再接続
            yield return new WaitForSeconds(1f);

            TryReconnect();


            // 再接続が成功したか確認

            yield return new WaitUntil(() => networkRunner.IsRunning);
            Debug.Log("Reconnected to the Cloud!");





            if (networkRunner.IsRunning)
            {
                Debug.Log("[Reconnect] Successfully reconnected!");
                yield break;  // 成功したらリトライループを抜ける
            }
            else
            {
                Debug.Log("[Reconnect] Retry failed, trying again…");
            }
        }

        // タイムアウト到達
        Debug.LogWarning($"[Reconnect] Failed to reconnect within {reconnectTimeout}s.");
        //ShowTimeoutDialog();
    }

    // 再接続時の処理例
    async void TryReconnect()
    {
        // 1. いったんセッションを終了（Shutdown が完了するまで待つ）
        if (networkRunner != null)
        {
            await networkRunner.Shutdown(destroyGameObject: true);
            //Destroy(networkRunner.gameObject); // Runnerを破棄


        }

        networkRunner = Instantiate(networkRunnerPrefab);

        OnNetworkRunnerGenerated?.Invoke(networkRunner); // 新しいRunnerを生成したことを通知


        StartSession(); // セッションを再開する

        OnSessionConnected?.Invoke(networkRunner); // 新しいRunnerが接続されたことを通知
    }


    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        Debug.Log("Connect request received.");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log($"Connection failed to {remoteAddress} due to {reason}.");
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        Debug.Log("User simulation message received.");
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log("Session list updated.");
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        Debug.Log("Custom authentication response received.");
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        Debug.Log("Host migration occurred.");
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
        Debug.Log($"Reliable data received from player {player}.");
    }

   

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        Debug.Log("Scene load started.");
    }

   
}
