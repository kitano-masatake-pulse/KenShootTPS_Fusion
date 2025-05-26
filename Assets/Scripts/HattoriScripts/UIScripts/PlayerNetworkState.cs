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
    public event Action<WeaponType> OnWeaponChanged;
  
    /// <summary>�X�R�A���ύX���ꂽ�Ƃ�</summary>
    public event Action<int, int> OnScoreChanged;

    /// <summary>�e�����ύX���ꂽ�Ƃ�(�`�[�g�΍��p�A��{�g��Ȃ�)</summary>
    public event Action<int, int> OnAmmoChanged;

    // ���[�J���v���C���[������
    public static event Action<PlayerNetworkState> OnLocalPlayerSpawned;

    #endregion

    #region Networked Properties

    // HP (���� / �ő�)
    [Networked(OnChanged = nameof(HPChangedCallback))]
    public int CurrentHP { get; set; }

    [Networked(OnChanged = nameof(HPChangedCallback))]
    public int MaxHP { get; set; }

    public float HpNormalized => MaxHP > 0 ? (float)CurrentHP / MaxHP : 0f;

    // �������̕���(�T�[�o�[���猩��)
    [Networked(OnChanged = nameof(WeaponChangedCallback))]
    public WeaponType CurrentWeapon { get; private set; } = WeaponType.Sword;

    // �X�R�A (�L�� / �f�X)
    [Networked(OnChanged = nameof(ScoreChangedCallback))]
    public int KillScore { get; set; }

    [Networked(OnChanged = nameof(ScoreChangedCallback))]
    public int DeathScore { get; set; }

  // �e�� (�}�K�W�� / ���U�[�u)(�`�[�g�΍��p�A��{�g��Ȃ�)
    [Networked(OnChanged = nameof(AmmoChangedCallback))]
    public int MagazineAmmo { get; set; }

    [Networked(OnChanged = nameof(AmmoChangedCallback))]
    public int ReserveAmmo { get; set; }

    #endregion

    #region Local Fields

    // ���[�J���ŊǗ����镐�킲�Ƃ̒e���e�[�u��(�`�[�g�΍��p)
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

    #endregion

    #region Public Methods
    //�\�\�\�\ �T�[�o�[���N���C�A���g�F�X�e�[�^�X�ύX �\�\�\�\
    //�z�X�g���݂̂��Ăяo�����\�b�h
    /// <summary>HP�����炷</summary>
    public void DamageHP(int damage)
    {
        if (!HasStateAuthority) return;
        CurrentHP = Mathf.Max(0, CurrentHP - damage);
        // HP��0�ɂȂ����玀�S�C�x���g���N��
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

    //�\�\�\�\ �N���C�A���g���T�[�o�[�F����ؑփ��N�G�X�g �\�\�\�\
    //�N���C�A���g������Ăяo�����\�b�h
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_RequestWeaponChange(WeaponType newWeapon)
    {
        Debug.Log($"RPC���s");
        CurrentWeapon = newWeapon;
    }


    #endregion
}
