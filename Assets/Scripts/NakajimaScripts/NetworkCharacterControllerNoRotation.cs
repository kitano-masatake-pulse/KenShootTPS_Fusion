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
        // �n�ʂɐڂ��Ă��Ȃ���Ώd�͂�������
        if (!controller.isGrounded)
        {
            velocity.y += gravity * Runner.DeltaTime;
        }
        else if (velocity.y < 0)
        {
            // �ڒn���Ă����牺�����̑��x�����Z�b�g
            velocity.y = -1f;

            // �W�����v���͂�����ꍇ�͏�����ɑ��x��������
            if (jump)
            {
                velocity.y = jumpImpulse;
            }
        }

        // �ړ������ɑ��x��K�p
        Vector3 move = direction * moveSpeed;
        move.y = velocity.y;

        // �ړ����s
        controller.Move(move * Runner.DeltaTime);

        // �ڒn���Ă�����y���x��ۑ��i�����������h�~�j
        if (controller.isGrounded)
        {
            velocity.y = move.y;
        }
    }
}
