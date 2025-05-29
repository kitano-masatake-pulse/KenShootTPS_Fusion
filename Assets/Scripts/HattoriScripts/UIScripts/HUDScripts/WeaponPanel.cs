using TMPro;
using UnityEngine;
using UnityEngine.UI;
// ����p�p�l��
public class WeaponPanel : MonoBehaviour, IHUDPanel
{
    [SerializeField] private TMP_Text resText, magText;
    [SerializeField] private Image weaponImage;
    [Header("WeaponType �ɑΉ�����X�v���C�g(���Ԃ� enum �ƍ��킹��)")]
    [Tooltip("0:Sword, 1:AssaultRifle, 2:SemiAutoRifle, 3:GrenadeLauncher")]
    [SerializeField] private Sprite[] weaponSprites = new Sprite[4];
    [Header("Image�̑傫���ݒ�")]
    [SerializeField] private float RifleScale = 0.53f; // AssaultRifle, SemiAutoRifle �̑傫��
    [SerializeField] private float GrenadeLauncherScale = 0.53f; // GrenadeLauncher �̑傫��
    private WeaponLocalState weaponState;

    public void Initialize(PlayerNetworkState _, WeaponLocalState wState)
    {
        weaponState = wState;
        //�C�x���g�o�^
        weaponState.OnWeaponChanged -= UpdateWeaponImage;
        weaponState.OnAmmoChanged -= UpdateAmmoText;
        weaponState.OnWeaponChanged += UpdateWeaponImage;
        weaponState.OnAmmoChanged += UpdateAmmoText;
        // �����l�̐ݒ�
        UpdateWeaponImage(weaponState.CurrentWeapon);
        var ammo = weaponState.GetCurrentAmmo();
        UpdateAmmoText(ammo.magazine, ammo.reserve);
    }
    public void Cleanup()
    {
        weaponState.OnWeaponChanged -= UpdateWeaponImage;
        weaponState.OnAmmoChanged -= UpdateAmmoText;
    }
    private void UpdateWeaponImage(WeaponType currentWeapon)
    {
        int idx = (int)currentWeapon;
        // �͈͊O�̃C���f�b�N�X�͖���
        if (idx < 0 || idx >= weaponSprites.Length) return;
        weaponImage.sprite = weaponSprites[idx];


        // ����̃X�v���C�g�T�C�Y�ɉ�����Image�̃T�C�Y�𒲐�
        float scale = 1f;
        if (currentWeapon == WeaponType.AssaultRifle || currentWeapon == WeaponType.SemiAutoRifle)
            scale = RifleScale;
        else if (currentWeapon == WeaponType.GrenadeLauncher)
            scale = GrenadeLauncherScale;

        var rt = weaponImage.rectTransform;
        rt.sizeDelta = weaponSprites[idx].rect.size * scale;
    }
    private void UpdateAmmoText(int mag, int res)
    {
        magText.text = mag.ToString();
        resText.text = res.ToString();
    }
}