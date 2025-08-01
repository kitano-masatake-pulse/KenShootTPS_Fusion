using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyPingDisplay : NetworkBehaviour ,INetworkRunnerCallbacks
{
    [SerializeField] RectTransform listContainer;
    [SerializeField] GameObject entryPrefab;
    //NetworkRunner Runner;
    Dictionary<PlayerRef, TextMeshProUGUI> _entries = new();

    float elapsedTime = 0f;
    float pingDisplayInterval = 1f; // 1秒ごとに更新

    bool isAddedCallback = false;



    public override void Spawned()
    {
        GeneratePingOfAlreadyPlayers(Runner);
        bool existRunner = CheckMyRunner();
        Debug.Log($"LobbyPingDisplay.Spawned called.{Time.time} {Runner.Tick},{Runner.SimulationTime},Runner exist {existRunner}  ");
        this.transform.SetParent(FindObjectOfType<MenberListGenerator>()?.transform, false);

        Runner?.AddCallbacks(this);

    }

    void Awake()
            {


        Debug.Log($"LobbyPingDisplay.Awake called.{Time.time} ");
    }


    void OnEnable()
    {

        bool existRunner = CheckMyRunner();
        Debug.Log($"LobbyPingDisplay.OnEnable called.{Time.time},Runner exist {existRunner}  ");

    }

    void OnDisable()
    {
        bool existRunner = CheckMyRunner();
        Debug.Log($"LobbyPingDisplay.OnDisable called.{Time.time} ,Runner exist {existRunner}");
        Runner?.RemoveCallbacks(this);
    }

    void Start()
    {

    }



    public bool CheckMyRunner()
    {
        return Runner != null;

    }




    void Update()
    {

        elapsedTime += Time.deltaTime;
        if (elapsedTime >= pingDisplayInterval)
        {
            elapsedTime = 0f;
            GetPingInHost();
        }
    }




    void GetPingInHost()
    {
        //Debug.Log("GetPingInHost called.");
        if (Runner == null  || !Runner.IsServer) return;
        foreach (var player in Runner.ActivePlayers)
        {
            double rttSec = Runner.GetPlayerRtt(player);
            int ms = Mathf.RoundToInt((float)(rttSec * 1000));
            if (_entries.TryGetValue(player, out var txt))
            {
                RPC_StreamPing(player, ms);
            }
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_StreamPing(PlayerRef pingPlayer, int pingTime, RpcInfo info = default)
    {
        if (_entries.TryGetValue(pingPlayer, out var txt))
        {
            txt.text = $"{pingPlayer} : {pingTime} ms";
        }



    }





    public void AddCallbackMe(NetworkRunner runner)
        {
            //Debug.Log("LobbyPingDisplay.AddCallbackMe called.");
            if (runner != null)
            {
                runner.AddCallbacks(this);
            }
           // Runner = runner;
    }


        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {


      
        if (player != runner.LocalPlayer)
        {

            var go = Instantiate(entryPrefab, listContainer);
            var txt = go.GetComponentInChildren<TextMeshProUGUI>();
            txt.text = $"{player} : -- ms";
            _entries[player] = txt;

            Debug.Log($"Player {player} joined. Ping entry created.");

        }
        else
        {
            // ローカルプレイヤーのエントリでは全員分のPingを生成する
            Debug.Log($"Local player {player} joined. Generating ping entries for all players.");
        }

        //GeneratePingOfAlreadyPlayers(runner );
        }

            void GeneratePingOfAlreadyPlayers(NetworkRunner runner)
            {
            foreach (var alreadyPlayer in runner.ActivePlayers)
            {
                var go = Instantiate(entryPrefab, listContainer);
                var txt = go.GetComponentInChildren<TextMeshProUGUI>();
                txt.text = $"{alreadyPlayer} : -- ms";
            _entries[alreadyPlayer] = txt;

            Debug.Log($"Player {alreadyPlayer} already joined. Ping entry created.");



            }
          
        }







    

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (_entries.TryGetValue(player, out var txt))
            {
                Destroy(txt.rectTransform.gameObject);
                _entries.Remove(player);
            }
        }

        // 他のコールバックは空実装で OK
        public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason reason) { }
        public void OnConnectedToServer(NetworkRunner runner) 
    
    {  
    
        Debug.Log($"LobbyPingDisplay.OnConnectedToServer called.{Time.time} ");
 

    }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }

        void INetworkRunnerCallbacks.OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
            //throw new NotImplementedException();
        }

        void INetworkRunnerCallbacks.OnDisconnectedFromServer(NetworkRunner runner)
        {
            //throw new NotImplementedException();
        }
    
}
