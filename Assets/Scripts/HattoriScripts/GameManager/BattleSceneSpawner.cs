using System;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;


public class BattleSceneSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField]
    private NetworkPrefabRef playerAvatarPrefab;
    [SerializeField]
    private NetworkPrefabRef dummyAvatarPrefab;
    private NetworkRunner runner;
    private HashSet<PlayerRef> spawnedPlayers = new();

    [Header("デバッグ用：ダミーアバターの生成数(0で無効)")]
    public int dummyAvatarCount = 1;

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
                var avatar = runner.Spawn(playerAvatarPrefab, spawnPosition, Quaternion.identity, player);
                spawnedPlayers.Add(player);
                // プレイヤー（PlayerRef）とアバター（NetworkObject）を関連付ける
                runner.SetPlayerObject(player, avatar);
                Debug.Log($"[Spawn] プレイヤー {player} をスポーンしました");
            }

            //CreateDummyAvatars(runner, dummyAvatarCount);
        }

        //ランナーが存在するか確認
        Debug.Log($"Battle Scene：Runner exists: {runner != null}");
        Debug.Log($"Battle Scene：SceneLoaded Pre called");
        Debug.Log($"GameManager called {GameManager.Instance != null} ");
        GameManager.Instance.SceneLoaded(); // GameManagerの初期化
        Debug.Log($"Battle Scene：SceneLoaded Post called");
    }

    public void CreateDummyAvatars(NetworkRunner runner, int DummyCount)
    {
        for (int i = 0; i < DummyCount; i++)
        {

            // ランダムな生成位置（半径5の円の内部）を取得する
            var randomValue = UnityEngine.Random.insideUnitCircle * 5f;
            var spawnPosition = new Vector3(randomValue.x, 5f, randomValue.y);

            var avatar = runner.Spawn(dummyAvatarPrefab, spawnPosition, Quaternion.identity, PlayerRef.None);
            // PlayerRef.Noneを使用して、ダミーアバターはプレイヤーに関連付けない


        }

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
