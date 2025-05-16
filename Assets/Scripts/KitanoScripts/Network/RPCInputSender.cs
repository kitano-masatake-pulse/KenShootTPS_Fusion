using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class RPCInputSender : NetworkBehaviour
{

    Vector3 cameraRotForward;

    bool isADSNow = false;

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
    


    private void InputCheck()
    {
        CameraControl();


        if (Input.GetKeyDown(KeyCode.Space))
        {
            //ジャンプ
            Jump();
        }

        //攻撃
        if (Input.GetMouseButtonDown(0))
        {
            //ジャンプ
            Attack();
        }

        //ADSモード切替
        if (Input.GetMouseButtonDown(0))
        {
            if (isADSNow)
            {
                //ADS
                ADSOn();
            }
            else
            {
                //ADS
                ADSOff();
            }
            
        }

        //武器替え


    }

    //美優力にカメラの向きを合わせる
    void CameraControl()
    {
        //カメラの向き
        cameraRotForward = Camera.main.transform.forward;
        //カメラの向きをY軸で回転させる
        cameraRotForward.y = 0;
        //カメラの向きを正規化する
        cameraRotForward.Normalize();
    }


    //ジャンプボタンを押したときの処理
    void Jump()
    {
        if (Object.HasInputAuthority)
        {

            JumpLocally();

        
            // RPC送信
            RPC_RequestJump();
        }

    }


    void JumpLocally()
    { 
        Debug.Log($"Jump Locally. {Runner.Tick} SimuTime: {Runner.SimulationTime}");


    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_RequestJump(RpcInfo info=default)
    {
        // RPC送信（即送信）
        Debug.Log($" {info.Source} Requests Jump. {info.Tick} SimuTime: {Runner.SimulationTime}");
        RPC_ApplyJump(info.Source);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_ApplyJump(PlayerRef  sourcePlayer, RpcInfo info = default)
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


    //攻撃ボタンを押したときの処理
    void Attack()
    {
        if (Object.HasInputAuthority)
        {
            //攻撃
            AttackLocally();
            // RPC送信
            RPC_RequestAttack(cameraRotForward);
        }
    }


    void AttackLocally()
    {
        Debug.Log($"Attack Locally. {Runner.Tick} SimuTime: {Runner.SimulationTime}");
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_RequestAttack(Vector3 cameraRotForward ,RpcInfo info = default)
    {
        // RPC送信（即送信）
        Debug.Log($" {info.Source} Requests Attack. {info.Tick} SimuTime: {Runner.SimulationTime}");
        RPC_ApplyAttack(info.Source);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_ApplyAttack(PlayerRef sourcePlayer, RpcInfo info = default)
    {
        Debug.Log($"LocalPlayer {Runner.LocalPlayer}");
        Debug.Log($"SourcePlayer {sourcePlayer}");
        if (Runner.LocalPlayer != sourcePlayer)
        {
            Debug.Log($" Apply Attack of  {sourcePlayer}. Tick:{info.Tick} SimuTime: {Runner.SimulationTime}");
        }
        else
        {
            Debug.Log($"Don't Apply Attack because I'm source  {sourcePlayer}.  {info.Tick} SimuTime: {Runner.SimulationTime}");
        }
    }


    //ADSモードをONにする
    void ADSOn()
    {
        if (Object.HasInputAuthority)
        {
            isADSNow = true;
            //ADSモード
            ADSOnLocally();
            // RPC送信
            RPC_RequestADSOn();
        }
    }

    void ADSOnLocally()
    {
        Debug.Log($"ADS On Locally. {Runner.Tick} SimuTime: {Runner.SimulationTime}");
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]

    public void RPC_RequestADSOn(RpcInfo info = default)
    {
        // RPC送信（即送信）
        Debug.Log($" {info.Source} Requests ADS On. {info.Tick} SimuTime: {Runner.SimulationTime}");
        RPC_ApplyADSOn(info.Source);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_ApplyADSOn(PlayerRef sourcePlayer, RpcInfo info = default)
    {
        Debug.Log($"LocalPlayer {Runner.LocalPlayer}");
        Debug.Log($"SourcePlayer {sourcePlayer}");
        if (Runner.LocalPlayer != sourcePlayer)
        {
            Debug.Log($" Apply ADS On of  {sourcePlayer}. Tick:{info.Tick} SimuTime: {Runner.SimulationTime}");
        }
        else
        {
            Debug.Log($"Don't Apply ADS On because I'm source  {sourcePlayer}.  {info.Tick} SimuTime: {Runner.SimulationTime}");
        }
    }


    //ADSモードをOFFにする

    void ADSOff()
    {
        if (Object.HasInputAuthority)
        {
            isADSNow = false;
            //ADSモード
            ADSOffLocally();
            // RPC送信
            RPC_RequestADSOff();
        }
    }
    void ADSOffLocally()
    {
        Debug.Log($"ADS Off Locally. {Runner.Tick} SimuTime: {Runner.SimulationTime}");
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_RequestADSOff(RpcInfo info = default)
    {
        // RPC送信（即送信）
        Debug.Log($" {info.Source} Requests ADS Off. {info.Tick} SimuTime: {Runner.SimulationTime}");
        RPC_ApplyADSOff(info.Source);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_ApplyADSOff(PlayerRef sourcePlayer, RpcInfo info = default)
    {
        Debug.Log($"LocalPlayer {Runner.LocalPlayer}");
        Debug.Log($"SourcePlayer {sourcePlayer}");
        if (Runner.LocalPlayer != sourcePlayer)
        {
            Debug.Log($" Apply ADS Off of  {sourcePlayer}. Tick:{info.Tick} SimuTime: {Runner.SimulationTime}");
        }
        else
        {
            Debug.Log($"Don't Apply ADS Off because I'm source  {sourcePlayer}.  {info.Tick} SimuTime: {Runner.SimulationTime}");
        }
    }





    //武器替えボタンを押したときの処理




}
