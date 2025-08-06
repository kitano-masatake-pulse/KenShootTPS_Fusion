using System.Collections;
using System.Collections.Generic;
using Fusion;
using RootMotion.FinalIK;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static RootMotion.FinalIK.IKSolverVR;


public class AnimationHandler : NetworkBehaviour
{
    [Header("References")]
    public PlayerAvatar playerAvatar;
    public Animator animator;
    public CharacterController characterController;
    [SerializeField] private Transform modelTransform;
    [Header("Animation Settings")]
    private const float MoveDeltaAmplify = 100f;      // 移動差分が小さくなりすぎないように拡大
    private const float DampTime = 0.001f;             // アニメーション補間に使う dampTime
    [Header("Animation State")]
    private Vector3 lastPlayerPosition;
    private float horizontal;
    private float vertical;
    private float LastTick = 0;
    private float changeTime = 1f; // 武器変更のアニメーション時間
    [SerializeField] private GameObject sword;
    [SerializeField] private GameObject assaultRifle;
    [SerializeField] private GameObject semiAutoRifle;
    [SerializeField] private GameObject grenade;

    private AimIK aimIK;
    private LimbIK limbIK;
    private AimController aimController;
    private bool isInTargetState;
    private bool wasInTargetState = false;
    private string targetStateName = "Put";
    


    public Transform hip;
    public Transform spine;
    public Transform spine1;
    public Transform spine2;
    public Transform rightShoulder;
    public Transform rightArm;
    public Transform rightHand;

    private void Start()
    {
        aimIK = GetComponentInChildren<AimIK>();
        limbIK = GetComponentInChildren<LimbIK>();
        aimController = GetComponentInChildren<AimController>();
    }
    // Update is called once per frame
    private void Update()
    {
        if (Runner.Simulation.Tick != LastTick)
        {
            MovementAnimation();
            LastTick = Runner.Simulation.Tick;
        }
        SetAnimationFromPlayList();
        isInTargetState = animator.GetCurrentAnimatorStateInfo(1).IsName(targetStateName);
        if (animator.GetCurrentAnimatorStateInfo(1).IsName("PutAway"))
        {
            FinalIKDisable();
        }
        if (animator.GetCurrentAnimatorStateInfo(1).IsName("Put"))
        {
            switch (playerAvatar.CurrentWeapon)
            {
                case WeaponType.Sword:
                    animator.SetBool("EquipSword", true);
                    break;
                case WeaponType.AssaultRifle:
                    animator.SetBool("EquipRifle", true);
                    break;
                case WeaponType.SemiAutoRifle:
                    animator.SetBool("EquipSemiAutoRifle", true);
                    break;
                case WeaponType.Grenade:
                    animator.SetBool("EquipGrenade", true);
                    break;
            }
        }
        if (animator.GetCurrentAnimatorStateInfo(1).IsName("PutBack"))
        {
        }
        if (wasInTargetState && !isInTargetState)//Putが終了したときに呼ばれるフラグ
        {
            HideAllWeapons();
            ShowNextWeapons();
        }
        if (animator.GetCurrentAnimatorStateInfo(1).IsName("IdleAssaultRifle") || animator.GetCurrentAnimatorStateInfo(1).IsName("IdleSemiAuto"))
        {
            RifleFinalIKenable();
        }
        if (animator.GetCurrentAnimatorStateInfo(1).IsName("IdleGrenade"))
        {
            GrenadeFinalIKenable();
        }
        wasInTargetState = isInTargetState;
        if (animator.GetCurrentAnimatorStateInfo(1).IsName("ReloadRifle"))
        {
            FinalIKDisable();
        }
        if (animator.GetCurrentAnimatorStateInfo(1).IsName("Stun"))
        {
            FinalIKDisable();
        }
    }
    private void MovementAnimation()
    {
        Vector3 worldDelta = transform.position - lastPlayerPosition;
        Vector3 localDelta = modelTransform.InverseTransformDirection(worldDelta);
        Vector3.Distance(lastPlayerPosition, transform.position); // 前回の位置と現在の位置の距離を計算
        horizontal = localDelta.x * MoveDeltaAmplify;
        vertical = localDelta.z * MoveDeltaAmplify;
        animator.SetFloat("Horizontal", horizontal, DampTime, Time.deltaTime);
        animator.SetFloat("Vertical", vertical, DampTime, Time.deltaTime);
        lastPlayerPosition = transform.position; // 前回の位置を保存
        if (!HasInputAuthority)
        {
            Debug.Log($"Animation called. Horizontal: {horizontal}, Vertical: {vertical} ");
        }
        LastTick = Runner.Simulation.Tick; // 前回のTickを保存
    }
    private void SetAnimationFromPlayList()
    {
        foreach (var action in playerAvatar.ActionAnimationPlayList)
        {
            switch (action.actionType)
            {
                case ActionType.Jump:
                    Debug.Log($"IsJumping True");
                    animator.SetBool("IsJumping", true);
                    break;
                case ActionType.Land:
                    Debug.Log($"IsJumping False");
                    animator.SetBool("IsJumping", false);
                    break;
                case ActionType.Dead:
                    Debug.Log($"IsDead True");
                    animator.SetBool("IsDead", true);
                    break;
                case ActionType.ADS_On:
                    Debug.Log($"IsADS True");
                    animator.SetBool("IsADS", true);
                    break;
                case ActionType.ADS_Off:
                    Debug.Log($"IsADS False");
                    animator.SetBool("IsADS", false);
                    break;
                case ActionType.Fire_Sword:
                    Debug.Log($"IsSwordAttack True");
                    animator.SetTrigger("IsSwordAttack");
                    break;
                case ActionType.FireStart_AssaultRifle:
                    Debug.Log($"IsRifleFire True");
                    animator.SetTrigger("IsRifleFire");
                    break;
                case ActionType.FireEnd_AssaultRifle:
                    break;
                case ActionType.Fire_SemiAutoRifle:
                    Debug.Log($"IsSemiAutoRifleFire True");
                    animator.SetBool("IsSemiAutoRifleFire", true);
                    break;
                case ActionType.FirePrepare_Grenade:
                    Debug.Log($"IsGrenadePreparation True");
                    animator.SetBool("IsGrenadePreparation", true);
                    break;
                case ActionType.FireThrow_Grenade:
                    Debug.Log($"IsGrenadePreparation False");
                    animator.SetBool("IsGrenadePreparation", false);
                    break;
                case ActionType.Reload_Sword:
                    //使っていない
                    break;
                case ActionType.Reload_AssaultRifle:
                    Debug.Log($"IsReloading True");
                    animator.SetTrigger("IsReload");
                    break;
                case ActionType.Reload_SemiAutoRifle:
                    Debug.Log($"IsReloading True");
                    animator.SetTrigger("IsReload");
                    break;
                case ActionType.Reload_Grenade:
                    //使っていない
                    break;
                case ActionType.ChangeWeaponTo_Sword:
                    Debug.Log($"ChangeWeaponTo_Sword");
                    ChangeWeapon();
                    //animator.SetBool("EquipRifle", true);//後で変える
                    break;
                case ActionType.ChangeWeaponTo_AssaultRifle:
                    Debug.Log($"ChangeWeaponTo_AssaultRifle");
                    ChangeWeapon();
                    //animator.SetBool("EquipRifle", true);
                    break;
                case ActionType.ChangeWeaponTo_SemiAutoRifle:
                    Debug.Log($"ChangeWeaponTo_SemiAutoRifle");
                    ChangeWeapon();
                    //animator.SetBool("EquipRifle", true);//後で変える
                    break;
                case ActionType.ChangeWeaponTo_Grenade:
                    Debug.Log($"ChangeWeaponTo_Grenade");
                    ChangeWeapon();
                    //animator.SetBool("EquipGrenade", true);
                    break;
            }
            //Debug.Log($"actionType: {action.actionType}, actionCalledTimeOnSimulationTime: {action.actionCalledTimeOnSimulationTime}");
        }
        playerAvatar.ClearActionAnimationPlayList();
    }
    private void ChangeWeapon()
    {
        ResetWeaponEquipBools();
        animator.SetTrigger("ChangeWeapons");
    }
    private void HideAllWeapons()
    {
        sword.SetActive(false);
        assaultRifle.SetActive(false);
        semiAutoRifle.SetActive(false);
        grenade.SetActive(false);
    }
    private void ShowNextWeapons()
    {
        switch (playerAvatar.CurrentWeapon)
        {
            case WeaponType.Sword:
                sword.SetActive(true);
                break;
            case WeaponType.AssaultRifle:
                assaultRifle.SetActive(true);
                break;
            case WeaponType.SemiAutoRifle:
                semiAutoRifle.SetActive(true);
                break;
            case WeaponType.Grenade:
                grenade.SetActive(true);
                break;
        }
    }
    void ResetWeaponEquipBools()
    {
        animator.SetBool("EquipRifle", false);
        animator.SetBool("EquipGrenade", false);
        animator.SetBool("EquipSword", false);
        animator.SetBool("EquipSemiAutoRifle", false);
    }
    private void FinalIKDisable()
    {
        aimIK.enabled = false;
        limbIK.enabled = false;
        aimController.enabled = false;
    }
    private void RifleFinalIKenable()
    {
        aimIK.enabled = true;
        limbIK.enabled = true;
        aimController.enabled = true;

        aimIK.solver.bones[0] = new IKSolverAim.Bone(hip, 0.397f);
        aimIK.solver.bones[1] = new IKSolverAim.Bone(spine, 0.4f);
        aimIK.solver.bones[2] = new IKSolverAim.Bone(spine1, 0.4f);
        aimIK.solver.bones[3] = new IKSolverAim.Bone(spine2, 0.4f);
        aimIK.solver.bones[4] = new IKSolverAim.Bone(rightShoulder, 0.4f);
        aimIK.solver.bones[5] = new IKSolverAim.Bone(rightArm, 0.64f);
        aimIK.solver.bones[6] = new IKSolverAim.Bone(rightHand, 1f);
    }

    private void GrenadeFinalIKenable()
    {
        aimIK.enabled = true;
        aimController.enabled = true;

        aimIK.solver.bones[0] = new IKSolverAim.Bone(hip, 0f);
        aimIK.solver.bones[1] = new IKSolverAim.Bone(spine, 0.4f);
        aimIK.solver.bones[2] = new IKSolverAim.Bone(spine1, 0.072f);
        aimIK.solver.bones[3] = new IKSolverAim.Bone(spine2, 0.061f);
        aimIK.solver.bones[4] = new IKSolverAim.Bone(rightShoulder, 0f);
        aimIK.solver.bones[5] = new IKSolverAim.Bone(rightArm, 0f);
        aimIK.solver.bones[6] = new IKSolverAim.Bone(rightHand, 0f);

    }
}