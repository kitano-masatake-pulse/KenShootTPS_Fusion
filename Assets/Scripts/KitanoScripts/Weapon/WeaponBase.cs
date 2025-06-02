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

    public virtual bool CanFire()
    {
        return true;
    }

    //protected abstract void FireRay();



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




    public virtual void TryReload()
    {
        Debug.Log("Reloading started.");
        // Start reloading animation or logic here



    Reload();
    }







    public virtual void Reload()
    {
        
    }

    public virtual void TryChangeWeapon(WeaponType newWeapon)
    {
        if (CanChangeWeapon(newWeapon))
        {
            ChangeWeapon(newWeapon);
        }
        else
        {
            Debug.Log("Cannot change weapon now.");
        }
    }


    public virtual bool CanChangeWeapon(WeaponType newWeapon)
    {
        // Check if the weapon can be changed, e.g., not reloading or firing
        return true;
    }

    public virtual void ChangeWeapon(WeaponType newWeapon)
    {
        Debug.Log($"Changing weapon to {newWeapon.GetName()}");
        // Change the weapon logic here
        weaponType = newWeapon;
        // Update local state or UI if necessary
    }



    protected virtual void OnEmptyAmmo()
    {
        Debug.Log("カチッ（弾切れSE）");
    }
}
