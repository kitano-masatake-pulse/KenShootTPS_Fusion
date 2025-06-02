using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class RPCInputSender : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        InputCheck();
    }

    public override void Spawned()
    {
        Runner.ProvideInput = true;
    }



    // アクション用の入力チェックメソッド
    //順序管理のため、PlayerCallOrderから呼ばれる
    public void InputCheck()
    {

        if (!Object.HasInputAuthority) return; //入力権限がない場合は何もしない



        if (Input.GetKeyDown(KeyCode.Space))//後々ここに接地判定を追加
        {
            Jump();
        }
    }
    void Jump()
    {
        
        
        float jumpCalledTime = Runner.SimulationTime;

        JumpLocally(jumpCalledTime);

        
            // RPC送信
        RPC_RequestJump(jumpCalledTime);
        

    }


    void JumpLocally(float calledTime)
    { 
        Debug.Log($"Jump Locally. {Runner.Tick} SimuTime: {Runner.SimulationTime}");
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_RequestJump(float calledTime, RpcInfo info=default)
    {
        // RPC送信（即送信）
        Debug.Log($" {info.Source} Requests Jump. {info.Tick} SimuTime: {calledTime}");
        RPC_ApplyJump(info.Source, calledTime);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_ApplyJump(PlayerRef  sourcePlayer, float calledTime, RpcInfo info = default)
    {
        Debug.Log($"LocalPlayer {Runner.LocalPlayer}");
        Debug.Log($"SourcePlayer {sourcePlayer}");
        if (Runner.LocalPlayer != sourcePlayer)
        {
            Debug.Log($" Apply Jump of  {sourcePlayer}. Tick:{info.Tick} SimuTime: {Runner.SimulationTime}");
        }
        else
        {
            Debug.Log($"Don't Apply Jump because I'm source  {sourcePlayer}.  {info.Tick} SimuTime: {Runner.SimulationTime}");
        }

    }


}
