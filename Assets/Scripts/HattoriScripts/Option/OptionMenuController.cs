using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionMenuController : MonoBehaviour
{
    [SerializeField] GameObject rootPanel; // ���j���[�̃��[�g�iCanvas���̃p�l���j
    [SerializeField] OptionMenuUI optionMenuUI; // �I�v�V�������j���[��UI�R���|�[�l���g
    public bool IsOpen => rootPanel.activeSelf;

    private void OnEnable()
    {
        UIInputHub.OnToggleMenu -= Toggle; // ���j���[�̃g�O���C�x���g���w�ǉ���
        UIInputHub.OnToggleMenu += Toggle; // ���j���[�̃g�O���C�x���g���w��
    }
    private void OnDisable()
    {
        UIInputHub.OnToggleMenu -= Toggle; // ���j���[�̃g�O���C�x���g���w�ǉ���
    }

    public void Toggle() { if (IsOpen) Close(); else Open(); }

    public void Open()
    {
        rootPanel.SetActive(true);
        CursorManager.Instance.RequestUI(this);
        LocalInputHandler.isOpenMenu = true; // ���j���[���J���Ă����Ԃ�ݒ�
    }

    public void Close()
    {
        rootPanel.SetActive(false);
        OptionsManager.Instance.Cancel(); 
        optionMenuUI.RefreshUI();
        CursorManager.Instance.ReleaseUI(this);
        LocalInputHandler.isOpenMenu = false; // ���j���[�����Ă����Ԃ�ݒ�

    }
}
