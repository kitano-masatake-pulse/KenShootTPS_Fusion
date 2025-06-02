using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{

    bool isReloading = false;
    bool isFiring = false;
    bool isChangingWeapon=false;
    public WeaponType currentWeaponType;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void TryFire()
    { 
    
    
    }

    public void TryFireDown()
    {


    }


    public void TryReload()
    {
        // ÉäÉçÅ[ÉhèàóùÇé¿çs
        Debug.Log("Reloading started.");
    }

    public void TryChangeWeapon(float weaponChangeScroll)
    { 
    
    }


}
