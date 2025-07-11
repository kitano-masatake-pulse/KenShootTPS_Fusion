using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion; // Fusion�̖��O��Ԃ��g�p

public class LobbyReturnButton : MonoBehaviour
{
    [Header("�J�ڂ������V�[��")]
    [SerializeField] SceneType sceneType; // �V�[���̎�ނ��w�肷�邽�߂̕ϐ�
    private NetworkRunner runner; // NetworkRunner�̃C���X�^���X��ێ�����ϐ�
    void Start()
    {
        gameObject.SetActive(false); // ������Ԃł͔�\���ɂ���
        // �{�^���̏�������ݒ肪�K�v�ȏꍇ�͂����ɋL�q
        runner = FindObjectOfType<NetworkRunner>(); // �V�[������NetworkRunner���擾
        if (runner != null)
        {
            if (runner.IsServer)
            {
                //���̃{�^����\��
                gameObject.SetActive(true);
            }
        }
    }

    public void OnReturnButtonClicked()
    {
        if (runner != null&& runner.IsServer)
        {
            // �V�[���J��
            SceneTransitionManager.Instance.ChangeScene(sceneType);
        }
        else if(runner == null)
        {
            Debug.LogError("NetworkRunner��������܂���B�V�[������NetworkRunner�����݂��邱�Ƃ��m�F���Ă��������B");
        }
        else
        {
            Debug.LogWarning("���̑���̓T�[�o�[�݂̂Ŏ��s�ł��܂��B�N���C�A���g�ł͎��s�ł��܂���B");
        }
    }
}
