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
        return 1f; // �S�Ă̕���œ������Ԃ�ݒ�
    }


    #region Recoil Methods

    //���R�C���̊p�x(degree)
    public static float RecoilAmount_Pitch(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => 0f, // ���̓��R�C���Ȃ�
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
            WeaponType.Sword => 0f, // ���̓��R�C���Ȃ�
            WeaponType.AssaultRifle => 0.5f,
            WeaponType.SemiAutoRifle => 0.3f,
            WeaponType.Grenade => 0.2f,
            _ => 0f
        };
    }




    //���R�C���̊p���x(degree/second)
    public static float RecoilAngularVelocity_Pitch(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => 0f, // ���̓��R�C���Ȃ�
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
            WeaponType.Sword => 0f, // ���̓��R�C���Ȃ�
            WeaponType.AssaultRifle => 0.5f,
            WeaponType.SemiAutoRifle => 0.3f,
            WeaponType.Grenade => 0.2f,
            _ => 0f
        };
    }





    //���R�C���̉񕜊p���x(degree/second)
    public static float RecoverAngularVelocity_Pitch(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => 0f, // ���̓��R�C���Ȃ�
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
            WeaponType.Sword => 0f, // ���̓��R�C���Ȃ�
            WeaponType.AssaultRifle => 20,
            WeaponType.SemiAutoRifle => 0.3f,
            WeaponType.Grenade => 0.2f,
            _ => 0f
        };
    }



    //���R�C���̊p�x����(degree)
    public static float RecoilLimit_Pitch(this WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => 0f, // ���̓��R�C���Ȃ�
            WeaponType.AssaultRifle => 30f,
            WeaponType.SemiAutoRifle => 30f,
            WeaponType.Grenade => 0.2f,
            _ => 0f
        };
    }


    #endregion



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
