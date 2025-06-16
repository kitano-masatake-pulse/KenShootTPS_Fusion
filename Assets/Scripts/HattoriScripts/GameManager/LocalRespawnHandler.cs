using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//���S�����v���C���[�̃��X�|�[���������s���N���X
//�A�j���[�V�����v���C���X�g�̃N���A�A�e��̏������A���X�|�[��UI�̃��Z�b�g�A�t�F�[�h�����Ȃǂ��s��

public class LocalRespawnHandler: MonoBehaviour
{
    [SerializeField] private Image fadePanel;
    [Header("���X�|�[�����̃t�F�[�h���Ԑݒ�")]
    [SerializeField] private float respawnFadeOutTime = 3f;
    [SerializeField] private float respawnFadeInTime = 1f;
    //�d������
    [SerializeField] private float respawnStunDuration = 3f;
    [SerializeField] private RespawnPanel respawnPanel;

    private Coroutine currentFade;
    private NetworkObject myPlayer;
    private PlayerAvatar playerAvatar;
    public event Action<float> OnRespawnStuned;


    public void RespawnStart()
    {
        // �v���C���[���擾
        myPlayer = GameManager.Instance.GetMyPlayer();
        playerAvatar = myPlayer.GetComponent<PlayerAvatar>();

        // �������̃R���[�`���Ŏ��s
        StartCoroutine(RespawnCoroutine());
    }
    private IEnumerator RespawnCoroutine()
    {
        // 1. ���X�|�[���O����
        InitializeBeforeFade();

        // 2. �t�F�[�h�A�E�g
        yield return StartCoroutine(FadeAlpha(0f, 1f, respawnFadeOutTime));

        // 3. �t�F�[�h�A�E�g�㏈��
        Debug.Log("LocalRespawnHandler: ���X�|�[���������J�n���܂��B");
        InitializeAfterFade();

        // 4. �t�F�[�h�C��
        yield return StartCoroutine(FadeAlpha(1f, 0f, respawnFadeInTime));

        // 5. �t�F�[�h�C���㏈��
        OnRespawnStuned?.Invoke(respawnStunDuration);

        // 6. �X�^�����ԑҋ@
        yield return new WaitForSeconds(respawnStunDuration);

        // 7. ���X�|�[����̏���
        AfterRespawn();
    }

    private void InitializeBeforeFade()
    {
        //Animation�v���C���X�g���N���A
        //(���X�|�[���P�\��ɌĂ΂�邩��ォ������Ă��Ȃ��Ɣ��f)
        playerAvatar.ClearActionAnimationPlayList();
    }

    private void InitializeAfterFade()
    {
        respawnPanel.ResetUI(); // UI�����Z�b�g
        playerAvatar.InitializeAllAmmo();// �e�򏉊���

        playerAvatar.IsHoming = false;
        playerAvatar.IsFollowingCameraForward = true; //�J�����Ǐ]��L����
        playerAvatar.TeleportToInitialSpawnPoint(GetRespawnPoint());

        //�z�X�g�Ƀ��X�|�[��������v��
        RespawnManager.Instance.RPC_RequestRespawn(myPlayer);
    }

    private void AfterRespawn()
    {
        //�s������������
        playerAvatar.IsDuringWeaponAction = false;
        playerAvatar.IsImmobilized = false;

        //�z�X�g�Ƀ��X�|�[����̏�����v��
        RespawnManager.Instance.RPC_RespawnEnd(myPlayer);
    }



    // from��to �̃A���t�@�l�� duration �b�����ĕ�Ԃ���R���[�`��
    private IEnumerator FadeAlpha(float from, float to, float duration)
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeRoutine(from, to, duration));
        yield return currentFade;
    }

    // ���ۂ̃t�F�[�h����
    private IEnumerator FadeRoutine(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;
        Color panelColor = fadePanel.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            panelColor.a = Mathf.Lerp(fromAlpha, toAlpha, t);
            fadePanel.color = panelColor;
            yield return null;
        }
        panelColor.a = toAlpha;
        fadePanel.color = panelColor;
        currentFade = null;
    }

    public Vector3 GetRespawnPoint()
    {
        var randomValue = UnityEngine.Random.insideUnitCircle * 5f;
        return new Vector3(randomValue.x, 5f, randomValue.y);
    }

}
