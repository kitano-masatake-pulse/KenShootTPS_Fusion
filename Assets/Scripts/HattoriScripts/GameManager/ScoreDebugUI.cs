using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public class ScoreDebugUI : MonoBehaviour
{
    private GameManager scoreManager;
    private bool isInitialized = false;

    private void OnEnable()
    {
        GameManager.OnGameManagerSpawned += StartDebugUI; // シーンロード完了時に初期化
    }
    private void OnDisable()
    {
        GameManager.OnGameManagerSpawned -= StartDebugUI; // イベント登録解除
    }

    private void StartDebugUI()
    {
        isInitialized = true;

    }
    void OnGUI()
    {

        if (isInitialized == false)
        {
            return; // 初期化されていない場合は何もしない
        }

        // 枠線付きの縦レイアウト開始
        GUILayout.BeginVertical("box", GUILayout.Width(300));
        GUILayout.Label("=== Player Scores ===");

        // ScoreManager から全スコア辞書を取得
        var SortedScores = GameManager.Instance.GetSortedScores();

        foreach (var kvp in SortedScores)
        {
            var playerRef = kvp.Key;
            var score = kvp.Value;

            // 行開始
            GUILayout.BeginHorizontal();

            // プレイヤー情報＋現在のスコアを表示
            GUILayout.Label($"Player: {playerRef}", GUILayout.Width(140));
            GUILayout.Label($"Kills: {score.Kills}  Deaths: {score.Deaths}", GUILayout.Width(120));
            GUILayout.EndHorizontal();

        }

        GUILayout.EndVertical();
    }
}
