using Cinemachine;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NetworkInputManager : MonoBehaviour,INetworkRunnerCallbacks
{

    
    public TPSCameraController tpsCameraController;

    public PlayerAvatar myPlayerAvatar;
    private NetworkRunner runner;



    //private void Awake()
    //{
    //    GameLauncher.Instance.OnNetworkRunnerGenerated -= AddCallbackMe;
    //    GameLauncher.Instance.OnNetworkRunnerGenerated += AddCallbackMe;
    //}

    public void AddCallbackMe(NetworkRunner runner)
    {
        //Debug.Log("NetworkInputManager.AddCallbackMe called.");
        if (runner != null)
        {
            runner.AddCallbacks(this);
        }
    }


    void OnEnable()
    {
        //Debug.Log("NetworkInputManager.OnEnable called.");
        GameLauncher.OnNetworkRunnerGenerated -= AddCallbackMe;
        

        GameLauncher.OnNetworkRunnerGenerated += AddCallbackMe;
        
    }


    // Start is called before the first frame update
    void Start()
    {

        runner = FindObjectOfType<NetworkRunner>();
        if (runner != null)
        { runner.AddCallbacks(this); }
        
    }

    void OnDisable()
    {
        GameLauncher.OnNetworkRunnerGenerated -= AddCallbackMe;


        if (runner != null)
            runner.RemoveCallbacks(this); 


    }



    void Update()
    {
        
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        //Debug.Log($"{runner.LocalPlayer} NetworkInputManager.OnInput called.");
        if (myPlayerAvatar==null)
        {
            //アバターが紐づいていないなら、入力を受け付けない
            input.Set(new NetworkInputData());
            return;
        }
        var data = new NetworkInputData();

        data.avatarPosition = myPlayerAvatar.transform.position;
        //data.avatarRotation = myPlayerAvatar.transform.rotation.eulerAngles;
        data.normalizedInputDirection=myPlayerAvatar.normalizedInputDirection;
        data.headForward = myPlayerAvatar.headObject.transform.forward;


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
