using System;
using System.Collections;
using UnityEngine;
using Fusion;


// 各プレイヤーのステータスを管理するクラス
public class PlayerNetworkState : NetworkBehaviour
{
    [SerializeField] private PlayerAvatar playerAvatar; // PlayerAvatarコンポーネントへの参照

    #region Events

    // インスタンスイベント
    /// <summary>HPが変更されたとき</summary>
    public event Action<float, PlayerRef> OnHPChanged;

    ///<summary> 無敵状態になった時 </summary>
    public event Action<bool, float> OnInvincibleChanged;

    /// <summary>武器切替がサーバー正史で確定したとき</summary>
    public event Action<WeaponType> OnWeaponChanged_Network;


    /// <summary>�`�[�����ύX���ꂽ�Ƃ�</summary>
    public event Action<TeamType> OnTeamChanged;

    // ���[�J���v���C���[������
    public static event Action<PlayerNetworkState> OnLocalPlayerSpawned;
    

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

    //�����`�[��
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


    void RaiseHPChanged() => OnHPChanged?.Invoke(HpNormalized,Object.InputAuthority);
    void RaiseWeaponChanged() => OnWeaponChanged_Network?.Invoke(CurrentWeapon_Network);
    void RaiseTeamChanged() => OnTeamChanged?.Invoke(Team);

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    void RPC_RaiseInvincibleChanged(bool isInvincible, float remainTime)
    {
        // クライアント側で無敵状態の変更を通知
        OnInvincibleChanged?.Invoke(isInvincible, remainTime);
    }
    #endregion

    #region Unity Callbacks

    public override void Spawned()
    {
        if (HasInputAuthority)
        {   
            //FindObjectOfType<HUDManager>()?.PlayerHUDInitialize(this);
            OnLocalPlayerSpawned?.Invoke(this);
            playerAvatar.OnWeaponChanged += RequestWeaponChange;
        }    
  
    }

    public override void Despawned(NetworkRunner runner, bool hasStateChanged)
    {
        // 破棄時にイベントを解除
        OnHPChanged = null;
        OnWeaponChanged_Network = null;
        OnTeamChanged = null;
        OnLocalPlayerSpawned = null;
        //OnPlayerDied = null;
        if (playerAvatar != null)
            playerAvatar.OnWeaponChanged -= RequestWeaponChange;
    }

    private void RequestWeaponChange(WeaponType newWeapon, int _, int __)
    {
        RPC_RequestWeaponChange(newWeapon);
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


    /// <summary>HPを減らすメソッド</summary>
    public void DamageHP(int damage, PlayerRef attacker = default, TeamType atkTeam = default)
    {
        Debug.Log($"DamageHPMethod");
        if (!HasStateAuthority) return;
        if(Team != TeamType.None && atkTeam != TeamType.None && Team == atkTeam) return; //同じチームならダメージ無効
        if (CurrentHP <= 0) return; //死亡しているならダメージ無効
        if (IsInvincible) return; //無敵ならダメージ無効



        CurrentHP = Mathf.Max(0, CurrentHP - damage);

        if (CurrentHP <= 0)
        {

            if (GameManager2.Instance != null)
            {

                //GameManagerに通知を送る
                GameManager2.Instance.NotifyDeath(Runner.SimulationTime,Object.InputAuthority, attacker);

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
        RPC_RaiseInvincibleChanged(isInvincible, 600f); 
    }

    public void SetInvincible(bool isInvincible, float duration)
    {
        if (!HasStateAuthority) return;
        IsInvincible = isInvincible;
        // 無敵状態の変更イベントを発火
        RPC_RaiseInvincibleChanged(isInvincible, duration);
        // 一定時間後に無敵状態を解除する
        if (isInvincible)
        {
            Runner.StartCoroutine(ResetInvincibilityAfterDelay(duration));
        }
    }

    // 無敵状態を一定時間後に解除するコルーチン
    private IEnumerator ResetInvincibilityAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        IsInvincible = false;
        RPC_RaiseInvincibleChanged(false, 0f); // 無敵状態が解除されたことを通知
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
