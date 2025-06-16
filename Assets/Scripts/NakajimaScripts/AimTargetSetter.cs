using Fusion;
using RootMotion.FinalIK;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations;

public class AimTargetSetter : NetworkBehaviour
{
    public AimIK aimIK;
    public AimController aimController;


    GameObject targetObj;

    void Start()
    {
        Debug.Log("AimTargetSetter LookTarget");
        targetObj = GameObject.FindWithTag("LookTarget");
        if (targetObj != null && HasInputAuthority)
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

    //void Update()
    //{
    //    changeTargetTransform(playerAvatar.normalizedInputDirection, playerAvatar.tpsCameraTransform.forward);
    //}

    //public void changeTargetTransform(Vector3 normalizedInputDir, Vector3 lookForwardDir)
    //{

    //    //if (playerAvatar.isFollowingCameraForward)
    //    //{
    //        //�f�o�b�O���O
    //        Vector3 bodyForward = new Vector3(lookForwardDir.x, 0f, lookForwardDir.z).normalized;
    //        // ���[�J���v���C���[�̈ړ�����


    //        if (bodyForward.sqrMagnitude > 0.0001f)
    //        {
    //            // �v���C���[�{�̂̌������J���������ɉ�]
    //            targetObj.transform.forward = bodyForward;
    //        }

    //    //}
    //}
    //playerAvatar.normalizedInputDirection;
    //playerAvatar.tpsCameraTransform.forward
}
