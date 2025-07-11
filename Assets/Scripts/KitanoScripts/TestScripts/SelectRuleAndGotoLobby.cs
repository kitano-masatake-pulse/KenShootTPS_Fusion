using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectRuleAndGotoLobby : MonoBehaviour
{
    [Header("移動先のシーン設定")]
    [SerializeField] private SceneType lobbySceneType = SceneType.Lobby; // ロビーシーンの種類を指定
    public void OnClickSelectDeathMatch()
    {
        Debug.Log("DeathMatch selected"); // デバッグ用
        GameRuleSettings.Instance.selectedRule = GameRule.DeathMatch;
        SceneTransitionManager.Instance.ChangeScene(lobbySceneType);
        //SceneLoader.Instance.LoadScene(lobbySceneType); // 任意のロビーシーンへ遷移
    }

    public void OnClickSelectTeamDeathMatch()
    {
        GameRuleSettings.Instance.selectedRule = GameRule.TeamDeathMatch;
        SceneLoader.Instance.LoadScene(lobbySceneType);
    }
}
