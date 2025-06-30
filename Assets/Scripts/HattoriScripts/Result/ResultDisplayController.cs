using Fusion;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// 結果画面でプレイヤーのスコアを表示・整列するクラス
/// </summary>
public class ResultDisplayController : NetworkBehaviour,IAfterSpawned
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject scoreRowPrefab;
    [Networked]
    [Capacity(8)] // 最大8人のプレイヤーを想定
    private NetworkDictionary<PlayerRef, PlayerScore> PlayerScores { get; }

    public override void Spawned()
    {

        //カーソル固定を無効化
        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true;

        if (contentParent == null)
        {
            var obj = GameObject.Find("ScoreBars");
            if (obj != null)
            {
                contentParent = obj.transform;
            }
            else
            {
                Debug.LogError("ContentParentタグのオブジェクトが見つかりませんでした。");
            }
        }

        if (Runner.IsServer)
        {
            //送られてきたスコアを取得
            ScoreTransfer scoreTransfer = FindObjectOfType<ScoreTransfer>();
            if (scoreTransfer == null)
            {
                Debug.LogError("ScoreTransfer component not found in the scene. Please ensure it is present.");
                return;
            }
            var sortedScores = scoreTransfer.GetScores();
            foreach (var score in sortedScores)
            {
                PlayerScores.Set(score.Key,score.Value); // スコアを辞書に追加
            }
            //役目を終えたらScoreTransferを削除
            Destroy(scoreTransfer.gameObject); 
        }


    }

    public void AfterSpawned()
    {
        // スコア辞書を元に結果画面を表示
        //スコアデータごとにスコア行を生成
        PlayerScore prevScore = default; 
        int prevRank = 1;
        int index = 0;
        foreach (var pair in GetSortedScores())
        {
            var playerRef = pair.Key;
            var score = pair.Value;
            int rank = 0;

            // 順位計算
            if (index == 0)
            {
                prevScore = score; 
                rank = 1; 
            }
            else
            {
                if (score.Kills == prevScore.Kills&&score.Deaths == prevScore.Deaths)
                {
                    // 前のスコアと同じなら順位を前回と同じにする
                    rank = prevRank; 
                }
                else
                {
                    // 前のスコアと異なる場合は新しい順位を設定
                    rank = index + 1; 
                }
            }

            GameObject scoreRow;
            scoreRow = Instantiate(scoreRowPrefab, contentParent);
            UpdateResultRow(scoreRow, rank, playerRef, score);
            scoreRow.transform.SetSiblingIndex(index);

            // 前のスコアと順位を更新
            prevScore = score;
            prevRank = rank; 
            index++;
        }
    }

    private void UpdateResultRow(GameObject row, int rank, PlayerRef playerRef, PlayerScore score)
    {
        row.transform.Find("RankImage/RankText").GetComponent<TMP_Text>().text = (rank).ToString();
        //Info : プレイヤー名称をつける場合はここをいじる
        row.transform.Find("PlayerNameText").GetComponent<TMP_Text>().text = $"Player {playerRef.PlayerId}"; 
        row.transform.Find("KillScoreImage/KillCountText").GetComponent<TMP_Text>().text = score.Kills.ToString();
        row.transform.Find("DeathScoreImage/DeathCountText").GetComponent<TMP_Text>().text = score.Deaths.ToString();
        // 自分のスコア行を目立たせるためにハイライト表示する
        if (playerRef == Runner.LocalPlayer)
        {
            row.transform.Find("ColorBar").gameObject.SetActive(true); 
        }
        else
        {
            row.transform.Find("ColorBar").gameObject.SetActive(false);
        }
    }

    public IReadOnlyList<KeyValuePair<PlayerRef, PlayerScore>> GetSortedScores()
    {
        var sortedScores = PlayerScores
         .OrderByDescending(kvp => kvp.Value.Kills)     // キル数が多い順iko
         .ThenBy(kvp => kvp.Value.Deaths)               // デス数が少ない順
         .ThenBy(kvp => kvp.Key.RawEncoded)             // PlayerRefの数値が小さい順
         .ToList();

        return sortedScores;
    }


}
