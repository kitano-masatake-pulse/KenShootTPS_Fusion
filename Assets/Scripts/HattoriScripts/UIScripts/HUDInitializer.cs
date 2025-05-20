using System.Collections.Generic;
using System;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class HUDInitializer : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private HUDManager hudManager;

    void Awake()
    {
        // どのクライアントでも自分の runner にコールバック登録
        var runner = FindObjectOfType<NetworkRunner>();
        runner.AddCallbacks(this);
    }

    // BattleScene のロード完了後に呼ばれます（全員に呼ばれる）
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        // ローカルプレイヤーのオブジェクトを取得できたら
        if (runner.TryGetPlayerObject(runner.LocalPlayer, out var go))
        {
            var state = go.GetComponent<PlayerNetworkState>();
            hudManager.SetLocalStateAndSubscribe(state);
        }
        else { Debug.LogError("HUD Initilaizer：ランナーが見つかりません"); }
    }

    // 他のコールバックは空実装
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason reason) { }
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