using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class Sword : WeaponBase
{
    [SerializeField]private LayerMask playerLayer;
    [SerializeField]private LayerMask obstructionMask;
    Transform myHeadTransform;
    Transform myPlayerTransform;
    PlayerAvatar myPlayerAvatar;


    

    [SerializeField] private float attackActiveTime = 0.5f; // 当たり判定が出る時間

    


    
    private bool isAttackActive = false; // 攻撃判定が出ているかどうか

    private void Start()
    {
        myPlayerAvatar= GetComponentInParent<PlayerAvatar>();
        myHeadTransform = myPlayerAvatar.headObject.transform;

    }



    // Update is called once per frame
    void Update()
    {

    }


    public override void FireDown()
    {
      
        base.FireDown();
            StartAttack(); // 攻撃開始

    }


    //void StartHoming( Transform targetTransform)
    //{
    //    if (targetTransform != null)
    //    {
    //        // 初期方向をターゲットの方向に設定
    //        Vector3 toTarget = (targetTransform.position - transform.position).normalized;

    //        myPlayerTransform.forward = new Vector3( toTarget.x,0f, toTarget.z ).normalized; // プレイヤーの向きをターゲット方向に設定(PlayerAvatarがやったほうがよさそう)

    //        homingMoveDirection = toTarget; // 現在の移動方向を更新
    //        isHoming = true; // ホーミングを開始
    //        StartCoroutine(HomingCoroutine()); // ホーミング時間の管理を開始

    //        myPlayerAvatar.isFollowingCameraForward = false; // カメラの向きに追従しないように設定
    //    }
    //}


    //void Homing( Vector3 origin ,Transform targetTransform )
    //{


    //    Vector3 toTarget = (targetTransform.position - origin);
    //    toTarget.y = 0f; // Y軸の成分をゼロにして水平面上の方向を取得
    //    toTarget.Normalize(); // 正規化して方向ベクトルにする

    //    // 今の移動方向から敵への方向との差を角度で計算
    //    float angleToTarget = Vector3.Angle(homingMoveDirection, toTarget);

    //    // 角度差がある場合は補正
    //    if (angleToTarget > 0f)
    //    {
    //        // 回転角の制限（最大maxTurnAnglePerFrame度）
    //        float t = Mathf.Min(1f, maxTurnAnglePerFrame / angleToTarget);
    //        homingMoveDirection = Vector3.Slerp(homingMoveDirection, toTarget, t);
    //    }



    //}

    ////ホーミング時間の管理
    //IEnumerator HomingCoroutine()
    //{
    //    yield return new WaitForSeconds(homingTime);

    //    EndHoming(); // ホーミング終了処理
    //}


    //void EndHoming()
    //{
    //    isHoming = false; // ホーミングを終了
    //    myPlayerAvatar.isFollowingCameraForward = true; // カメラの向きに追従するように設定

    //}




    public void StartAttack()
    {
       

    }

    //public void EndAttack()
    //{
    //    isAttackActive = false; // 攻撃判定を終了
    //    StartCoroutine(PostAttackDelayCoroutine()); // 攻撃後の硬直時間の管理を開始
    //}




    ////攻撃判定が出る時間の管理
    //IEnumerator AttakingCoroutine()
    //{
    //    yield return new WaitForSeconds(attackActiveTime);
    //    isHoming = false; // ホーミングを終了
    //    EndAttack(); // ホーミング終了処理
    //}


    ////攻撃後の硬直時間の管理
    //IEnumerator PostAttackDelayCoroutine()
    //{
    //    yield return new WaitForSeconds(postAttackDelayTime);
    //    isHoming = false; // ホーミングを終了
    //    EndHoming(); // ホーミング終了処理
    //}

    //public void NotHomingAttack()
    //{
    //    // ホーミングしていない場合の攻撃処理
    //}



    //public  bool TryGetClosestTargetInRange(Vector3 origin, Vector3 forward, float range, float fovAngle, out Transform targetTransform)
    //{


    //    Collider[] hits = Physics.OverlapSphere(origin, range, playerLayer); // プレイヤーのレイヤーマスクを使用して近くの敵を検出
    //    float closestDistance = Mathf.Infinity;
    //    targetTransform = null;
    //    float minDistance = Mathf.Infinity;




    //    foreach (Collider hit in hits)
    //    {
    //        Transform enemyTransform = hit.transform;
    //        Transform enemyHeadTransform = enemyTransform.GetComponent<PlayerAvatar>().headObject.transform;

    //        Vector3 toEnemy = (enemyTransform.position - origin);
    //        float angle = Vector3.Angle(forward, toEnemy.normalized); // 必要に応じて forward をプレイヤーの forward に変更
    //        float distance = toEnemy.magnitude;



    //        if (angle > fovAngle / 2f || distance > range)
    //        {
    //            continue;
    //        }


    //        // Raycast視線チェック
    //        Vector3 from = myHeadTransform.position;
    //        Vector3 to = enemyHeadTransform.position;
    //        Vector3 direction = (to - from).normalized;
    //        float rayDistance = Vector3.Distance(from, to);

    //        if (Physics.Raycast(from, direction, out RaycastHit hitInfo, rayDistance, obstructionMask))
    //        {
    //            // Rayが途中で何かに当たっていたら視線が通っていない
    //            continue;
    //        }

    //        if (distance < closestDistance)
    //        {
    //            closestDistance = distance;
    //            targetTransform = enemyTransform;
    //        }


    //    }

    //    return targetTransform != null;
    //}






}
