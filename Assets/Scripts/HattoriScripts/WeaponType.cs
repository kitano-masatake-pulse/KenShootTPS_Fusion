using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum WeaponType : byte
{
    Sword = 0,
    AssaultRifle,
    SemiAutoRifle,
    GrenadeLauncher
}


public static class WeaponTypeExtensions
{
    public static string GetName(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => "Sword",
            WeaponType.AssaultRifle => "Assault Rifle",
            WeaponType.SemiAutoRifle => "Semi-Auto Rifle",
            WeaponType.GrenadeLauncher => "Grenade Launcher",
            _ => "Unknown"
        };
    }
    public static int MagazineCapacity(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => 0,
            WeaponType.AssaultRifle => 20,
            WeaponType.SemiAutoRifle => 5,
            WeaponType.GrenadeLauncher => 1,
            _ => 0
        };
    }
    public static int ReserveCapacity(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => 0,
            WeaponType.AssaultRifle => 100,
            WeaponType.SemiAutoRifle => 15,
            WeaponType.GrenadeLauncher => 5,
            _ => 0
        };
    }


    //’P”­Œ‚‚¿•Ší‚Å‚ ‚é‚©‚Ç‚¤‚©(GetMouseButtonDown‚ð‚Â‚©‚¤‚©GetMouseBotton‚ð‚Â‚©‚¤‚©)
    public  static bool isOneShotWeapon(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => true,
            WeaponType.AssaultRifle => false,
            WeaponType.SemiAutoRifle => true,
            WeaponType.GrenadeLauncher => true,
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
            WeaponType.GrenadeLauncher => 1f,
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
            WeaponType.GrenadeLauncher => 3f,
            _ => 0f
        };
    }

   

    public static float WeaponChangeTime(this WeaponType weaponType)
    {
        return 0f; // ‘S‚Ä‚Ì•Ší‚Å“¯‚¶ŽžŠÔ‚ðÝ’è
    }



    #region Return ActionType Methods


    //actionType‚ð•Ô‚·
    public static ActionType FireDownAction(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => ActionType.Fire_Sword,
            WeaponType.AssaultRifle => ActionType.FireStart_AssaultRifle,
            WeaponType.SemiAutoRifle => ActionType.Fire_SemiAutoRifle,
            WeaponType.GrenadeLauncher => ActionType.Fire_Grenade,
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
            WeaponType.GrenadeLauncher => ActionType.Reload_Grenade,
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
            WeaponType.GrenadeLauncher => ActionType.ChangeWeaponTo_Grenade,
            _ => ActionType.None
        };
    }

    #endregion

}
