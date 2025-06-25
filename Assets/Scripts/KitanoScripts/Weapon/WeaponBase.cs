using UnityEngine;
using Fusion;

public abstract class WeaponBase : NetworkBehaviour
{
    //protected WeaponLocalState localState;
    protected abstract WeaponType weapon { get; }

    public WeaponType weaponType => weapon;

    public TPSCameraController playerCamera; // 自分のTPSカメラ
    public float fireDistance = 100f;
    public abstract LayerMask PlayerLayer { get; }
    public abstract LayerMask ObstructionLayer { get; }

    public int currentMagazine;
    public int currentReserve;



    public void InitializeAmmo()
    {
        currentMagazine = weaponType.MagazineCapacity();
        currentReserve = weaponType.ReserveCapacity();
        Debug.Log($"Weapon {weaponType.GetName()} initialized with Magazine: {currentMagazine}, Reserve: {currentReserve}");
    }

    public virtual void FireDown()
    {
        //FireRay();
        //localState.ConsumeAmmo(weaponType);
        if (weaponType!=WeaponType.Sword)
        {
            currentMagazine--;
        }
        
        Debug.Log($"{weaponType.GetName()} fired down! Current Magazine: {currentMagazine}, Current Reserve: {currentReserve}");
    }

    public virtual void Reload()
    {
        int currentMagazine = this.currentMagazine;
        int currentReserve = this.currentReserve;
        int magazineCapacity = weaponType.MagazineCapacity();
        int reloadededAmmo = Mathf.Min(currentReserve, magazineCapacity - currentMagazine); //リロードされる弾薬数

        this.currentMagazine += reloadededAmmo; //マガジンにリロードされた弾薬を追加
        this.currentReserve -= reloadededAmmo; //リザーブからリロードされた弾薬を減らす
    }


    public virtual void Fire()
    {
        //FireRay();
        //localState.ConsumeAmmo(weaponType);

        if (weaponType != WeaponType.Sword)
        {
            currentMagazine--;
        }
        Debug.Log($"{weaponType.GetName()} fired! Current Magazine: {currentMagazine}, Current Reserve: {currentReserve}");
    }

    public virtual void FireUp()
    {
       
        Debug.Log($"{weaponType.GetName()} fired up! Current Magazine: {currentMagazine}, Current Reserve: {currentReserve}");
    }


    public virtual bool IsMagazineEmpty()
    {
        return currentMagazine <= 0;
    }


    public virtual bool IsMagazineFull()
    {
        return currentMagazine >=  weaponType.MagazineCapacity() ;
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

    public virtual void SetADS(bool ADSflag)
    {
       
    }

}
