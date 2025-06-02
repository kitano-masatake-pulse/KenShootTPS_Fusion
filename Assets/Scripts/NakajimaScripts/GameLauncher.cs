using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

public class GameLauncher : MonoBehaviour, INetworkRunnerCallbacks
{
    //�V���O���g���̐錾
    public static GameLauncher Instance { get; private set; }


    [SerializeField]
    private NetworkRunner networkRunnerPrefab;
    [SerializeField]
    private NetworkPrefabRef playerAvatarPrefab;
    [SerializeField]
    private LobbyUIController lobbyUI;

    [SerializeField]
    private NetworkInputManager networkInputManager;

    [SerializeField]
    private int maxPlayerNum=3;

    private NetworkRunner networkRunner;

    public TextMeshProUGUI sessionNameText;



    [Header("���ɑJ�ڂ���V�[��(�f�t�H���gBattleScene)")]
    public  SceneType nextScene= SceneType.Battle;



    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject); // ��d�����h�~
            return;
        }
        Instance = this;
    }


    private async void Start()
    {
        // NetworkRunner�𐶐�����
        networkRunner = Instantiate(networkRunnerPrefab);
        // NetworkRunner�̃R�[���o�b�N�ΏۂɁA���̃X�N���v�g�iGameLauncher�j��o�^����
        networkRunner.AddCallbacks(this);

        
        networkRunner.AddCallbacks(networkInputManager);


        var customProps = new Dictionary<string, SessionProperty>();

        if (GameRuleSettings.Instance != null)
        {
            customProps["GameRule"] = (int)GameRuleSettings.Instance.selectedRule;
        }
        else
        {
            customProps["GameRule"] = (int)GameRule.DeathMatch;
        }


        // StartGameArgs�ɓn�����ݒ�ŁA�Z�b�V�����ɎQ������
        var result = await networkRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionProperties = customProps,
            PlayerCount = maxPlayerNum,
            SceneManager = networkRunner.GetComponent<NetworkSceneManagerDefault>()
        });



        

        if (result.Ok)
        {
            Debug.Log("�����I");
        }
        else
        {
            Debug.Log("���s�I");
        }
}

   



    // INetworkRunnerCallbacks�C���^�[�t�F�[�X�̋����
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {

        if (runner.SessionInfo != null && runner.SessionInfo.Properties != null)
        {
            if (runner.SessionInfo.Properties.TryGetValue("GameRule", out var gameRuleProp))
            {
                int gameRuleValue = (int)gameRuleProp;
                string sessionName = runner.SessionInfo.Name;
                Debug.Log($"GameRule from SessionProperties: {gameRuleValue}");
                Debug.Log($"RoomName from SessionProperties: {sessionName}");

                if (sessionNameText != null)
                {
                    sessionNameText.SetText( $"RoomID: {sessionName}");
                }
            }
            else
            {
                Debug.Log("GameRule not found in SessionProperties.");
            }
        }





        // �z�X�g�i�T�[�o�[���N���C�A���g�j���ǂ�����IsServer�Ŕ���ł���
        if (!runner.IsServer) { return; }
        // �����_���Ȑ����ʒu�i���a5�̉~�̓����j���擾����
        var randomValue = UnityEngine.Random.insideUnitCircle * 5f;
        var spawnPosition = new Vector3(randomValue.x, 5f, randomValue.y);
        // �Q�������v���C���[�̃A�o�^�[�𐶐�����
        var avatar = runner.Spawn(playerAvatarPrefab, spawnPosition, Quaternion.identity, player);
        // �v���C���[�iPlayerRef�j�ƃA�o�^�[�iNetworkObject�j���֘A�t����
        runner.SetPlayerObject(player, avatar);

        //�z�X�g�̂݃o�g���X�^�[�g�{�^����\��
        if (runner.IsServer && player == runner.LocalPlayer)
        {
            Debug.Log("�z�X�g���Q�� �� �{�^���\���w��");
            lobbyUI.ShowStartButton(runner);
        }

    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
        if (!runner.IsServer) { return; }
        // �ޏo�����v���C���[�̃A�o�^�[��j������
        if (runner.TryGetPlayerObject(player, out var avatar))
        {
            runner.Despawn(avatar);
        }
    }

    //public void OnInput(NetworkRunner runner, NetworkInput input) {
    //    var data = new NetworkInputData();

    //    data.Direction = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

    //    input.Set(data);
    //}

    public void OnInput(NetworkRunner runner, NetworkInput input) { }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }


}
