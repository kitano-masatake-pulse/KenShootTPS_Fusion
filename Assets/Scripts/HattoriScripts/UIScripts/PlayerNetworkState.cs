using System;
using Fusion;

public class PlayerNetworkState : NetworkBehaviour
{
    // イベント発行部分
    public event Action<float> OnHPChanged;
    public event Action<WeaponType> OnWeaponChanged;
    public event Action<int, int> OnAmmoChanged;   // (totalAmmo, magazineAmmo)
    public event Action<int> OnScoreChanged;

    // ──── HP(現在/最大)────────────────────
    [Networked(OnChanged = nameof(HPChangedCallback))] public int CurrentHP { get; set; }
    [Networked] public int MaxHP { get; set; }
    public float HpNormalized => MaxHP > 0 ? (float)CurrentHP / MaxHP : 0f;
    static void HPChangedCallback(Changed<PlayerNetworkState> c) => c.Behaviour.RaiseHPChanged();
    void RaiseHPChanged() => OnHPChanged?.Invoke(HpNormalized);

    // ──── 装備中武器 ─────────────────────
    [Networked(OnChanged = nameof(WeaponChangedCallback))]
    public WeaponType CurrentWeapon { get; set; } = WeaponType.Sword;
    static void WeaponChangedCallback(Changed<PlayerNetworkState> c) => c.Behaviour.RaiseWeaponChanged();
    void RaiseWeaponChanged() => OnWeaponChanged?.Invoke(CurrentWeapon);

    // ──── 弾薬（総弾数／マガジン弾数）───
    [Networked(OnChanged = nameof(AmmoChangedCallback))]
    public int TotalAmmo { get; set; }
    [Networked(OnChanged = nameof(AmmoChangedCallback))]
    public int MagazineAmmo { get; set; }
    static void AmmoChangedCallback(Changed<PlayerNetworkState> c) => c.Behaviour.RaiseAmmoChanged(); 
    void RaiseAmmoChanged() => OnAmmoChanged?.Invoke(TotalAmmo, MagazineAmmo);

    // ──── スコア ──────────────────────────
    [Networked(OnChanged = nameof(ScoreChangedCallback))]
    public int Score { get; set; }
    static void ScoreChangedCallback(Changed<PlayerNetworkState> c) => c.Behaviour.RaiseScoreChanged();
    void RaiseScoreChanged() => OnScoreChanged?.Invoke(Score);




}
