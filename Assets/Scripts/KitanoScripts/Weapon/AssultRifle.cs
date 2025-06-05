using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssultRifle : WeaponBase
{

    protected override WeaponType weapon => WeaponType.AssaultRifle; // 武器の種類を指定












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
