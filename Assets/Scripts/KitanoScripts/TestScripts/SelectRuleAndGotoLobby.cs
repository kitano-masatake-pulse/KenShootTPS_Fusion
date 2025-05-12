using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectRuleAndGotoLobby : MonoBehaviour
{
    public void OnClickSelectDeathMatch()
    {
        Debug.Log("DeathMatch selected"); // デバッグ用
        GameRuleSettings.Instance.selectedRule = GameRule.DeathMatch;
        SceneLoader.Instance.LoadScene(SceneType.Sample); // 任意のロビーシーンへ遷移
    }

    public void OnClickSelectTeamDeathMatch()
    {
        GameRuleSettings.Instance.selectedRule = GameRule.TeamDeathMatch;
        SceneLoader.Instance.LoadScene(SceneType.Sample);
    }
}
