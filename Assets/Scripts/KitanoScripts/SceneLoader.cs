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
        // �V���O���g��������
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        this.transform.SetParent(null); // �e�I�u�W�F�N�g������
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

        SceneManager.sceneLoaded -= OnSceneLoaded; // �d���h�~
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// �V�[���J�ڂ̊J�n�iSceneType�Ŏw��j
    /// </summary>
    private bool isLoading = false;

    public void LoadScene(SceneType sceneType)
    {
        if (isLoading) return; // 2�d�J�ڂ�h�~
        isLoading = true;

        string sceneName = sceneType.ToSceneName();
        if (!string.IsNullOrEmpty(sceneName))
        {
            StartCoroutine(LoadSceneRoutine(sceneName));
        }
        else
        {
            Debug.LogError("SceneLoader: ������SceneType");
            isLoading = false; // ���s�����ꍇ�̓t���O�߂�
        }


    }

    /// <summary>
    /// SceneType �� ���ۂ̃V�[�����ɕϊ�
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

        // �t�F�[�h�A�E�g�͐V�V�[�����Ŗ����I�ɌĂ�
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
        Debug.Log("�V�����V�[�����ǂݍ��܂ꂽ�̂ŏ��������܂��I");
        // �v���C���[������HUD�������Ȃǂ��s�������Ƃ�
        
        if(loadingOverlay.alpha != 0)
        {
            FadeOut(); // ���[�f�B���OUI������
        }


    }

    void OnDestroy()
    {
        // �C�x���g�����i�d�v�I�j
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

}
