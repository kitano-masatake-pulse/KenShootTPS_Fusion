using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeUI : MonoBehaviour, IUIPanel
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] private Image fadePanel;

    private Coroutine currentFade;

    public void Initialize()
    {
        canvasGroup.alpha = 1f; 
        canvasGroup.interactable = false; // インタラクション不可
        canvasGroup.blocksRaycasts = false; // レイキャストをブロックしない
    }

    public void Cleanup()
    {
        StopAllCoroutines(); // 全てのコルーチンを停止
    }

    // Update is called once per frame
    public void SetVisible(bool visible)
    {
        canvasGroup.alpha = visible ? 1f : 0f;
    }

    public IEnumerator FadeAlpha(float from, float to, float duration)
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeRoutine(from, to, duration));
        yield return currentFade;
    }

    // 実際のフェード処理
    private IEnumerator FadeRoutine(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;
        Color panelColor = fadePanel.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            panelColor.a = Mathf.Lerp(fromAlpha, toAlpha, t);
            fadePanel.color = panelColor;
            yield return null;
        }
        panelColor.a = toAlpha;
        fadePanel.color = panelColor;
        currentFade = null;
    }

}
