using TMPro;
using UnityEngine;
using UnityEngine.UI;
// 武器用パネル
public class WeaponPanel : MonoBehaviour, IHUDPanel
{
    [SerializeField] private TMP_Text resText, magText;
    [SerializeField] private Image weaponImage;
    [Header("WeaponType に対応するスプライト(順番を enum と合わせる)")]
    [Tooltip("0:Sword, 1:AssaultRifle, 2:SemiAutoRifle, 3:GrenadeLauncher")]
    [SerializeField] private Sprite[] weaponSprites = new Sprite[4];
    [Header("Imageの大きさ設定")]
    [SerializeField] private float RifleScale = 0.53f; // AssaultRifle, SemiAutoRifle の大きさ
    [SerializeField] private float GrenadeScale = 0.68f; // Grenade の大きさ
    private PlayerAvatar weaponState;

    public void Initialize(PlayerNetworkState _, PlayerAvatar wState)
    {
        weaponState = wState;
        //イベント登録
        weaponState.OnWeaponChanged -= UpdateWeaponImage;
        weaponState.OnAmmoChanged -= UpdateAmmoText;
        weaponState.OnWeaponChanged += UpdateWeaponImage;
        weaponState.OnAmmoChanged += UpdateAmmoText;
        // 初期値の設定
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
        // 範囲外のインデックスは無視
        if (idx < 0 || idx >= weaponSprites.Length) return;
        weaponImage.sprite = weaponSprites[idx];


        // 武器のスプライトサイズに応じてImageのサイズを調整
        //武器の種類によってテキストの表示非表示を変更
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