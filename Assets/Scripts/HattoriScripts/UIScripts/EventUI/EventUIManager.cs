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
    [SerializeField]
    BattleEndProcessor battleEndProcessor;


    private UIInputData localInputData;


    private void OnEnable()
    {
        GameManager.OnGameManagerSpawned += InitializeUI;
    }
    private void OnDisable()
    {
        GameManager.OnGameManagerSpawned -= InitializeUI;
        CleanupAll();
    }

    private void InitializeUI()
    {
       
        scoreboardUI.Initialize();

        respawnUI.SetRespawnHandler(respawnHandler);
        respawnUI.Initialize();

        countDownUI.SetRespawnHandler(respawnHandler);
        countDownUI.Initialize();

        gameEndUI.SetEndProcessor(battleEndProcessor);
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

    void Update()
    {
        InputCheck();
    }

    private void InputCheck()
    {
        localInputData = LocalInputHandler.CollectUIInput();

        if(localInputData.ScoreBoardPressdDown)
        {
            scoreboardUI.SetVisible(true);
        }else if(localInputData.ScoreBoardPressdUp)
        {
            scoreboardUI.SetVisible(false);
        }

    }
}
