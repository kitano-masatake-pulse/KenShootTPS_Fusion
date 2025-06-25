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
            exitButtonText.text = "Leave the Room"; // �N���C�A���g�̏ꍇ
        }

    }

    //������ޏoor���������{�^���������ꂽ�Ƃ��ɌĂ΂�郁�\�b�h
    public async void OnExitButtonClicked()
    {
        if (runner != null)
        {
            if (runner.IsServer)
            {
                // �V�[���J��
                runner.SetActiveScene(sceneType.ToSceneName());

                // �����ҋ@���Ă���V���b�g�_�E���i�V�[���J�ڂ����f�����܂ő҂j
                await System.Threading.Tasks.Task.Delay(100); // 0.1�b�ҋ@�i�K�v�ɉ����Ē���)//��������������Ă�̂ŁA��Œ������K�v����

                await runner.Shutdown();
            }
            else
            {
                // �N���C�A���g�̏ꍇ�͎��������ޏo
                await runner.Shutdown(true, ShutdownReason.Ok);
                //�^�C�g����ʂɃV�[���J��
                SceneManager.LoadScene(sceneType.ToSceneName());
            }
        }
    }
}
