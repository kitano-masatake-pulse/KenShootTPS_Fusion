using System.Collections;
using System;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ���X�|�[���p�p�l��
public class RespawnPanel : MonoBehaviour, IHUDPanel
{
    [SerializeField] private CanvasGroup respawnPanelGroup;
    [SerializeField] private TMP_Text countdownText, killerText;
    [SerializeField] private Button respawnBtn;
    [SerializeField] private Image fadePanel;
    [Header("���X�|�[��UI�\���̃t�F�[�h�ƒx���̐ݒ�")]
    [SerializeField] private float UIfadeTime = 0.5f, delay = 5f;
    [Header("���X�|�[�����̃t�F�[�h���Ԑݒ�")]
    [SerializeField] private float respawnFadeTime = 3f;

    public static event Action OnRespawnClicked;

    private Coroutine co;
    private Coroutine currentFade;

    public void Initialize(PlayerNetworkState _, PlayerAvatar __)
    {
        GameManager.Instance.OnMyPlayerDied -= DisplayRespawnPanel;
        GameManager.Instance.OnMyPlayerDied += DisplayRespawnPanel;
    }
    public void Cleanup()
    {
        GameManager.Instance.OnMyPlayerDied -= DisplayRespawnPanel;
    }

    private void DisplayRespawnPanel(PlayerRef killer, float hostTimeStamp)
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(WaitRespawnCoroutine(killer));
    }

    private IEnumerator WaitRespawnCoroutine(PlayerRef killer)
    {
        respawnBtn.gameObject.SetActive(false);
        respawnPanelGroup.alpha = 0; 
        respawnPanelGroup.blocksRaycasts = true;
        killerText.text = "";
        countdownText.text = "";

        // �t�F�[�h�C��
        float t = 0;
        while (t < UIfadeTime)
        {
            t += Time.deltaTime;
            respawnPanelGroup.alpha = t / UIfadeTime;
            yield return null;
        }
        respawnPanelGroup.alpha = 1; 
        killerText.text = $"You were killed by {killer.PlayerId}";

        // �J�E���g�_�E���J�n
        float rem = delay;
        while (rem > 0)
        {
            countdownText.text = $"Respawn in {rem:F0}";
            yield return new WaitForSeconds(1f);
            rem -= 1f;
        }
        countdownText.text = "";
        respawnBtn.gameObject.SetActive(true);
    }
    // ���X�|�[���{�^���������ꂽ���̏���
    public void OnRespawnClick()
    {
        StopAllCoroutines();
        respawnPanelGroup.alpha = 1;
        respawnPanelGroup.blocksRaycasts = false;
        killerText.text = "";
        countdownText.text = "";

        if (fadePanel == null)
            Debug.LogError("Fade Image ���A�T�C������Ă��܂���");
        fadePanel.color = new Color(0, 0, 0, 0);
        FadeOut(() =>
        {
            OnRespawnClicked?.Invoke();
        });
    }
    /// <summary>
    /// ��ʂ����Ƀt�F�[�h�A�E�g���A������� onComplete ���Ă�
    /// </summary>
    public void FadeOut(System.Action onComplete = null)
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeRoutine(0f, 1f, respawnFadeTime, onComplete));
        respawnPanelGroup.alpha = 0;
        respawnPanelGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// ��ʂ𓧖��Ƀt�F�[�h�C�����A������� onComplete ���Ă�
    /// </summary>
    public void FadeIn(System.Action onComplete = null)
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeRoutine(1f, 0f, respawnFadeTime, onComplete));
    }

    private IEnumerator FadeRoutine(float fromAlpha, float toAlpha, float duration, System.Action onComplete)
    {
        float elapsed = 0f;
        Color c = fadePanel.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            c.a = Mathf.Lerp(fromAlpha, toAlpha, t);
            fadePanel.color = c;
            yield return null;
        }
        // �ŏI�l���m���ɃZ�b�g
        c.a = toAlpha;
        fadePanel.color = c;
        currentFade = null;
        onComplete?.Invoke();
    }
   
}
