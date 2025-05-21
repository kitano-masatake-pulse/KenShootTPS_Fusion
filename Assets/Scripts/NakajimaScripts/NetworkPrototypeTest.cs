using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class NetworkPrototypeTest : NetworkBehaviour
{
    private float gravity = -9.81f;
    private float moveSpeed = 5f;
    public Vector3 velocity = Vector3.zero;

    CharacterController characterController;
    Animator animator;
    [Networked] public bool NetIsJumping { get; set; }


    //NetworkCharacterControllerPrototype characterController;
    // Start is called before the first frame update
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
                // �⓹�Ή��FMove�͎����Œn�`�̌X�΂ɍ��킹�Ă����
                characterController.Move((data.wasdInputDirection * moveSpeed + velocity) * Runner.DeltaTime);
                // ���n���Ă���Ȃ�d�̓��Z�b�g
                if (data.jumpPressed && characterController.isGrounded)
                {
                    velocity.y = 3;
                }
                NetIsJumping = !characterController.isGrounded;

            }
            Debug.Log($"Jump: {data.jumpPressed} {NetIsJumping}");
            animator.SetBool("IsJumping", NetIsJumping);
        }
        animator.SetBool("IsJumping", NetIsJumping);
        Debug.Log($"!GetInput: {data.jumpPressed} {NetIsJumping}");
    }
}






