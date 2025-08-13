
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;



public class LobbyUIController : MonoBehaviour
{
    [Header("バトルシーン開始ボタン")] 
    public Button startBattleButton;

    [Header("退出ボタン")]
    public Button leaveRoomButton;

    [Header("スタートテキスト")]
    [SerializeField]
    private GameObject startText;

    private NetworkRunner runner;

    //[SerializeField] GameLauncher gameLauncher;

    void Start()
    {
        // 初期非表示
        startBattleButton.gameObject.SetActive(false);
        startText.GetComponentInChildren<TextMeshProUGUI>().text = "Waiting for Host to Start Battle…";
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
            // バトルシーンに遷移
            Debug.Log("バトルシーンに遷移します");
            runner.SessionInfo.IsOpen = false; // セッションを閉じる
            runner.SessionInfo.IsVisible = false; // セッションを非表示にする
            string sceneName = GameLauncher.Instance.nextScene.ToSceneName();
            SceneTransitionManager.Instance.ChangeScene(GameLauncher.Instance.nextScene);
            //runner.SetActiveScene(sceneName);
        }
        else
        {
            Debug.LogError("クライアントはバトル開始ボタンを押せません");
        }
    }


    void OnLeaveRoomClicked()
    {
        GameLauncher.Instance.LeaveRoom();



    }

}
