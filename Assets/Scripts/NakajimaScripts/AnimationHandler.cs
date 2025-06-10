using System.Collections;
using System.Collections.Generic;
using Fusion;
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


    // Update is called once per frame
    private void Update()
    {

        if (Runner.Simulation.Tick != LastTick)
        {
            MovementAnimation();
            LastTick = Runner.Simulation.Tick;
        }
        SetAnimationFromPlayList();
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
            }
            Debug.Log($"actionType: {action.actionType}, actionCalledTimeOnSimulationTime: {action.actionCalledTimeOnSimulationTime}");
        }
        playerAvatar.ClearActionAnimationPlayList();
    }
}
