using UnityEngine;

/// <summary>
/// LocalWeaponManager �� HUDManager �̓���e�X�g�p�R���|�[�l���g
/// �L�[����ŕ���ύX�A���ˁA�����[�h�A���U�[�u��U�������܂��B
/// </summary>
public class WeaponHUDTest : MonoBehaviour
{
    [Header("�Q�ƃR���|�[�l���g�iInspector �Őݒ�\�j")]
    [SerializeField] private WeaponLocalState weaponManager;
    [SerializeField] private HUDManager hudManager;

    [Header("���U�[�u��U�e�X�g��")]
    [SerializeField] private int reserveAmount = 5;

    private void Start()
    {
        // �V�[�����ɂ���Ύ����擾
        if (weaponManager == null)
            weaponManager = FindObjectOfType<WeaponLocalState>();
        if (hudManager == null)
            hudManager = FindObjectOfType<HUDManager>();

        if (weaponManager == null || hudManager == null)
            Debug.LogError("WeaponHUDTest: LocalWeaponManager �܂��� HUDManager ��������܂���");
    }

    private void Update()
    {
        // 1/2/3 �L�[�ŕ���ؑ�
        if (Input.GetKeyDown(KeyCode.Alpha1))
            weaponManager.ChangeWeapon(WeaponType.AssaultRifle);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            weaponManager.ChangeWeapon(WeaponType.SemiAutoRifle);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            weaponManager.ChangeWeapon(WeaponType.GrenadeLauncher);
        if (Input.GetKeyDown(KeyCode.Alpha0))
            weaponManager.ChangeWeapon(WeaponType.Sword);

        // F �L�[�Ŕ��˃e�X�g
        if (Input.GetKeyDown(KeyCode.F))
            Debug.Log("TryFire returned: " + weaponManager.TryFire());

        // R �L�[�Ń����[�h�e�X�g
        if (Input.GetKeyDown(KeyCode.R))
            weaponManager.StartReload();

        // T �L�[�Ń��U�[�u��U�e�X�g
        if (Input.GetKeyDown(KeyCode.T))
            weaponManager.AddReserve(reserveAmount);
    }
}

