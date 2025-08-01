using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameEndUI : MonoBehaviour,IUIPanel
{
    [SerializeField] private CanvasGroup scoreboardGroup;
    [SerializeField] private TMP_Text endMessageText;

    public void Initialize()
    {
        BattleEndProcessor.Instance.OnBattleEnd -= ShowEndMessage;
        BattleEndProcessor.Instance.OnBattleEnd += ShowEndMessage;
    }
    public void Cleanup()
    {
        BattleEndProcessor.Instance.OnBattleEnd -= ShowEndMessage;
    }

    public void SetVisible(bool visible)
    {
        scoreboardGroup.alpha = 1;
    }


    public void ShowEndMessage()
    {
        SetVisible(true);
    }
}
