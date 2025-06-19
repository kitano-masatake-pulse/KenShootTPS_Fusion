using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoardUI : MonoBehaviour, IUIPanel
{
    [SerializeField] private CanvasGroup scoreboardGroup;
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject scoreRowPrefab;
    [SerializeField] private HPObserver hpObserver; 

    private Dictionary<PlayerRef, GameObject> rowInstances = new();

    public void Initialize()
    {
        GameManager.Instance.OnScoreChanged -= UpdateAllRows;
        GameManager.Instance.OnScoreChanged += UpdateAllRows;
        hpObserver.OnAnyHPChanged -= UpdateLivingStates;
        hpObserver.OnAnyHPChanged += UpdateLivingStates;
        scoreboardGroup.alpha = 0;
        scoreboardGroup.interactable = false;
        scoreboardGroup.blocksRaycasts = false;
        UpdateAllRows();
    }

    public void Cleanup()
    {
        GameManager.Instance.OnScoreChanged -= UpdateAllRows;
        hpObserver.OnAnyHPChanged -= UpdateLivingStates;
        // ���ׂĂ̍s���폜
        foreach (var row in rowInstances.Values)
        {
            Destroy(row);
        }
        rowInstances.Clear();
        SetVisible(false); // �p�l�����\���ɂ���
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

    public void UpdateLivingStates() 
    {
        if (hpObserver == null) return; // HPObserver���ݒ肳��Ă��Ȃ��ꍇ�͉������Ȃ�
        foreach (var pair in rowInstances)
        {
            var playerRef = pair.Key;
            var row = pair.Value;
            if (hpObserver.PlayerHPDict.TryGetValue(playerRef, out float hpNormalized))
            {
                var deathImage = row.transform.Find("LivingStateImage/DeathImage").GetComponent<Image>();
                var color = deathImage.color;
                if (hpNormalized > 0)
                {
                    // �������͓����ɂ���
                    color.a = 0f;
                }
                else
                {
                    // ���S���͕s�����ɂ���
                    color.a = 1f;
                }
                Debug.Log($"ScoreBoardUI : Player {playerRef.PlayerId} HP Normalized: {hpNormalized}, DeathImage Alpha: {color.a}");
                deathImage.color = color;
            }
        }
    }

    private void UpdateRow(GameObject row, PlayerRef playerRef, PlayerScore score)
    {
        row.transform.Find("PlayerNameText").GetComponent<TMP_Text>().text = $"Player {playerRef.PlayerId}"; // or custom name
        row.transform.Find("KillScoreImage/KillCountText").GetComponent<TMP_Text>().text = score.Kills.ToString();
        row.transform.Find("DeathScoreImage/DeathCountText").GetComponent<TMP_Text>().text = score.Deaths.ToString();
        // �����Ȃ�n�C���C�g����
        if (playerRef == GameManager.Instance.GetMyPlayerRef())
        {
            row.transform.Find("ColorBar").gameObject.SetActive(true); // �����̍s�̓n�C���C�g
        }
        else
        {
            row.transform.Find("ColorBar").gameObject.SetActive(false);
        }
    }

}
