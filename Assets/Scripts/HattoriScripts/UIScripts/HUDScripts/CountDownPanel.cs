using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class CountDownPanel : MonoBehaviour, IHUDPanel
{
    [SerializeField] LocalRespawnHandler respawnHandler;
    [SerializeField] TMP_Text countdownText;

    //�R���[�`��
    private Coroutine countdownCoroutine;

    //Initialize���\�b�h�́A�p�l���̏��������s��
    // Cleanup���\�b�h�́A�p�l���̃N���[���A�b�v���s��
    public void Initialize(PlayerNetworkState _, PlayerAvatar __)
    {
        respawnHandler.OnRespawnStuned -= DisplayCountDownPanel;
        respawnHandler.OnRespawnStuned += DisplayCountDownPanel;
        countdownText.raycastTarget = false; // �J�E���g�_�E���e�L�X�g�̓N���b�N�ł��Ȃ��悤�ɂ���
    }

    public void Cleanup()
    {
        respawnHandler.OnRespawnStuned -= DisplayCountDownPanel;
    }

    //�R���[�`���ɂ���āA�J�E���g�_�E���p�l����\������
    private void DisplayCountDownPanel(float stunDuration)
    {
        // �����̃J�E���g�_�E���R���[�`�����~
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }
        StartCoroutine(CountDownCoroutine(stunDuration)); 
    }
    private IEnumerator CountDownCoroutine(float stunDuration)
    {
        float rem = stunDuration;
        while (rem > 0)
        {
            countdownText.text = $"{rem:F0}";
            yield return new WaitForSeconds(1f);
            rem -= 1f;
        }
        countdownText.text = "GO!";
        yield return new WaitForSeconds(1f);
        countdownText.text = ""; // �J�E���g�_�E���I����̓e�L�X�g���N���A
    }
}
