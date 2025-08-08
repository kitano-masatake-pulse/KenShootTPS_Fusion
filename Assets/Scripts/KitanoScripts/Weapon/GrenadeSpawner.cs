using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class GrenadeSpawner : WeaponBase
{
    protected override WeaponType weapon => WeaponType.Grenade; // 武器の種類を指定


    [SerializeField] private LayerMask playerLayer;
    public override LayerMask PlayerLayer => playerLayer;

    [SerializeField] private LayerMask obstructionLayer;
    public override LayerMask ObstructionLayer => obstructionLayer;


    [SerializeField] private Transform explosionCenter; // 爆発の中心位置
                                                        // Start is called before the first frame update

    // グレネードのプレハブ
    [SerializeField] private NetworkObject grenadePrefab;
    // 投げる位置
    [SerializeField] private Transform throwPoint;
    // 軌跡描画のスクリプト
    [SerializeField] private TrajectoryDrawer trajectoryDrawer;
    // アニメータ―型変数
    [SerializeField] private Animator animator;

    // 投げる力の大きさ
    private float throwForce = 10f;
    // 投げるアニメーションの速度
    [SerializeField]
    private float throwAnimSpeed = 1.0f;
    // 投げる向きベクトル
    Vector3 throwDirection;
    // 投げる力の大きさ
    Vector3 velocity;

    // 投擲準備中かどうかのフラグ
    private bool hasEnterThrowOnce;



    public override void CalledOnUpdate(PlayerInputData localInputData, InputBufferStruct inputBuffer, WeaponActionState currentAction)
    {

        if (IsHoldingGrenade(localInputData, inputBuffer, currentAction))
        {
            //投擲予測線
            DisplayTrajectory();
            Debug.Log("Holding grenade...");

        }

        if (CanPrepare(localInputData, inputBuffer, currentAction))
        {
            // 投擲準備を開始
            playerAvatar.PrepareGrenade();
            Debug.Log("Preparing grenade...");
      
        }
        else if (CanThrow(localInputData, inputBuffer, currentAction))
        {
            // 投擲を開始
            playerAvatar.ThrowGrenade();
            Debug.Log("Throwing grenade...");

        }
    }



    bool CanPrepare(PlayerInputData localInputData, InputBufferStruct inputBuffer, WeaponActionState currentAction)
    {
        // 投擲可能な状態かどうかを判定
        bool inputCondition = localInputData.FirePressedStay;
        bool stateCondition = currentAction == WeaponActionState.Idle; // ���݂̃A�N�V�������A�C�h����Ԃł��邱�Ƃ��m�F
        bool bulletCondition = currentMagazine > 0; // グレネードが残っているかどうか
        return inputCondition && stateCondition && bulletCondition;
    }

    //投擲準備が完了しているかどうかを判定する関数
    bool IsHoldingGrenade(PlayerInputData localInputData, InputBufferStruct inputBuffer, WeaponActionState currentAction)
    {
        // グレネードを保持している状態かどうかを判定
        bool inputCondition = localInputData.FirePressedStay; // 投げるボタンが押されている間
        bool stateCondition =
            (currentAction == WeaponActionState.GrenadePreparing) &&
            (animator.GetCurrentAnimatorStateInfo(1).IsName("PrepareToThrowLoop")); // 投擲アニメーション中
        return inputCondition && stateCondition;
    }



    bool CanThrow(PlayerInputData localInputData, InputBufferStruct inputBuffer, WeaponActionState currentAction)
    {
        // 投擲可能な状態かどうかを判定
        bool inputCondition = !(localInputData.FirePressedStay); // 投げるボタンが離されたとき


        bool stateCondition =
            (currentAction == WeaponActionState.GrenadePreparing) &&
            (animator.GetCurrentAnimatorStateInfo(1).IsName("PrepareToThrowLoop")); // 投擲アニメーション中


        return inputCondition && stateCondition;
    }



    

    void DisplayTrajectory()
    {
        // 投げる向きを取得する
        throwDirection = throwPoint.forward;
        // 投げる力の大きさを計算する
        velocity = throwDirection * throwForce;
        // 軌跡を描画
        trajectoryDrawer.RaycastDrawTrajectory(throwPoint.position, velocity);
    }

    private IEnumerator HideTrajectoryCoroutine()
    {
        yield return null; // 1フレーム待つ
        trajectoryDrawer.HideTrajectory();
    }

    public override void FireDown()
    {
       
    }


    public override void FireUp()
    {
        
        // 投げる向きを取得する
        throwDirection = throwPoint.forward;
        // 投げる力の大きさを計算する
        velocity = throwDirection * throwForce;
        ThrowGrenade(throwPoint.position, velocity);
    }

    void ThrowGrenade(Vector3 throwPosition, Vector3 velocity)
    {

        DisplayTrajectory(); // 軌跡を表示
        StartCoroutine(HideTrajectoryCoroutine()); // 軌跡を非表示にする
        RPC_LaunchGrenade(throwPosition, velocity);
        currentMagazine= Mathf.Max(currentMagazine - 1, 0); // グレネードを1つ消費

    }

    // 投擲処理RPC(同期処理)
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_LaunchGrenade(Vector3 launchPosition, Vector3 launchVelocity, RpcInfo  rpcInfo = default)
    { 
        // グレネードのインスタンスを生成
        NetworkObject grenade = Runner.Spawn(
            grenadePrefab,
            throwPoint.position,
            Quaternion.identity,
            inputAuthority: rpcInfo.Source, // 入力権限を持たない場合はPlayerRef.Noneを指定

            // ← ここが OnBeforeSpawned デリゲート
            (runner, spawnedObj) =>
            {
               spawnedObj.GetComponent<GrenadeBomb>().SetThrowPlayer(rpcInfo.Source); 
 
            }
            );

        // グレネードのRigidbodyコンポーネントを取得し、投げる力を設定
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.velocity = launchVelocity;
    }


    public override void ResetOnChangeWeapon()
    {
        trajectoryDrawer.HideTrajectory();

    }

}
