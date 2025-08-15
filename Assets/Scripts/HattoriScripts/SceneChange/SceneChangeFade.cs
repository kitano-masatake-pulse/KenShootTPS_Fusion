using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class SceneChangeFade : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] private Image fadePanel;
    private Coroutine currentFade;


    private void Awake()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false; // �C���^���N�V�����s��
        canvasGroup.blocksRaycasts = false; // ���C�L���X�g���u���b�N���Ȃ�
    }

    // ���ۂ̃t�F�[�h����
    public IEnumerator FadeRoutine(float fromAlpha, float toAlpha, float duration, Action onComplete = null)
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
        onComplete?.Invoke(); // �t�F�[�h�������ɃR�[���o�b�N���Ă�
    }
}



