using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiPeerLauncher : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField]
    private NetworkRunner networkRunnerPrefab;

    [SerializeField]
    private NetworkPrefabRef playerAvatarPrefab;

    [SerializeField]
    private NetworkPrefabRef dummyAvatarPrefab;

    [SerializeField]
    private LobbyUIController lobbyUI;

    public TextMeshProUGUI sessionNameText;



    private NetworkRunner networkRunner;

    private NetworkRunner dummyPlayerNetworkRunner;



    private async void Start()
    {

        //string sceneName = SceneType.KitanoBattleTest.ToSceneName();

        //// �񓯊���Additive�ɃV�[�������[�h����
        //AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        //// ��������܂ő҂�
        //while (!loadOperation.isDone)
        //{
        //    await System.Threading.Tasks.Task.Yield(); // �t���[�����܂����ő҂�
        //}

        //// ���[�h��ɃV�[���擾
        //Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        //if (loadedScene.IsValid() && loadedScene.isLoaded)
        //{
        //    SceneManager.SetActiveScene(loadedScene);
        //}
        //else
        //{
        //    Debug.LogError("SetActiveScene �ł��܂���F�V�[�����܂��ǂݍ��܂�Ă��܂���B");
        //    return;
        //}

        for (int i = 0; i < 3; i++)
        {
            // �eNetworkRunner�̐ڑ��������s��
            var runner = Instantiate(networkRunnerPrefab);
            // NetworkRunner�̃R�[���o�b�N�ΏۂɁA���̃X�N���v�g�iGameLauncher�j��o�^����
            runner.AddCallbacks(this);
            // Unity�Ŏ��O�Ƀ��[�h���Ȃ��BStartGame��Scene�� or buildIndex��n��������OK�B
            var result=await runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.AutoHostOrClient,
                Scene = SceneType.KitanoBattleTest.ToSceneBuildIndex(), // �� ���O�� buildIndex ���w��
                SceneManager = runner.GetComponent<NetworkSceneManagerDefault>()
            });

            // �eNetworkRunner�̃Q�[���I�u�W�F�N�g�̖��O��ύX���Ď��ʂ��₷������
            if (result.Ok)
            {
                runner.name = (runner.IsServer) ? "HostRunner" : $"Client {runner.LocalPlayer.PlayerId}Runner";
            }
        }

    }


    public void CreateDummyClients(int DummyCount)
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

        for (int i = 0; i < DummyCount; i++)
        {
            // �_�~�[�̃v���C���[�𐶐����邽�߂�NetworkRunner�𐶐�
            dummyPlayerNetworkRunner = Instantiate(networkRunnerPrefab);
            // �_�~�[�̃v���C���[�p�ɃR�[���o�b�N��o�^
            //dummyPlayerNetworkRunner.AddCallbacks(this);
            // �_�~�[�̃v���C���[���Q��������
            var result = dummyPlayerNetworkRunner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Client,
                SessionProperties = customProps,
                //PlayerCount = 1, // �_�~�[��1�l����
                SceneManager = networkRunner.GetComponent<NetworkSceneManagerDefault>()
            });


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
                    sessionNameText.SetText($"RoomID: {sessionName}");
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

        if (runner != null)
        {
            // �Q�������v���C���[�̃A�o�^�[�𐶐�����
            var avatar = runner.Spawn(playerAvatarPrefab, spawnPosition, Quaternion.identity, player);
            // �v���C���[�iPlayerRef�j�ƃA�o�^�[�iNetworkObject�j���֘A�t����
            runner.SetPlayerObject(player, avatar);

        }
        else
        {
            // �Q�������v���C���[�̃A�o�^�[�𐶐�����
            var avatar = runner.Spawn(dummyAvatarPrefab, spawnPosition, Quaternion.identity, player);
            // �v���C���[�iPlayerRef�j�ƃA�o�^�[�iNetworkObject�j���֘A�t����
            runner.SetPlayerObject(player, avatar);

        }


        //�z�X�g�̂݃o�g���X�^�[�g�{�^����\��
        if (runner.IsServer && player == runner.LocalPlayer)
        {
            Debug.Log("�z�X�g���Q�� �� �{�^���\���w��");
            //lobbyUI.ShowStartButton(runner);
        }



        // �_�~�[�̃v���C���[�𐶐�����
        //CreateDummyClients(1);


    }




    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
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