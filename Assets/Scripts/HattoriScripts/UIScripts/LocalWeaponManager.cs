using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���킲�Ƃ̃��[�J���e��Ǘ��Ƒ���i���ˁE�����[�h�E���U�[�u��U�j���s���R���|�[�l���g
/// </summary>
public class LocalWeaponManager : MonoBehaviour
{
    [Header("���탊�X�g�Ə����e���ݒ�")]
    [Tooltip("���p�\�ȕ���̈ꗗ�iWeaponType enum�j")]
    [SerializeField]
    private WeaponType[] availableWeapons = new[] {
        WeaponType.Sword,
        WeaponType.AssaultRifle,
        WeaponType.SemiAutoRifle,
        WeaponType.GrenadeLauncher
    };
    [Tooltip("���킲�ƂɑΉ�����}�K�W���ő�e���i�z�񒷂� availableWeapons �Ɠ����j")]
    private int[] magazineCapacities = new[] {
        0,    // Sword
        20,   // AssaultRifle
        5,    // SemiAutoRifle
        1     // GrenadeLauncher
    };
    [Tooltip("���킲�ƂɑΉ����郊�U�[�u�ő�e���i�z�񒷂� availableWeapons �Ɠ����j")]
    private int[] reserveCapacities = new[] {
        0,    // Sword
        100,  // AssaultRifle
        15,   // SemiAutoRifle
        5     // GrenadeLauncher
    };

    [Header("�����[�h�ݒ�")]
    [SerializeField] private float reloadDuration = 2.0f;

    // ����: ���킲�Ƃ̒e����
    private struct AmmoData { public int Magazine; public int Reserve; }
    private Dictionary<WeaponType, AmmoData> ammoTable;

    // ���ݑI�𒆂̕���
    private WeaponType currentWeapon;
    public WeaponType CurrentWeapon => currentWeapon;

    // �e��f�[�^�ύX���̃C�x���g
    public event Action<WeaponType, int, int> OnAmmoChanged;
    public event Action<WeaponType> OnWeaponChanged;

    // �����[�h���t���O
    private bool isReloading;

    private void Awake()
    {
        // �e�[�u��������
        ammoTable = new Dictionary<WeaponType, AmmoData>();
        for (int i = 0; i < availableWeapons.Length; i++)
        {
            var w = availableWeapons[i];
            var magCap = magazineCapacities[i];
            var resCap = reserveCapacities[i];
            ammoTable[w] = new AmmoData { Magazine = magCap, Reserve = resCap };
        }
        // ��������Z�b�g
        if (availableWeapons.Length > 0)
            ChangeWeapon(availableWeapons[0]);
    }

    /// <summary>
    /// �����؂�ւ���
    /// </summary>
    public void ChangeWeapon(WeaponType newWeapon)
    {
        if (!ammoTable.ContainsKey(newWeapon))
            throw new ArgumentException($"���ݒ�̕���ł�: {newWeapon}");

        currentWeapon = newWeapon;
        OnWeaponChanged?.Invoke(currentWeapon);
        // �e��\���X�V
        var ad = ammoTable[currentWeapon];
        OnAmmoChanged?.Invoke(currentWeapon, ad.Magazine, ad.Reserve);
    }

    /// <summary>
    /// ���˂ł��邩�`�F�b�N���A�\�Ȃ�}�K�W��1�������Z
    /// </summary>
    public bool TryFire()
    {
        if (isReloading) return false;
        var ad = ammoTable[currentWeapon];
        //����킪 Sword �̏ꍇ�̓}�K�W�������炳�Ȃ�
        if (currentWeapon == WeaponType.Sword) return true;
        // �}�K�W������Ȃ甭�˕s��
        if (ad.Magazine <= 0) return false;
        // ���ˏ����i�����ł͒P���Ƀ}�K�W�������炷�j
        ad.Magazine--;
        ammoTable[currentWeapon] = ad;
        OnAmmoChanged?.Invoke(currentWeapon, ad.Magazine, ad.Reserve);
        return true;
    }

    /// <summary>
    /// �����[�h���J�n�i�R���[�`���j
    /// </summary>
    public void StartReload()
    {
        if (isReloading) return;
        var ad = ammoTable[currentWeapon];
        if (ad.Reserve <= 0 || ad.Magazine >= magazineCapacities[Array.IndexOf(availableWeapons, currentWeapon)])
            return; // �����[�h�s�v�܂��̓��U�[�u����

        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadDuration);

        var idx = Array.IndexOf(availableWeapons, currentWeapon);
        var cap = magazineCapacities[idx];
        var ad = ammoTable[currentWeapon];

        int needed = cap - ad.Magazine;
        int toReload = Mathf.Min(needed, ad.Reserve);

        ad.Magazine += toReload;
        ad.Reserve -= toReload;
        ammoTable[currentWeapon] = ad;
        isReloading = false;

        OnAmmoChanged?.Invoke(currentWeapon, ad.Magazine, ad.Reserve);
    }

    /// <summary>
    /// ���U�[�u�e��ǉ�
    /// </summary>
    public void AddReserve(int amount)
    {
        var ad = ammoTable[currentWeapon];
        ad.Reserve += amount;
        ammoTable[currentWeapon] = ad;
        OnAmmoChanged?.Invoke(currentWeapon, ad.Magazine, ad.Reserve);
    }

    /// <summary>
    /// ���݂̒e��f�[�^���擾
    /// </summary>
    public (int magazine, int reserve) GetCurrentAmmo()
    {
        var ad = ammoTable[currentWeapon];
        return (ad.Magazine, ad.Reserve);
    }
}

