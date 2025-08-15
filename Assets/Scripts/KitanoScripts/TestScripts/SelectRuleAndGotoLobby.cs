using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectRuleAndGotoLobby : MonoBehaviour
{
    [Header("�ړ���̃V�[���ݒ�")]
    [SerializeField] private SceneType lobbySceneType = SceneType.Lobby; // ���r�[�V�[���̎�ނ��w��
    public void OnClickSelectDeathMatch()
    {
        Debug.Log("DeathMatch selected"); // �f�o�b�O�p
        GameRuleSettings.Instance.selectedRule = GameRule.DeathMatch;
        SceneTransitionManager.Instance.ChangeScene(lobbySceneType);
        //SceneLoader.Instance.LoadScene(lobbySceneType); // �C�ӂ̃��r�[�V�[���֑J��
    }

    public void OnClickSelectTeamDeathMatch()
    {
        GameRuleSettings.Instance.selectedRule = GameRule.TeamDeathMatch;
        SceneLoader.Instance.LoadScene(lobbySceneType);
    }
}
