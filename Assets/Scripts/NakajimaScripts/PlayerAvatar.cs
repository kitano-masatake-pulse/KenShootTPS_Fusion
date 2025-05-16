using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

public class PlayerAvatar : NetworkBehaviour
{
    [SerializeField]
    private TextMeshPro nameLabel;

    private NetworkCharacterControllerPrototype characterController;

    private void Awake()
    {
        characterController = GetComponent<NetworkCharacterControllerPrototype>();
    }

    public override void Spawned()
    {
        SetNickName($"Player({Object.InputAuthority.PlayerId})"); 
        
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
    private void LateUpdate()
    {
        // �v���C���[���̃e�L�X�g���A��ɃJ�����̐��ʌ����ɂ���
        nameLabel.transform.rotation = Camera.main.transform.rotation;
    }

    // �v���C���[�����e�L�X�g�ɐݒ肷��
    public void SetNickName(string nickName)
    {
        nameLabel.text = nickName;
    }
}