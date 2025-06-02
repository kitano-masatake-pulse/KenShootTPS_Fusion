using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    //protected WeaponLocalState localState;
    public WeaponType weaponType;

    public int CurrentMagazine;
    public int CurrentReserve;
    


    public virtual void Fire()
    {
        //FireRay();
        //localState.ConsumeAmmo(weaponType);
        Debug.Log($"{weaponType.GetName()} fired! Current Magazine: {CurrentMagazine}, Current Reserve: {CurrentReserve}");
    }


    protected virtual void OnEmptyAmmo()
    {
        Debug.Log("カチッ（弾切れSE）");
    }
}
