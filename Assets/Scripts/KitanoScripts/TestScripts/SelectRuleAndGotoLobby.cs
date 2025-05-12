using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectRuleAndGotoLobby : MonoBehaviour
{
    public void OnClickSelectDeathMatch()
    {
        Debug.Log("DeathMatch selected"); // �f�o�b�O�p
        GameRuleSettings.Instance.selectedRule = GameRule.DeathMatch;
        SceneLoader.Instance.LoadScene(SceneType.Sample); // �C�ӂ̃��r�[�V�[���֑J��
    }

    public void OnClickSelectTeamDeathMatch()
    {
        GameRuleSettings.Instance.selectedRule = GameRule.TeamDeathMatch;
        SceneLoader.Instance.LoadScene(SceneType.Sample);
    }
}
