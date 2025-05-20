using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class WeaponIconSwitcher : MonoBehaviour
{
    [Header("WeaponType に対応するスプライト(順番を enum と合わせる)")]
    [Tooltip("0:Sword, 1:AssaultRifle, 2:SemiAutoRifle, 3:GrenadeLauncher")]
    [SerializeField]
    private Sprite[] weaponSprites = new Sprite[4];
    private Image targetImage;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
        // 長さチェック（Editor 時にも走ると便利）
        if (weaponSprites == null || weaponSprites.Length != System.Enum.GetValues(typeof(WeaponType)).Length)
        {
            Debug.LogError($"[{nameof(WeaponIconSwitcher)}] weaponSprites の要素数は {System.Enum.GetValues(typeof(WeaponType)).Length} にしてください");
        }
    }

    /// <summary>
    /// index 番目のスプライトに切り替えて、元サイズに合わせる
    /// </summary>
    public void SetWeaponIcon(WeaponType type, float scale = 1f)
    {
        Sprite sp = weaponSprites[(int)type];
        targetImage.sprite = sp;
        if (type == WeaponType.Sword)
        {
            scale = 1.6f; // Sword のみサイズを大きくする
        }
        else if (type == WeaponType.GrenadeLauncher)
        {
            scale = 0.85f; // GrenadeLauncher のみサイズを小さくする   
        }
        // RectTransform 取得
        RectTransform rt = targetImage.rectTransform;

        // スプライトの元ピクセルサイズ × scale 倍 を sizeDelta にセット
        Vector2 native = sp.rect.size;        // ex. (512, 512)
        rt.sizeDelta = native * scale;        // ex. (512,512) * 0.25f → (128,128)

    }
    
}
