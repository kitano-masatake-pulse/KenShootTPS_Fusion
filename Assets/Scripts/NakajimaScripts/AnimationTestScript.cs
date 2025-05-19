using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class AnimationTestScript : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float gravity = 9.81f;
    private bool isJumping = false;
    private bool isPressLeftKey = false;

    private CharacterController controller;
    private Vector3 moveDirection;
    private Animator animator;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }


    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 input = new Vector3(h, 0, v);
        input = Vector3.ClampMagnitude(input, 1f);

        Vector3 move = transform.TransformDirection(input) * moveSpeed;
        move.y = moveDirection.y;

        if (controller.isGrounded)
        {
            if (isJumping)
            {
                isJumping = false;
            }

            move.y = -1f;

            if (Input.GetButtonDown("Jump"))
            {
                move.y = jumpForce;
                isJumping = true;
            }
        }
        else
        {
            move.y -= gravity * Time.deltaTime;
        }

        if (Input.GetMouseButton(0))
        {
            isPressLeftKey = true;
        }
        else
        {
            isPressLeftKey = false;
        }

        controller.Move(move * Time.deltaTime);
        moveDirection = move;

        // Animator çXêV
        animator.SetFloat("Horizontal", input.x);
        animator.SetFloat("Vertical", input.z);
        animator.SetBool("IsJumping", isJumping);
        animator.SetFloat("YVelocity", move.y);
        animator.SetBool("IsPressLeftKey", isPressLeftKey);
    }

}
