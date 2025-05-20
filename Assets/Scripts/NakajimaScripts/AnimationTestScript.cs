using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// このコンポーネントを使うGameObjectには必ずCharacterControllerが必要
[RequireComponent(typeof(CharacterController))]
public class AnimationTestScript : MonoBehaviour
{
    // プレイヤーの移動速度
    public float moveSpeed = 5f;
    // ジャンプ時の上向き力
    public float jumpForce = 10f;
    // 重力加速度
    public float gravity = 9.81f;
    // ジャンプ中かどうかのフラグ
    private bool isJumping = false;
    // 左クリックが押されているかどうかのフラグ
    private bool isPressLeftKey = false;

    // キャラクターコントローラーの参照
    private CharacterController controller;
    // 移動方向ベクトル
    private Vector3 moveDirection;
    // アニメーターの参照
    private Animator animator;

    bool grounded;

    void Start()
    {
        // CharacterControllerコンポーネントの取得
        controller = GetComponent<CharacterController>();
        // Animatorコンポーネントを取得
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 入力（横：A/Dや←→キー、縦：W/Sや↑↓キー）の取得
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 input = new Vector3(h, 0, v);
        // 入力の大きさを1以下に制限（斜め移動が速くなりすぎるのを防止）
        input = Vector3.ClampMagnitude(input, 1f);

        // ワールド空間ではなく、オブジェクトの向きに基づいた移動方向を算出
        Vector3 move = transform.TransformDirection(input) * moveSpeed;
        // Y方向の速度は以前の値を保持
        move.y = moveDirection.y;

        // 地面に接しているときの処理
        if (grounded)
        {
            // 着地したらジャンプフラグをリセット
            if (isJumping)
            {
                isJumping = false;
            }

            // 地面に押し付けるための下方向の力
            move.y = -1f;



            // ジャンプ入力（スペースキー）を検出
            if (Input.GetButtonDown("Jump"))
            {
                // 上方向にジャンプ力を加える
                move.y = jumpForce;
                isJumping = true;
            }
        }
        else
        {
            // 空中では重力を加える
            move.y -= gravity * Time.deltaTime;
        }

        

        // マウス左クリックが押されているか確認
        if (Input.GetMouseButton(0))
        {
            isPressLeftKey = true;
        }
        else
        {
            isPressLeftKey = false;
        }

        // キャラクターの移動を実行
        // controller.Move(move * Time.deltaTime);

        grounded = controller.isGrounded;

        Debug.Log($"controller.isGrounded : {grounded}");
        // 現在の移動方向を保存
        moveDirection = move;

        // アニメーション用パラメータを更新
        animator.SetFloat("Horizontal", input.x);           // 横方向の入力
        animator.SetFloat("Vertical", input.z);             // 縦方向の入力
        animator.SetBool("IsJumping", isJumping);           // ジャンプ中かどうか
        animator.SetFloat("YVelocity", move.y);             // Y方向の速度
        animator.SetBool("IsPressLeftKey", isPressLeftKey); // 左クリックが押されているか
    }
}
