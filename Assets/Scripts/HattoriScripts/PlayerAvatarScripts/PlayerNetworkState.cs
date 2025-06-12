using System;
using UnityEngine;
using Fusion;


// �e�v���C���[�̃X�e�[�^�X���Ǘ�����N���X
public class PlayerNetworkState : NetworkBehaviour
{
    #region Events

    // �C���X�^���X�C�x���g
    /// <summary>HP���ύX���ꂽ�Ƃ�</summary>
    public event Action<float> OnHPChanged;

    /// <summary>����ؑւ��T�[�o�[���j�Ŋm�肵���Ƃ�</summary>
    public event Action<WeaponType> OnWeaponChanged_Network;

    /// <summary>�`�[�����ύX���ꂽ�Ƃ�</summary>
    public event Action<TeamType> OnTeamChanged;

    // ���[�J���v���C���[������
    public static event Action<PlayerNetworkState> OnLocalPlayerSpawned;
    
    /// <summary>�v���C���[���S��(Victim,Killer)</summary>
    public event Action<PlayerRef, PlayerRef> OnPlayerDied;

    #endregion

    #region Networked Properties

    // HP (���� / �ő�)
    [Networked(OnChanged = nameof(HPChangedCallback))]
    public int CurrentHP { get; private set; }

    [Networked(OnChanged = nameof(HPChangedCallback))]
    public int MaxHP { get; private set; }

    public float HpNormalized => MaxHP > 0 ? (float)CurrentHP / MaxHP : 0f;


    //���G��Ԃ��ǂ���
    [Networked]
    public bool IsInvincible { get; private set; } = false;


    // �������̕���(�T�[�o�[���猩��)
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

    //�f�o�b�O�p 
    void Update()
    {

        if (HasInputAuthority && Input.GetKeyDown(KeyCode.K))
        {
            // �����𑦎�������
            RPC_RequestDamageHP(int.MaxValue, PlayerRef.None);
        }
    }
    #endregion

    #region Public Methods
    //�\�\�\�\ �T�[�o�[���N���C�A���g�F�X�e�[�^�X�ύX �\�\�\�\
    //�z�X�g���݂̂��Ăяo�����\�b�h
    /// <summary>HP�����炷</summary>
    public void DamageHP(int damage, PlayerRef attacker = default, TeamType atkTeam = default)
    {
        Debug.Log($"DamageHPMethod");
        if (!HasStateAuthority) return;
        if(Team != TeamType.None && atkTeam != TeamType.None && Team == atkTeam) return; // �����`�[������̍U���͖���
        if (CurrentHP <= 0) return; // ���Ɏ��S���Ă���Ȃ疳��
        if (IsInvincible) return; // ���G��ԂȂ疳��

        CurrentHP = Mathf.Max(0, CurrentHP - damage);

        if (CurrentHP <= 0)
        {
            if (GameManager.Instance != null)
            {
                //GameManager�Ɏ��S��ʒm
                GameManager.Instance.NotifyDeath(Runner.SimulationTime,Object.InputAuthority, attacker);
            }
            
        }
    }

    /// <summary>HP���񕜂���</summary>
    public void HealHP(int heal)
    {
        if (!HasStateAuthority) return;
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + heal);
    }

    //���G�t���O�ύX
    public void SetInvincible(bool isInvincible)
    {
        if (!HasStateAuthority) return;
        IsInvincible = isInvincible;
    }

    //�`�[���ݒ�
    public void SetTeam(TeamType team)
    {
        if (!HasStateAuthority) return;
        Team = team;
    }


    public void InitializeHP()
    {
        if (!HasStateAuthority) return;
        CurrentHP = MaxHP = 100; // ����HP��100�ɐݒ�
    }

    //�\�\�\�\ �N���C�A���g���T�[�o�[�F����ؑփ��N�G�X�g �\�\�\�\
    //�N���C�A���g������Ăяo�����\�b�h
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestWeaponChange(WeaponType newWeapon)
    {
        CurrentWeapon_Network = newWeapon;
    }
    //�N���C�A���g������HP�����炷���N�G�X�g(�f�o�b�O�p)
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestDamageHP(int damage, PlayerRef attacker = default)
    {
        DamageHP(damage, attacker);
    }

    #endregion
}
