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
    public static int GetMaxMagazineCapacity(this WeaponType weaponType)
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
    public static int GetMaxReserveCapacity(this WeaponType weaponType)
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


    //’P”­Œ‚‚¿•Ší‚Å‚ ‚é‚©‚Ç‚¤‚©(GetMouseButtonDown‚ğ‚Â‚©‚¤‚©GetMouseBotton‚ğ‚Â‚©‚¤‚©)
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
}
