using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class ScoreTransfer : MonoBehaviour
{
    private List<KeyValuePair<PlayerRef, PlayerScore>> scores = new List<KeyValuePair<PlayerRef, PlayerScore>>();

    public void SetScores(IReadOnlyList<KeyValuePair<PlayerRef, PlayerScore>> scoreList)
    {
        scores.Clear();
        foreach (var score in scoreList)
        {
            scores.Add(score);
        }
    }

    public IReadOnlyList<KeyValuePair<PlayerRef, PlayerScore>> GetScores()
    {
        return scores.AsReadOnly();
    }
}
