using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneChangeFade : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeInDuration = 2f; // フェードのデフォルト時間
    // Start is called before the first frame update
    private Coroutine currentFade;


    public void Awake()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false; // インタラクション不可
        canvasGroup.blocksRaycasts = false; // レイキャストをブロックしない
        SceneManager.sceneLoaded += SceneFadeIn; // シーンロード時にフェードイン
    }
    private void OnDestroy()
    {
        StopAllCoroutines(); // 全てのコルーチンを停止
        SceneManager.sceneLoaded -= SceneFadeIn; // シーンロード時のイベントを解除
    }

    private void SceneFadeIn(Scene scene, LoadSceneMode mode)
    {
        //シーンロード後にフェードインを開始
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeRoutine(1f, 0f, fadeInDuration)); // フェードインの時間を2秒に設定
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
