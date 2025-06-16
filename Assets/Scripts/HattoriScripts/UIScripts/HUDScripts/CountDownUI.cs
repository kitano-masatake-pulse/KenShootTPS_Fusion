using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class CountDownUI : MonoBehaviour, IUIPanel
{
    private LocalRespawnHandler respawnHandler;
    [SerializeField] TMP_Text countdownText;
    [SerializeField] float defaultCountTime = 3f; // �f�t�H���g�̃X�^������
    //�R���[�`��
    private Coroutine countdownCoroutine;

    //Initialize���\�b�h�́A�p�l���̏��������s��
    // Cleanup���\�b�h�́A�p�l���̃N���[���A�b�v���s��
    public void Initialize()
    {
        respawnHandler.OnRespawnStuned -= DisplayCountDownPanel;
        respawnHandler.OnRespawnStuned += DisplayCountDownPanel;
        countdownText.raycastTarget = false; // �J�E���g�_�E���e�L�X�g�̓N���b�N�ł��Ȃ��悤�ɂ���
        countdownText.text = ""; 
    }

    public void Cleanup()
    {
        respawnHandler.OnRespawnStuned -= DisplayCountDownPanel;
        SetVisible(false); // �p�l�����\���ɂ���
    }

    public void SetRespawnHandler(LocalRespawnHandler handler)
    {
        respawnHandler = handler;
    }

    //�R���[�`���ɂ���āA�J�E���g�_�E���p�l����\������
    private void DisplayCountDownPanel(float countTime)
    {
        // �����̃J�E���g�_�E���R���[�`�����~
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }
        StartCoroutine(CountDownCoroutine(countTime)); 
    }
    private IEnumerator CountDownCoroutine(float countTime)
    {
        float rem = countTime;
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
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
        if (visible && countdownCoroutine == null)
        {
            countdownCoroutine = StartCoroutine(CountDownCoroutine(defaultCountTime)); 
        }
        else if (!visible && countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
            countdownText.text = ""; // ��\�����Ƀe�L�X�g���N���A
        }
    }
}
