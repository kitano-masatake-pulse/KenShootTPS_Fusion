using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class WeaponIconSwitcher : MonoBehaviour
{
    [Header("WeaponType �ɑΉ�����X�v���C�g(���Ԃ� enum �ƍ��킹��)")]
    [Tooltip("0:Sword, 1:AssaultRifle, 2:SemiAutoRifle, 3:GrenadeLauncher")]
    [SerializeField]
    private Sprite[] weaponSprites = new Sprite[4];
    private Image targetImage;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
        // �����`�F�b�N�iEditor ���ɂ�����ƕ֗��j
        if (weaponSprites == null || weaponSprites.Length != System.Enum.GetValues(typeof(WeaponType)).Length)
        {
            Debug.LogError($"[{nameof(WeaponIconSwitcher)}] weaponSprites �̗v�f���� {System.Enum.GetValues(typeof(WeaponType)).Length} �ɂ��Ă�������");
        }
    }

    /// <summary>
    /// index �Ԗڂ̃X�v���C�g�ɐ؂�ւ��āA���T�C�Y�ɍ��킹��
    /// </summary>
    public void SetWeaponIcon(WeaponType type, float scale = 1f)
    {
        Sprite sp = weaponSprites[(int)type];
        targetImage.sprite = sp;
        if (type == WeaponType.Sword)
        {
            scale = 1.6f; // Sword �̂݃T�C�Y��傫������
        }
        else if (type == WeaponType.GrenadeLauncher)
        {
            scale = 0.85f; // GrenadeLauncher �̂݃T�C�Y������������   
        }
        // RectTransform �擾
        RectTransform rt = targetImage.rectTransform;

        // �X�v���C�g�̌��s�N�Z���T�C�Y �~ scale �{ �� sizeDelta �ɃZ�b�g
        Vector2 native = sp.rect.size;        // ex. (512, 512)
        rt.sizeDelta = native * scale;        // ex. (512,512) * 0.25f �� (128,128)

    }
    
}
