using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneChangeFade : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeInDuration = 2f; // �t�F�[�h�̃f�t�H���g����
    // Start is called before the first frame update
    private Coroutine currentFade;


    public void Awake()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false; // �C���^���N�V�����s��
        canvasGroup.blocksRaycasts = false; // ���C�L���X�g���u���b�N���Ȃ�
        SceneManager.sceneLoaded += SceneFadeIn; // �V�[�����[�h���Ƀt�F�[�h�C��
    }
    private void OnDestroy()
    {
        StopAllCoroutines(); // �S�ẴR���[�`�����~
        SceneManager.sceneLoaded -= SceneFadeIn; // �V�[�����[�h���̃C�x���g������
    }

    private void SceneFadeIn(Scene scene, LoadSceneMode mode)
    {
        //�V�[�����[�h��Ƀt�F�[�h�C�����J�n
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeRoutine(1f, 0f, fadeInDuration)); // �t�F�[�h�C���̎��Ԃ�2�b�ɐݒ�
    }
    


    public IEnumerator FadeAlpha(float from, float to, float duration)
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeRoutine(from, to, duration));
        yield return currentFade;
    }

    // ���ۂ̃t�F�[�h����
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
