using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SemiAutoRifle : WeaponBase
{

     protected override WeaponType weapon=> WeaponType.SemiAutoRifle; // 武器の種類を指定

    [SerializeField] private Transform muzzleTransform; // 銃口(=Raycastの光源)の位置を指定するTransform

    [SerializeField] private LayerMask playerLayer;
    public override LayerMask PlayerLayer => playerLayer;

    [SerializeField] private LayerMask obstructionLayer;
    public override LayerMask ObstructionLayer => obstructionLayer;

    float rayDrawingDuration = 1 / 60f; // Rayの描画時間(1/60秒)

    public override void FireDown()
    {
        base.FireDown();

        // ここにセミオートライフルの特有の処理を追加
        GunRaycast(muzzleTransform.position, muzzleTransform.forward);
        Debug.Log($"{weaponType.GetName()} fired down in SemiAutoRifle!");
    }


    void GunRaycast(Vector3 origin, Vector3 direction)
    {
        Runner.LagCompensation.Raycast(
              origin,
              direction,
              fireDistance,
              Object.InputAuthority,
              out var hit,
             playerLayer | obstructionLayer, //判定を行うレイヤーを制限する
              HitOptions.IgnoreInputAuthority);

        if (RaycastLinePoolManager.Instance != null)
        {
            Vector3 rayEnd = Vector3.zero;
            
            //rayEnd = origin + direction * fireDistance; // ヒットポイントがない場合は剣の長さまでのRayを描画

            if (hit.Point != null)
            {
                rayEnd = hit.Point; // ヒットしたポイントがある場合はそこまでのRayを描画
            }
            else
            {
                rayEnd = origin + direction * fireDistance;  // ヒットポイントがない場合は剣の長さまでのRayを描画

            }

            RaycastLinePoolManager.Instance.ShowRay(origin, rayEnd, Color.red, rayDrawingDuration);
        }


        Debug.Log("Hit?" + hit.GameObject);
        //着弾処理 
        if (hit.GameObject != null)
        {
            Debug.Log("Hit!" + hit.GameObject);

            //当たった対象がPlayerHitboxを持っていたらダメージ処理
            if (hit.Hitbox is PlayerHitbox playerHitbox)
            {

                CauseDamage(hit, weaponType.Damage());
            }
            else
            {
                Debug.Log("Hit! but not Player");
            }


        }

    }
        // Start is called before the first frame update
        void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
