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
    //            // プレイヤー本体の向きをカメラ方向に回転
    //            transform.forward = bodyForward;
    //        }

    //        // cameraForward から pitch を求める 上蓋が前面を向くように
    //        float pitch = -Mathf.Asin(data.cameraForward.y) * Mathf.Rad2Deg+90;
    //                  // 頭部回転を仰角だけに限定
    //        playerHead.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

    //        // キャラのY軸回転を適用
    //        Quaternion yRot = Quaternion.Euler(0, transform.eulerAngles.y, 0);

    //        // 入力方向のベクトルを正規化する
    //        //data.wasdInputDirection.Normalize();

    //        Vector3 moveDirection = yRot * data.wasdInputDirection.normalized;  // 入力方向のベクトルを正規化する
    //        // 入力方向を移動方向としてそのまま渡す
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

        // スペースキーが押されていたら Jump フラグを立てる。FixedUpdateNetworkで取得するとNetworkInputDataに保存されないので検知されない場合がある
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
