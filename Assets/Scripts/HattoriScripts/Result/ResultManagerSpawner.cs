using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;

public class ResultManagerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{

    [SerializeField] private NetworkPrefabRef resultManagerPrefab; // ResultManager�̃v���n�u
    private NetworkRunner runner; // NetworkRunner�̃C���X�^���X��ێ�����ϐ�

    void Start() {
        runner = FindObjectOfType<NetworkRunner>();
        runner.AddCallbacks(this); // �R�[���o�b�N��o�^
    }

    void OnDisable() {
        if (runner != null) {
            runner.RemoveCallbacks(this); // �R�[���o�b�N��o�^����
        }
    }

    public void OnSceneLoadDone(NetworkRunner runner) {
        //�V�[�������[�h���ꂽ��Result�V�[����ResultDisplayManager�Ȃǂ𐶐�
        if (runner.IsServer) {
            runner.Spawn(resultManagerPrefab);
        }
        
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
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
