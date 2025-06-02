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

    public static float ReloadTime(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => 3f,
            WeaponType.AssaultRifle => 1f,
            WeaponType.SemiAutoRifle => 2f,
            WeaponType.GrenadeLauncher => 3f,
            _ => 0f
        };
    }

    public static float FireWaitTime(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => 0.5f,
            WeaponType.AssaultRifle => 0.1f,
            WeaponType.SemiAutoRifle => 0.5f,
            WeaponType.GrenadeLauncher => 1f,
            _ => 0f
        };
    }

}
