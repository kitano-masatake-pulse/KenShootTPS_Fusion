using System.Collections;
using System.Collections.Generic;
using Fusion;
using RootMotion.FinalIK;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class AnimationHandler : NetworkBehaviour
{
    [Header("References")]
    public PlayerAvatar playerAvatar;
    public Animator animator;
    public CharacterController characterController;
    [SerializeField] private Transform modelTransform;

    [Header("Animation Settings")]
    private const float MoveDeltaAmplify = 100f;      // �ړ��������������Ȃ肷���Ȃ��悤�Ɋg��
    private const float DampTime = 0.001f;             // �A�j���[�V������ԂɎg�� dampTime

    [Header("Animation State")]
    private Vector3 lastPlayerPosition;
    private float horizontal;
    private float vertical;

    private float LastTick = 0;
    private float changeTime = 1f; // ����ύX�̃A�j���[�V��������

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
    private int idleTagHash = Animator.StringToHash("Idle");


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
        if (animator.GetCurrentAnimatorStateInfo(1).IsName("Put")){
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
        if (wasInTargetState && !isInTargetState)//Put���I�������Ƃ��ɌĂ΂��t���O
        {

            HideAllWeapons();
            ShowNextWeapons();

        }

        if (animator.GetCurrentAnimatorStateInfo(1).tagHash == idleTagHash)
        {
            FinalIKenable();
        }

        wasInTargetState = isInTargetState;

        if (animator.GetCurrentAnimatorStateInfo(1).IsName("ReloadRifle"))
        {
            FinalIKDisable();
        }
        if (animator.GetCurrentAnimatorStateInfo(1).IsName("Dead"))
        {
            FinalIKDisable();
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            animator.SetTrigger("IsReload");
        }
    }


    private void MovementAnimation()
    {
        Vector3 worldDelta = transform.position - lastPlayerPosition;
        Vector3 localDelta = modelTransform.InverseTransformDirection(worldDelta);

        Vector3.Distance(lastPlayerPosition, transform.position); // �O��̈ʒu�ƌ��݂̈ʒu�̋������v�Z


        horizontal = localDelta.x * MoveDeltaAmplify;
        vertical = localDelta.z * MoveDeltaAmplify;
        animator.SetFloat("Horizontal", horizontal, DampTime, Time.deltaTime);
        animator.SetFloat("Vertical", vertical, DampTime, Time.deltaTime);
        lastPlayerPosition = transform.position; // �O��̈ʒu��ۑ�
        if (!HasInputAuthority)
        {
            Debug.Log($"Animation called. Horizontal: {horizontal}, Vertical: {vertical} ");
        }
        LastTick = Runner.Simulation.Tick; // �O���Tick��ۑ�
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
                    animator.SetBool("IsRifleFire", true);
                    break;

                case ActionType.FireEnd_AssaultRifle:
                    Debug.Log($"IsRifleFire False");
                    animator.SetBool("IsRifleFire", false);
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
                    //�g���Ă��Ȃ�
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
                    //�g���Ă��Ȃ�
                    break;

                case ActionType.ChangeWeaponTo_Sword:
                    Debug.Log($"ChangeWeaponTo_Sword");
                    ChangeWeapon();
                    //animator.SetBool("EquipRifle", true);//��ŕς���
                    break;

                case ActionType.ChangeWeaponTo_AssaultRifle:
                    Debug.Log($"ChangeWeaponTo_AssaultRifle");
                    ChangeWeapon();
                    //animator.SetBool("EquipRifle", true);
                    break;

                case ActionType.ChangeWeaponTo_SemiAutoRifle:
                    Debug.Log($"ChangeWeaponTo_SemiAutoRifle");
                    ChangeWeapon();
                    //animator.SetBool("EquipRifle", true);//��ŕς���
                    break;

                case ActionType.ChangeWeaponTo_Grenade:
                    Debug.Log($"ChangeWeaponTo_Grenade");
                    ChangeWeapon();
                    //animator.SetBool("EquipGrenade", true);
                    break;
            }
            Debug.Log($"actionType: {action.actionType}, actionCalledTimeOnSimulationTime: {action.actionCalledTimeOnSimulationTime}");
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
    private void FinalIKenable()
    {
        aimIK.enabled = true;
        limbIK.enabled = true;
        aimController.enabled = true;
    }
}
