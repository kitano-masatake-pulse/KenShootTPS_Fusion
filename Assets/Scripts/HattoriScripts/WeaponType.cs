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
            WeaponType.AssaultRifle => "Assault Rifle",
            WeaponType.SemiAutoRifle => "Semi-Auto Rifle",
            WeaponType.Grenade => "Grenade Launcher",
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
            WeaponType.Grenade => 1,
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
            WeaponType.Grenade => 5,
            _ => 0
        };
    }


    public static int Damage(this WeaponType weaponType)
    { 
        return weaponType switch
        {
            WeaponType.Sword => 50, // ���̃_���[�W�͌��܂��ĂȂ��̂ŉ���50�Ƃ���
            WeaponType.AssaultRifle => 5,
            WeaponType.SemiAutoRifle => 20,
            WeaponType.Grenade => 50,
            _ => 0
        };

    }


    //�P����������ł��邩�ǂ���(GetMouseButtonDown��������GetMouseBotton��������)
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
        return 0f; // �S�Ă̕���œ������Ԃ�ݒ�
    }



    #region Return ActionType Methods


    //actionType��Ԃ�
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
