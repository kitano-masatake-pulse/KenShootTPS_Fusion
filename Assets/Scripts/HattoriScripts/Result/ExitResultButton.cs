using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using TMPro;
using System;

public class ExitResultButton : MonoBehaviour
{
    [Header("遷移したいシーン")]
    [SerializeField] SceneType sceneType; // シーンの種類を指定するための変数
    [SerializeField] TMP_Text exitButtonText; // ボタンのテキストを変更するための変数
    private NetworkRunner runner;

    void Start()
    {
        runner = FindObjectOfType<NetworkRunner>();
        //ホストか否かでボタンのテキストを変更
        if (runner != null && runner.IsServer)
        {
            exitButtonText.text = "Close the Room"; // ホストの場合
        }
        else
        {
            exitButtonText.text = "Leave the Room"; // クライアントの場合
        }

    }

    //部屋を退出or部屋を閉じるボタンが押されたときに呼ばれるメソッド
    public async void OnExitButtonClicked()
    {
        if (runner != null)
        {
            if (runner.IsServer)
            {
                // シーン遷移
                runner.SetActiveScene(sceneType.ToSceneName());

                // 少し待機してからシャットダウン（シーン遷移が反映されるまで待つ）
                await System.Threading.Tasks.Task.Delay(100); // 0.1秒待機（必要に応じて調整)//無理くり実装してるので、後で調整が必要かも

                await runner.Shutdown();
            }
            else
            {
                // クライアントの場合は自分だけ退出
                await runner.Shutdown(true, ShutdownReason.Ok);
                //タイトル画面にシーン遷移
                SceneManager.LoadScene(sceneType.ToSceneName());
            }
        }
    }
}
