using System;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;


public class BattleSceneSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField]
    private NetworkPrefabRef playerPrefab;
    private NetworkRunner runner;
    private HashSet<PlayerRef> spawnedPlayers = new();


    void Start()
    {
        runner = FindObjectOfType<NetworkRunner>();
        runner.AddCallbacks(this); // コールバック登録

    }
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (!runner.IsServer) return;

        foreach (var player in runner.ActivePlayers)
        {
            if (spawnedPlayers.Contains(player)) continue;

            var randomValue = UnityEngine.Random.insideUnitCircle * 5f;
            var spawnPosition = new Vector3(randomValue.x, 5f, randomValue.y);
            runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
            spawnedPlayers.Add(player);

            Debug.Log($"[Spawn] プレイヤー {player} をスポーンしました");
        }
    }
    
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player){ }

    // 他のINetworkRunnerCallbacksは空実装でOK
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
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
