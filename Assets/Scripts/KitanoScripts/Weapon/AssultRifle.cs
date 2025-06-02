using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssultRifle : WeaponBase
{




   



    public override void Fire()
    {
        //FireRay();
        //localState.ConsumeAmmo(weaponType);
        Debug.Log($"{weaponType.GetName()} fired! Current Magazine: {CurrentMagazine}, Current Reserve: {CurrentReserve}");
    }






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
}
