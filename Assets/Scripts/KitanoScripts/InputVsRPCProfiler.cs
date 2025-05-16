using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using System;

public struct MyInputData : INetworkInput
{
    public bool jumpPressed;
    public int inputSentTick; // 入力を送った時のTickを記録
}

public enum InputButtons
{
    Space,
}

public class InputVsRPCProfiler : NetworkBehaviour,INetworkRunnerCallbacks
{

    public static InputVsRPCProfiler  Instance { get; private set; }


    private bool jumpPressedLocal;
    private int localSentTick;

    private int? receivedInputTick = null;
    private float? inputReceiveTime = null;

    private int? receivedRpcTick = null;
    private float? rpcReceiveTime = null;

    bool inputPressedLocalEver=false;


    void Awake()
    {
       
    }

    public override void Spawned()
    {

        if (Instance != null)
        {
            Destroy(gameObject); // 二重生成防止
            return;
        }
        Instance = this;

            Runner.ProvideInput = true;
        
    }

    void Update()
    {
        //if (!Object.HasInputAuthority) return;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            localSentTick = Runner.Tick;
            jumpPressedLocal = true;

            // Debug.Log($" Space pressed locally at tick: {localSentTick}, time: {Time.time}");

            // RPC送信（即送信）
            RPC_JumpBenchmark(localSentTick);
        }

    }

    public override void FixedUpdateNetwork()
    {
        // Tickに同期したログ出力（1回だけ）
        if (receivedInputTick.HasValue && receivedRpcTick.HasValue)
        {
            Debug.Log($" Comparison Report:");
            Debug.Log($" Input received at tick: {receivedInputTick}, time: {inputReceiveTime:F4}");
            Debug.Log($" RPC   received at tick: {receivedRpcTick}, time: {rpcReceiveTime:F4}");

            int tickDelta = receivedInputTick.Value - receivedRpcTick.Value;
            float timeDelta = inputReceiveTime.Value - rpcReceiveTime.Value;
            Debug.Log($" Tick Delta: {tickDelta}, Time Delta: {timeDelta:F4} seconds");

            // リセット（連続比較したい場合はコメントアウト）
            receivedInputTick = null;
            receivedRpcTick = null;
            inputReceiveTime = null;
            rpcReceiveTime = null;
        }

        //if (Input.GetKey(KeyCode.Space))
        //{
        //    localSentTick = Runner.Tick;
        //    jumpPressedLocal = true;

        //    // Debug.Log($" Space pressed locally at tick: {localSentTick}, time: {Time.time}");

        //    // RPC送信（即送信）
        //    RPC_JumpBenchmark(localSentTick);
        //}

        if (GetInput(out MyInputData input))
        {
           //Debug.Log($"GetInput Tick:{Runner.Tick}");
            //Debug.Log("GetInput");
            if (inputPressedLocalEver && !receivedInputTick.HasValue)
            {
                receivedInputTick = Runner.Tick;
                inputReceiveTime = Time.time;

                Debug.Log($" Input received. Sent at tick: {input.inputSentTick}, received at tick: {Runner.Tick}, time: {inputReceiveTime}");
            }
        }

       Debug.Log($"FixedUpdateNetwork Execute Tick:{Runner.Tick}"); // 追加
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.All,TickAligned =false)]
    public void RPC_JumpBenchmark(int sentTick)
    {
        if (!receivedRpcTick.HasValue)
        {
            receivedRpcTick = Runner.Tick;
            rpcReceiveTime = Time.time;

            Debug.Log($"🔴 RPC received. Sent at tick: {sentTick}, received at tick: {Runner.Tick}, time: {rpcReceiveTime}");
        }
    }



public void OnInput(NetworkRunner runner, NetworkInput input)
{
        var data = new MyInputData();


        data.jumpPressed=Input.GetKey(KeyCode.Space);
        data.inputSentTick = Runner.Tick; // 入力を送った時のTickを記録
        if (data.jumpPressed)
        {
            inputPressedLocalEver = true;
           // Debug.Log($"OnInput dataPressed sent at tick: {Runner.Tick}, time: {Time.time}");
        }


        input.Set(data);
        data = default;

        Debug.Log($"OnInput Called sent at tick: {Runner.Tick}, time: {Time.time}");
    }


    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
   
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnSceneLoadStart(NetworkRunner runner) { Debug.Log("シーンロード開始"); }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        // デフォルトで全ての接続を許可
        request.Accept();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        // 今回は未使用のため何もしない
        // 将来的にカスタムメッセージ受信処理を追加することが可能
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        // Sharedモードでセッションリスト取得時に呼ばれる
        // ログを出しておくとデバッグに便利
        Debug.Log($"[Fusion] セッション一覧が更新されました: {sessionList.Count} 件");
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        // カスタム認証レスポンス（未使用の場合は空でOK）
        // PlayFabなどと組み合わせた認証処理で活用できる
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        // Hostモードでホストが落ちた場合の引き継ぎ処理
        Debug.LogWarning("⚠️ ホストの移行処理が必要ですが、未対応です。");
        // 必要があればここで `StartGame()` を再呼び出す設計が可能
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
        // カスタムデータ（バイナリ）の送受信。今回は未使用
        // RaiseEvent/RPCの代替などで使える拡張ポイント
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        
    }

   
}
