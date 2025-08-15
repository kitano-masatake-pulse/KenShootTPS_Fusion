using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//キー入力や特定条件で出てくるUIを管理するクラス
public class EventUIManager : MonoBehaviour
{
    [SerializeField]
    ScoreBoardUI scoreboardUI;
    [SerializeField]
    RespawnUI respawnUI;
    [SerializeField]
    CountDownUI countDownUI;
    [SerializeField]
    GameEndUI gameEndUI;
    [SerializeField]
    FadeUI fadeUI;
    [SerializeField]
    LocalRespawnHandler respawnHandler;



    private void OnEnable()
    {
        GameManager2.OnManagerInitialized += InitializeUI;
    }
    private void OnDisable()
    {
        GameManager2.OnManagerInitialized -= InitializeUI;
        CleanupAll();
    }

    private void InitializeUI()
    {
       Debug.Log("EventUIManager: Initializing UI components.");
        scoreboardUI.Initialize();

        respawnUI.SetRespawnHandler(respawnHandler);
        respawnUI.Initialize();

        countDownUI.SetRespawnHandler(respawnHandler);
        countDownUI.Initialize();

        gameEndUI.Initialize();

        fadeUI.Initialize();

    }

    private void CleanupAll()
    {
        scoreboardUI.Cleanup();
        respawnUI.Cleanup();
        countDownUI.Cleanup();
        gameEndUI.Cleanup();
        fadeUI.Cleanup();
    }
}
