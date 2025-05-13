
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
        // 初期非表示
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
            // バトルシーンに遷移
            Debug.Log("バトルシーンに遷移します");
            string sceneName = SceneType.Battle.ToSceneName();
            runner.SetActiveScene(sceneName);
        }
        else
        {
            Debug.LogError("クライアントはバトル開始ボタンを押せません");
        }
    }
}
