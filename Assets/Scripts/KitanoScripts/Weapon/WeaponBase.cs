using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    protected WeaponLocalState localState;
    public WeaponType weaponType;



    public virtual void TryFire()
        {
        if (CanFire())
        {
            Fire();
        }
        else
        {
            Debug.Log("Cannot fire now.");
            OnEmptyAmmo();
        }
    }





    public virtual void Fire()
    {
        if (!CanFire())
        {
            OnEmptyAmmo();
            return;
        }

        //FireRay();
        //localState.ConsumeAmmo(weaponType);
    }

    public virtual bool CanFire()
    {
        return true;
    }

    //protected abstract void FireRay();

    protected virtual void OnEmptyAmmo()
    {
        Debug.Log("カチッ（弾切れSE）");
    }
}
