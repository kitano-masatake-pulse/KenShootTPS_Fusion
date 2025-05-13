
using UnityEngine;
using UnityEngine.UI;
using Fusion;



public class LobbyUIController : MonoBehaviour
{
    [SerializeField] 
    private Button startBattleButton;
    private NetworkRunner runner;

    void Start()
    {
        // ������\��
        startBattleButton.gameObject.SetActive(false);
        startBattleButton.onClick.AddListener(OnStartBattleClicked);
    }

    public void ShowStartButton(NetworkRunner runner)
    {
        this.runner = runner;
        startBattleButton.gameObject.SetActive(true);
    }

    void OnStartBattleClicked()
    {
        if (runner != null && runner.IsServer)
        {
            // �o�g���V�[���ɑJ��
            Debug.Log("�o�g���V�[���ɑJ�ڂ��܂�");
            string sceneName = SceneType.Battle.ToSceneName();
            runner.SetActiveScene(sceneName);
        }
        else
        {
            Debug.LogError("�N���C�A���g�̓o�g���J�n�{�^���������܂���");
        }
    }
}
