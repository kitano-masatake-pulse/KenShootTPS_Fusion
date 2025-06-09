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
            Debug.Log("LookTarget ��������܂����B");
            aimIK.solver.target = targetObj.transform;
            aimController.target = targetObj.transform; // AimController �̃^�[�Q�b�g���ݒ�
        }
        else
        {
            Debug.Log("LookTarget ��������܂���ł����B");
        }
    }
}
