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
            GameManager.Instance.OnAnyPlayerDied += HandleDeath;
        }
        
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnAnyPlayerDied -= HandleDeath;
        }
    }

    private void HandleDeath(PlayerRef victim, PlayerRef killer, float hostTimeStamp)
    {
        //���̃v���C���[�łȂ��ꍇ�͉������Ȃ�
        if (victim != Object.InputAuthority)
        {
            return;
        }
        Debug.Log($"PlayerDeathHandler: Player{victim.PlayerId} has died at {hostTimeStamp} by {killer.PlayerId}");
        //�v���C���[�A�o�^�[�Ɏ��S�A�j���[�V������ݒ�
        playerAvatar.SetActionAnimationPlayList(ActionType.Dead, hostTimeStamp);
        //�s���s�\��
        playerAvatar.IsDuringWeaponAction = true;
        playerAvatar.IsImmobilized = true;
        //��̃J�����Ǐ]��؂�
        playerAvatar.IsFollowingCameraForward = false;
        //Collider ������(���C���[�؂�ւ�)
        foreach (var col in GetComponentsInChildren<Collider>())
            col.gameObject.layer = LayerMask.NameToLayer("DeadPlayer");
        //Hitbox ������(���C���[�؂�ւ�)
        foreach (var hitbox in GetComponentsInChildren<PlayerHitbox>())
            hitbox.gameObject.layer = LayerMask.NameToLayer("DeadHitbox");
        //�l�[���^�O��\��
        WorldUICanvas.SetActive(false);
        

    }

}