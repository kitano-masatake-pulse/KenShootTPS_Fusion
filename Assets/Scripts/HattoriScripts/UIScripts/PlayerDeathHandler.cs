using UnityEngine;
using System.Collections;
using Fusion;
using TMPro;

public class PlayerDeathHandler : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Collider[] colliders;
    [SerializeField] private GameObject[] weaponModels;
    [SerializeField] private GameObject playerHUDCanvas;
    [SerializeField] private CanvasGroup respawnUI;
    [SerializeField] private TMP_Text respawnCountdown;
    [SerializeField] private float respawnDelay = 5f;

    private bool _isDead;

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

        _isDead = true;
        // 1. ���͖����iPlayerAvatar ���ɂ��t���O�𑗂邩���L�j
        GetComponent<PlayerAvatar>().enabled = false;

        // 2. Collider ������(���C���[�؂�ւ�)
        foreach (var col in GetComponentsInChildren<Collider>())
            col.gameObject.layer = LayerMask.NameToLayer("DeadLayer");

        // 3. ���S�A�j���Đ�
        animator.SetTrigger("Die");

        // 4. ���탂�f���^�l�[���^�O��\��
        foreach (var w in weaponModels) w.SetActive(false);
        playerHUDCanvas.SetActive(false);


        //���S�v���C���[�Ȃ���s
        if (victim == Object.InputAuthority)
        {
            // 5. ���X�|�[��UI �\��
            StartCoroutine(RespawnCountdown());

        }
        //�E�Q�v���C���[�Ȃ���s
        else if (killer == Object.InputAuthority)
        {
            // 6. �L�����o
            // �����ɃL�����o�̃R�[�h��ǉ��i��F�G�t�F�N�g�A�T�E���h�Ȃǁj

        }



    }

    private IEnumerator RespawnCountdown()
    {
        
        respawnUI.alpha = 1;
        float t = respawnDelay;
        while (t > 0)
        {
            respawnCountdown.text = Mathf.CeilToInt(t).ToString();
            yield return new WaitForSeconds(1f);
            t -= 1f;
        }
        respawnUI.alpha = 0;
        // �����Ń��X�|�[���������g���K�[�iRPC or �C�x���g�Ńz�X�g�Ɉ˗��j
    }
}