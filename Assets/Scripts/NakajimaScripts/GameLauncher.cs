using Cinemachine;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


//[DefaultExecutionOrder(-100)]
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

    [SerializeField]
    private NetworkRunner networkRunner;

    public TextMeshProUGUI sessionNameText;

    [Header("デバッグ用：ダミーアバターの生成数(0で無効)")]
    public int dummyAvatarCount = 1;



    [Header("次に遷移するシーン(デフォルトBattleScene)")]
    public  SceneType nextScene= SceneType.Battle;

    public static Action<NetworkRunner> OnNetworkRunnerGenerated;


    //デバッグ用
    [SerializeField] LobbyPingDisplay lobbyPingDisplay;



    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject); // 二重生成防止
            return;
        }
        Instance = this;

        OnNetworkRunnerGenerated -= AddCallbackMe;
        OnNetworkRunnerGenerated += AddCallbackMe;
    }
    //タイトルシーンでデスマッチなのかチームデスマッチかを選択する
    //その情報を持って、セッションに参加する

    void AddCallbackMe(NetworkRunner runner)
        {
        // NetworkRunnerのコールバック対象に、このスクリプト（GameLauncher）を登録する
        if (runner != null)
        {
            runner.AddCallbacks(this);
        }
    }


    void OnEnable()
    {
      
     
    }



    private async void Start()
    {




        // NetworkRunnerを生成する


        //networkRunner = Instantiate(networkRunnerPrefab);

        networkRunner=FindObjectOfType<NetworkRunner>();

        //AddCallbackMe(networkRunner);

        // NetworkRunnerのコールバック対象に、このスクリプト（GameLauncher）を登録する
        //networkRunner.AddCallbacks(this);




        //networkRunner.AddCallbacks(networkInputManager);


        //OnNetworkRunnerGenerated?.Invoke(networkRunner);


        var customProps = new Dictionary<string, SessionProperty>();

        if (GameRuleSettings.Instance != null)
        {
            customProps["GameRule"] = (int)GameRuleSettings.Instance.selectedRule;
        }
        else
        {
            customProps["GameRule"] = (int)GameRule.DeathMatch;
        }





        //StartCoroutine(StartGameCoroutine(new StartGameArgs
        //{
        //    GameMode = GameMode.AutoHostOrClient,
        //    SessionProperties = customProps,
        //    PlayerCount = maxPlayerNum,
        //    Scene = SceneManager.GetActiveScene().buildIndex,
        //    SceneManager = networkRunner.GetComponent<NetworkSceneManagerDefault>()
        //}));


        OnNetworkRunnerGenerated?.Invoke(networkRunner);


        Debug.Log($"GameLauncher.PreStartGame called. {Time.time} {networkRunner.Tick},{networkRunner.SimulationTime} ");

        bool existRunner = lobbyPingDisplay.CheckMyRunner();

        Debug.Log($"called Runner exists  Pre : {existRunner}");

        // StartGameArgsに渡した設定で、セッションに参加する
        var result = await networkRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionProperties = customProps,
            PlayerCount = maxPlayerNum,
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneManager = networkRunner.GetComponent<NetworkSceneManagerDefault>()
        });

        Debug.Log($"GameLauncher.PostStartGame called. {Time.time} {networkRunner.Tick},{networkRunner.SimulationTime} ");

        bool existRunnerPost = lobbyPingDisplay.CheckMyRunner();

        Debug.Log($"called Runner exists  Post : {existRunnerPost}");


        if (result.Ok)
        {
            Debug.Log("成功！");
        }
        else
        {
            Debug.Log("失敗！");
        }



    }

    IEnumerator StartGameCoroutine(StartGameArgs args)
    {
        yield return null; // フレーム待機してから実行するためのコルーチン
        // StartGameを非同期で実行
        var result = networkRunner.StartGame(args);
        yield return new WaitUntil(() => result.IsCompleted);
        if (result.IsCompletedSuccessfully)
        {
            Debug.Log("ゲーム開始成功！");
        }
        else
        {
            Debug.LogError("ゲーム開始失敗！: " + result.Exception);
        }
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
                    sessionNameText.SetText( $"RoomID: {sessionName}");
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
        // 参加したプレイヤーのアバターを生成する
        var avatar = runner.Spawn(playerAvatarPrefab, spawnPosition, Quaternion.identity, player);
        // プレイヤー（PlayerRef）とアバター（NetworkObject）を関連付ける
        runner.SetPlayerObject(player, avatar);

        //ホストのみバトルスタートボタンを表示
        if (runner.IsServer && player == runner.LocalPlayer)
        {
            Debug.Log("ホストが参加 → ボタン表示指示");
            lobbyUI.ShowStartButton(runner);
        }


        if (runner.IsServer && player == runner.LocalPlayer)
        {
            //CreateDummyAvatars(dummyAvatarCount);


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
