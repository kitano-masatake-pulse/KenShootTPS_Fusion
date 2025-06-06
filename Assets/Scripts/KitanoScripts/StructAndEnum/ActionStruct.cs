using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//アクションとそれが行われた時間(Runner.simulationTimeベース)
public struct ActionStruct
{
   public ActionType actionType;
   public float actionCalledTimeOnSimulationTime; //アクションが行われた時間(Runner.simulationTimeベース)
}

public enum ActionType
{
    None,
    Jump,
    Land,

    Dead,

    Fire_Sword,
    FireStart_AssaultRifle,
    FireEnd_AssaultRifle,
    Fire_SemiAutoRifle,
    Fire_Grenade,

    Reload_Sword,
    Reload_AssaultRifle,
    Reload_SemiAutoRifle,
    Reload_Grenade,

    ChangeWeaponTo_Sword,
    ChangeWeaponTo_AssaultRifle,
    ChangeWeaponTo_SemiAutoRifle,
    ChangeWeaponTo_Grenade,

}
