using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssaultRifle : WeaponBase
{

    protected override WeaponType weapon => WeaponType.AssaultRifle; // 武器の種類を指定

    [SerializeField] private Transform muzzleTransform; // 銃口(=Raycastの光源)の位置を指定するTransform



    [SerializeField] private LayerMask playerLayer;
    public override LayerMask PlayerLayer => playerLayer;

    [SerializeField] private LayerMask obstructionLayer;
    public override LayerMask ObstructionLayer => obstructionLayer;








    protected override void OnEmptyAmmo()
    {
        Debug.Log("カチッ（弾切れSE）");
    }



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
        GunRaycast(muzzleTransform.position, muzzleTransform.forward);

    }


    public override void Fire()
    { 
        base.Fire();
        GunRaycast(muzzleTransform.position, muzzleTransform.forward);
        // ここにアサルトライフル特有の発射処理を追加することができます
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

        Debug.DrawRay(
            origin,
            direction * fireDistance,
            Color.red, 1f);


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
}
