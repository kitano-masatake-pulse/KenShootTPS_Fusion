using TMPro;
using UnityEngine;

// スコア用パネル  
public class ScorePanel : MonoBehaviour, IHUDPanel
{
    [SerializeField] private TMP_Text killCount, deathCount;

    public void Initialize(PlayerNetworkState _, PlayerAvatar __)
    {


        GameManager2.Instance.OnScoreChanged -= UpdateScoreText;
        GameManager2.Instance.OnScoreChanged += UpdateScoreText;
        //初期値設定  
        UpdateScoreText();

        
    }

    public void Cleanup()
    {
        GameManager2.Instance.OnScoreChanged -= UpdateScoreText;
    }

  

    private void UpdateScoreText()
    {

        if (GameManager2.Instance.TryGetMyScore(out var score))
        {
            killCount.text = score.Kills.ToString();
            deathCount.text = score.Deaths.ToString();
        }
       
    }
}
