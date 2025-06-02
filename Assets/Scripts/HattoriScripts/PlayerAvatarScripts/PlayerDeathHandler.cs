using UnityEngine;
using System.Collections;
using Fusion;
using TMPro;

public class PlayerDeathHandler : NetworkBehaviour
{
    [SerializeField] private GameObject WorldUICanvas;

    void OnEnable()
    {
        // PlayerNetworkState �̎��S�ʒm���w��
        var state = GetComponent<PlayerNetworkState>();
        state.OnPlayerDied += HandleDeath;
    }

    void OnDisable()
    {
        GetComponent<PlayerNetworkState>().OnPlayerDied -= HandleDeath;
    }

    private void HandleDeath(PlayerRef victim, PlayerRef killer)
    {
        //���͖����iPlayerAvatar ���ɂ��t���O�𑗂邩���L�j
        GetComponent<PlayerAvatar>().enabled = false;

        //Collider ������(���C���[�؂�ւ�)
        foreach (var col in GetComponentsInChildren<Collider>())
            col.gameObject.layer = LayerMask.NameToLayer("DeadPlayer");
        //Hitbox ������(���C���[�؂�ւ�)
        foreach (var hitbox in GetComponentsInChildren<PlayerHitbox>())
            hitbox.gameObject.layer = LayerMask.NameToLayer("DeadHitbox");

        //���탂�f���^�l�[���^�O��\��
        WorldUICanvas.SetActive(false);

        //���ɂ��邱��
        //���S���A�j���[�V����

        //���S�v���C���[�Ȃ���s
        if (victim == Object.InputAuthority)
        {
           
        }
        //�E�Q�v���C���[�Ȃ���s
        else if (killer == Object.InputAuthority)
        {
            // 6. �L�����o
            // �����ɃL�����o�̃R�[�h��ǉ��i��F�G�t�F�N�g�A�T�E���h�Ȃǁj

        }

    }

   
}