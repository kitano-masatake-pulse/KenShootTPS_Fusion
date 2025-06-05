using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public class ScoreDebugUI : MonoBehaviour
{
    private GameManager scoreManager;

    void Start()
    {
        // シーンに配置されている ScoreManager を探して格納
        scoreManager = FindObjectOfType<GameManager>();
        if (scoreManager == null)
        {
            Debug.LogError("[ScoreDebugUI] シーン内に ScoreManager が見つかりません。");
        }
    }


    void OnGUI()
    {
        if (scoreManager == null) return;

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
