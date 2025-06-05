using UnityEngine;
using Fusion;

public abstract class WeaponBase : NetworkBehaviour
{
    //protected WeaponLocalState localState;
    protected abstract WeaponType weapon { get; }

    public WeaponType weaponType => weapon;

    public int CurrentMagazine;
    public int CurrentReserve;


    public virtual void FireDown()
    {
        //FireRay();
        //localState.ConsumeAmmo(weaponType);
        CurrentMagazine--;
        Debug.Log($"{weaponType.GetName()} fired down! Current Magazine: {CurrentMagazine}, Current Reserve: {CurrentReserve}");
    }


    public virtual void Fire()
    {
        //FireRay();
        //localState.ConsumeAmmo(weaponType);

        CurrentMagazine--;
        Debug.Log($"{weaponType.GetName()} fired! Current Magazine: {CurrentMagazine}, Current Reserve: {CurrentReserve}");
    }


    public virtual bool IsMagazineEmpty()
    {
        return CurrentMagazine <= 0;
    }


    public virtual bool IsMagazineFull()
    {
        return CurrentMagazine >=  weaponType.MagazineCapacity() ;
    }



    public virtual void CauseDamage(LagCompensatedHit hit, int weaponDamage)
    {

        //当たった対象がPlayerHitboxを持っていたらダメージ処理
        if (hit.Hitbox is PlayerHitbox playerHitbox)
        {
            PlayerRef targetPlayerRef = playerHitbox.hitPlayerRef;
            PlayerRef myPlayerRef = Object.InputAuthority;
            Debug.Log($"Player {myPlayerRef} hit Player {targetPlayerRef} with {weaponDamage} damage");
            PlayerHP targetHP = playerHitbox.GetComponentInParent<PlayerHP>();
            targetHP.RPC_RequestDamage(myPlayerRef, weaponDamage);
        }
        else

        {
            Debug.Log($"Couldn't Get playerHitbox, but{hit.Hitbox} ");
        }

    }

    protected virtual void OnEmptyAmmo()
    {
        Debug.Log("カチッ（弾切れSE）");
    }
}
