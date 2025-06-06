using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Fusion.NetworkCharacterController;
using static UnityEngine.UI.Image;

public class Sword : WeaponBase
{
    protected override WeaponType weapon => WeaponType.Sword; // 武器の種類を指定


    [SerializeField]private LayerMask playerLayer;
    [SerializeField]private LayerMask obstructionMask;
    [SerializeField] Transform swordRoot;//球状の判定の中心位置
    float swordLength = 1f; // 球状の判定の半径
    float timeUntilAttack= 1f; // モーション開始から攻撃判定までの時間
    int attackFrameCount = 1; // 攻撃判定のフレーム数


    //Raycastの方向を計算するための変数
    float cornRayAngleDeg = 30f; // 円錐形の角度
    int cornRayNum = 10; // 円錐形の放射状のRayの本数


    PlayerAvatar myPlayerAvatar;


    

    [SerializeField] private float attackActiveTime = 0.5f; // 当たり判定が出る時間

    
    


    
    private bool isAttackActive = false; // 攻撃判定が出ているかどうか

    private void Start()
    {
        

    }



    // Update is called once per frame
    void Update()
    {

    }


    public override void FireDown()
    {
      
        //base.FireDown();
        StartCoroutine(CollisionDetectionCoroutine()); // 攻撃判定のコルーチンを開始
    }


    IEnumerator CollisionDetectionCoroutine()
    {
        Debug.Log("Start Attack Collision Detection Coroutine!");
        yield return new WaitForSeconds(timeUntilAttack); // モーション開始から攻撃判定までの時間を待つ
        isAttackActive = true; // 攻撃判定を有効にする
        GenerateCollisionDetection(); // 攻撃判定を生成
        Debug.Log("Attack Collision Detection Generated!");
        yield return new WaitForSeconds(1f); // モーション開始から攻撃判定までの時間を待つ
        isAttackActive = false; // 攻撃判定を無効にする


    }


    // 攻撃判定を生成するメソッド
    public void GenerateCollisionDetection()
    {
        var hits = new List<LagCompensatedHit>();
        int hitCount=Runner.LagCompensation.OverlapSphere( // 攻撃判定を行う
            swordRoot.position, 
            swordLength, 
            Object.InputAuthority, 
            hits, 
            playerLayer,
            //HitOptions.IgnoreInputAuthority
            HitOptions.IgnoreInputAuthority // HitOptions.Noneを使用して、すべてのヒットを取得する
            );

        Debug.Log($"OverlapSphere hit count: {hitCount}");

        //bool rayHit=Runner.LagCompensation.Raycast(
        //                swordRoot.position,
        //                transform.forward,
        //                swordLength * 5,   // 剣の長さの5倍の距離でレイキャスト
        //                Object.InputAuthority,
        //                out LagCompensatedHit hitRes,
        //                playerLayer | obstructionMask, // 判定を行うレイヤーを制限する。プレイヤーと障害物のレイヤーを指定
        //                HitOptions.IgnoreInputAuthority

        //            );

        //Debug.Log($"Raycast hit: {rayHit}");


        if (hitCount > 0)
        {

            Dictionary<PlayerRef,LagCompensatedHit> damagedPlayerWithHit= new Dictionary<PlayerRef, LagCompensatedHit>();

            Debug.Log($"Detected {hitCount} hits!");
            foreach (var hit in hits)
            {



                // 当たった対象がPlayerHitboxを持っていたら障害物の判定
                if (hit.Hitbox is PlayerHitbox playerHitbox)
                {
                    PlayerRef targetPlayerRef = playerHitbox.hitPlayerRef;

                    if(damagedPlayerWithHit.ContainsKey(targetPlayerRef)) // 既にダメージを与えたプレイヤーならスキップ
                    {
                        Debug.Log($"Already hit player {targetPlayerRef}, skipping damage.");
                        continue;
                    }

                    if (TryRaycastCornRadial(hit))// 円錐形のRaycastを行う
                    {

                        damagedPlayerWithHit[targetPlayerRef]= hit; // ダメージを与えたプレイヤーとヒット情報を記録
                    }
                }
                else
                {
                    Debug.Log("Hit but not a PlayerHitbox: " + hit.GameObject);
                }
            }

            // damagedPlayerWithHitに含まれるプレイヤーにダメージを与える
            Debug.Log($"Damaged players count: {damagedPlayerWithHit.Count}");
            Debug.Log($"damagedPlayerWithHit: {damagedPlayerWithHit}");

            foreach (var kv　in damagedPlayerWithHit)
            {
                CauseDamage(kv.Value, weaponType.Damage());
            }

        }
        else 
        { 
            Debug.Log("No hits detected!");
        }
    }

    // 円錐形のRaycastを行う
    bool TryRaycastCornRadial(LagCompensatedHit hit)
    {
        Vector3 swordDirection = hit.GameObject.transform.position - swordRoot.position; // 剣の方向を計算(後々、被弾側のspineを参照するようにする)
        List<Vector3> rayDirections = CornRaycastDirections(swordDirection, cornRayAngleDeg, cornRayNum); // 30度の円錐形の方向に4本のRayを放射状に飛ばす

        foreach (var direction in rayDirections)
        {
            Runner.LagCompensation.Raycast(
           swordRoot.position,
           direction,
           swordLength * 5,   // 剣の長さの5倍の距離でレイキャスト
           Object.InputAuthority,
           out LagCompensatedHit hitResult,
           playerLayer | obstructionMask, // 判定を行うレイヤーを制限する。プレイヤーと障害物のレイヤーを指定
           HitOptions.IgnoreInputAuthority

            );


            Debug.DrawRay(swordRoot.position, direction * swordLength * 5, Color.red, 1f);


            // レイキャストの結果を確認,何回も刺さないように注意
            //着弾処理 
            if (hit.GameObject != null)
            {
                Debug.Log("Hit!" + hit.GameObject);

                

                return true; // ヒットした場合はtrueを返す


            }


            Debug.Log($"Raycast direction: {direction}");
        }

        return false; // どのRayもヒットしなかった場合はfalseを返す


    }

    //対象のいる方向から、飛ばすRaycastの方向を計算するメソッド
    List<Vector3> CornRaycastDirections(Vector3 axisDirection, float cornAngleDeg, int radialRayNum) // 方向と放射状のRayの本数(引数の方向にも飛ばすので、合計で[radialRayNum+1]本になる)
    { 
        List<Vector3> directions = new List<Vector3>();


        // 軸ベクトルを正規化
        Vector3 axis = axisDirection.normalized;

        // 円錐の開き角の半分（ラジアンに変換）
        float theta = cornAngleDeg * 0.5f * Mathf.Deg2Rad;

        // 軸と垂直なベクトルを作る（任意でOK）
        Vector3 ortho = Vector3.Cross(axis, Vector3.up); // 外積を計算している
        if (ortho == Vector3.zero) ortho = Vector3.Cross(axis, Vector3.right);
        ortho.Normalize();

        // 円錐の表面上の1点（軸から角度thetaに回転）
        Quaternion tilt = Quaternion.AngleAxis(theta * Mathf.Rad2Deg, Vector3.Cross(axis, ortho));
        Vector3 baseVec = tilt * axis;


        // 最初の1本は軸方向に設定
        directions.Add(axis);
        // 等間隔で回転して[radialRayNum]個作成
        for (int i = 0; i < radialRayNum; i++)
        {
            float angleAroundAxis = (360f / radialRayNum) * i;
            Quaternion rot = Quaternion.AngleAxis(angleAroundAxis, axis);
            Vector3 e = rot * baseVec;
            directions.Add(e.normalized ); // 必要なら長さaに揃える
        }


        return directions;
    }

    void OnDrawGizmos()
    {
        if (isAttackActive)
        {

            // Gizmosを使用して攻撃判定の範囲を可視化
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(swordRoot.position, swordLength);
   
        }
        
    }




}
