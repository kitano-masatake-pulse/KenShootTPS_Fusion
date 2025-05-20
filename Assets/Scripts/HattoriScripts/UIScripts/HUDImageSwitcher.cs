using UnityEngine;
using UnityEngine.UI;

public class HUDDynamicSizeSwitcher : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite[] sprites;  // �S�̃X�v���C�g

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();
    }

    /// <summary>
    /// index �Ԗڂ̃X�v���C�g�ɐ؂�ւ��āA���T�C�Y�ɍ��킹��
    /// </summary>
    public void SetSpriteAndResize(int index)
    {
        index = Mathf.Clamp(index, 0, sprites.Length - 1);
        var sp = sprites[index];
        targetImage.sprite = sp;

        // �@ �X�v���C�g�̃l�C�e�B�u�T�C�Y�ɑ�����
        targetImage.SetNativeSize();
        transform.localScale = Vector3.one * 0.25f;
        // �� �����ŏI�I�� Canvas �X�P�[���Ȃǂő傫����ς������ꍇ��
        //    transform.localScale = Vector3.one * �C�ӂ̔{��;
    }
}
