using System.Collections;
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
    [Header("�t�F�[�h�ƒx���̐ݒ�")]
    [SerializeField] private float fadeTime = 1f, delay = 5f;
    private PlayerNetworkState playerState;

    private Coroutine co;

    public void Initialize(PlayerNetworkState pState, WeaponLocalState _)
    {
        playerState = pState;
        // �C�x���g�o�^
        playerState.OnPlayerDied -= DisplayRespawnPanel; 
        playerState.OnPlayerDied += DisplayRespawnPanel;
    }
    public void Cleanup()
    {
        playerState.OnPlayerDied -= DisplayRespawnPanel;
    }

    private void DisplayRespawnPanel(PlayerRef victim, PlayerRef killer)
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

        // �t�F�[�h�C��
        float t = 0;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            respawnPanelGroup.alpha = t / fadeTime;
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
        respawnPanelGroup.alpha = 0; 
        respawnPanelGroup.blocksRaycasts = false;
    }
}
