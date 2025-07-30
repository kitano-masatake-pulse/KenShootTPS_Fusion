using System.Collections;
using System.Collections.Generic;
using Fusion;
using RootMotion.FinalIK;
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

    [SerializeField] private GameObject sword;
    [SerializeField] private GameObject assaultRifle;
    [SerializeField] private GameObject semiAutoRifle;
    [SerializeField] private GameObject grenade;

    private AimIK aimIK;
    private LimbIK limbIK;
    private AimController aimController;

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

        //�ȉ��̓e�X�g
        //if (Input.GetMouseButton(0)) // ���N���b�N�ŃW�����v
        //{
        //    animator.SetBool("IsGrenadePreparation", true);
        //}
        //if (Input.GetMouseButtonUp(0))
        //{
        //    animator.SetBool("IsGrenadePreparation", false);
        //}
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

                case ActionType.ADS_On:
                    Debug.Log($"IsADS True");
                    animator.SetBool("IsADS", true);
                    break;

                case ActionType.ADS_Off:
                    Debug.Log($"IsADS False");
                    animator.SetBool("IsADS", false);
                    break;

                case ActionType.ChangeWeaponTo_Sword:
                    Debug.Log($"ChangeWeaponTo_Sword");
                    animator.SetTrigger("ChangeWeapons");
                    FinalIKDisable();
                    StartCoroutine(CoroutineToEnableFinalIK());
                    HideAllWeapons();
                    sword.SetActive(true);
                    break;

                case ActionType.ChangeWeaponTo_AssaultRifle:
                    Debug.Log($"ChangeWeaponTo_AssaultRifle");
                    animator.SetTrigger("ChangeWeapons");
                    FinalIKDisable();
                    StartCoroutine(CoroutineToEnableFinalIK());
                    HideAllWeapons();
                    assaultRifle.SetActive(true);
                    break;

                case ActionType.ChangeWeaponTo_SemiAutoRifle:
                    Debug.Log($"ChangeWeaponTo_SemiAutoRifle");
                    animator.SetTrigger("ChangeWeapons");
                    FinalIKDisable();
                    StartCoroutine(CoroutineToEnableFinalIK());
                    HideAllWeapons();
                    semiAutoRifle.SetActive(true);
                    break;

                case ActionType.ChangeWeaponTo_Grenade:
                    Debug.Log($"ChangeWeaponTo_Grenade");
                    animator.SetTrigger("ChangeWeapons");
                    FinalIKDisable();
                    StartCoroutine(CoroutineToEnableFinalIK());
                    HideAllWeapons();
                    grenade.SetActive(true);
                    break;
            }
            Debug.Log($"actionType: {action.actionType}, actionCalledTimeOnSimulationTime: {action.actionCalledTimeOnSimulationTime}");
        }
        playerAvatar.ClearActionAnimationPlayList();
    }


    private void HideAllWeapons()
    {
        sword.SetActive(false);
        assaultRifle.SetActive(false);
        semiAutoRifle.SetActive(false);
        grenade.SetActive(false);
    }

    IEnumerator CoroutineToEnableFinalIK()
    {
        yield return new WaitForSeconds(1f); // 1�b�҂�
        FinalIKenable();
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
