using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class GameLauncher : MonoBehaviour
{
    [SerializeField]
    private NetworkRunner networkRunnerPrefab;

    private NetworkRunner networkRunner;

    private async void Start()
    {
        // NetworkRunnerを生成する
        networkRunner = Instantiate(networkRunnerPrefab);
        // StartGameArgsに渡した設定で、セッションに参加する
        var result = await networkRunner.StartGame(new StartGameArgs {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = "KenShootTPS_Fusion_DeathMatch",
            SceneManager = networkRunner.GetComponent<NetworkSceneManagerDefault>()
        });

        if (result.Ok)
        {
            Debug.Log("成功！");
        }
        else
        {
            Debug.Log("失敗！");
        }
    }
}
