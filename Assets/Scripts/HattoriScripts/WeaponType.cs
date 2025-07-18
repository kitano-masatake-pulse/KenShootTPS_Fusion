using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum WeaponType : byte
{
    Sword = 0,
    AssaultRifle,
    SemiAutoRifle,
    Grenade
}


public static class WeaponTypeExtensions
{
    public static string GetName(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => "Sword",
            WeaponType.AssaultRifle => "AssaultRifle",
            WeaponType.SemiAutoRifle => "SemiAutoRifle",
            WeaponType.Grenade => "Grenade",
            _ => "Unknown"
        };
    }
    public static int MagazineCapacity(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => 0,
            WeaponType.AssaultRifle => 200,
            WeaponType.SemiAutoRifle => 500,
            WeaponType.Grenade => 1,
            _ => 0
        };
    }
    public static int ReserveCapacity(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => 0,
            WeaponType.AssaultRifle => 1000,
            WeaponType.SemiAutoRifle => 1500,
            WeaponType.Grenade => 5,
            _ => 0
        };
    }


    public static int Damage(this WeaponType weaponType)
    { 
        return weaponType switch
        {
            WeaponType.Sword => 50, // 剣のダメージは決まってないので仮に50とする
            WeaponType.AssaultRifle => 5,
            WeaponType.SemiAutoRifle => 20,
            WeaponType.Grenade => 50,
            _ => 0
        };

    }


    //単発撃ち武器であるかどうか(GetMouseButtonDownをつかうかGetMouseBottonをつかうか)
    public  static bool isOneShotWeapon(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => true,
            WeaponType.AssaultRifle => false,
            WeaponType.SemiAutoRifle => true,
            WeaponType.Grenade => true,
            _ => false
        };
    }

    public static float FireWaitTime(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => 1f,
            WeaponType.AssaultRifle => 0.1f,
            WeaponType.SemiAutoRifle => 1f,
            WeaponType.Grenade => 1f,
            _ => 0f
        };
    }

    public static float ReloadTime(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => 3f,
            WeaponType.AssaultRifle => 3f,
            WeaponType.SemiAutoRifle => 5f,
            WeaponType.Grenade => 3f,
            _ => 0f
        };
    }

   

    public static float WeaponChangeTime(this WeaponType weaponType)
    {
        return 1f; // 全ての武器で同じ時間を設定
    }


    #region Recoil Methods

    //リコイルの角度(degree)
    public static float RecoilAmount_Pitch(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => 0f, // 剣はリコイルなし
            WeaponType.AssaultRifle => 5f,
            WeaponType.SemiAutoRifle => 30f,
            WeaponType.Grenade => 0.2f,
            _ => 0f
        };
    }

    public static float RecoilAmount_Yaw(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => 0f, // 剣はリコイルなし
            WeaponType.AssaultRifle => 0.5f,
            WeaponType.SemiAutoRifle => 0.3f,
            WeaponType.Grenade => 0.2f,
            _ => 0f
        };
    }




    //リコイルの角速度(degree/second)
    public static float RecoilAngularVelocity_Pitch(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => 0f, // 剣はリコイルなし
            WeaponType.AssaultRifle => 40f,
            WeaponType.SemiAutoRifle => 180f,
            WeaponType.Grenade => 0.2f,
            _ => 0f
        };
    }

    public static float RecoilAngularVelocity_Yaw(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => 0f, // 剣はリコイルなし
            WeaponType.AssaultRifle => 0.5f,
            WeaponType.SemiAutoRifle => 0.3f,
            WeaponType.Grenade => 0.2f,
            _ => 0f
        };
    }





    //リコイルの回復角速度(degree/second)
    public static float RecoverAngularVelocity_Pitch(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => 0f, // 剣はリコイルなし
            WeaponType.AssaultRifle => 30f,
            WeaponType.SemiAutoRifle => 45f,
            WeaponType.Grenade => 0.2f,
            _ => 0f
        };
    }

    public static float RecoverAngularVelocity_Yaw(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => 0f, // 剣はリコイルなし
            WeaponType.AssaultRifle => 20,
            WeaponType.SemiAutoRifle => 0.3f,
            WeaponType.Grenade => 0.2f,
            _ => 0f
        };
    }



    //リコイルの角度制限(degree)
    public static float RecoilLimit_Pitch(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => 0f, // 剣はリコイルなし
            WeaponType.AssaultRifle => 30f,
            WeaponType.SemiAutoRifle => 30f,
            WeaponType.Grenade => 0.2f,
            _ => 0f
        };
    }


    #endregion



    #region Return ActionType Methods


    //actionTypeを返す
    public static ActionType FireDownAction(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => ActionType.Fire_Sword,
            WeaponType.AssaultRifle => ActionType.FireStart_AssaultRifle,
            WeaponType.SemiAutoRifle => ActionType.Fire_SemiAutoRifle,
            WeaponType.Grenade => ActionType.Fire_Grenade,
            _ => ActionType.None
        };


    }

    public static ActionType FireUpAction(this WeaponType weaponType)
    {
        return weaponType switch
        { 
            WeaponType.AssaultRifle => ActionType.FireEnd_AssaultRifle,
     
            _ => ActionType.None
        };


    }

    public static ActionType ReloadAction(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => ActionType.Reload_Sword,
            WeaponType.AssaultRifle => ActionType.Reload_AssaultRifle,
            WeaponType.SemiAutoRifle => ActionType.Reload_SemiAutoRifle,
            WeaponType.Grenade => ActionType.Reload_Grenade,
            _ => ActionType.None
        };
    }

    public static ActionType ChangeWeaponAction(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => ActionType.ChangeWeaponTo_Sword,
            WeaponType.AssaultRifle => ActionType.ChangeWeaponTo_AssaultRifle,
            WeaponType.SemiAutoRifle => ActionType.ChangeWeaponTo_SemiAutoRifle,
            WeaponType.Grenade => ActionType.ChangeWeaponTo_Grenade,
            _ => ActionType.None
        };
    }

    #endregion

}
