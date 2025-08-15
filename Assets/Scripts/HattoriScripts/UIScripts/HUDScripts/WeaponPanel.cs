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
    [SerializeField] private float GrenadeScale = 0.68f; // Grenade �̑傫��
    private PlayerAvatar weaponState;

    public void Initialize(PlayerNetworkState _, PlayerAvatar wState)
    {
        weaponState = wState;
        //�C�x���g�o�^
        weaponState.OnWeaponChanged -= UpdateWeaponImage;
        weaponState.OnAmmoChanged -= UpdateAmmoText;
        weaponState.OnWeaponChanged += UpdateWeaponImage;
        weaponState.OnAmmoChanged += UpdateAmmoText;
        // �����l�̐ݒ�
        WeaponType currentWeapon = weaponState.CurrentWeapon;
        UpdateWeaponImage(currentWeapon, weaponState.WeaponClassDictionary[currentWeapon].currentMagazine, weaponState.WeaponClassDictionary[currentWeapon].currentReserve);
    }
    public void Cleanup()
    {
        weaponState.OnWeaponChanged -= UpdateWeaponImage;
        weaponState.OnAmmoChanged -= UpdateAmmoText;
    }
    private void UpdateWeaponImage(WeaponType currentWeapon,int magazine, int reserve)
    {
        int idx = (int)currentWeapon;
        // �͈͊O�̃C���f�b�N�X�͖���
        if (idx < 0 || idx >= weaponSprites.Length) return;
        weaponImage.sprite = weaponSprites[idx];


        // ����̃X�v���C�g�T�C�Y�ɉ�����Image�̃T�C�Y�𒲐�
        //����̎�ނɂ���ăe�L�X�g�̕\����\����ύX
        float scale = 1f;
        if (currentWeapon == WeaponType.AssaultRifle || currentWeapon == WeaponType.SemiAutoRifle)
        {
            scale = RifleScale;
            magText.enabled = true;
            resText.enabled = true;
        }
        else if (currentWeapon == WeaponType.Grenade)
        {
            scale = GrenadeScale;
            magText.enabled = true;
            resText.enabled = false; 
        }
        else if (currentWeapon == WeaponType.Sword)
        {
            scale = 1f;
            magText.enabled = false;
            resText.enabled = false;
        }
        var rt = weaponImage.rectTransform;
        rt.sizeDelta = weaponSprites[idx].rect.size * scale;

        UpdateAmmoText(magazine, reserve);
    }
    private void UpdateAmmoText(int mag, int res)
    {
        magText.text = mag.ToString();
        resText.text = res.ToString();
    }
}