using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class CountDownPanel : MonoBehaviour, IHUDPanel
{
    [SerializeField] LocalRespawnHandler respawnHandler;
    [SerializeField] TMP_Text countdownText;

    //コルーチン
    private Coroutine countdownCoroutine;

    //Initializeメソッドは、パネルの初期化を行う
    // Cleanupメソッドは、パネルのクリーンアップを行う
    public void Initialize(PlayerNetworkState _, PlayerAvatar __)
    {
        respawnHandler.OnRespawnStuned -= DisplayCountDownPanel;
        respawnHandler.OnRespawnStuned += DisplayCountDownPanel;
        countdownText.raycastTarget = false; // カウントダウンテキストはクリックできないようにする
    }

    public void Cleanup()
    {
        respawnHandler.OnRespawnStuned -= DisplayCountDownPanel;
    }

    //コルーチンによって、カウントダウンパネルを表示する
    private void DisplayCountDownPanel(float stunDuration)
    {
        // 既存のカウントダウンコルーチンを停止
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
        }
        StartCoroutine(CountDownCoroutine(stunDuration)); 
    }
    private IEnumerator CountDownCoroutine(float stunDuration)
    {
        float rem = stunDuration;
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
}
