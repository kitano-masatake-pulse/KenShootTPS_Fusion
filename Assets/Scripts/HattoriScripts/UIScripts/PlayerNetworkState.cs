using System;
using UnityEngine;
using Fusion;

// 弾薬データ構造
public struct Ammo
{
    public int Magazine;
    public int Reserve;

    public Ammo(int mag, int reserve)
    {
        Magazine = mag;
        Reserve = reserve;
    }
}

// 各プレイヤーのステータスを管理するクラス
public class PlayerNetworkState : NetworkBehaviour
{
    #region Events

    // インスタンスイベント
    public event Action<float> OnHPChanged;
    public event Action<WeaponType> OnWeaponChanged;
    public event Action<int, int> OnAmmoChanged;
    public event Action<int, int> OnScoreChanged;

    // ローカルプレイヤー生成時
    public static event Action<PlayerNetworkState> OnLocalPlayerSpawned;

    #endregion

    #region Networked Properties

    // HP (現在 / 最大)
    [Networked(OnChanged = nameof(HPChangedCallback))]
    public int CurrentHP { get; set; }

    [Networked(OnChanged = nameof(HPChangedCallback))]
    public int MaxHP { get; set; }

    public float HpNormalized => MaxHP > 0 ? (float)CurrentHP / MaxHP : 0f;

    // 装備中の武器
    [Networked(OnChanged = nameof(WeaponChangedCallback))]
    public WeaponType CurrentWeapon { get; set; } = WeaponType.Sword;

    // 弾薬 (マガジン / リザーブ)
    [Networked(OnChanged = nameof(AmmoChangedCallback))]
    public int MagazineAmmo { get; set; }

    [Networked(OnChanged = nameof(AmmoChangedCallback))]
    public int ReserveAmmo { get; set; }

    // スコア (キル / デス)
    [Networked(OnChanged = nameof(ScoreChangedCallback))]
    public int KillScore { get; set; }

    [Networked(OnChanged = nameof(ScoreChangedCallback))]
    public int DeathScore { get; set; }

    #endregion

    #region Local Fields

    // ローカルで管理する武器ごとの弾数テーブル
    private Ammo[] ammoPerWeapon;

    #endregion

    #region Change Callbacks

    static void HPChangedCallback(Changed<PlayerNetworkState> c)
        => c.Behaviour.RaiseHPChanged();

    static void WeaponChangedCallback(Changed<PlayerNetworkState> c)
    {
        var s = c.Behaviour;
        s.RaiseWeaponChanged();
        s.ApplyAmmoForWeapon(s.CurrentWeapon);
    }

    static void AmmoChangedCallback(Changed<PlayerNetworkState> c)
        => c.Behaviour.RaiseAmmoChanged();

    static void ScoreChangedCallback(Changed<PlayerNetworkState> c)
        => c.Behaviour.RaiseScoreChanged();

    void RaiseHPChanged() => OnHPChanged?.Invoke(HpNormalized);
    void RaiseWeaponChanged() => OnWeaponChanged?.Invoke(CurrentWeapon);
    void RaiseAmmoChanged() => OnAmmoChanged?.Invoke(MagazineAmmo, ReserveAmmo);
    void RaiseScoreChanged() => OnScoreChanged?.Invoke(KillScore, DeathScore);

    #endregion

    #region Unity Callbacks

    public override void Spawned()
    {       
        // 配列初期化
        int count = (int)WeaponType.GrenadeLauncher + 1;
        ammoPerWeapon = new Ammo[count];

        // 初期弾数設定
        ammoPerWeapon[(int)WeaponType.Sword] = new Ammo(1, 0);
        ammoPerWeapon[(int)WeaponType.AssaultRifle] = new Ammo(20, 100);
        ammoPerWeapon[(int)WeaponType.SemiAutoRifle] = new Ammo(5, 15);
        ammoPerWeapon[(int)WeaponType.GrenadeLauncher] = new Ammo(1, 5);
        if (HasInputAuthority)
        {    
            OnLocalPlayerSpawned?.Invoke(this);
        }

        if (Object.HasStateAuthority)
        {
            ApplyAmmoForWeapon(CurrentWeapon);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>ローカルから武器切替要求</summary>
    public void ChangeWeapon(WeaponType newWeapon)
    {
        if (!HasInputAuthority) return;
        CurrentWeapon = newWeapon;     // 同期
        ApplyAmmoForWeapon(newWeapon); // ホストで反映
    }

    /// <summary>ローカルで弾数を調整</summary>
    public void ModifyLocalAmmo(int magazineDelta, int reserveDelta)
    {
        var idx = (int)CurrentWeapon;
        ammoPerWeapon[idx].Magazine += magazineDelta;
        ammoPerWeapon[idx].Reserve += reserveDelta;
        ApplyAmmoForWeapon(CurrentWeapon);
    }

    /// <summary>指定武器の弾数をネットワークプロパティに書き込む</summary>
    public void ApplyAmmoForWeapon(WeaponType weapon)
    {
        var a = ammoPerWeapon[(int)weapon];
        MagazineAmmo = a.Magazine;
        ReserveAmmo = a.Reserve;
    }


    /// <summary>HPを減らす</summary>
    public void DamageHP(int damage)
    {
        if (!HasStateAuthority) return;
        CurrentHP = Mathf.Max(0, CurrentHP - damage);
    }
    /// <summary>HPを回復する</summary>
    public void HealHP(int heal)
    {
        if (!HasStateAuthority) return;
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + heal);
    }
    /// <summary>Killスコアを加算する</summary>
    public void AddKillScore()
    {
        if (!HasStateAuthority) return;
        KillScore ++;
    }
    /// <summary>Deathスコアを加算する</summary>
    public void AddDeathScore()
    {
        if (!HasStateAuthority) return;
        DeathScore++;
    }

    #endregion
}
