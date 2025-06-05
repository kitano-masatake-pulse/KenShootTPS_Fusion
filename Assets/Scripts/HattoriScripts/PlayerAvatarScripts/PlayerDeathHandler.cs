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
        if(GameManager.Instance != null)
        {
            GameManager.Instance.OnMyPlayerDied += HandleDeath;
        }
        
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMyPlayerDied -= HandleDeath;
        }
    }

    private void HandleDeath(PlayerRef killer, float hostTimeStamp)
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
        playerAvatar.SetActionAnimationPlayList(ActionType.Dead, hostTimeStamp);

    }

}