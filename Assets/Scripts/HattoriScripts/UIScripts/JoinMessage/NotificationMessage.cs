using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro���g�p����ꍇ�́ATextMeshPro�p�b�P�[�W���K�v�ł�
public class NotificationMessage : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText; // TextMeshPro�̃e�L�X�g�R���|�[�l���g���g�p����ꍇ
    private bool isHost = false; // �z�X�g���ǂ����̃t���O

    public void SetText(string message, bool state)
    {
        isHost = state; // �z�X�g�t���O��ݒ�
        if (messageText != null)
        {
            messageText.text = message;
        }
        else
        {
            Debug.LogWarning("MessageText is not assigned in NotificationMessage.");
        }

        //�z�X�g�̏ꍇ�̓e�L�X�g�����F�ɕύX
        if (isHost)
        {
            messageText.color = Color.yellow; // �z�X�g�̃��b�Z�[�W�͉��F
        }
        else
        {
            messageText.color = Color.white; // �ʏ�̃��b�Z�[�W�͔�
        }
    }
}
