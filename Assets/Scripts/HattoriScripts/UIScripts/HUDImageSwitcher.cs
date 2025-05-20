using UnityEngine;
using UnityEngine.UI;

public class HUDDynamicSizeSwitcher : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite[] sprites;  // ４つのスプライト

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();
    }

    /// <summary>
    /// index 番目のスプライトに切り替えて、元サイズに合わせる
    /// </summary>
    public void SetSpriteAndResize(int index)
    {
        index = Mathf.Clamp(index, 0, sprites.Length - 1);
        var sp = sprites[index];
        targetImage.sprite = sp;

        // ① スプライトのネイティブサイズに揃える
        targetImage.SetNativeSize();
        transform.localScale = Vector3.one * 0.25f;
        // ※ もし最終的に Canvas スケールなどで大きさを変えたい場合は
        //    transform.localScale = Vector3.one * 任意の倍率;
    }
}
