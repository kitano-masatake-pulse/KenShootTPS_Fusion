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
    /// <summary>弾数が変更されたとき</summary>
    public event Action<float> OnHPChanged;
    /// <summary>武器切替がサーバー正史で確定したとき</summary>
    public event Action<WeaponType> OnWeaponChanged;
  
    /// <summary>スコアが変更されたとき</summary>
    public event Action<int, int> OnScoreChanged;

    /// <summary>弾数が変更されたとき(チート対策用、基本使わない)</summary>
    public event Action<int, int> OnAmmoChanged;

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

    // 装備中の武器(サーバーから見て)
    [Networked(OnChanged = nameof(WeaponChangedCallback))]
    public WeaponType CurrentWeapon { get; private set; } = WeaponType.Sword;

    // スコア (キル / デス)
    [Networked(OnChanged = nameof(ScoreChangedCallback))]
    public int KillScore { get; set; }

    [Networked(OnChanged = nameof(ScoreChangedCallback))]
    public int DeathScore { get; set; }

  // 弾薬 (マガジン / リザーブ)(チート対策用、基本使わない)
    [Networked(OnChanged = nameof(AmmoChangedCallback))]
    public int MagazineAmmo { get; set; }

    [Networked(OnChanged = nameof(AmmoChangedCallback))]
    public int ReserveAmmo { get; set; }

    #endregion

    #region Local Fields

    // ローカルで管理する武器ごとの弾数テーブル(チート対策用)
    private Ammo[] ammoPerWeapon;

    #endregion

    #region Change Callbacks

    static void HPChangedCallback(Changed<PlayerNetworkState> c)
        => c.Behaviour.RaiseHPChanged();

    static void WeaponChangedCallback(Changed<PlayerNetworkState> c)
        => c.Behaviour.RaiseWeaponChanged();


    static void ScoreChangedCallback(Changed<PlayerNetworkState> c)
        => c.Behaviour.RaiseScoreChanged();

    static void AmmoChangedCallback(Changed<PlayerNetworkState> c)
        => c.Behaviour.RaiseAmmoChanged();

    void RaiseHPChanged() => OnHPChanged?.Invoke(HpNormalized);
    void RaiseWeaponChanged() => OnWeaponChanged?.Invoke(CurrentWeapon);
    void RaiseScoreChanged() => OnScoreChanged?.Invoke(KillScore, DeathScore);
    void RaiseAmmoChanged() => OnAmmoChanged?.Invoke(MagazineAmmo, ReserveAmmo);
    #endregion

    #region Unity Callbacks

    public override void Spawned()
    {
        if (HasInputAuthority)
        {    
            OnLocalPlayerSpawned?.Invoke(this);
        }    
        
        
        //ホストのみプレイヤーごとの弾数を初期化(チート対策用)
        if (Object.HasStateAuthority)
        {
            // 配列初期化
            int count = (int)WeaponType.GrenadeLauncher + 1;
            ammoPerWeapon = new Ammo[count];

            // 初期弾数設定
            ammoPerWeapon[(int)WeaponType.Sword] = new Ammo(1, 0);
            ammoPerWeapon[(int)WeaponType.AssaultRifle] = new Ammo(20, 100);
            ammoPerWeapon[(int)WeaponType.SemiAutoRifle] = new Ammo(5, 15);
            ammoPerWeapon[(int)WeaponType.GrenadeLauncher] = new Ammo(1, 5);
        }
    }

    #endregion

    #region Public Methods
    //―――― サーバー→クライアント：ステータス変更 ――――
    //ホスト側のみが呼び出すメソッド
    /// <summary>HPを減らす</summary>
    public void DamageHP(int damage)
    {
        if (!HasStateAuthority) return;
        CurrentHP = Mathf.Max(0, CurrentHP - damage);
        // HPが0になったら死亡イベントを起動
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

    //―――― クライアント→サーバー：武器切替リクエスト ――――
    //クライアント側から呼び出すメソッド
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestWeaponChange(WeaponType newWeapon)
    {
        Debug.Log($"RPC実行");
        CurrentWeapon = newWeapon;
    }


    #endregion
}
