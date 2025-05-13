using Fusion;
using UnityEngine;
using TMPro;

public class PlayerAvatar : NetworkBehaviour
{
    [SerializeField] private TextMeshPro idText;

    private NetworkCharacterControllerPrototype characterController;

    private void Awake()
    {
        characterController = GetComponent<NetworkCharacterControllerPrototype>();
    }

    public override void Spawned()
    {
        // ������PlayerRef����m��I��ID�𐶐��i��F1000�ԑ�j
        int playerId = 1000 + Object.InputAuthority.RawEncoded;
        // �\���p�e�L�X�g�ɔ��f
        idText.text = $"ID: {playerId:D4}";

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
        idText.transform.rotation = Camera.main.transform.rotation;

    }
}