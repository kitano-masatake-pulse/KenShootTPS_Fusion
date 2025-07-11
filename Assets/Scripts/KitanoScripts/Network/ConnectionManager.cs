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


    public static Action<NetworkRunner> OnNetworkRunnerGenerated;// Runner���������ꂽ�Ƃ��̃C�x���g�AStartGame�O
    public static Action<NetworkRunner> OnSessionConnected;// Runner���ڑ����ꂽ�Ƃ��̃C�x���g�AStartGame��



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
        // NetworkRunner�̃R�[���o�b�N�ΏۂɁA���̃X�N���v�g�iGameLauncher�j��o�^����
        if (runner != null)
        {
            runner.AddCallbacks(this);
            networkRunner = runner; // ���݂�Runner��ێ�

        }
    }



    private async void Start()
    {
        Debug.Log("GameLauncher: Start Called");
        //�����i�[����ɂȂ����
        // NetworkRunner�𐶐�����
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
            //�V�[����NetworkRunner�����݂��Ȃ��ꍇ�́APrefab���琶��
            networkRunner = Instantiate(networkRunnerPrefab);
            OnNetworkRunnerGenerated?.Invoke(networkRunner);

            StartSession(); // �Z�b�V�������J�n����


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

        // StartGameArgs�ɓn�����ݒ�ŁA�Z�b�V�����ɎQ������
        var result = await networkRunner.StartGame(startGameArgs);

        Debug.Log($"GameLauncher.PreStartGame called. {Time.time} {networkRunner.Tick},{networkRunner.SimulationTime} ");


        if (result.Ok)
        {
            Debug.Log("�����I");
        }
        else
        {
            Debug.Log("���s�I");
        }
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
    //�ޏo����
    public async void LeaveRoom()
    {
        // ���ׂẴR�[���o�b�N���폜
        networkRunner.RemoveCallbacks(this);
        // Runner���~
        await networkRunner.Shutdown();
        // �V�[�����^�C�g���V�[���ɖ߂�
        SceneManager.LoadScene("TitleScene");
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

        OnNetworkRunnerGenerated?.Invoke(networkRunner); // �V����Runner�𐶐��������Ƃ�ʒm


        StartSession(); // �Z�b�V�������ĊJ����

        OnSessionConnected?.Invoke(networkRunner); // �V����Runner���ڑ����ꂽ���Ƃ�ʒm
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
