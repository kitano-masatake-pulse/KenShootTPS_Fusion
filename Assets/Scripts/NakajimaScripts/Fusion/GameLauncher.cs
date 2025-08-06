using Cinemachine;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    [Header("戻り先のシーン(デフォルトLobbyScene)")]
    public SceneType returnScene = SceneType.Lobby;

    //public static Action<NetworkRunner> OnNetworkRunnerGenerated;// Runnerが生成されたときのイベント、StartGame前
    //public static Action<NetworkRunner> OnNetworkRunnerConnected;// Runnerが接続されたときのイベント、StartGame後


    private StartGameArgs startGameArgs;
    [SerializeField] private float reconnectTimeout = 30f;  // タイムアウト時間（秒）
    string localUserId = ""; // ローカルユーザーIDを保存する変数
    [SerializeField] private bool _isFirstTime = true;

    //デバッグ用
    [SerializeField] LobbyPingDisplay lobbyPingDisplay;
    private Dictionary<string, SessionProperty> customProps;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject); // 二重生成防止
            return;
        }
        Instance = this;

    }
    //タイトルシーンでデスマッチなのかチームデスマッチかを選択する
    //その情報を持って、セッションに参加する

   


    void OnEnable()
    {

        ConnectionManager.OnNetworkRunnerGenerated -= AddCallbackMe;
        ConnectionManager.OnNetworkRunnerGenerated += AddCallbackMe;

    }

    void AddCallbackMe(NetworkRunner runner)
    {
        // NetworkRunnerのコールバック対象に、このスクリプト（GameLauncher）を登録する
        if (runner != null)
        {
            networkRunner= runner; // 現在のRunnerを更新
            runner.AddCallbacks(this);
            NetworkRunner.CloudConnectionLost += (NetworkRunner runner, ShutdownReason reason, bool reconnecting) => { };
            Debug.Log("GameLauncher.AddCallbackMe called. Runner: " + runner);


            if(runner.IsServer)
            {
                runner.SessionInfo.IsOpen = true; // セッションを開く   
                runner.SessionInfo.IsVisible = true; // セッションを表示する
            }
        }
    }

    private void OnDisable()
    {
        ConnectionManager.OnNetworkRunnerGenerated -= AddCallbackMe;
        
        if (networkRunner != null)
        { 
        networkRunner.RemoveCallbacks(this);
            Debug.Log("GameLauncher.OnDisable called. Runner: " + networkRunner);
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
    //PlayerRefを指定して、アバターを生成し、紐づけまでする
    private void CreatePlayerAvatar(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) { return; }
        var randomValue = UnityEngine.Random.insideUnitCircle * 5f;
        var spawnPosition = new Vector3(randomValue.x, 5f, randomValue.y);
        var avatar = runner.Spawn(playerAvatarPrefab, spawnPosition, Quaternion.identity, player);
        runner.SetPlayerObject(player, avatar);
    }



    //退出処理
    public async void LeaveRoom()
    {
        if(networkRunner != null)
        {
            // すべてのコールバックを削除
            networkRunner.RemoveCallbacks(this);
        }
        SceneTransitionManager.Instance.ChangeScene(returnScene,true); // 戻り先のシーンに遷移
    }

    #region 異常系


    void HandleDisconnect(NetworkRunner runner, ShutdownReason reason, bool reconnecting)
    {
        if (reconnecting)
        {
            // Fusion が自動再接続を試みているので完了を待つ
            Debug.Log("Disconnected but Attempting to reconnect to the Cloud...");
            StartCoroutine(WaitForReconnection(runner));
        }
        else
        {
            // 自動再接続なし → ユーザーに通知したり手動リトライUIを出す
            Debug.Log("Disconnected from the Cloud. Please check your network connection.");
            ShowReconnectDialog();
        }
    }

    System.Collections.IEnumerator WaitForReconnection(NetworkRunner runner)
    {
        yield return new WaitUntil(() => runner.IsInSession);
        Debug.Log("Reconnected to the Cloud!");
    }

    void ShowReconnectDialog()
    {
        // 任意実装：ダイアログ出す、ボタン有効化 など
    }

    #endregion







    // INetworkRunnerCallbacksインターフェースの空実装
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        networkRunner = runner; // 現在のRunnerを更新
        Debug.Log($"GameLauncher:OnPlayerJoined.");
        

        Debug.Log($"OnPlayerJoined called. Join {player}!!");

        ShowSessionInfo(runner);
        
        // ホスト（サーバー兼クライアント）かどうかはIsServerで判定できる
        if (runner.IsServer) 
        {
            //入室したクライアントのアバターを生成
            //if(_isFirstTime){ClientAvatarSpawn(runner, player);}

            if (runner.GetPlayerObject(player) == null)
            {
                CreatePlayerAvatar(runner, player);
            }
            

                if (player == runner.LocalPlayer)
            {

                //ホストのみバトルスタートボタンを表示
                lobbyUI.ShowStartButton(runner);
            }


        }

        if(player == runner.LocalPlayer)
        {
            SceneTransitionManager.Instance.StartScene();
        }

    }

    void ShowSessionInfo(NetworkRunner runner)
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
    }


    void ClientAvatarSpawn(NetworkRunner runner, PlayerRef player)
    {

        // ランダムな生成位置（半径5の円の内部）を取得する
        var randomValue = UnityEngine.Random.insideUnitCircle * 5f;

        var spawnPosition = new Vector3(randomValue.x, 5f, randomValue.y);

        // 参加したプレイヤーのアバターを生成する
        var avatar = runner.Spawn(playerAvatarPrefab, spawnPosition, Quaternion.identity, player);

        // プレイヤー（PlayerRef）とアバター（NetworkObject）を関連付ける
        runner.SetPlayerObject(player, avatar);



    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
        if (!runner.IsServer) { return; }
        // 退出したプレイヤーのアバターを破棄する
        if (runner.TryGetPlayerObject(player, out var avatar))
        {
            runner.Despawn(avatar);
        }
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }


    public void OnInput(NetworkRunner runner, NetworkInput input) { }

    
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) 
    {
        Debug.Log($"GameLauncher runner Shutdown. OnShutdown called. Reason: {shutdownReason}");
    }
    public void OnConnectedToServer(NetworkRunner runner) 
    
    { 

        Debug.Log($"GameLauncher runner Connected . OnConnectedToServer called. Runner: {runner}");
        //runner.Spawn(dummyAvatarPrefab, Vector3.zero, Quaternion.identity, PlayerRef.None);

    }
    public void OnDisconnectedFromServer(NetworkRunner runner) 
    { 
        Debug.Log($"GameLauncher runner Disconnected . OnDisconnectedFromServer called. Runner: {runner}");

    }
    



    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {
    
        Debug.Log($"OnConnectRequest called. Request from ");
        // ここで接続リクエストを処理することができます
        // 例えば、特定の条件を満たすプレイヤーのみを許可するなど
        // runner.Accept(request); // 接続を許可する場合
        // runner.Deny(request); // 接続を拒否する場合
    }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) 
    { 
    
        Debug.LogError($"OnConnectFailed called. Reason: {reason} Remote Address: {remoteAddress}");
        // 接続失敗時の処理をここに記述できます
        // 例えば、ユーザーにエラーメッセージを表示するなど
        
        
    }

    public void OnSceneLoadDone(NetworkRunner runner) 
    {
        Debug.Log($"GameLauncher:OnSceneLoadDone called. {Time.time} {runner.Tick},{runner.SimulationTime} ");
        //if (_isFirstTime) return; // 2回目以降のみ処理を実行
        Debug.Log($"GameLauncher:OnSceneLoadDone called and not return. {Time.time} {runner.Tick},{runner.SimulationTime} ");
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


        if (runner.IsServer)
        {
            foreach (var player in runner.ActivePlayers)
            {
                if (runner.GetPlayerObject(player) == null)
                {
                    CreatePlayerAvatar(runner, player);
                }

            }

            Debug.Log("ホストが参加 → ボタン表示指示");
            lobbyUI.ShowStartButton(runner);
        }



        SceneTransitionManager.Instance.StartScene();

        //ConnectionManager.OnNetworkRunnerConnected?.Invoke(networkRunner); // ゲーム開始のイベントを発火(NetworkObjectのSpawnなど)

    }


    
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }



    public void OnSceneLoadStart(NetworkRunner runner) { }


}
