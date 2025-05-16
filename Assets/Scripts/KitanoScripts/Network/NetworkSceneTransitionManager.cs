
using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkSceneTransitionManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkRunner runner;
    [SerializeField] private NetworkObject battlePlayerPrefab;
    public static NetworkSceneTransitionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject); // 二重生成防止
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    public void ChangeScene(SceneType sceneTypeToGo)
    {
        if (!runner.IsServer)
        {
            Debug.LogWarning("シーン遷移はサーバーのみが実行可能です");
            return;
        }

        Debug.Log($"シーン {sceneTypeToGo.ToSceneName()} に遷移します");

        //runner.LoadScene(sceneIndex);
        runner.SetActiveScene(sceneTypeToGo.ToSceneBuildIndex());//バトルシーンを指定すること
        Debug.Log($"シーン {sceneTypeToGo.ToSceneName()} に遷移するようにランナーに言いました");


    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("シーンロード完了：プレイヤー再生成開始");

        if (!runner.IsServer)
        {
            return;
        }
        
    

        foreach (var player in runner.ActivePlayers)
        {
            Vector3 spawnPos = GetSpawnPointFor(player);
            runner.Spawn(battlePlayerPrefab, spawnPos, Quaternion.identity, player);
        }
    }

    private Vector3 GetSpawnPointFor(PlayerRef player)
    {
        // プレイヤーごとに異なる初期位置を割り当てる例
        int i = player.RawEncoded;
        return new Vector3(i * 2f, 1f, 0f);
    }

    // 他のコールバック（未使用）
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnSceneLoadStart(NetworkRunner runner) { Debug.Log("シーンロード開始"); }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        // デフォルトで全ての接続を許可
        request.Accept();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        // 今回は未使用のため何もしない
        // 将来的にカスタムメッセージ受信処理を追加することが可能
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        // Sharedモードでセッションリスト取得時に呼ばれる
        // ログを出しておくとデバッグに便利
        Debug.Log($"[Fusion] セッション一覧が更新されました: {sessionList.Count} 件");
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        // カスタム認証レスポンス（未使用の場合は空でOK）
        // PlayFabなどと組み合わせた認証処理で活用できる
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        // Hostモードでホストが落ちた場合の引き継ぎ処理
        Debug.LogWarning("⚠️ ホストの移行処理が必要ですが、未対応です。");
        // 必要があればここで `StartGame()` を再呼び出す設計が可能
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
        // カスタムデータ（バイナリ）の送受信。今回は未使用
        // RaiseEvent/RPCの代替などで使える拡張ポイント
    }

}
