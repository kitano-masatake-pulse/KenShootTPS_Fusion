using System;
using UnityEngine;
using Fusion;


// 各プレイヤーのステータスを管理するクラス
public class PlayerNetworkState : NetworkBehaviour
{
    #region Events

    // インスタンスイベント
    /// <summary>HPが変更されたとき</summary>
    public event Action<float> OnHPChanged;

    /// <summary>武器切替がサーバー正史で確定したとき</summary>
    public event Action<WeaponType> OnWeaponChanged_Network;

    /// <summary>チームが変更されたとき</summary>
    public event Action<TeamType> OnTeamChanged;

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


    //無敵状態かどうか
    [Networked]
    public bool IsInvincible { get; private set; } = false;


    // 装備中の武器(サーバーから見て)
    [Networked(OnChanged = nameof(WeaponChangedCallback))]
    public WeaponType CurrentWeapon_Network { get; private set; } = WeaponType.Sword;

    //所属チーム
    [Networked(OnChanged =nameof(TeamChangedCallback))]
    public TeamType Team { get; private set; } = TeamType.None;

    #endregion

    #region Change Callbacks

    static void HPChangedCallback(Changed<PlayerNetworkState> changed)
        =>changed.Behaviour.RaiseHPChanged();
    static void WeaponChangedCallback(Changed<PlayerNetworkState> changed)
        => changed.Behaviour.RaiseWeaponChanged();
    static void TeamChangedCallback(Changed<PlayerNetworkState> changed)
        => changed.Behaviour.RaiseTeamChanged();


    void RaiseHPChanged() => OnHPChanged?.Invoke(HpNormalized);
    void RaiseWeaponChanged() => OnWeaponChanged_Network?.Invoke(CurrentWeapon_Network);
    void RaiseTeamChanged() => OnTeamChanged?.Invoke(Team);
    #endregion

    #region Unity Callbacks

    public override void Spawned()
    {
        if (HasInputAuthority)
        {   
            //FindObjectOfType<HUDManager>()?.PlayerHUDInitialize(this);
            OnLocalPlayerSpawned?.Invoke(this);
        }    
  
    }

    //デバッグ用 
    void Update()
    {

        if (HasInputAuthority && Input.GetKeyDown(KeyCode.K))
        {
            // 自分を即死させる
            RPC_RequestDamageHP(int.MaxValue, PlayerRef.None);
        }
    }
    #endregion

    #region Public Methods
    //―――― サーバー→クライアント：ステータス変更 ――――
    //ホスト側のみが呼び出すメソッド
    /// <summary>HPを減らす</summary>
    public void DamageHP(int damage, PlayerRef attacker = default, TeamType atkTeam = default)
    {
        Debug.Log($"DamageHPMethod");
        if (!HasStateAuthority) return;
        if(Team != TeamType.None && atkTeam != TeamType.None && Team == atkTeam) return; // 同じチームからの攻撃は無効
        if (CurrentHP <= 0) return; // 既に死亡しているなら無視
        if (IsInvincible) return; // 無敵状態なら無視

        CurrentHP = Mathf.Max(0, CurrentHP - damage);

        if (CurrentHP <= 0)
        {
            if (GameManager.Instance != null)
            {
                //GameManagerに死亡を通知
                GameManager.Instance.NotifyDeath(Runner.SimulationTime,Object.InputAuthority, attacker);
            }
            
        }
    }

    /// <summary>HPを回復する</summary>
    public void HealHP(int heal)
    {
        if (!HasStateAuthority) return;
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + heal);
    }

    //無敵フラグ変更
    public void SetInvincible(bool isInvincible)
    {
        if (!HasStateAuthority) return;
        IsInvincible = isInvincible;
    }

    //チーム設定
    public void SetTeam(TeamType team)
    {
        if (!HasStateAuthority) return;
        Team = team;
    }


    public void InitializeHP()
    {
        if (!HasStateAuthority) return;
        CurrentHP = MaxHP = 100; // 初期HPを100に設定
    }

    //―――― クライアント→サーバー：武器切替リクエスト ――――
    //クライアント側から呼び出すメソッド
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestWeaponChange(WeaponType newWeapon)
    {
        CurrentWeapon_Network = newWeapon;
    }
    //クライアント側からHPを減らすリクエスト(デバッグ用)
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestDamageHP(int damage, PlayerRef attacker = default)
    {
        DamageHP(damage, attacker);
    }

    #endregion
}
