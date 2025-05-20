using Fusion;
using UnityEngine;
using TMPro;
using UnityEngine.Windows;

public class PlayerAvatar : NetworkBehaviour
{
    [SerializeField] private TextMeshPro idText;

    private NetworkCharacterControllerNoRotation characterController;
    [SerializeField] private Animator animator;


    private void Awake()
    {
        characterController = GetComponent<NetworkCharacterControllerNoRotation>();
        animator = GetComponent<Animator>();
    }

    public override void Spawned()
    {
        // 自分のPlayerRefから確定的なIDを生成（例：1000番台）
        int playerId = 1000 + Object.InputAuthority.RawEncoded;
        // 表示用テキストに反映
        idText.text = $"ID: {playerId:D4}";

    }


    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            // 入力方向のベクトルを正規化する
            data.Direction.Normalize();
            // 入力方向を移動方向としてそのまま渡す

            characterController.Move(data.Direction, data.Buttons.IsSet(NetworkInputButtons.Jump));

            UpdateAnimator(data.IsJumping);

            Debug.Log($"Move: {data.Direction}");
        }
        idText.transform.rotation = Camera.main.transform.rotation;

    }

    private void UpdateAnimator(bool isJumping)
    {

        //animator.SetFloat("Horizontal", input.x);           // 横方向の入力
        //animator.SetFloat("Vertical", input.z);             // 縦方向の入力
        animator.SetBool("IsJumping", isJumping);           // ジャンプ中かどうか
        Debug.Log($"IsJumping: {isJumping}");
        //animator.SetFloat("YVelocity", move.y);
    }
}