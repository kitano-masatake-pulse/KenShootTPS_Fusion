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

        // �����l���f
        hpSlider.value = localState.HpNormalized;
    }

    void InitializeSubscriptions()
    {
        // ��ɉ������Ă���
        localState.OnHPChanged -= OnLocalHPChanged;
        localState.OnWeaponChanged -= OnLocalWeaponChanged;
        // �c

        // ���߂ēo�^
        localState.OnHPChanged += OnLocalHPChanged;
        localState.OnWeaponChanged += OnLocalWeaponChanged;
        // �c
    }

    private void OnLocalHPChanged(float normalized)
    {
        hpSlider.value = normalized;
    }
    void OnLocalWeaponChanged(WeaponType w) { /* ���햼���X�V */ }


    void OnDestroy()
    {
        if (localState != null)
            localState.OnHPChanged -= OnLocalHPChanged;

    }
}
