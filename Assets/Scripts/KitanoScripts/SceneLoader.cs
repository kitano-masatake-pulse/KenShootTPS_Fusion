using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private CanvasGroup loadingOverlay;
    [SerializeField] private GameObject spinner;
    [SerializeField] private float fadeDuration = 0.5f;

    private void Awake()
    {
        // シングルトン初期化
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        this.transform.SetParent(null); // 親オブジェクトを解除
        DontDestroyOnLoad(gameObject);

        if (loadingOverlay != null)
        {
            loadingOverlay.alpha = 0;
            loadingOverlay.blocksRaycasts = false;
        }

        if (spinner != null)
        {
            spinner.SetActive(false);
        }

        SceneManager.sceneLoaded -= OnSceneLoaded; // 重複防止
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// シーン遷移の開始（SceneTypeで指定）
    /// </summary>
    private bool isLoading = false;

    public void LoadScene(SceneType sceneType)
    {
        if (isLoading) return; // 2重遷移を防止
        isLoading = true;

        string sceneName = sceneType.ToSceneName();
        if (!string.IsNullOrEmpty(sceneName))
        {
            StartCoroutine(LoadSceneRoutine(sceneName));
        }
        else
        {
            Debug.LogError("SceneLoader: 無効なSceneType");
            isLoading = false; // 失敗した場合はフラグ戻す
        }


    }

    /// <summary>
    /// SceneType → 実際のシーン名に変換
    /// </summary>
    private string GetSceneName(SceneType type)
    {
        switch (type)
        {
            case SceneType.Title: return "TitleScene";
            case SceneType.Lobby: return "LobbyScene";
            case SceneType.Battle: return "BattleScene";
            case SceneType.Result: return "ResultScene";
            default: return null;
        }
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        yield return StartCoroutine(FadeIn());

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        op.allowSceneActivation = true;

        // フェードアウトは新シーン側で明示的に呼ぶ
    }

    private IEnumerator FadeIn()
    {
        if (loadingOverlay != null) loadingOverlay.blocksRaycasts = true;
        if (spinner != null) spinner.SetActive(true);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            if (loadingOverlay != null)
                loadingOverlay.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }
    }

    public void FadeOut()
    {
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            if (loadingOverlay != null)
                loadingOverlay.alpha = Mathf.Clamp01(1 - (t / fadeDuration));
            yield return null;
        }

        if (loadingOverlay != null)
        {
            loadingOverlay.alpha = 0;
            loadingOverlay.blocksRaycasts = false;
        }

        if (spinner != null)
        {
            spinner.SetActive(false);
        }

        isLoading = false;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("新しいシーンが読み込まれたので初期化します！");
        // プレイヤー生成やHUD初期化などを行ったあとに
        
        if(loadingOverlay.alpha != 0)
        {
            FadeOut(); // ローディングUIを消す
        }


    }

    void OnDestroy()
    {
        // イベント解除（重要！）
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

}
