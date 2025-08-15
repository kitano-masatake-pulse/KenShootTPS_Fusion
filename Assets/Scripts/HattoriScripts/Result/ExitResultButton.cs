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
            exitButtonText.text = "Return to Title"; // クライアントの場合
        }

    }

    //部屋を退出or部屋を閉じるボタンが押されたときに呼ばれるメソッド
    public async void OnExitButtonClicked()
    {
            SceneTransitionManager.Instance.ChangeScene(sceneType, true); // シーンを変更

    }
}
