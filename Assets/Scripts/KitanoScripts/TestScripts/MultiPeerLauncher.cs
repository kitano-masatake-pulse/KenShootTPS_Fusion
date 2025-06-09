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

        //// 非同期でAdditiveにシーンをロードする
        //AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        //// 完了するまで待つ
        //while (!loadOperation.isDone)
        //{
        //    await System.Threading.Tasks.Task.Yield(); // フレームをまたいで待つ
        //}

        //// ロード後にシーン取得
        //Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        //if (loadedScene.IsValid() && loadedScene.isLoaded)
        //{
        //    SceneManager.SetActiveScene(loadedScene);
        //}
        //else
        //{
        //    Debug.LogError("SetActiveScene できません：シーンがまだ読み込まれていません。");
        //    return;
        //}

        for (int i = 0; i < 3; i++)
        {
            // 各NetworkRunnerの接続処理を行う
            var runner = Instantiate(networkRunnerPrefab);
            // NetworkRunnerのコールバック対象に、このスクリプト（GameLauncher）を登録する
            runner.AddCallbacks(this);
            // Unityで事前にロードしない。StartGameでScene名 or buildIndexを渡すだけでOK。
            var result=await runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.AutoHostOrClient,
                Scene = SceneType.KitanoBattleTest.ToSceneBuildIndex(), // ← 名前か buildIndex を指定
                SceneManager = runner.GetComponent<NetworkSceneManagerDefault>()
            });

            // 各NetworkRunnerのゲームオブジェクトの名前を変更して識別しやすくする
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
            // ダミーのプレイヤーを生成するためのNetworkRunnerを生成
            dummyPlayerNetworkRunner = Instantiate(networkRunnerPrefab);
            // ダミーのプレイヤー用にコールバックを登録
            //dummyPlayerNetworkRunner.AddCallbacks(this);
            // ダミーのプレイヤーを参加させる
            var result = dummyPlayerNetworkRunner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Client,
                SessionProperties = customProps,
                //PlayerCount = 1, // ダミーは1人だけ
                SceneManager = networkRunner.GetComponent<NetworkSceneManagerDefault>()
            });


        }

    }


    // INetworkRunnerCallbacksインターフェースの空実装
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





        // ホスト（サーバー兼クライアント）かどうかはIsServerで判定できる
        if (!runner.IsServer) { return; }
        // ランダムな生成位置（半径5の円の内部）を取得する
        var randomValue = UnityEngine.Random.insideUnitCircle * 5f;
        var spawnPosition = new Vector3(randomValue.x, 5f, randomValue.y);

        if (runner != null)
        {
            // 参加したプレイヤーのアバターを生成する
            var avatar = runner.Spawn(playerAvatarPrefab, spawnPosition, Quaternion.identity, player);
            // プレイヤー（PlayerRef）とアバター（NetworkObject）を関連付ける
            runner.SetPlayerObject(player, avatar);

        }
        else
        {
            // 参加したプレイヤーのアバターを生成する
            var avatar = runner.Spawn(dummyAvatarPrefab, spawnPosition, Quaternion.identity, player);
            // プレイヤー（PlayerRef）とアバター（NetworkObject）を関連付ける
            runner.SetPlayerObject(player, avatar);

        }


        //ホストのみバトルスタートボタンを表示
        if (runner.IsServer && player == runner.LocalPlayer)
        {
            Debug.Log("ホストが参加 → ボタン表示指示");
            //lobbyUI.ShowStartButton(runner);
        }



        // ダミーのプレイヤーを生成する
        //CreateDummyClients(1);


    }




    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) { return; }
        // 退出したプレイヤーのアバターを破棄する
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