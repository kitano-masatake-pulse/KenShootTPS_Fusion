using Cinemachine;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPCInputSender : NetworkBehaviour
{
    NetworkPrototypeTest networkPrototypeTest;
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        networkPrototypeTest = GetComponent<NetworkPrototypeTest>();
        animator = GetComponent<Animator>();
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
    


    private void InputCheck()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RpcTriggerAttack();
        }
    }
    //void Jump()
    //{
    //    if (Object.HasInputAuthority)
    //    {

    //        JumpLocallly();

        
    //        // RPC送信
    //        RPC_RequestJump();
    //    }

    //}


    //void JumpLocallly()
    //{
    //    var data = new NetworkInputData();
    //    networkPrototypeTest.velocity.y = 10;
    //    data.wasdInputDirection = new Vector3(0, 10f, 0);


    //    //input.Set(data);
    //    Debug.Log($"Jump Locally. {Runner.Tick} SimuTime: {Runner.SimulationTime}networkPrototypeTest.velocity.y: {networkPrototypeTest.velocity.y}");
    //}


    //[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    //public void RPC_RequestJump(RpcInfo info=default)
    //{
    //    // RPC送信（即送信）
    //    Debug.Log($" {info.Source} Requests Jump. {info.Tick} SimuTime: {Runner.SimulationTime}");
    //    RPC_ApplyJump(info.Source);
    //}

    //[Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    //public void RPC_ApplyJump(PlayerRef  sourcePlayer, RpcInfo info = default)
    //{
    //    Debug.Log($"LocalPlayer {Runner.LocalPlayer}");
    //    Debug.Log($"SourcePlayer {sourcePlayer}");
    //    if (Runner.LocalPlayer != sourcePlayer)
    //    {
    //        Debug.Log($" Apply Jump of  {sourcePlayer}. Tick:{info.Tick} SimuTime: {Runner.SimulationTime}");
    //    }
    //    else
    //    {
    //        Debug.Log($"Don't Apply Jump because I'm source  {sourcePlayer}.  {info.Tick} SimuTime: {Runner.SimulationTime}");
    //    }

    //}
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcTriggerAttack()
    {
        animator.SetTrigger("IsPressLeftKey");
        Debug.Log("Attack Triggered");
    }
}