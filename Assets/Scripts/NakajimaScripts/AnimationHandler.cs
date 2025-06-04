using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class AnimationHandler : NetworkBehaviour
{
    public PlayerAvatar playerAvatar;
    public Animator animator;

    public Vector3 lastplayerPosition;
    public float Horizontal;
    public float Vertical;

    float LastTick = 0;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"AnimationHandler Start!");
    }


    // Update is called once per frame
    void Update()
    {
        SetAnimationFromAnimationPlayList();
        foreach (var action in playerAvatar.ActionAnimationPlayList)
        {
            Debug.Log($"actionType: {action.actionType}, actionCalledTimeOnSimulationTime: {action.actionCalledTimeOnSimulationTime}");
        }
        if (Runner.Simulation.Tick != LastTick) // 2 Tick���ƂɃA�j���[�V�������X�V
        {
            Animation();
        }
    }

    void Animation()
    {
        Horizontal = (transform.position.x - lastplayerPosition.x) * 1000;
        Vertical = (transform.position.z - lastplayerPosition.z) * 1000;
        animator.SetFloat("Horizontal", Horizontal, 0.001f, Time.deltaTime);
        animator.SetFloat("Vertical", Vertical, 0.001f, Time.deltaTime);
        lastplayerPosition = transform.position; // �O��̈ʒu��ۑ�
        if (!HasInputAuthority)
        {
            Debug.Log($"Animation called. Horizontal: {Horizontal}, Vertical: {Vertical} ");

        }
        LastTick = Runner.Simulation.Tick; // �O���Tick��ۑ�
    }

    public void SetAnimationFromAnimationPlayList()
    {
        foreach (var action in playerAvatar.ActionAnimationPlayList)
        {

            switch (action.actionType)
            {
                case ActionType.Jump:
                Debug.Log($"JUMP!!!!");
                animator.SetTrigger("Jump");
                    break;
            }
            Debug.Log($"actionType: {action.actionType}, actionCalledTimeOnSimulationTime: {action.actionCalledTimeOnSimulationTime}");
        }
        playerAvatar.ClearActionAnimationPlayList();
    }
}
