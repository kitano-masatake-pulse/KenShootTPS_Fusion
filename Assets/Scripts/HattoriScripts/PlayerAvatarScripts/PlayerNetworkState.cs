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
    public event Action<WeaponType> OnWeaponChanged_Network;
  
    /// <summary>スコアが変更されたとき</summary>
    public event Action<int, int> OnScoreChanged;

    /// <summary>弾数が変更されたとき(チート対策用、基本使わない)</summary>
    public event Action<int, int> OnAmmoChanged_Network;

    // ローカルプレイヤー生成時
    public static event Action<PlayerNetworkState> OnLocalPlayerSpawned;
    
    /// <summary>プレイヤー死亡時(Victim,Killer)</summary>
    public event Action<PlayerRef, PlayerRef> OnPlayerDied;

    #endregion

    #region Networked Properties

    // HP (現在 / 最大)
    [Networked(OnChanged = nameof(HPChangedCallback))]
    public int CurrentHP { get; private set; }

    [Networked(OnChanged = nameof(HPChangedCallback))]
    public int MaxHP { get; private set; }

    public float HpNormalized => MaxHP > 0 ? (float)CurrentHP / MaxHP : 0f;

    // 装備中の武器(サーバーから見て)
    [Networked(OnChanged = nameof(WeaponChangedCallback))]
    public WeaponType CurrentWeapon_Network { get; private set; } = WeaponType.Sword;

    // スコア (キル / デス)
    [Networked(OnChanged = nameof(ScoreChangedCallback))]
    public int KillScore { get; private set; }

    [Networked(OnChanged = nameof(ScoreChangedCallback))]
    public int DeathScore { get; private set; }

  // 弾薬 (マガジン / リザーブ)(チート対策用、基本使わない)
    [Networked(OnChanged = nameof(AmmoChangedCallback))]
    public int MagazineAmmo_Network { get; private set; }

    [Networked(OnChanged = nameof(AmmoChangedCallback))]
    public int ReserveAmmo_Network { get; private set; }

    #endregion

    #region Local Fields

    // ローカルで管理する武器ごとの弾数テーブル(チート対策用)
    private Ammo[] ammoPerWeapon;

    #endregion

    #region Change Callbacks

    static void HPChangedCallback(Changed<PlayerNetworkState> changed)
    { 
        // コールバックが呼ばれたときにデバッグログを出力
        Debug.Log($"HPコールバック呼び出し");
        changed.Behaviour.RaiseHPChanged();
        
    }

    static void WeaponChangedCallback(Changed<PlayerNetworkState> changed)
        => changed.Behaviour.RaiseWeaponChanged();


    static void ScoreChangedCallback(Changed<PlayerNetworkState> changed)
        => changed.Behaviour.RaiseScoreChanged();

    static void AmmoChangedCallback(Changed<PlayerNetworkState> changed)
        => changed.Behaviour.RaiseAmmoChanged();

    void RaiseHPChanged() => OnHPChanged?.Invoke(HpNormalized);
    void RaiseWeaponChanged() => OnWeaponChanged_Network?.Invoke(CurrentWeapon_Network);
    void RaiseScoreChanged() => OnScoreChanged?.Invoke(KillScore, DeathScore);
    void RaiseAmmoChanged() => OnAmmoChanged_Network?.Invoke(MagazineAmmo_Network, ReserveAmmo_Network);
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

    //デバッグ用 
    void Update()
    {

        if (HasInputAuthority && Input.GetKeyDown(KeyCode.K))
        {
            // 自分を即死させる
            DamageHP(int.MaxValue, PlayerRef.None);
        }
    }

    #endregion

    #region Public Methods
    //―――― サーバー→クライアント：ステータス変更 ――――
    //ホスト側のみが呼び出すメソッド
    /// <summary>HPを減らす</summary>
    public void DamageHP(int damage, PlayerRef attacker = default)
    {
        Debug.Log($"DamageHPMethod");
        if (!HasStateAuthority) return;
        if (damage <= 0) return; // ダメージが0以下なら無視
        if (CurrentHP <= 0) return; // 既に死亡しているなら無視

        CurrentHP = Mathf.Max(0, CurrentHP - damage);

        Debug.Log($"Player {Object.InputAuthority} took {damage} damage, remaining HP: {CurrentHP}");

        if (CurrentHP == 0)
        {
            Debug.Log($"Player {Object.InputAuthority} has died.");
            // 自身のデススコア加算
            AddDeathScore();

            // 攻撃者が指定されていればキルスコア加算
            if (attacker != PlayerRef.None && attacker != Object.InputAuthority)
            {
                if (Runner.TryGetPlayerObject(attacker, out var atkGo))
                {
                    var atkState = atkGo.GetComponent<PlayerNetworkState>();
                    if (atkState != null && atkState.HasStateAuthority)
                        atkState.AddKillScore();
                }
            }

            // 死亡通知 RPC
            RPC_PlayerDeath( Object.InputAuthority, attacker);
        }
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
    ///<summary>死亡イベント</summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayerDeath(PlayerRef victim, PlayerRef killer)
    {
        Debug.Log("死亡イベント発火");
        OnPlayerDied?.Invoke(victim, killer);
    }

    //―――― クライアント→サーバー：武器切替リクエスト ――――
    //クライアント側から呼び出すメソッド
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestWeaponChange(WeaponType newWeapon)
    {
        CurrentWeapon_Network = newWeapon;
    }

    #endregion
}
