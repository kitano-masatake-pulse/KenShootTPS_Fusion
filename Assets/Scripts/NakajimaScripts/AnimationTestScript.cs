using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// ���̃R���|�[�l���g���g��GameObject�ɂ͕K��CharacterController���K�v
[RequireComponent(typeof(CharacterController))]
public class AnimationTestScript : MonoBehaviour
{
    // �v���C���[�̈ړ����x
    public float moveSpeed = 5f;
    // �W�����v���̏������
    public float jumpForce = 10f;
    // �d�͉����x
    public float gravity = 9.81f;
    // �W�����v�����ǂ����̃t���O
    private bool isJumping = false;
    // ���N���b�N��������Ă��邩�ǂ����̃t���O
    private bool isPressLeftKey = false;

    // �L�����N�^�[�R���g���[���[�̎Q��
    private CharacterController controller;
    // �ړ������x�N�g��
    private Vector3 moveDirection;
    // �A�j���[�^�[�̎Q��
    private Animator animator;

    bool grounded;

    void Start()
    {
        // CharacterController�R���|�[�l���g�̎擾
        controller = GetComponent<CharacterController>();
        // Animator�R���|�[�l���g���擾
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // ���́i���FA/D�⁩���L�[�A�c�FW/S�⁪���L�[�j�̎擾
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 input = new Vector3(h, 0, v);
        // ���͂̑傫����1�ȉ��ɐ����i�΂߈ړ��������Ȃ肷����̂�h�~�j
        input = Vector3.ClampMagnitude(input, 1f);

        // ���[���h��Ԃł͂Ȃ��A�I�u�W�F�N�g�̌����Ɋ�Â����ړ��������Z�o
        Vector3 move = transform.TransformDirection(input) * moveSpeed;
        // Y�����̑��x�͈ȑO�̒l��ێ�
        move.y = moveDirection.y;

        // �n�ʂɐڂ��Ă���Ƃ��̏���
        if (grounded)
        {
            // ���n������W�����v�t���O�����Z�b�g
            if (isJumping)
            {
                isJumping = false;
            }

            // �n�ʂɉ����t���邽�߂̉������̗�
            move.y = -1f;



            // �W�����v���́i�X�y�[�X�L�[�j�����o
            if (Input.GetButtonDown("Jump"))
            {
                // ������ɃW�����v�͂�������
                move.y = jumpForce;
                isJumping = true;
            }
        }
        else
        {
            // �󒆂ł͏d�͂�������
            move.y -= gravity * Time.deltaTime;
        }

        

        // �}�E�X���N���b�N��������Ă��邩�m�F
        if (Input.GetMouseButton(0))
        {
            isPressLeftKey = true;
        }
        else
        {
            isPressLeftKey = false;
        }

        // �L�����N�^�[�̈ړ������s
        // controller.Move(move * Time.deltaTime);

        grounded = controller.isGrounded;

        Debug.Log($"controller.isGrounded : {grounded}");
        // ���݂̈ړ�������ۑ�
        moveDirection = move;

        // �A�j���[�V�����p�p�����[�^���X�V
        animator.SetFloat("Horizontal", input.x);           // �������̓���
        animator.SetFloat("Vertical", input.z);             // �c�����̓���
        animator.SetBool("IsJumping", isJumping);           // �W�����v�����ǂ���
        animator.SetFloat("YVelocity", move.y);             // Y�����̑��x
        animator.SetBool("IsPressLeftKey", isPressLeftKey); // ���N���b�N��������Ă��邩
    }
}
