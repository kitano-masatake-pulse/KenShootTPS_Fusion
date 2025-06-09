using RootMotion.FinalIK;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations;

public class AimTargetSetter : MonoBehaviour
{
    public AimIK aimIK;
    public AimController aimController;

    void Start()
    {
        Debug.Log("AimTargetSetter LookTarget");
        GameObject targetObj = GameObject.FindWithTag("LookTarget");
        if (targetObj != null)
        {
            Debug.Log("LookTarget が見つかりました。");
            aimIK.solver.target = targetObj.transform;
            aimController.target = targetObj.transform; // AimController のターゲットも設定
        }
        else
        {
            Debug.Log("LookTarget が見つかりませんでした。");
        }
    }
}
