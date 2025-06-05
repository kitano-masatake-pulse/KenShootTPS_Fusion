using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

public class AnimationHandler : NetworkBehaviour
{
    public PlayerAvatar playerAvatar;
    public Animator animator;

    public Vector3 lastplayerPosition;
    public float Horizontal;
    public float Vertical;
    public CharacterController characterController;

    float LastTick = 0;
    bool IsJumping = false;

    public float verticalSpeed = 0f; // ジャンプ時の垂直速度

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"AnimationHandler Start!");
    }


    // Update is called once per frame
    void Update()
    {
        if (Runner.Simulation.Tick != LastTick) // 2 Tickごとにアニメーションを更新
        {
            Animation();
        }
        SetAnimationFromAnimationPlayList();
        if (IsJumping == true)
        {
            //JumpAnimationHandler();
        }
    }


    [SerializeField] private Transform modelTransform;
    void Animation()
    {
        Vector3 worldDelta = transform.position - lastplayerPosition;
        Vector3 localDelta = modelTransform.InverseTransformDirection(worldDelta);

        Horizontal = localDelta.x * 1000;
        Vertical = localDelta.z * 1000;
        animator.SetFloat("Horizontal", Horizontal, 0.001f, Time.deltaTime);
        animator.SetFloat("Vertical", Vertical, 0.001f, Time.deltaTime);
        lastplayerPosition = transform.position; // 前回の位置を保存
        if (!HasInputAuthority)
        {
            Debug.Log($"Animation called. Horizontal: {Horizontal}, Vertical: {Vertical} ");
        }
        LastTick = Runner.Simulation.Tick; // 前回のTickを保存
    }

    public void SetAnimationFromAnimationPlayList()
    {
        foreach (var action in playerAvatar.ActionAnimationPlayList)
        {

            switch (action.actionType)
            {
                case ActionType.Jump:
                    Debug.Log($"IsJumping True");
                    animator.SetBool("IsJumping", true);
                    IsJumping = true;
                    break;
            }
            Debug.Log($"actionType: {action.actionType}, actionCalledTimeOnSimulationTime: {action.actionCalledTimeOnSimulationTime}");
        }
        playerAvatar.ClearActionAnimationPlayList();
    }
}
