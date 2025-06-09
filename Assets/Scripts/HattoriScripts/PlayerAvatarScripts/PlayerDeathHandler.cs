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
        //�v���C���[�A�o�^�[�Ɏ��S�A�j���[�V������ݒ�
        playerAvatar.SetActionAnimationPlayList(ActionType.Dead, hostTimeStamp);
        //�s���s�\��
        playerAvatar.SetDuringWeaponAction(true);
        playerAvatar.SetImmobilized(true);
        //��̃J�����Ǐ]��؂�
        playerAvatar.SetFollowingCameraForward(false);
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