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
            Debug.Log("LookTarget が見つかりました。");
            aimIK.solver.target = targetObj.transform;
            aimController.target = targetObj.transform; // AimController のターゲットも設定
        }
        else
        {
            Debug.Log("LookTarget が見つかりませんでした。");
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
    //        //デバッグログ
    //        Vector3 bodyForward = new Vector3(lookForwardDir.x, 0f, lookForwardDir.z).normalized;
    //        // ローカルプレイヤーの移動処理


    //        if (bodyForward.sqrMagnitude > 0.0001f)
    //        {
    //            // プレイヤー本体の向きをカメラ方向に回転
    //            targetObj.transform.forward = bodyForward;
    //        }

    //    //}
    //}
    //playerAvatar.normalizedInputDirection;
    //playerAvatar.tpsCameraTransform.forward
}
