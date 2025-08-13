
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;



public class LobbyUIController : MonoBehaviour
{
    [Header("�o�g���V�[���J�n�{�^��")] 
    public Button startBattleButton;

    [Header("�ޏo�{�^��")]
    public Button leaveRoomButton;

    [Header("�X�^�[�g�e�L�X�g")]
    [SerializeField]
    private GameObject startText;

    private NetworkRunner runner;

    //[SerializeField] GameLauncher gameLauncher;

    void Start()
    {
        // ������\��
        startBattleButton.gameObject.SetActive(false);
        startText.GetComponentInChildren<TextMeshProUGUI>().text = "Waiting for Host to Start Battle�c";
        startBattleButton.onClick.AddListener(OnStartBattleClicked);
        leaveRoomButton.onClick.AddListener(OnLeaveRoomClicked);

    }

    public void ShowStartButton(NetworkRunner runner)
    {
        this.runner = runner;
        //startBattleButton.gameObject.SetActive(true);
        startText.GetComponentInChildren<TextMeshProUGUI>().text = "Press Enter to Start Battle";
    }

    void OnStartBattleClicked()
    {
        if (runner != null && runner.IsServer)
        {
            // �o�g���V�[���ɑJ��
            Debug.Log("�o�g���V�[���ɑJ�ڂ��܂�");
            runner.SessionInfo.IsOpen = false; // �Z�b�V���������
            runner.SessionInfo.IsVisible = false; // �Z�b�V�������\���ɂ���
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
