using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToBattle : MonoBehaviour
{
    // Start is called before the first frame update
    public void OnClickStartBattle()
    {
        // �V�[���J�ڂ��s��
        NetworkSceneTransitionManager.Instance.ChangeScene(SceneType.Battle);
    }
}
