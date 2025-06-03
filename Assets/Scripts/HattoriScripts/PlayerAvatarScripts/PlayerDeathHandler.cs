using UnityEngine;
using System.Collections;
using Fusion;
using TMPro;

public class PlayerDeathHandler : NetworkBehaviour
{
    [SerializeField] private PlayerAvatar playerAvatar;
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
        //PlayerAvatar�̍s���s�\�t���O��L��������
        //Collider ������(���C���[�؂�ւ�)
        foreach (var col in GetComponentsInChildren<Collider>())
            col.gameObject.layer = LayerMask.NameToLayer("DeadPlayer");
        //Hitbox ������(���C���[�؂�ւ�)
        foreach (var hitbox in GetComponentsInChildren<PlayerHitbox>())
            hitbox.gameObject.layer = LayerMask.NameToLayer("DeadHitbox");
        //�l�[���^�O��\��
        WorldUICanvas.SetActive(false);
        //�v���C���[�A�o�^�[�Ɏ��S�A�j���[�V������ݒ�
        playerAvatar.SetActionAnimationPlayList(ActionType.Dead, Runner.SimulationTime);
    }


}