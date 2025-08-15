﻿using Fusion;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// リスポーン用パネル
public class RespawnUI : MonoBehaviour,IUIPanel
{
    [SerializeField] private CanvasGroup respawnPanelGroup;
    [SerializeField] private TMP_Text countdownText, killerText;
    [SerializeField] private Button respawnBtn;
    [Header("リスポーンUI表示のフェードと遅延の設定")]
    [SerializeField] private float UIfadeTime = 0.3f, delay = 5f;

    private LocalRespawnHandler respawnHandler;


    private Coroutine co;

    public void Initialize()
    {
        GameManager2.Instance.OnMyPlayerDied -= DisplayRespawnPanel;
        GameManager2.Instance.OnMyPlayerDied += DisplayRespawnPanel;
        ResetUI(); // UIをリセット
    }
    public void Cleanup()
    {
        GameManager2.Instance.OnMyPlayerDied -= DisplayRespawnPanel;
        CursorManager.Instance.ReleaseUI(this);

        ResetUI(); // UIをリセット
    }

    public void SetRespawnHandler(LocalRespawnHandler handler)
    {
        respawnHandler = handler;
    }

    private void DisplayRespawnPanel(float hostTimeStamp, PlayerRef killer)
    {
        CursorManager.Instance.RequestUI(this);
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(WaitRespawnCoroutine(killer));
    }

    private IEnumerator WaitRespawnCoroutine(PlayerRef killer)
    {
        ResetUI(); // UIをリセット
        respawnPanelGroup.blocksRaycasts = true;
        respawnPanelGroup.interactable = true;

        // フェードイン
        float t = 0;
        while (t < UIfadeTime)
        {
            t += Time.deltaTime;
            respawnPanelGroup.alpha = t / UIfadeTime;
            yield return null;
        }
        respawnPanelGroup.alpha = 1;
        killerText.text = $"You were killed by Player{killer.PlayerId}";

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
        Debug.Log("RespawnPanel:リスポーンボタンが押されました。リスポーン処理を開始します。");
        CursorManager.Instance.ReleaseUI(this);
        // コルーチンを停止
        if (co != null)
        {
            StopCoroutine(co);
            co = null;
        }
        respawnPanelGroup.interactable = false;
        killerText.text = "Now Respawning…";

        // UI選択状態を解除
        EventSystem.current.SetSelectedGameObject(null);

        // リスポーン要求
        respawnHandler.RespawnStart();
    }

    //UIの初期化
    public void ResetUI()
    {
        SetVisible(false);
    }

    public void SetVisible(bool visible)
    {
        respawnPanelGroup.alpha = visible ? 1 : 0;
        respawnPanelGroup.blocksRaycasts = visible;
        respawnPanelGroup.interactable = visible;
        killerText.text = visible ? "Test Now" : "";
        countdownText.text ="";
        respawnBtn.gameObject.SetActive(visible);
    }

}

