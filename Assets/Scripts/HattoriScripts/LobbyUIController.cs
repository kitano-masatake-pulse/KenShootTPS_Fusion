
using UnityEngine;
using UnityEngine.UI;
using Fusion;



public class LobbyUIController : MonoBehaviour
{
    [Header("�o�g���V�[���J�n�{�^��")] 
    public Button startBattleButton;

    [Header("�ޏo�{�^��")]
    public Button leaveRoomButton;

    private NetworkRunner runner;

    //[SerializeField] GameLauncher gameLauncher;

    void Start()
    {
        // ������\��
        startBattleButton.gameObject.SetActive(false);
        startBattleButton.onClick.AddListener(OnStartBattleClicked);
        leaveRoomButton.onClick.AddListener(OnLeaveRoomClicked);

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
            string sceneName = GameLauncher.Instance.nextScene.ToSceneName();
            SceneTransitionManager.Instance.ChangeScene(GameLauncher.Instance.nextScene);
            //runner.SetActiveScene(sceneName);
        }
        else
        {
            Debug.LogError("�N���C�A���g�̓o�g���J�n�{�^���������܂���");
        }
    }


    void OnLeaveRoomClicked()
    {
        GameLauncher.Instance.LeaveRoom();



    }

}
