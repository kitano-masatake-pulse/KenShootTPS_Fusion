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


    [SerializeField] private float directHitRadius = 1f;// 直撃判定の半径
    [SerializeField] private float blastHitRadius = 5f; // 爆風の半径

    [SerializeField] private float minBlastDamage = 20f; // 爆風の最小ダメージ


    [SerializeField] private float damageDuration = 1f; // 爆発時間(当たり時間)
    [SerializeField] private float explosionDelay = 3f; // 爆発までの遅延時間

    [SerializeField] private Transform explosionCenter; // 爆発の中心位置
                                                        // Start is called before the first frame update

    // グレネードのプレハブ
    public GameObject grenadePrefab;
    // 投げる位置
    public Transform throwPoint;
    // 軌跡描画のスクリプト
    public TrajectoryDrawer trajectoryDrawer;
    // アニメータ―型変数
    public Animator animator;

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
    void Update()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(1);

        // 投擲準備完了モーションの間LineRendererを表示
        // ボタン離した後でも、アニメーションは少し続くので、フラグ判定挿入
        if (animator.GetCurrentAnimatorStateInfo(1).IsName("Prepare to throw loop"))
        {
            Debug.Log("Throwing grenade Loop...");
            //　投げる向きを取得する
            throwDirection = throwPoint.forward;
            //　投げる力の大きさを計算する
            velocity = throwDirection * throwForce;

            // 関数呼び出し（投げる位置,投げる向きと大きさ）
            trajectoryDrawer.RaycastDrawTrajectory(throwPoint.position, velocity);
        }

        if (animator.GetCurrentAnimatorStateInfo(1).IsName("Throw"))
        {
            if (!hasEnterThrowOnce)
            {
                // グレネードを投げる
                ThrowGrenade(velocity);
                // 軌跡を非表示にする
                trajectoryDrawer.HideTrajectory();
                hasEnterThrowOnce = true;
            }
        }
        else
        {
            hasEnterThrowOnce = false;
        }



    }

    void ThrowGrenade(Vector3 velocity)
    {
        // グレネードのインスタンスを生成
        GameObject grenade = Instantiate(grenadePrefab, throwPoint.position, Quaternion.identity);
        // グレネードのRigidbodyコンポーネントを取得し、投げる力を設定
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.velocity = velocity;
    }

    // 投擲処理RPC(同期処理)

}
