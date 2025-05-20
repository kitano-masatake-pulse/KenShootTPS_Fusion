using TMPro;
using Fusion;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class HUDManager : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private TMP_Text scoreText;

    private PlayerNetworkState localState;

    public void SetLocalStateAndSubscribe(PlayerNetworkState state)
    {
        localState = state;
        InitializeSubscriptions();

        // 初期値反映
        hpSlider.value = localState.HpNormalized;
    }

    void InitializeSubscriptions()
    {
        // 先に解除してから
        localState.OnHPChanged -= OnLocalHPChanged;
        localState.OnWeaponChanged -= OnLocalWeaponChanged;
        // …

        // 改めて登録
        localState.OnHPChanged += OnLocalHPChanged;
        localState.OnWeaponChanged += OnLocalWeaponChanged;
        // …
    }

    private void OnLocalHPChanged(float normalized)
    {
        hpSlider.value = normalized;
    }
    void OnLocalWeaponChanged(WeaponType w) { /* 武器名を更新 */ }


    void OnDestroy()
    {
        if (localState != null)
            localState.OnHPChanged -= OnLocalHPChanged;

    }
}
