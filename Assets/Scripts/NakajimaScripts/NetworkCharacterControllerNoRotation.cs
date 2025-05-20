using Fusion;
using UnityEngine;

public class NetworkCharacterControllerNoRotation : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float jumpImpulse = 8f;

    private CharacterController controller;
    private Vector3 velocity;

    public override void Spawned()
    {
        controller = GetComponent<CharacterController>();
    }

    public void Move(Vector3 direction, bool jump)
    {
        // 地面に接していなければ重力を加える
        if (!controller.isGrounded)
        {
            velocity.y += gravity * Runner.DeltaTime;
        }
        else if (velocity.y < 0)
        {
            // 接地していたら下方向の速度をリセット
            velocity.y = -1f;

            // ジャンプ入力がある場合は上向きに速度を加える
            if (jump)
            {
                velocity.y = jumpImpulse;
            }
        }

        // 移動方向に速度を適用
        Vector3 move = direction * moveSpeed;
        move.y = velocity.y;

        // 移動実行
        controller.Move(move * Runner.DeltaTime);

        // 接地していたらy速度を保存（落下しすぎ防止）
        if (controller.isGrounded)
        {
            velocity.y = move.y;
        }
    }
}
