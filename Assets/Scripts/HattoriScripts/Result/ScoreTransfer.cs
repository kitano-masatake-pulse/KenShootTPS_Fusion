using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
public class ScoreTransfer : MonoBehaviour
{
    private List<UserData> scores= new List<UserData>();

    public void SetScores(IReadOnlyList<UserData> scoreList)
    {
        scores.Clear();
        foreach (var score in scoreList)
        {
            scores.Add(score);
        }
    }

    public IReadOnlyList<UserData> GetScores()
    {
        return scores.AsReadOnly();
    }
}
