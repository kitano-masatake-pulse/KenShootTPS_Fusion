using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : WeaponBase
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

    private float rayDrawingDuration=1f; // Rayの描画時間
    bool isAttackActive = false; // 攻撃がアクティブかどうかのフラグ

    //Raycastの方向を計算するための変数
    [SerializeField]float cornRayAngleDeg = 30f; // 円錐形の角度
    [SerializeField]int cornRayNum = 10; // 円錐形の放射状のRayの本数


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }



    public override void FireDown()
    {
        base.FireDown();
        Debug.Log($"{weaponType.GetName()} fired down!");
        // 爆発までの遅延時間を待つ
        StartCoroutine(DamageRoutine());
    }

    public override void FireUp()
    {
        
    }


    //ここから攻撃判定(爆弾に付ける機能)

    private IEnumerator DamageRoutine()
    {
        //ここに爆発まで待つしょり


        float elapsed = 0f;
        List<HitboxRoot> alreadyDamagedPlayers = new List<HitboxRoot>(); // すでにダメージを与えたプレイヤーを記録するリスト(今のフレームでダメージが確定した人も含む)
        isAttackActive = true; // 攻撃判定を有効にする

        // 爆発範囲の描画
        if (OverlapSphereVisualizer.Instance != null)
        {
            OverlapSphereVisualizer.Instance.ShowSphere(explosionCenter.position, blastHitRadius, rayDrawingDuration, "Sword Attack Area", Color.blue); // 攻撃判定の範囲を可視化する
        }
        else
        {
            Debug.LogWarning("OverlapSphereVisualizer.Instance is null! Please ensure it is set up in the scene.");
        }

        while (elapsed < damageDuration)
        {
            CollisionDetection(alreadyDamagedPlayers);

            elapsed += Time.deltaTime;
            yield return null;

        }
        //ダメージ用のリストをクリア
        alreadyDamagedPlayers.Clear();
        isAttackActive = false; // 攻撃判定を無効にする
    }

    void CollisionDetection(List<HitboxRoot> alreadyDamaged)
    {

        var hits = new List<LagCompensatedHit>();
        int hitCount = Runner.LagCompensation.OverlapSphere( // 攻撃判定を行う
            explosionCenter.position,
            blastHitRadius,
            Object.InputAuthority,
            hits,
            playerLayer,
            //HitOptions.IgnoreInputAuthority
            HitOptions.None // HitOptions.Noneを使用して、すべてのヒットを取得する
            );

        



        if (hitCount > 0)
        {
           
            Dictionary< LagCompensatedHit,float> damagedHitsWithDistance = new Dictionary<LagCompensatedHit, float>();//このフレームでダメージを与えるプレイヤーとそのヒット情報
            foreach (var hit in hits)
            {



                // 当たった対象がPlayerHitboxを持っていたら障害物の判定
                if (hit.Hitbox is PlayerHitbox playerHitbox)
                {
                    HitboxRoot targetPlayerRoot = playerHitbox.Root; 

                    if (alreadyDamaged.Contains(targetPlayerRoot)) // 既にダメージを与えたプレイヤーならスキップ
                    {
                        Debug.Log($"Already hit player {targetPlayerRoot}, skipping damage.");
                        continue;
                    }

                    if (TryRaycastCornRadialAndGetDistance(hit,out LagCompensatedHit raycastHit , out float hitDistance))// 円錐形のRaycastを行う
                    {
                        alreadyDamaged.Add(targetPlayerRoot); // ヒットしたプレイヤーのRootを記録
                        damagedHitsWithDistance[hit] = hitDistance; // ダメージを与えたヒット情報、ヒット距離を記録
                    }
                }
                else
                {
                    Debug.Log("Hit but not a PlayerHitbox: " + hit.GameObject);
                }
            }

            foreach (var kv in damagedHitsWithDistance)
            {
                CauseDamage(kv.Key, GetDamageByDistance(kv.Value));
            }

            
            damagedHitsWithDistance.Clear();



        }
        else
        {
            Debug.Log("No hits detected in OverlapSphere!");
        }




    }

    // 円錐形のRaycastを行う
    bool TryRaycastCornRadialAndGetDistance(LagCompensatedHit sphereHit, out LagCompensatedHit raycastHit, out float minHitDistance)
    {
        raycastHit = default; // 初期化
        minHitDistance = float.PositiveInfinity;
        Vector3 explosionDirection = sphereHit.GameObject.transform.position - explosionCenter.position; // 剣の方向を計算(後々、被弾側のspineを参照するようにする)
        List<Vector3> rayDirections = CornRaycastDirections(explosionDirection, cornRayAngleDeg, cornRayNum); // 30度の円錐形の方向に4本のRayを放射状に飛ばす

        foreach (var direction in rayDirections)
        {
            Runner.LagCompensation.Raycast(
           explosionCenter.position,
           direction,
           blastHitRadius,   // 爆風の範囲でレイキャスト
           Object.InputAuthority,
           out LagCompensatedHit hitResult,
           playerLayer | obstructionLayer, // 判定を行うレイヤーを制限する。プレイヤーと障害物のレイヤーを指定
           HitOptions.IgnoreInputAuthority

            );


            //Debug.DrawRay(explosionCenter.position, direction * blastHitRadius, Color.blue, rayDrawingDuration);


            if (RaycastLinePoolManager.Instance != null)
            {
                Vector3 rayEnd = Vector3.zero;
                
                //rayEnd = explosionCenter.position + direction * blastHitRadius; // ヒットポイントがない場合は剣の長さまでのRayを描画


                if (hitResult.Point != null)
                {
                    rayEnd = hitResult.Point; // ヒットしたポイントがある場合はそこまでのRayを描画
                }
                else
                {
                    rayEnd = explosionCenter.position + direction * blastHitRadius; // ヒットポイントがない場合は爆風範囲までのRayを描画

                }

                RaycastLinePoolManager.Instance.ShowRay(explosionCenter.position, rayEnd, Color.blue, rayDrawingDuration);
            }


            //最近接の着弾位置を更新 
            if (hitResult.GameObject != null && ((1 << hitResult.GameObject.layer) & playerLayer) != 0 && hitResult.Distance <minHitDistance)
            {
                raycastHit = hitResult; // ヒットした情報を返す
                minHitDistance = hitResult.Distance; // ヒットした距離を更新

            }


            Debug.Log($"Raycast direction: {direction}");
        }


        if (minHitDistance <= blastHitRadius)
        { 
            return true; // いずれかのRayがヒットした場合はtrueを返す

        }
        else
        {
            Debug.Log("No hits detected!");
            return false; // どのRayもヒットしなかった場合はfalseを返す
        }

        


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
                directions.Add(e.normalized); // 必要なら長さaに揃える
            }


            return directions;
        }

    private int GetDamageByDistance(float distance)
    {

        if (distance <= directHitRadius) return weaponType.Damage();
        if (distance >= blastHitRadius) return 0;
        float t = (distance - directHitRadius) / (blastHitRadius - directHitRadius);
        return Mathf.RoundToInt(Mathf.Lerp(weaponType.Damage(), minBlastDamage, t));
    }

    //ここまで攻撃判定(爆弾に付ける機能)


    //void OnDrawGizmos()
    //{
    //    if (isAttackActive)
    //    {

    //        // Gizmosを使用して攻撃判定の範囲を可視化
    //        Gizmos.color = Color.blue;
    //        Gizmos.DrawWireSphere( explosionCenter.position, blastHitRadius);

    //    }

    //}

}





