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
    [SerializeField] private FadeUI fadeUI;
    [Header("���X�|�[�����̃t�F�[�h���Ԑݒ�")]
    [SerializeField] private float respawnFadeOutTime = 3f;
    [SerializeField] private float respawnFadeInTime = 1f;
    //�d������
    [SerializeField] private float respawnStunDuration = 3f;
    [SerializeField] private RespawnUI respawnPanel;

    private Coroutine currentFade;
    private NetworkObject myPlayer;
    private PlayerAvatar playerAvatar;
    public event Action<float> OnRespawnStuned;


    public void RespawnStart()
    {
        // �v���C���[���擾
        myPlayer = GameManager2.Instance.GetMyPlayer();
        playerAvatar = myPlayer.GetComponent<PlayerAvatar>();

        // �������̃R���[�`���Ŏ��s
        StartCoroutine(RespawnCoroutine());
    }
    private IEnumerator RespawnCoroutine()
    {
        // 1. ���X�|�[���O����
        InitializeBeforeFade();

        // 2. �t�F�[�h�A�E�g
        yield return StartCoroutine(fadeUI.FadeAlpha(0f, 1f, respawnFadeOutTime));

        // 3. �t�F�[�h�A�E�g�㏈��
        Debug.Log("LocalRespawnHandler: ���X�|�[���������J�n���܂��B");
        InitializeAfterFade();
        yield return new WaitForSeconds(0.5f); // �t�F�[�h�A�E�g��̑ҋ@����

        // 4. �t�F�[�h�C��
        yield return StartCoroutine(fadeUI.FadeAlpha(1f, 0f, respawnFadeInTime));

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

}
