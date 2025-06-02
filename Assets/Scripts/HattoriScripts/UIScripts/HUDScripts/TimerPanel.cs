using TMPro;
using UnityEngine;

// タイマー用パネル
public class TimerPanel : MonoBehaviour, IHUDPanel
{
    [SerializeField] private TMP_Text timerText;

    public void Initialize(PlayerNetworkState _, WeaponLocalState __)
    {
        GameTimeManager.OnTimeChanged -= UpdateTimerText;
        GameTimeManager.OnTimeChanged += UpdateTimerText;
        UpdateTimerText(GameTimeManager.initialTimeSec);
    }
    public void Cleanup()
    {
        GameTimeManager.OnTimeChanged -= UpdateTimerText;
    }
    private void UpdateTimerText(int sec)
    {
        timerText.text = $"{sec / 60:00}:{sec % 60:00}";
    }
}
