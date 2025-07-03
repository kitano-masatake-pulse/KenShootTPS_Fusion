using UnityEditor;
using UnityEngine;

// グレネードを投げるためのスプリクト
public class GrenadeThrower : MonoBehaviour
{
    // グレネードのプレハブ
    public GameObject grenadePrefab;
    // 投げる位置
    public Transform throwPoint;
    // 投げる力の大きさ
    private float throwForce = 10f;
    // 軌跡描画のスクリプト
    public TrajectoryDrawer trajectoryDrawer;
    // アニメータ―型変数
    public Animator animator;
    // 投げる向きベクトル
    Vector3 throwDirection;
    // 投げる力の大きさ
    Vector3 velocity;

    // 投擲準備中かどうかのフラグ
    private bool isAiming;
    void Update()
    {
        // マウスの左ボタンが押されたら投擲準備を開始
        if (Input.GetMouseButtonDown(0))
        {
            isAiming = true;
        }

        // マウスの左ボタンが押されている間の処理
        if (Input.GetMouseButton(0) && isAiming)
        {
        }

        // マウスの左ボタンが離された時の処理
        if (Input.GetMouseButtonUp(0) && isAiming)
        {
            Debug.Log("Throwing grenade...");
            // グレネードを投げる
            ThrowGrenade(velocity);
            isAiming = false;
            // 軌跡を非表示にする
            trajectoryDrawer.HideTrajectory();
        }
        // 投擲準備完了モーションの間LineRendererを表示する
        if (animator.GetCurrentAnimatorStateInfo(1).IsName("Prepare to throw loop") && isAiming)
        {
            Debug.Log("Throwing grenade Loop...");
            //　投げる向きを取得する
            throwDirection = throwPoint.forward;
            //　投げる力の大きさを計算する
            velocity = throwDirection * throwForce;

            // 関数呼び出し（投げる位置,投げる向きと大きさ）
            trajectoryDrawer.RaycastDrawTrajectory(throwPoint.position, velocity);
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

    void OnArmRaised()
    {

    }
    
}