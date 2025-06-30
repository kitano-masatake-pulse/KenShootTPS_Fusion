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

    public static Action<NetworkRunner> OnNetworkRunnerGenerated;
    public static Action<NetworkRunner> OnStartedGame;


    private StartGameArgs startGameArgs;


    [SerializeField] private float reconnectTimeout = 30f;  // タイムアウト時間（秒）
    string localUserId = ""; // ローカルユーザーIDを保存する変数

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

        OnNetworkRunnerGenerated -= AddCallbackMe;
        OnNetworkRunnerGenerated += AddCallbackMe;

    }

    void AddCallbackMe(NetworkRunner runner)
    {
        // NetworkRunnerのコールバック対象に、このスクリプト（GameLauncher）を登録する
        if (runner != null)
        {
            runner.AddCallbacks(this);
            NetworkRunner.CloudConnectionLost += (NetworkRunner runner, ShutdownReason reason, bool reconnecting) => { };
        }
    }

    private void OnDestroy()
    {
        OnNetworkRunnerGenerated -= AddCallbackMe;
        NetworkRunner.CloudConnectionLost -= HandleDisconnect;
    }





    private async void Start()
    {

        // NetworkRunnerを生成する
        networkRunner=Instantiate(networkRunnerPrefab, Vector3.zero, Quaternion.identity);


        networkRunner.transform.parent = null; // NetworkRunnerをシーンのルートに移動する

        OnNetworkRunnerGenerated?.Invoke(networkRunner);
        //networkRunner.AddCallbacks(this); // コールバックを登録する

        NetworkRunner.CloudConnectionLost -= HandleDisconnect;
        NetworkRunner.CloudConnectionLost+= HandleDisconnect;


       customProps = new Dictionary<string, SessionProperty>();

        if (GameRuleSettings.Instance != null)
        {
            customProps["GameRule"] = (int)GameRuleSettings.Instance.selectedRule;
        }
        else
        {
            customProps["GameRule"] = (int)GameRule.DeathMatch;
        }

        startGameArgs=
            new StartGameArgs
            {
                GameMode = GameMode.AutoHostOrClient,
                SessionProperties = customProps,
                PlayerCount = maxPlayerNum,
                Scene = SceneManager.GetActiveScene().buildIndex,
                SceneManager = networkRunner.GetComponent<NetworkSceneManagerDefault>()
            };

        // StartGameArgsに渡した設定で、セッションに参加する
        var result = await networkRunner.StartGame(startGameArgs );  

        OnStartedGame?.Invoke(networkRunner);

        if (result.Ok)
        {
            Debug.Log("成功！");
        }
        else
        {
            Debug.Log("失敗！");
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



    //退出処理
    public void LeaveRoom()
    {
        // すべてのコールバックを削除
        networkRunner.RemoveCallbacks(this);
        // Runnerを停止
        networkRunner.Shutdown();
        // シーンをタイトルシーンに戻す
        SceneManager.LoadScene("TitleScene");
    }

    #region 異常系


    void Update()
    {
        //// デバッグ用：ダミーアバターの生成数を変更できるようにする
        //if (Input.GetKeyDown(KeyCode.F1))
        //{
        //    Destroy(networkRunner); // Runnerを停止
        //}
    }

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

        Debug.Log($"OnPlayerJoined called. Join {player}!!");

        ShowSessionInfo(runner);

        // ホスト（サーバー兼クライアント）かどうかはIsServerで判定できる
        if (runner.IsServer) 
        {
            //入室したクライアントのアバターを生成
            ClientAvatarSpawn(runner, player);

            if (player == runner.LocalPlayer)
            {

                //ホストのみバトルスタートボタンを表示
                lobbyUI.ShowStartButton(runner);
            }


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
        StartCoroutine(TryReconnectCorpoutine()); // 再接続を試みる
    }
    IEnumerator TryReconnectCorpoutine()
    {

        float startTime = Time.time;

        //いったんシャットダウンしておく
        //networkRunner.Shutdown();


        while (Time.time - startTime < reconnectTimeout)
        {
            // 少し待ってから再接続
            yield return new WaitForSeconds(1f);

            TryReconnect();

            // 再接続が成功したか確認
            
                yield return new WaitUntil(() => networkRunner.IsRunning);
                Debug.Log("Reconnected to the Cloud!");
            




            if (networkRunner.IsRunning)
            {
                Debug.Log("[Reconnect] Successfully reconnected!");
                yield break;  // 成功したらリトライループを抜ける
            }
            else
            {
                Debug.Log("[Reconnect] Retry failed, trying again…");
            }
        }

        // タイムアウト到達
        Debug.LogWarning($"[Reconnect] Failed to reconnect within {reconnectTimeout}s.");
        //ShowTimeoutDialog();
    }

    // 再接続時の処理例
    async void TryReconnect()
    {
        // 1. いったんセッションを終了（Shutdown が完了するまで待つ）
        if (networkRunner != null)
        { 
            await networkRunner.Shutdown(destroyGameObject : false);
            //Destroy(networkRunner.gameObject); // Runnerを破棄


        }

        networkRunner=Instantiate(networkRunnerPrefab);


        // 2. 必要ならコールバック再登録
        networkRunner.AddCallbacks(this);
        networkRunner.AddCallbacks(networkInputManager);
        OnNetworkRunnerGenerated?.Invoke(networkRunner);


        startGameArgs =
            new StartGameArgs
            {
                GameMode = GameMode.AutoHostOrClient,
                SessionProperties = customProps,
                PlayerCount = maxPlayerNum,
                Scene = SceneManager.GetActiveScene().buildIndex,
                SceneManager = networkRunner.GetComponent<NetworkSceneManagerDefault>()
            };

        // 3. 前回と同じ Args で再起動
        var result = await networkRunner.StartGame(startGameArgs);

        if (result.Ok)
            Debug.Log("Reconnected!");
        else
            Debug.LogError($"Reconnect failed: {result.ErrorMessage}");
    }


    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {
    
        Debug.Log($"OnConnectRequest called. Request from ");
        // ここで接続リクエストを処理することができます
        // 例えば、特定の条件を満たすプレイヤーのみを許可するなど
        // runner.Accept(request); // 接続を許可する場合
        // runner.Deny(request); // 接続を拒否する場合
    }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { 
    
        Debug.LogError($"OnConnectFailed called. Reason: {reason} Remote Address: {remoteAddress}");
        // 接続失敗時の処理をここに記述できます
        // 例えば、ユーザーにエラーメッセージを表示するなど

    }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) {
    
    
        Debug.Log($"OnSceneLoadDone called. Runner: {runner}");
        // シーンロード完了後の処理をここに記述できます
        // 例えば、UIの更新やゲーム開始の準備など
        
    }
    public void OnSceneLoadStart(NetworkRunner runner) { }


}
