using Cinemachine;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NetworkInputManager : MonoBehaviour,INetworkRunnerCallbacks
{

    
    //public TPSCameraController tpsCameraController;

    

    private void Awake()
    {
        
    }


    // Start is called before the first frame update
    void Start()
    {
        NetworkRunner networkRunner = FindObjectOfType<NetworkRunner>();

        //networkRunner.AddCallbacks(this);
    }


    //public override void FixedUpdateNetwork()
    //{

    //    Debug.Log("NetworkInput");
    //    if (GetInput(out NetworkInputData data) )
    //    {


    //        Vector3 bodyForward = new Vector3(data.cameraForward.x, 0f, data.cameraForward.z).normalized;

    //        if (bodyForward.sqrMagnitude > 0.0001f)
    //        {
    //            // �v���C���[�{�̂̌������J���������ɉ�]
    //            transform.forward = bodyForward;
    //        }

    //        // cameraForward ���� pitch �����߂� ��W���O�ʂ������悤��
    //        float pitch = -Mathf.Asin(data.cameraForward.y) * Mathf.Rad2Deg+90;
    //                  // ������]���p�����Ɍ���
    //        playerHead.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

    //        // �L������Y����]��K�p
    //        Quaternion yRot = Quaternion.Euler(0, transform.eulerAngles.y, 0);

    //        // ���͕����̃x�N�g���𐳋K������
    //        //data.wasdInputDirection.Normalize();

    //        Vector3 moveDirection = yRot * data.wasdInputDirection.normalized;  // ���͕����̃x�N�g���𐳋K������
    //        // ���͕������ړ������Ƃ��Ă��̂܂ܓn��
    //        characterController.Move(moveDirection);
    //    }
    //}
    // Update is called once per frame




    void Update()
    {
        
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        data.wasdInputDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        // �X�y�[�X�L�[��������Ă����� Jump �t���O�𗧂Ă�BFixedUpdateNetwork�Ŏ擾�����NetworkInputData�ɕۑ�����Ȃ��̂Ō��m����Ȃ��ꍇ������
        data.jumpPressed = Input.GetKey(KeyCode.Space);
        data.attackClicked = Input.GetMouseButtonDown(0);



        input.Set(data);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
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
