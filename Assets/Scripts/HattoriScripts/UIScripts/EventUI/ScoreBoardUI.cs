using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreBoardUI : MonoBehaviour, IUIPanel
{
    [SerializeField] private CanvasGroup scoreboardGroup;
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject scoreRowPrefab;

    private Dictionary<PlayerRef, GameObject> rowInstances = new();

    public void Initialize()
    {
        GameManager.Instance.OnScoreChanged -= UpdateAllRows;
        GameManager.Instance.OnScoreChanged += UpdateAllRows;
        scoreboardGroup.alpha = 0;
        scoreboardGroup.interactable = false;
        scoreboardGroup.blocksRaycasts = false;
        UpdateAllRows();
    }

    public void Cleanup()
    {
        GameManager.Instance.OnScoreChanged -= UpdateAllRows;
        // すべての行を削除
        foreach (var row in rowInstances.Values)
        {
            Destroy(row);
        }
        rowInstances.Clear();
        SetVisible(false); // パネルを非表示にする
    }

    public void SetVisible(bool visible)
    {
        scoreboardGroup.alpha = visible ? 1 : 0;
    }

    public void UpdateAllRows()
    {
        var sortedScores = GameManager.Instance.GetSortedScores();

        int index = 0;
        foreach (var pair in sortedScores)
        {
            var playerRef = pair.Key;
            var score = pair.Value;
            Debug.Log($"ScoreBoardUI : Updating score for Player {playerRef.PlayerId}: Kills={score.Kills}, Deaths={score.Deaths}");
            if (!rowInstances.TryGetValue(playerRef, out GameObject row))
            {
                Debug.Log($"ScoreBoardUI : Creating new row for Player {playerRef.PlayerId}");
                row = Instantiate(scoreRowPrefab, contentParent);
                rowInstances[playerRef] = row;
            }

            UpdateRow(row, playerRef, score);
            row.transform.SetSiblingIndex(index);
            index++;
        }
    }

    private void UpdateRow(GameObject row, PlayerRef playerRef, PlayerScore score)
    {
        row.transform.Find("PlayerNameText").GetComponent<TMP_Text>().text = $"Player {playerRef.PlayerId}"; // or custom name
        row.transform.Find("KillScoreImage/KillCountText").GetComponent<TMP_Text>().text = score.Kills.ToString();
        row.transform.Find("DeathScoreImage/DeathCountText").GetComponent<TMP_Text>().text = score.Deaths.ToString();
        //自分ならハイライトする
        if (playerRef == GameManager.Instance.GetMyPlayerRef())
        {
            row.transform.Find("ColorBar").gameObject.SetActive(true); // 自分の行はハイライト
        }
        else
        {
            row.transform.Find("ColorBar").gameObject.SetActive(false);
        }



    }

}
