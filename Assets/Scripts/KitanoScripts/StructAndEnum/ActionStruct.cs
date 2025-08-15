using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//�A�N�V�����Ƃ��ꂪ�s��ꂽ����(Runner.simulationTime�x�[�X)
public struct ActionStruct
{
   public ActionType actionType;
   public float actionCalledTimeOnSimulationTime; //�A�N�V�������s��ꂽ����(Runner.simulationTime�x�[�X)
}

public enum ActionType
{
    None,
    Jump,
    Land,

    Dead,
    Respawn,

    ADS_On,
    ADS_Off,

    Fire_Sword,
    FireStart_AssaultRifle,
    FireEnd_AssaultRifle,
    Fire_SemiAutoRifle,

    FirePrepare_Grenade,
    FireThrow_Grenade,

    Reload_Sword,
    Reload_AssaultRifle,
    Reload_SemiAutoRifle,
    Reload_Grenade,

    ChangeWeaponTo_Sword,
    ChangeWeaponTo_AssaultRifle,
    ChangeWeaponTo_SemiAutoRifle,
    ChangeWeaponTo_Grenade,

}
