using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUIPanel
{
    /// <summary>
    /// �p�l���̏������������s���܂��B
    /// </summary>
    void Initialize();
    /// <summary>
    /// �p�l���̃N���[���A�b�v�������s���܂��B
    /// </summary>
    void Cleanup();
    /// <summary>
    /// �p�l���̕\��/��\����ݒ肵�܂��B
    /// </summary>
    /// <param name="visible">�\������ꍇ��true�A��\���ɂ���ꍇ��false�B</param>
    void SetVisible(bool visible);
}
