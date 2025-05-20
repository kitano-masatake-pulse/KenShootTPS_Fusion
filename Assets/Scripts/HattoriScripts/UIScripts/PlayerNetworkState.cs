using System;
using Fusion;

public class PlayerNetworkState : NetworkBehaviour
{
    // �C�x���g���s����
    public event Action<float> OnHPChanged;
    public event Action<WeaponType> OnWeaponChanged;
    public event Action<int, int> OnAmmoChanged;   // (totalAmmo, magazineAmmo)
    public event Action<int> OnScoreChanged;

    // �������� HP(����/�ő�)����������������������������������������
    [Networked(OnChanged = nameof(HPChangedCallback))] public int CurrentHP { get; set; }
    [Networked] public int MaxHP { get; set; }
    public float HpNormalized => MaxHP > 0 ? (float)CurrentHP / MaxHP : 0f;
    static void HPChangedCallback(Changed<PlayerNetworkState> c) => c.Behaviour.RaiseHPChanged();
    void RaiseHPChanged() => OnHPChanged?.Invoke(HpNormalized);

    // �������� ���������� ������������������������������������������
    [Networked(OnChanged = nameof(WeaponChangedCallback))]
    public WeaponType CurrentWeapon { get; set; } = WeaponType.Sword;
    static void WeaponChangedCallback(Changed<PlayerNetworkState> c) => c.Behaviour.RaiseWeaponChanged();
    void RaiseWeaponChanged() => OnWeaponChanged?.Invoke(CurrentWeapon);

    // �������� �e��i���e���^�}�K�W���e���j������
    [Networked(OnChanged = nameof(AmmoChangedCallback))]
    public int TotalAmmo { get; set; }
    [Networked(OnChanged = nameof(AmmoChangedCallback))]
    public int MagazineAmmo { get; set; }
    static void AmmoChangedCallback(Changed<PlayerNetworkState> c) => c.Behaviour.RaiseAmmoChanged(); 
    void RaiseAmmoChanged() => OnAmmoChanged?.Invoke(TotalAmmo, MagazineAmmo);

    // �������� �X�R�A ����������������������������������������������������
    [Networked(OnChanged = nameof(ScoreChangedCallback))]
    public int Score { get; set; }
    static void ScoreChangedCallback(Changed<PlayerNetworkState> c) => c.Behaviour.RaiseScoreChanged();
    void RaiseScoreChanged() => OnScoreChanged?.Invoke(Score);




}
