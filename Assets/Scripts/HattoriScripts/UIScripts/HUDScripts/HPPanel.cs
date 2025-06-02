using UnityEngine;
using UnityEngine.UI;

// HP 用パネル
public class HPPanel : MonoBehaviour, IHUDPanel
{
    [SerializeField] private Slider hpSlider;
    private PlayerNetworkState playerState;

    public void Initialize(PlayerNetworkState pState, WeaponLocalState _)
    {
        playerState = pState;
        // イベント登録
        playerState.OnHPChanged -= UpdateHPBar;
        playerState.OnHPChanged += UpdateHPBar;
        // 初期値設定
        UpdateHPBar(playerState.HpNormalized);
    }
    public void Cleanup()
    {
       playerState.OnHPChanged -= UpdateHPBar;
    }
    private void UpdateHPBar(float hpNormalized) => hpSlider.value = hpNormalized;
}