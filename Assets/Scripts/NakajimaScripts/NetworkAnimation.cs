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

    public Vector3 inputDirection;

    NetworkInputManager networkInputManager;


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!HasInputAuthority) return;
        // ローカルだった
        inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        animator.SetFloat("Horizontal", inputDirection.x);
        animator.SetFloat("Vertical", inputDirection.z);
        transform.position += inputDirection * moveSpeed * Time.deltaTime;
    }

    public override void Spawned()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        networkInputManager = FindObjectOfType<NetworkInputManager>();
        if (networkInputManager == null)
        {
            Debug.LogError("NetworkInputManager がシーンに存在しません！");
        }
        if (HasInputAuthority)
        {
            networkInputManager.networkAnimation = this;
            networkInputManager.isMyAvatarAttached = true;
        }
    }
    public override void FixedUpdateNetwork()
    {
        Debug.Log($"FixedUpdateNetwork");
        if (HasStateAuthority)
        {
            Horizontal = inputDirection.x;
            Vertical = inputDirection.z;
            Debug.Log($"HasStateAuthority: {Horizontal}{Vertical}");
        }
        if (GetInput(out NetworkInputData data))
        {
            if (!HasInputAuthority)
            {
                //velocity.y += gravity * Runner.DeltaTime;
                // 坂道対応：Moveは自動で地形の傾斜に合わせてくれる
                transform.position = data.transform;
                //Debug.Log($"FixedUpdateNetwork: {data.transform.x} {data.transform.y} {data.transform.z}");
                //characterController.Move((data.wasdInputDirection * moveSpeed + velocity) * Runner.DeltaTime);
                // 着地しているなら重力リセット
                //if (data.jumpPressed && characterController.isGrounded)
                //{
                //    velocity.y = 3;
                //}
                //NetIsJumping = !characterController.isGrounded;

                //if (data.attackClicked)
                //{
                //    RpcTriggerAttack();
                //}

                //animator.SetBool("IsJumping", NetIsJumping);

            }
        }
        animator.SetFloat("Horizontal", Horizontal);
        animator.SetFloat("Vertical", Vertical);
        Debug.Log($"FixedUpdateNetwork: {Horizontal} {data.transform.y} {Vertical}");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcTriggerAttack()
    {
        animator.SetTrigger("IsPressLeftKey");
        Debug.Log("NetworkAnimation Attack Triggered");
    }
}






