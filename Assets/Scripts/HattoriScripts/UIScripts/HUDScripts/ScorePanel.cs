using TMPro;
using UnityEngine;

// スコア用パネル
public class ScorePanel : MonoBehaviour, IHUDPanel
{
    [SerializeField] private TMP_Text killCount, deathCount;
    private PlayerNetworkState playerState;
    public void Initialize(PlayerNetworkState pState, WeaponLocalState _)
    {
        playerState = pState;
        //イベント登録
        playerState.OnScoreChanged -= UpdateScoreText; 
        playerState.OnScoreChanged += UpdateScoreText;
        //初期値設定
        UpdateScoreText(playerState.KillScore, playerState.DeathScore);
    }
    public void Cleanup() {
        playerState.OnScoreChanged -= UpdateScoreText;
    }
    private void UpdateScoreText(int k, int d)
    {
        killCount.text = k.ToString();
        deathCount.text = d.ToString();
    }
}
