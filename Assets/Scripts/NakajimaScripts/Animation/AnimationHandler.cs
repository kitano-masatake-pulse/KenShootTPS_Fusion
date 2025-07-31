using System.Collections;
using System.Collections.Generic;
using Fusion;
using RootMotion.FinalIK;
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
    private Coroutine changeWeaponCoroutine;
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
                    animator.SetBool("EquipRifle", true);
                    break;
                case WeaponType.AssaultRifle:
                    animator.SetBool("EquipRifle", true);
                    break;
                case WeaponType.SemiAutoRifle:
                    animator.SetBool("EquipRifle", true);
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

        if (animator.GetCurrentAnimatorStateInfo(1).tagHash == idleTagHash)
        {
            FinalIKenable();
        }

        wasInTargetState = isInTargetState;



        //以下はテスト。グレネードを投げる
        //if (Input.GetMouseButton(0)) // 左クリックでジャンプ
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
            Debug.Log($"actionType: {action.actionType}, actionCalledTimeOnSimulationTime: {action.actionCalledTimeOnSimulationTime}");
        }
        playerAvatar.ClearActionAnimationPlayList();
    }




    private void ChangeWeapon()
    {
        ResetWeaponEquipBools();
        //if (changeWeaponCoroutine != null)
        //{
        //    StopCoroutine(changeWeaponCoroutine);
        //    ResetWeaponEquipBools();
        //    changeWeaponCoroutine = null;
        //}
        animator.SetTrigger("ChangeWeapons");
        animator.SetBool("IsADS", false);
        //changeWeaponCoroutine = StartCoroutine(ChangeWeaponCoroutine());
    }

    //IEnumerator ChangeWeaponCoroutine()
    //{
    //    animator.SetTrigger("ChangeWeapons");
    //    animator.SetBool("IsADS", false);
    //    //FinalIKDisable();
    //    yield return new WaitForSeconds(changeTime/2);
    //    //HideAllWeapons();
    //    //ShowNextWeapons();
    //    yield return new WaitForSeconds(0.7f);
    //    ResetWeaponEquipBools();
    //    FinalIKenable();
    //    changeWeaponCoroutine = null;
    //    //マジックナンバーにしない
    //}

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
