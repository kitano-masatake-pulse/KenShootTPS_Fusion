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

    [SerializeField]
    private GameObject TPSCamera;


    void Start()
    {
        runner = FindObjectOfType<NetworkRunner>();
        runner.AddCallbacks(this); // コールバック登録

    }
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (runner.IsServer)
        {
            //ホストが全員分のアバターを生成
            foreach (var player in runner.ActivePlayers)
            {
                if (spawnedPlayers.Contains(player)) continue;

                var randomValue = UnityEngine.Random.insideUnitCircle * 5f;
                var spawnPosition = new Vector3(randomValue.x, 5f, randomValue.y);
                var avatar = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
                spawnedPlayers.Add(player);
                // プレイヤー（PlayerRef）とアバター（NetworkObject）を関連付ける
                runner.SetPlayerObject(player, avatar);
                Debug.Log($"[Spawn] プレイヤー {player} をスポーンしました");
            }
        }

        //カメラを各クライアントに追従させるようにセットさせる(RPC)
        //TPSCameraController tpsCameraController = TPSCamera.GetComponent<TPSCameraController>();
        //tpsCameraController.RPC_SetCameraToMyAvatar();

    }

   




    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player){ }

    // 他のINetworkRunnerCallbacksは空実装でOK
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) 
    {
       
        if (runner.IsServer && runner.TryGetPlayerObject(player, out var avatar))
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
    public void OnSceneLoadStart(NetworkRunner runner) { }
}
