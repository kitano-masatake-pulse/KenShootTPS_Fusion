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

    NetworkRunner networkRunner;
    private StartGameArgs startGameArgs;
    private Dictionary<string, SessionProperty> customProps;
    private int? maxPlayerNum=3;
    private float  reconnectTimeout=30f;
    [SerializeField]private NetworkRunner networkRunnerPrefab;

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

        GameLauncher.OnNetworkRunnerGenerated -= AddCallbackMe;
        GameLauncher.OnNetworkRunnerGenerated += AddCallbackMe;
    }

    void OnDisable()
    {
        GameLauncher.OnNetworkRunnerGenerated -= AddCallbackMe;
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



    void Start()
    {
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


        // 2. 必要ならコールバック再登録
        networkRunner.AddCallbacks(this);
       

        startGameArgs =
            new StartGameArgs
            {
                GameMode = GameMode.AutoHostOrClient,
                SessionProperties = customProps,
                PlayerCount = maxPlayerNum,
                Scene = SceneManager.GetActiveScene().buildIndex,
                SceneManager = networkRunner.GetComponent<NetworkSceneManagerDefault>()
            };

        // 3. 前回と同じ Args で再起動
        var result = await networkRunner.StartGame(startGameArgs);

        if (result.Ok)
            Debug.Log("Reconnected!");
        else
            Debug.LogError($"Reconnect failed: {result.ErrorMessage}");
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
