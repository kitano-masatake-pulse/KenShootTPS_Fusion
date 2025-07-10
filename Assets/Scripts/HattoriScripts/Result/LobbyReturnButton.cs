using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion; // Fusionの名前空間を使用

public class LobbyReturnButton : MonoBehaviour
{
    [Header("遷移したいシーン")]
    [SerializeField] SceneType sceneType; // シーンの種類を指定するための変数
    private NetworkRunner runner; // NetworkRunnerのインスタンスを保持する変数
    void Start()
    {
        gameObject.SetActive(false); // 初期状態では非表示にする
        // ボタンの初期化や設定が必要な場合はここに記述
        runner = FindObjectOfType<NetworkRunner>(); // シーン内のNetworkRunnerを取得
        if (runner != null)
        {
            if (runner.IsServer)
            {
                //このボタンを表示
                gameObject.SetActive(true);
            }
        }
    }

    public void OnReturnButtonClicked()
    {
        if (runner != null&& runner.IsServer)
        {
            // シーン遷移
            SceneTransitionManager.Instance.ChangeScene(sceneType);
        }
        else if(runner == null)
        {
            Debug.LogError("NetworkRunnerが見つかりません。シーン内にNetworkRunnerが存在することを確認してください。");
        }
        else
        {
            Debug.LogWarning("この操作はサーバーのみで実行できます。クライアントでは実行できません。");
        }
    }
}
