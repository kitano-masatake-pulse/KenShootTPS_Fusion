using Fusion;

public class PlayerAvatar : NetworkBehaviour
{
    private NetworkCharacterControllerPrototype characterController;

    private void Awake()
    {
        characterController = GetComponent<NetworkCharacterControllerPrototype>();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            // ���͕����̃x�N�g���𐳋K������
            data.Direction.Normalize();
            // ���͕������ړ������Ƃ��Ă��̂܂ܓn��
            characterController.Move(data.Direction);
        }
    }
}