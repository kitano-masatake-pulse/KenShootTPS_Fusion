using Fusion;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// ���X�|�[���p�p�l��
public class RespawnPanel : MonoBehaviour, IHUDPanel
{
    [SerializeField] private CanvasGroup respawnPanelGroup;
    [SerializeField] private TMP_Text countdownText, killerText;
    [SerializeField] private Button respawnBtn;
    [Header("���X�|�[��UI�\���̃t�F�[�h�ƒx���̐ݒ�")]
    [SerializeField] private float UIfadeTime = 0.3f, delay = 5f;

    [SerializeField] private LocalRespawnHandler respawnHandler;


    private Coroutine co;

    public void Initialize(PlayerNetworkState _, PlayerAvatar __)
    {
        GameManager.Instance.OnMyPlayerDied -= DisplayRespawnPanel;
        GameManager.Instance.OnMyPlayerDied += DisplayRespawnPanel;
    }
    public void Cleanup()
    {
        GameManager.Instance.OnMyPlayerDied -= DisplayRespawnPanel;
    }

    private void DisplayRespawnPanel(float hostTimeStamp, PlayerRef killer)
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(WaitRespawnCoroutine(killer));
    }

    private IEnumerator WaitRespawnCoroutine(PlayerRef killer)
    {
        ResetUI(); // UI�����Z�b�g
        respawnPanelGroup.blocksRaycasts = true;
        respawnPanelGroup.interactable = true;

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
        Debug.Log("RespawnPanel:���X�|�[���{�^����������܂����B���X�|�[���������J�n���܂��B");

        // �R���[�`�����~
        if (co != null)
        {
            StopCoroutine(co);
            co = null;
        }
        respawnPanelGroup.interactable = false;
        killerText.text = "Now Respawning�c";

        // UI�I����Ԃ�����
        EventSystem.current.SetSelectedGameObject(null);

        // ���X�|�[���v��
        respawnHandler.RespawnStart();
    }

    //UI�̏�����
    public void ResetUI()
    {
        respawnPanelGroup.alpha = 0;
        respawnPanelGroup.blocksRaycasts = false;
        respawnPanelGroup.interactable = false;
        killerText.text = "";
        countdownText.text = "";
        respawnBtn.gameObject.SetActive(false);
    }

}

