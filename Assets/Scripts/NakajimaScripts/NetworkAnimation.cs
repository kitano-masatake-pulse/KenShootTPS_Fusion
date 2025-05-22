using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class NetworkAnimation : NetworkBehaviour
{
    private float gravity = -9.81f;
    private float moveSpeed = 5f;
    public Vector3 velocity = Vector3.zero;


    CharacterController characterController;
    Animator animator;
    [Networked] public bool NetIsJumping { get; set; }
    [Networked] public float Horizontal { get; set; }
    [Networked] public float Vertical { get; set; }


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Spawned()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }
    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            if (HasStateAuthority)
            {
                velocity.y += gravity * Runner.DeltaTime;
                // 坂道対応：Moveは自動で地形の傾斜に合わせてくれる
                characterController.Move((data.wasdInputDirection * moveSpeed + velocity) * Runner.DeltaTime);
                // 着地しているなら重力リセット
                if (data.jumpPressed && characterController.isGrounded)
                {
                    velocity.y = 3;
                }
                NetIsJumping = !characterController.isGrounded;
                Vector3 test = data.wasdInputDirection.normalized;
                Horizontal = test.x;
                Vertical = test.z;

                if (data.attackClicked)
                {
                    RpcTriggerAttack();
                }


            }
        }
        animator.SetBool("IsJumping", NetIsJumping);
        animator.SetFloat("Horizontal", Horizontal);
        animator.SetFloat("Vertical", Vertical);


        // Debug.Log($"GetInput: {Horizontal} {Vertical}");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcTriggerAttack()
    {
        animator.SetTrigger("IsPressLeftKey");
        Debug.Log("Attack Triggered");
    }
}






