using TMPro;
using UnityEngine;
using Fusion;

// スコア用パネル  
public class ScorePanel : MonoBehaviour, IHUDPanel
{
    [SerializeField] private TMP_Text killCount, deathCount;

    public void Initialize(PlayerNetworkState _, PlayerAvatar __)
    {


        GameManager.Instance.OnScoreChanged -= UpdateScoreText;
        GameManager.Instance.OnScoreChanged += UpdateScoreText;
        //初期値設定  
        UpdateScoreText();

        
    }

    public void Cleanup()
    {
        GameManager.Instance.OnScoreChanged -= UpdateScoreText;
    }

  

    private void UpdateScoreText()
    {

        if (GameManager.Instance.TryGetMyScore(out var score))
        {
            killCount.text = score.Kills.ToString();
            deathCount.text = score.Deaths.ToString();
        }
       
    }
}
