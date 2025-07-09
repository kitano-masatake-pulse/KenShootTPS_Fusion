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
    private bool isAimingReady;
    private bool hasEnterThrowOnce;
    void Update()
    {

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
            isAimingReady = true;
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
                isAimingReady = false;
            }
        }
        else
        {
            hasEnterThrowOnce = false;
        }

        // マウスの左ボタンが離された時の処理
        if (Input.GetMouseButtonUp(0) && isAimingReady)
        {
            //// Updateメソッド内のDebug.Log行を以下のように変更
            //Debug.Log($"Throwing grenade... isAimingReady: {isAimingReady}");
            //// グレネードを投げる
            //ThrowGrenade(velocity);
            //// 軌跡を非表示にする
            //trajectoryDrawer.HideTrajectory();
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