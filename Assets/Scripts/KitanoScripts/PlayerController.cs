using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Text idText;

    public override void Spawned()
    {
        // ������PlayerRef����m��I��ID�𐶐��i��F1000�ԑ�j
        int playerId = 1000 + Object.InputAuthority.RawEncoded;

        // �\���p�e�L�X�g�ɔ��f
        idText.text = $"ID: {playerId:D4}";
    }

    void Update()
    {
        // ���O�v���[�g���J�����̕����������悤��
        idText.transform.rotation = Quaternion.LookRotation(idText.transform.position - Camera.main.transform.position);
    }
}
