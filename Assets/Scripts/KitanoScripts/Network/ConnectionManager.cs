using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;


public class ConnectionManager : MonoBehaviour, INetworkRunnerCallbacks
{
    // Start is called before the first frame update  

    public static ConnectionManager Instance;

    [SerializeField]string localUserId = "";


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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

        if (GameManager.Instance != null)
        {

           // InquireGameManager(GameManager.Instance);


        }
    }


    

    ////GameManagerに問い合わせる
    //void InquireGameManager(GameManager  gameManager)
    //{
      
    //        Debug.Log("GameManager found: " + gameManager.name);
    //    // ここでGameManagerにアクセスして必要な処理を行う

    //    // var score = gameManager.RegisterUserId(localUserId); // ユーザーIDを登録し、スコアをもらう
    //    // HUDMAnager.Instance.UpdateScore(score); // HUDManagerにスコアを更新する

    //}

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player} joined.");


        if (player==runner.LocalPlayer && localUserId == "") { localUserId = runner.UserId; }
       
        

    }
    




    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player} left.");
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        Debug.Log("Input received.");
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
        Debug.Log("Disconnected from server.");
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
