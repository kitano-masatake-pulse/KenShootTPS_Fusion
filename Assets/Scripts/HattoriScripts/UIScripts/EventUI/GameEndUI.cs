using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameEndUI : MonoBehaviour,IUIPanel
{
    [SerializeField] private CanvasGroup scoreboardGroup;
    [SerializeField] private TMP_Text endMessageText;
    private BattleEndProcessor battleEndProcessor;

    public void Initialize()
    {

        battleEndProcessor.OnGameEnd -= ShowEndMessage;
        battleEndProcessor.OnGameEnd += ShowEndMessage;
    }
    public void Cleanup()
    {
        battleEndProcessor.OnGameEnd -= ShowEndMessage;
    }

    public void SetVisible(bool visible)
    {
        scoreboardGroup.alpha = 1;
    }

    public void SetEndProcessor(BattleEndProcessor bep)
    {
        battleEndProcessor = bep;
    }

    public void ShowEndMessage()
    {
        SetVisible(true);
    }
}
