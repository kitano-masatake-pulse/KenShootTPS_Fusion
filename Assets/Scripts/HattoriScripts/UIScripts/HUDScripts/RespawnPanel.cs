using System.Collections;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// リスポーン用パネル
public class RespawnPanel : MonoBehaviour, IHUDPanel
{
    [SerializeField] private CanvasGroup respawnPanelGroup;
    [SerializeField] private TMP_Text countdownText, killerText;
    [SerializeField] private Button respawnBtn;
    [Header("フェードと遅延の設定")]
    [SerializeField] private float fadeTime = 1f, delay = 5f;

    private Coroutine co;

    public void Initialize(PlayerNetworkState _, WeaponLocalState __)
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
        co = StartCoroutine(DoRespawn(killer));
    }

    private IEnumerator DoRespawn(PlayerRef killer)
    {
        respawnBtn.gameObject.SetActive(false);
        respawnPanelGroup.alpha = 0; 
        respawnPanelGroup.blocksRaycasts = true;
        killerText.text = "";
        countdownText.text = "";

        // フェードイン
        float t = 0;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            respawnPanelGroup.alpha = t / fadeTime;
            yield return null;
        }
        respawnPanelGroup.alpha = 1; 
        killerText.text = $"You were killed by {killer.PlayerId}";

        // カウントダウン開始
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
    // リスポーンボタンが押された時の処理
    public void OnRespawnClick()
    {
        StopAllCoroutines();
        respawnPanelGroup.alpha = 0; 
        respawnPanelGroup.blocksRaycasts = false;
    }
}
