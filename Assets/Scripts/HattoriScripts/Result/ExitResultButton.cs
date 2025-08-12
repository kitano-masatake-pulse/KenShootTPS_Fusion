using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using TMPro;
using System;

public class ExitResultButton : MonoBehaviour
{
    [Header("�J�ڂ������V�[��")]
    [SerializeField] SceneType sceneType; // �V�[���̎�ނ��w�肷�邽�߂̕ϐ�
    [SerializeField] TMP_Text exitButtonText; // �{�^���̃e�L�X�g��ύX���邽�߂̕ϐ�
    private NetworkRunner runner;

    void Start()
    {
        runner = FindObjectOfType<NetworkRunner>();
        //�z�X�g���ۂ��Ń{�^���̃e�L�X�g��ύX
        if (runner != null && runner.IsServer)
        {
            exitButtonText.text = "Close the Room"; // �z�X�g�̏ꍇ
        }
        else
        {
            exitButtonText.text = "Return to Title"; // �N���C�A���g�̏ꍇ
        }

    }

    //������ޏoor���������{�^���������ꂽ�Ƃ��ɌĂ΂�郁�\�b�h
    public async void OnExitButtonClicked()
    {
            SceneTransitionManager.Instance.ChangeScene(sceneType, true); // �V�[����ύX

    }
}
