using System;
using UnityEngine;
using Fusion;

// �e��f�[�^�\��
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

// �e�v���C���[�̃X�e�[�^�X���Ǘ�����N���X
public class PlayerNetworkState : NetworkBehaviour
{
    #region Events

    // �C���X�^���X�C�x���g
    /// <summary>�e�����ύX���ꂽ�Ƃ�</summary>
    public event Action<float> OnHPChanged;
    /// <summary>����ؑւ��T�[�o�[���j�Ŋm�肵���Ƃ�</summary>
    public event Action<WeaponType> OnWeaponChanged_Network;
  
    /// <summary>�X�R�A���ύX���ꂽ�Ƃ�</summary>
    public event Action<int, int> OnScoreChanged;

    /// <summary>�e�����ύX���ꂽ�Ƃ�(�`�[�g�΍��p�A��{�g��Ȃ�)</summary>
    public event Action<int, int> OnAmmoChanged_Network;

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

    // �������̕���(�T�[�o�[���猩��)
    [Networked(OnChanged = nameof(WeaponChangedCallback))]
    public WeaponType CurrentWeapon_Network { get; private set; } = WeaponType.Sword;

    // �X�R�A (�L�� / �f�X)
    [Networked(OnChanged = nameof(ScoreChangedCallback))]
    public int KillScore { get; private set; }

    [Networked(OnChanged = nameof(ScoreChangedCallback))]
    public int DeathScore { get; private set; }

  // �e�� (�}�K�W�� / ���U�[�u)(�`�[�g�΍��p�A��{�g��Ȃ�)
    [Networked(OnChanged = nameof(AmmoChangedCallback))]
    public int MagazineAmmo_Network { get; private set; }

    [Networked(OnChanged = nameof(AmmoChangedCallback))]
    public int ReserveAmmo_Network { get; private set; }

    #endregion

    #region Local Fields

    // ���[�J���ŊǗ����镐�킲�Ƃ̒e���e�[�u��(�`�[�g�΍��p)
    private Ammo[] ammoPerWeapon;

    #endregion

    #region Change Callbacks

    static void HPChangedCallback(Changed<PlayerNetworkState> changed)
    { 
        // �R�[���o�b�N���Ă΂ꂽ�Ƃ��Ƀf�o�b�O���O���o��
        Debug.Log($"HP�R�[���o�b�N�Ăяo��");
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
        
        
        //�z�X�g�̂݃v���C���[���Ƃ̒e����������(�`�[�g�΍��p)
        if (Object.HasStateAuthority)
        {
            // �z�񏉊���
            int count = (int)WeaponType.GrenadeLauncher + 1;
            ammoPerWeapon = new Ammo[count];

            // �����e���ݒ�
            ammoPerWeapon[(int)WeaponType.Sword] = new Ammo(1, 0);
            ammoPerWeapon[(int)WeaponType.AssaultRifle] = new Ammo(20, 100);
            ammoPerWeapon[(int)WeaponType.SemiAutoRifle] = new Ammo(5, 15);
            ammoPerWeapon[(int)WeaponType.GrenadeLauncher] = new Ammo(1, 5);
        }
    }

    //�f�o�b�O�p 
    void Update()
    {

        if (HasInputAuthority && Input.GetKeyDown(KeyCode.K))
        {
            // �����𑦎�������
            DamageHP(int.MaxValue, PlayerRef.None);
        }
    }

    #endregion

    #region Public Methods
    //�\�\�\�\ �T�[�o�[���N���C�A���g�F�X�e�[�^�X�ύX �\�\�\�\
    //�z�X�g���݂̂��Ăяo�����\�b�h
    /// <summary>HP�����炷</summary>
    public void DamageHP(int damage, PlayerRef attacker = default)
    {
        Debug.Log($"DamageHPMethod");
        if (!HasStateAuthority) return;
        if (damage <= 0) return; // �_���[�W��0�ȉ��Ȃ疳��
        if (CurrentHP <= 0) return; // ���Ɏ��S���Ă���Ȃ疳��

        CurrentHP = Mathf.Max(0, CurrentHP - damage);

        Debug.Log($"Player {Object.InputAuthority} took {damage} damage, remaining HP: {CurrentHP}");

        if (CurrentHP == 0)
        {
            Debug.Log($"Player {Object.InputAuthority} has died.");
            // ���g�̃f�X�X�R�A���Z
            AddDeathScore();

            // �U���҂��w�肳��Ă���΃L���X�R�A���Z
            if (attacker != PlayerRef.None && attacker != Object.InputAuthority)
            {
                if (Runner.TryGetPlayerObject(attacker, out var atkGo))
                {
                    var atkState = atkGo.GetComponent<PlayerNetworkState>();
                    if (atkState != null && atkState.HasStateAuthority)
                        atkState.AddKillScore();
                }
            }

            // ���S�ʒm RPC
            RPC_PlayerDeath( Object.InputAuthority, attacker);
        }
    }
    /// <summary>HP���񕜂���</summary>
    public void HealHP(int heal)
    {
        if (!HasStateAuthority) return;
        CurrentHP = Mathf.Min(MaxHP, CurrentHP + heal);
    }
    /// <summary>Kill�X�R�A�����Z����</summary>
    public void AddKillScore()
    {
        if (!HasStateAuthority) return;
        KillScore ++;
    }
    /// <summary>Death�X�R�A�����Z����</summary>
    public void AddDeathScore()
    {
        if (!HasStateAuthority) return;
        DeathScore++;
    }
    ///<summary>���S�C�x���g</summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayerDeath(PlayerRef victim, PlayerRef killer)
    {
        Debug.Log("���S�C�x���g����");
        OnPlayerDied?.Invoke(victim, killer);
    }

    //�\�\�\�\ �N���C�A���g���T�[�o�[�F����ؑփ��N�G�X�g �\�\�\�\
    //�N���C�A���g������Ăяo�����\�b�h
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestWeaponChange(WeaponType newWeapon)
    {
        CurrentWeapon_Network = newWeapon;
    }

    #endregion
}
