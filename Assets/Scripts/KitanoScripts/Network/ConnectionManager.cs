using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using static Unity.Collections.Unicode;


public class ConnectionManager : MonoBehaviour, INetworkRunnerCallbacks
{
    // Start is called before the first frame update  

    public static ConnectionManager Instance;

    Guid localUserGuid ;
    [SerializeField] string guidValue;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject); // �V�[�����܂����ŕێ�
        }
        else
        {
            Destroy(gameObject); // ������������Ȃ��悤��
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
        // NetworkRunner�̃R�[���o�b�N�ΏۂɁA���̃X�N���v�g�iGameLauncher�j��o�^����
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


    // INetworkRunnerCallbacks�̎���

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("Scene load done.");

        if (GameManager2.Instance != null)
        {

           // InquireGameManager(GameManager.Instance);


        }
    }


    public Guid GetLocalID()
    { 
    
        return localUserGuid;
    }

    //NetworkRunner.UserId��16bytes�ŕ\��
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
            guidValue = localUserGuid.ToString("N"); // GUID�𕶎���ɕϊ����ĕێ�
        }

        if(GameManager2.Instance != null)
        {
            // GameManager2�̃��\�b�h���Ăяo���ă��[�U�[ID��o�^
            GameManager2.Instance.RegisterUserID(localUserGuid, player);
        }
        else
        {
            Debug.LogWarning("GameManager2 instance is not available.");
        }



    }
    




    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player} left.");
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
