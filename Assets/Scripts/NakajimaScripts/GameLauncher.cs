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
    //シングルトンの宣言
    public static GameLauncher Instance { get; private set; }


    [SerializeField]
    private NetworkRunner networkRunnerPrefab;
    [SerializeField]
    private NetworkPrefabRef playerAvatarPrefab;

    [SerializeField]
    private NetworkPrefabRef dummyAvatarPrefab;

    [SerializeField]
    private LobbyUIController lobbyUI;

    [SerializeField]
    private NetworkInputManager networkInputManager;

    [SerializeField]
    private int maxPlayerNum=3;

    private NetworkRunner networkRunner;

    public TextMeshProUGUI sessionNameText;

    [Header("デバッグ用：ダミーアバターの生成数(0で無効)")]
    public int dummyAvatarCount = 1;

    [Header("次に遷移するシーン(デフォルトBattleScene)")]
    public  SceneType nextScene= SceneType.Battle;



    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject); // 二重生成防止
            return;
        }
        Instance = this;
        Debug.Log("GameLauncher: Awake Called");
    }
    //タイトルシーンでデスマッチなのかチームデスマッチかを選択する
    //その情報を持って、セッションに参加する


    private async void Start()
    {
        Debug.Log("GameLauncher: Start Called");
        //ランナーが場になければ
        // NetworkRunnerを生成する
        networkRunner = FindObjectOfType<NetworkRunner>();
        if (networkRunner != null)
        {
            Debug.Log("GameLauncher: Found existing NetworkRunner in the scene.");
            networkRunner.AddCallbacks(this);
            
        }
        else if(networkRunner == null)
        {
            //シーンにNetworkRunnerが存在しない場合は、Prefabから生成
            networkRunner = Instantiate(networkRunnerPrefab);
            networkRunner.AddCallbacks(this);

            var customProps = new Dictionary<string, SessionProperty>();

            if (GameRuleSettings.Instance != null)
            {
                customProps["GameRule"] = (int)GameRuleSettings.Instance.selectedRule;
            }
            else
            {
                customProps["GameRule"] = (int)GameRule.DeathMatch;
            }


            // StartGameArgsに渡した設定で、セッションに参加する
            var result = await networkRunner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.AutoHostOrClient,
                SessionProperties = customProps,
                PlayerCount = maxPlayerNum,
                SceneManager = networkRunner.GetComponent<NetworkSceneManagerDefault>()
            });


            if (result.Ok)
            {
                Debug.Log("成功！");
            }
            else
            {
                Debug.Log("失敗！");
            }

        }

    }

    private void OnDisable()
    {
        networkRunner.RemoveCallbacks(this);
    }

    public void CreateDummyAvatars(int DummyCount)
    {
        for (int i = 0; i < DummyCount; i++)
        {

            // ランダムな生成位置（半径5の円の内部）を取得する
            var randomValue = UnityEngine.Random.insideUnitCircle * 5f;
            var spawnPosition = new Vector3(randomValue.x, 5f, randomValue.y);

            var avatar = networkRunner.Spawn(dummyAvatarPrefab, spawnPosition  , Quaternion.identity, PlayerRef.None);
            // PlayerRef.Noneを使用して、ダミーアバターはプレイヤーに関連付けない


        }

    }
    //PlayerRefを指定して、アバターを生成し、紐づけまでする
    private void CreatePlayerAvatar(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) { return; }
        var randomValue = UnityEngine.Random.insideUnitCircle * 5f;
        var spawnPosition = new Vector3(randomValue.x, 5f, randomValue.y);
        var avatar = runner.Spawn(playerAvatarPrefab, spawnPosition, Quaternion.identity, player);
        runner.SetPlayerObject(player, avatar);
    }



    // INetworkRunnerCallbacksインターフェースの空実装
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"GameLauncher:OnPlayerJoined.");

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


        CreatePlayerAvatar(runner, player);

        //ホストのみバトルスタートボタンを表示
        if (runner.IsServer && player == runner.LocalPlayer)
        {
            Debug.Log("ホストが参加 → ボタン表示指示");
            lobbyUI.ShowStartButton(runner);
        }


        if (runner.IsServer && player == runner.LocalPlayer)
        {
            CreateDummyAvatars(dummyAvatarCount);


        }

    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
        if (!runner.IsServer) { return; }
        // 退出したプレイヤーのアバターを破棄する
        if (runner.TryGetPlayerObject(player, out var avatar))
        {
            runner.Despawn(avatar);
        }
    }


    public void OnSceneLoadDone(NetworkRunner runner) 
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

        //以降の処理はホストのみ実行
        if (!runner.IsServer) return;

        foreach (var player in runner.ActivePlayers)
        {
            CreatePlayerAvatar(runner,player);
        }

        Debug.Log("ホストが参加 → ボタン表示指示");
        lobbyUI.ShowStartButton(runner);

    }

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
    public void OnSceneLoadStart(NetworkRunner runner) { }


}
