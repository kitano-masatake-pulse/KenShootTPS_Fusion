using UnityEngine;
using UnityEngine.UI;
using Fusion;

// HP 用パネル
public class HPPanel : MonoBehaviour, IHUDPanel
{
    [SerializeField] private Slider hpSlider;
    private PlayerNetworkState playerState;

    public void Initialize(PlayerNetworkState pState, PlayerAvatar _)
    {
        playerState = pState;
        // イベント登録
        playerState.OnHPChanged -= UpdateHPBar;
        playerState.OnHPChanged += UpdateHPBar;
        playerState.OnHPChanged += OrderTestHP; // テスト用イベント登録
        if (GameManager2.Instance != null)
        {
            GameManager2.Instance.OnScoreChanged += OrderTestScore; // GameManagerのイベント登録
            GameManager2.Instance.OnAnyPlayerDied += OrderTestDeath; // Player死亡イベント登録
        }
        // 初期値設定
        UpdateHPBar(playerState.HpNormalized, PlayerRef.None);
    }
    public void Cleanup()
    {
        playerState.OnHPChanged -= UpdateHPBar;
    }
    private void UpdateHPBar(float hpNormalized,PlayerRef _) => hpSlider.value = hpNormalized;

    private void OrderTestHP(float hpNormalized, PlayerRef _)
    {
        // ここにテストコードを追加することができます。

        Debug.Log("OrderTest:HPPanel OrderTest called with HPChanged");
    }
    private void OrderTestScore()
    {
        // ここにテストコードを追加することができます。
        Debug.Log("OrderTest:HPPanel OrderTest called with ScoreChanged");
    }
    private void OrderTestDeath(float timeStamp, PlayerRef victim, PlayerRef killer)
    {
        // ここにテストコードを追加することができます。
        Debug.Log($"OrderTest:HPPanel OrderTest called with PlayerDied");
    }
}