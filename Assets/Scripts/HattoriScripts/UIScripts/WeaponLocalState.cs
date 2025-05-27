using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.Playables;        

/// <summary>
/// ���킲�Ƃ̃��[�J���e��Ǘ��Ƒ���i���ˁE�����[�h�E���U�[�u��U�j���s���R���|�[�l���g
/// </summary>
public class WeaponLocalState : NetworkBehaviour
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
    [Header("���킲�Ƃ̃}�K�W���ő吔")]
    [Tooltip("���킲�ƂɑΉ�����}�K�W���ő�e���i�z�񒷂� availableWeapons �Ɠ����j")]
    [SerializeField]
    private int[] magazineCapacities = new[] {
        0,    // Sword
        20,   // AssaultRifle
        5,    // SemiAutoRifle
        1     // GrenadeLauncher
    };
    [Header("���킲�Ƃ̃��U�[�u�ő吔")]
    [Tooltip("���킲�ƂɑΉ����郊�U�[�u�ő�e���i�z�񒷂� availableWeapons �Ɠ����j")]
    [SerializeField]
    private int[] reserveCapacities = new[] {
        0,    // Sword
        100,  // AssaultRifle
        15,   // SemiAutoRifle
        5     // GrenadeLauncher
    };

    [Header("�����[�h�ݒ�")]
    [SerializeField] private float reloadDuration = 2.0f;

    [Header("�ˑ��R���|�[�l���g")]
    [SerializeField] private PlayerNetworkState playerState;

    // ����: ���킲�Ƃ̒e����
    private struct AmmoData { public int Magazine; public int Reserve; }
    private Dictionary<WeaponType, AmmoData> ammoTable;

    // ���ݑI�𒆂̕���
    private WeaponType currentWeapon;
    public WeaponType CurrentWeapon => currentWeapon;

    // �N���X�����E�e��f�[�^�ύX���̃C�x���g
    public static event Action<WeaponLocalState> OnWeaponSpawned;
    public event Action<int, int> OnAmmoChanged;
    public event Action<WeaponType> OnWeaponChanged;

    // �����[�h���t���O
    private bool isReloading;

    public override void Spawned()
    {

        //�����ȊO�͂��̃R���|�[�l���g���g��Ȃ�
        if (!HasInputAuthority) return;
         
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

        OnWeaponSpawned?.Invoke(this);


    }

    /// <summary>
    /// ���݂̒e��f�[�^���擾
    /// </summary>
    public (int magazine, int reserve) GetCurrentAmmo()
    {
        var ad = ammoTable[currentWeapon];
        return (ad.Magazine, ad.Reserve);
    }

    /// <summary>
    /// �C�ӂ̕���̒e��f�[�^���擾
    /// </summary>
    public (int magazine, int reserve) GetAmmo(WeaponType weapon)
    {
        if (ammoTable.TryGetValue(weapon, out var ad))
            return (ad.Magazine, ad.Reserve);
        throw new ArgumentException($"���ݒ�̕���ł�: {weapon}");
    }

    /// <summary>
    /// �����؂�ւ���
    /// </summary>
    public void ChangeWeapon(WeaponType newWeapon)
    {
        //�����ȊO�͂��̃R���|�[�l���g���g��Ȃ�
        if (!HasInputAuthority) return;

        if (isReloading) return; // �����[�h���͐؂�ւ��s��
        if (!ammoTable.ContainsKey(newWeapon))
            throw new ArgumentException($"���ݒ�̕���ł�: {newWeapon}");

        currentWeapon = newWeapon;
        // ����ύX�C�x���g�ʒm
        OnWeaponChanged?.Invoke(currentWeapon);
        // �e��\���X�V
        var ad = ammoTable[currentWeapon];
        // ���݂̕���̒e�����ʒm
        OnAmmoChanged?.Invoke( ad.Magazine, ad.Reserve);

        // RPC�Ńz�X�g�ɐ؂�ւ������N�G�X�g
        //    ��HasInputAuthority �����N���C�A���g����̂݌Ă�
        if (playerState != null && playerState.HasInputAuthority)
        {
            Debug.Log($"RPC�Ăяo��");
            playerState.RPC_RequestWeaponChange(currentWeapon);
        }else 
        {
            Debug.Log($"RPC�Ăяo����");
        }
    }

    /// <summary>
    /// ���˂ł��邩�`�F�b�N���A�\�Ȃ�}�K�W��1�������Z
    /// </summary>
    public bool TryFire()
    {
        //�����ȊO�͂��̃R���|�[�l���g���g��Ȃ�
        if (!HasInputAuthority) return false; 

        if (isReloading) return false;
        var ad = ammoTable[currentWeapon];
        //����킪 Sword �̏ꍇ�̓}�K�W�������炳�Ȃ�
        if (currentWeapon == WeaponType.Sword) return true;
        // �}�K�W������Ȃ甭�˕s��
        if (ad.Magazine <= 0) return false;
        // ���ˏ����i�����ł͒P���Ƀ}�K�W�������炷�j
        ad.Magazine--;
        ammoTable[currentWeapon] = ad;
        OnAmmoChanged?.Invoke(ad.Magazine, ad.Reserve);
        return true;
    }

    /// <summary>
    /// �����[�h���J�n�i�R���[�`���j
    /// </summary>
    public void StartReload()
    {
        //�����ȊO�͂��̃R���|�[�l���g���g��Ȃ�
        if (!HasInputAuthority) return;

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

        OnAmmoChanged?.Invoke(ad.Magazine, ad.Reserve);
    }

    /// <summary>
    /// ���U�[�u�e��ǉ�
    /// </summary>
    /// �e���ǉ������ƁA���U�[�u���ő�l�𒴂��Ȃ��悤�ɒ�������邪�A�ǂꂾ�����������Ɋւ�炸�A����悤�Ǝ��݂��e�͏��ł���
    public void AddReserve(int amount)
    {
        //�����ȊO�͂��̃R���|�[�l���g���g��Ȃ�
        if (!HasInputAuthority) return;

        // ����킪 Sword �̏ꍇ�̓��U�[�u�𑝂₳�Ȃ�
        if (currentWeapon == WeaponType.Sword) return;
        var ad = ammoTable[currentWeapon];

        int maxReserve = reserveCapacities[Array.IndexOf(availableWeapons, currentWeapon)];
        //���U�[�u���ő�ł���Βǉ����Ȃ�
        if (ad.Reserve >= maxReserve) return;
        // �ǉ��ʂ��ő�l�𒴂���ꍇ�͒���
        if (ad.Reserve + amount > maxReserve)
        {
            amount = maxReserve - ad.Reserve; // �ő�l�𒴂��Ȃ��悤�ɒ���
        }

        ad.Reserve += amount;
        ammoTable[currentWeapon] = ad;
        OnAmmoChanged?.Invoke(ad.Magazine, ad.Reserve);
    }

    
}

