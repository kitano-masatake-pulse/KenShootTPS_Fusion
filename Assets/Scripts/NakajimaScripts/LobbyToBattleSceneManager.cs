using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyToBattleSceneManager : MonoBehaviour
{
    public void OnClickGameStart()
    {
        SceneLoader.Instance.LoadScene(SceneType.Battle); // �C�ӂ̃��r�[�V�[���֑J��
    }
}
