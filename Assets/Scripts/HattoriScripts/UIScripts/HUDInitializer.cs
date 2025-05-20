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
        // �ǂ̃N���C�A���g�ł������� runner �ɃR�[���o�b�N�o�^
        var runner = FindObjectOfType<NetworkRunner>();
        runner.AddCallbacks(this);
    }

    // BattleScene �̃��[�h������ɌĂ΂�܂��i�S���ɌĂ΂��j
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        // ���[�J���v���C���[�̃I�u�W�F�N�g���擾�ł�����
        if (runner.TryGetPlayerObject(runner.LocalPlayer, out var go))
        {
            var state = go.GetComponent<PlayerNetworkState>();
            hudManager.SetLocalStateAndSubscribe(state);
        }
        else { Debug.LogError("HUD Initilaizer�F�����i�[��������܂���"); }
    }

    // ���̃R�[���o�b�N�͋����
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