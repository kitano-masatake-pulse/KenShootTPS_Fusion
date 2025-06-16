using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class CountDownUI : MonoBehaviour, IUIPanel
{
    private LocalRespawnHandler respawnHandler;
    [SerializeField] TMP_Text countdownText;
    [SerializeField] float defaultCountTime = 3f; // デフォルトのスタン時間
    //コルーチン
    private Coroutine countdownCoroutine;

    //Initializeメソッドは、パネルの初期化を行う
    // Cleanupメソッドは、パネルのクリーンアップを行う
    public void Initialize()
    {
        respawnHandler.OnRespawnStuned -= DisplayCountDownPanel;
        respawnHandler.OnRespawnStuned += DisplayCountDownPanel;
        countdownText.raycastTarget = false; // カウントダウンテキストはクリックできないようにする
        countdownText.text = ""; 
    }

    public void Cleanup()
    {
        respawnHandler.OnRespawnStuned -= DisplayCountDownPanel;
        SetVisible(false); // パネルを非表示にする
    }

    public void SetRespawnHandler(LocalRespawnHandler handler)
    {
        respawnHandler = handler;
    }

    //コルーチンによって、カウントダウンパネルを表示する
    private void DisplayCountDownPanel(float countTime)
    {
        // 既存のカウントダウンコルーチンを停止
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }
        StartCoroutine(CountDownCoroutine(countTime)); 
    }
    private IEnumerator CountDownCoroutine(float countTime)
    {
        float rem = countTime;
        while (rem > 0)
        {
            countdownText.text = $"{rem:F0}";
            yield return new WaitForSeconds(1f);
            rem -= 1f;
        }
        countdownText.text = "GO!";
        yield return new WaitForSeconds(1f);
        countdownText.text = ""; // カウントダウン終了後はテキストをクリア
    }
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
        if (visible && countdownCoroutine == null)
        {
            countdownCoroutine = StartCoroutine(CountDownCoroutine(defaultCountTime)); 
        }
        else if (!visible && countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
            countdownText.text = ""; // 非表示時にテキストをクリア
        }
    }
}
