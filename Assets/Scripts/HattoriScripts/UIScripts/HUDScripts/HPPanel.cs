using UnityEngine;
using UnityEngine.UI;

// HP �p�p�l��
public class HPPanel : MonoBehaviour, IHUDPanel
{
    [SerializeField] private Slider hpSlider;
    private PlayerNetworkState playerState;

    public void Initialize(PlayerNetworkState pState, WeaponLocalState _)
    {
        playerState = pState;
        // �C�x���g�o�^
        playerState.OnHPChanged -= UpdateHPBar;
        playerState.OnHPChanged += UpdateHPBar;
        // �����l�ݒ�
        UpdateHPBar(playerState.HpNormalized);
    }
    public void Cleanup()
    {
       playerState.OnHPChanged -= UpdateHPBar;
    }
    private void UpdateHPBar(float hpNormalized) => hpSlider.value = hpNormalized;
}