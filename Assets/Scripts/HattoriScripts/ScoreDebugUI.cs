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
        IEnumerable<KeyValuePair<PlayerRef, PlayerScore>> allScores
            = scoreManager.GetAllScores();

        var SortedScores = allScores
            .OrderByDescending(kvp => kvp.Value.Kills)
            .ToList();

        foreach (var kvp in SortedScores)
        {
            var playerRef = kvp.Key;
            var score = kvp.Value;

            // 行開始
            GUILayout.BeginHorizontal();

            // プレイヤー情報＋現在のスコアを表示
            GUILayout.Label($"Player: {playerRef}", GUILayout.Width(140));
            GUILayout.Label($"Kills: {score.Kills}  Deaths: {score.Deaths}", GUILayout.Width(120));

            // もしこのインスタンスがホスト(HasStateAuthority) なら、ボタンを表示してスコアをいじれるようにする
            if (scoreManager.Object.HasStateAuthority)
            {
                // 「+1 キル」ボタンを押すと ModifyScore で +1 する
                if (GUILayout.Button("+Kill", GUILayout.Width(50)))
                {
                    scoreManager.ModifyScore(playerRef, +1, 0);
                }
                // 「+1 デス」ボタンを押すと ModifyScore で +1 する
                if (GUILayout.Button("+Death", GUILayout.Width(60)))
                {
                    scoreManager.ModifyScore(playerRef, 0, +1);
                }

                // 任意で「−1 キル」「−1 デス」ボタンを追加したい場合は以下のように
                if (GUILayout.Button("-Kill", GUILayout.Width(50)))
                {
                    scoreManager.ModifyScore(playerRef, -1, 0);
                }
                if (GUILayout.Button("-Death", GUILayout.Width(60)))
                {
                    scoreManager.ModifyScore(playerRef, 0, -1);
                }
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }
}
