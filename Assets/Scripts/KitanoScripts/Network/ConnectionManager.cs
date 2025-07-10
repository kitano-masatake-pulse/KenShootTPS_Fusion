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
            networkRunner = runner; // ���݂�Runner��ێ�

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

           // InquireGameManager(GameManager2.Instance);


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
            if (player == runner.LocalPlayer)
            {
                // GameManager2�̃��\�b�h���Ăяo���ă��[�U�[ID��o�^
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
                // GameManager2�̃��\�b�h���Ăяo���ă��[�U�[ID���폜
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
        StartCoroutine(TryReconnectCoroutine()); // �Đڑ������݂�
    }
    IEnumerator TryReconnectCoroutine()
    {

        float startTime = Time.time;

        //��������V���b�g�_�E�����Ă���
        //networkRunner.Shutdown();


        while (Time.time - startTime < reconnectTimeout)
        {
            // �����҂��Ă���Đڑ�
            yield return new WaitForSeconds(1f);

            TryReconnect();


            // �Đڑ��������������m�F

            yield return new WaitUntil(() => networkRunner.IsRunning);
            Debug.Log("Reconnected to the Cloud!");





            if (networkRunner.IsRunning)
            {
                Debug.Log("[Reconnect] Successfully reconnected!");
                yield break;  // ���������烊�g���C���[�v�𔲂���
            }
            else
            {
                Debug.Log("[Reconnect] Retry failed, trying again�c");
            }
        }

        // �^�C���A�E�g���B
        Debug.LogWarning($"[Reconnect] Failed to reconnect within {reconnectTimeout}s.");
        //ShowTimeoutDialog();
    }

    // �Đڑ����̏�����
    async void TryReconnect()
    {
        // 1. ��������Z�b�V�������I���iShutdown ����������܂ő҂j
        if (networkRunner != null)
        {
            await networkRunner.Shutdown(destroyGameObject: true);
            //Destroy(networkRunner.gameObject); // Runner��j��


        }

        networkRunner = Instantiate(networkRunnerPrefab);


        // 2. �K�v�Ȃ�R�[���o�b�N�ēo�^
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

        // 3. �O��Ɠ��� Args �ōċN��
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
