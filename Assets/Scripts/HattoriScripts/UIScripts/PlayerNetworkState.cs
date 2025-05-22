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
    public event Action<float> OnHPChanged;
    public event Action<WeaponType> OnWeaponChanged;
    public event Action<int, int> OnAmmoChanged;
    public event Action<int, int> OnScoreChanged;

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

    // �������̕���
    [Networked(OnChanged = nameof(WeaponChangedCallback))]
    public WeaponType CurrentWeapon { get; set; } = WeaponType.Sword;

    // �e�� (�}�K�W�� / ���U�[�u)
    [Networked(OnChanged = nameof(AmmoChangedCallback))]
    public int MagazineAmmo { get; set; }

    [Networked(OnChanged = nameof(AmmoChangedCallback))]
    public int ReserveAmmo { get; set; }

    // �X�R�A (�L�� / �f�X)
    [Networked(OnChanged = nameof(ScoreChangedCallback))]
    public int KillScore { get; set; }

    [Networked(OnChanged = nameof(ScoreChangedCallback))]
    public int DeathScore { get; set; }

    #endregion

    #region Local Fields

    // ���[�J���ŊǗ����镐�킲�Ƃ̒e���e�[�u��
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
        // �z�񏉊���
        int count = (int)WeaponType.GrenadeLauncher + 1;
        ammoPerWeapon = new Ammo[count];

        // �����e���ݒ�
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

    /// <summary>���[�J�����畐��ؑ֗v��</summary>
    public void ChangeWeapon(WeaponType newWeapon)
    {
        if (!HasInputAuthority) return;
        CurrentWeapon = newWeapon;     // ����
        ApplyAmmoForWeapon(newWeapon); // �z�X�g�Ŕ��f
    }

    /// <summary>���[�J���Œe���𒲐�</summary>
    public void ModifyLocalAmmo(int magazineDelta, int reserveDelta)
    {
        var idx = (int)CurrentWeapon;
        ammoPerWeapon[idx].Magazine += magazineDelta;
        ammoPerWeapon[idx].Reserve += reserveDelta;
        ApplyAmmoForWeapon(CurrentWeapon);
    }

    /// <summary>�w�蕐��̒e�����l�b�g���[�N�v���p�e�B�ɏ�������</summary>
    public void ApplyAmmoForWeapon(WeaponType weapon)
    {
        var a = ammoPerWeapon[(int)weapon];
        MagazineAmmo = a.Magazine;
        ReserveAmmo = a.Reserve;
    }


    /// <summary>HP�����炷</summary>
    public void DamageHP(int damage)
    {
        if (!HasStateAuthority) return;
        CurrentHP = Mathf.Max(0, CurrentHP - damage);
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

    #endregion
}
