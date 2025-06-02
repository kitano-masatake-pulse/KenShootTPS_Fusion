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



    // �A�N�V�����p�̓��̓`�F�b�N���\�b�h
    //�����Ǘ��̂��߁APlayerCallOrder����Ă΂��
    public void InputCheck()
    {

        if (!Object.HasInputAuthority) return; //���͌������Ȃ��ꍇ�͉������Ȃ�



        if (Input.GetKeyDown(KeyCode.Space))//��X�����ɐڒn�����ǉ�
        {
            Jump();
        }
    }
    void Jump()
    {
        
        
        float jumpCalledTime = Runner.SimulationTime;

        JumpLocally(jumpCalledTime);

        
            // RPC���M
        RPC_RequestJump(jumpCalledTime);
        

    }


    void JumpLocally(float calledTime)
    { 
        Debug.Log($"Jump Locally. {Runner.Tick} SimuTime: {Runner.SimulationTime}");
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_RequestJump(float calledTime, RpcInfo info=default)
    {
        // RPC���M�i�����M�j
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
