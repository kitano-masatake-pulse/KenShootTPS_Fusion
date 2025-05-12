using Fusion;
using UnityEngine;

public class SceneTransitionStarter : MonoBehaviour
{
    [Header("Network Runner（自動生成される）")]
    private NetworkRunner runner;

    [Header("遷移先のシーン名")]
    public string sceneName = "BattleScene"; // Build Settings に登録済みのシーン名

    public async void StartNetworkGame()
    {
        // すでにRunnerがあるかチェック
        if (runner == null)
        {
            runner = gameObject.AddComponent<NetworkRunner>();
        }

        // 必要なら入力権限を与える（プレイヤー操作がある場合）
        runner.ProvideInput = true;

        // シーン遷移用のデフォルトマネージャを追加
        var sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = "KenShootTPS_Fusion_DeathMatch",
            Scene = UnityEngine.SceneManagement.SceneUtility.GetBuildIndexByScenePath($"Assets/Scenes/{sceneName}.unity"), // もしくは明示的に index
            SceneManager = sceneManager
        });

        if (!result.Ok)
        {
            Debug.LogError("ゲーム開始に失敗しました: " + result.ShutdownReason);
        }
    }
}