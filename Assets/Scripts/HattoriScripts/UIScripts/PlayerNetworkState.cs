using Fusion;

public class PlayerNetworkState : NetworkBehaviour
{
    [Networked(OnChanged = nameof(OnHPChanged))] public int CurrentHP { get; set; }
    [Networked] public int MaxHP { get; set; }
    [Networked] public int Ammo { get; set; }
    [Networked] public int Score { get; set; }

    // HUDManager�ɓn���p�v���p�e�B(����HP�́���Ԃ�)
    public float HpNormalized => MaxHP > 0 ? (float)CurrentHP / MaxHP : 0f;
    
    // Change�C�x���g��UI�X�V���Ăяo��
    static void OnHPChanged(Changed<PlayerNetworkState> changed)
    {
        changed.Behaviour.OnNetworkedHPChanged();
    }
    // �C���X�^���X���\�b�h�ŁA�o�^���ꂽUI�ɒʒm
    void OnNetworkedHPChanged()
    {
        var hud = GetComponentInChildren<PlayerHUDController>();
        if (hud != null)
            hud.UpdateHPBar(HpNormalized);
    }
}
