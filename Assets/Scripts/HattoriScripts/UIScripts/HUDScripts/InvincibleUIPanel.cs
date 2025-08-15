using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InvincibleUIPanel : MonoBehaviour,IHUDPanel
{
    [SerializeField]
    private Image invincibleIcon; // ���G��Ԃ������A�C�R��
    private PlayerNetworkState playerState;
    private Coroutine invincibleCoroutine;

    public void Initialize(PlayerNetworkState pState, PlayerAvatar _)
    {
        playerState = pState;
        // �C�x���g�o�^
        playerState.OnInvincibleChanged -= UpdateInvincibleStatus;
        playerState.OnInvincibleChanged += UpdateInvincibleStatus;
        // �����l�ݒ�
        UpdateInvincibleStatus(playerState.IsInvincible, 0f);
    }

    public void Cleanup()
    {
        playerState.OnInvincibleChanged -= UpdateInvincibleStatus;
    }

    // Update is called once per frame
    private void UpdateInvincibleStatus(bool isInvincible, float remainTime)
    {
        // Invincible�̏�Ԃɉ�����UI���X�V
        if (isInvincible)
        {
            // Invincible��Ԃ�UI�X�V����
            Debug.Log("Player is invincible.");
            invincibleIcon.gameObject.SetActive(true);
            if (invincibleCoroutine != null)
            {
                StopCoroutine(invincibleCoroutine);
            }
            invincibleCoroutine = StartCoroutine(InvincibleCountdown(remainTime));
        }
        else
        {
            invincibleIcon.gameObject.SetActive(false);
            if (invincibleCoroutine != null)
            {
                StopCoroutine(invincibleCoroutine);
                invincibleCoroutine = null;
            }
        }
    }

    private IEnumerator InvincibleCountdown(float remainTime)
    {
        // ���G���Ԃ̃J�E���g�_�E������
        while (remainTime > 0f)
        {
            // UI�̍X�V�����i��: �c�莞�Ԃ�\������Ȃǁj
            Debug.Log($"Invincible time remaining: {remainTime}");
            yield return new WaitForSeconds(1f);
            remainTime -= 1f; // 1�b������
        }
        // ���G���Ԃ��I��������UI���\���ɂ���
        invincibleIcon.gameObject.SetActive(false);
    }
}
