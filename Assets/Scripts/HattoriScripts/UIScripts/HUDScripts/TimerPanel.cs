using TMPro;
using UnityEngine;

// タイマー用パネル
public class TimerPanel : MonoBehaviour, IHUDPanel
{
    [SerializeField] private TMP_Text timerText;

    public void Initialize(PlayerNetworkState _, WeaponLocalState __)
    {
        GameManager.Instance.OnTimeChanged -= UpdateTimerText;
        GameManager.Instance.OnTimeChanged += UpdateTimerText;
        UpdateTimerText(GameTimeManager.initialTimeSec);
    }
    public void Cleanup()
    {
        GameTimeManager.OnTimeChanged -= UpdateTimerText;
    }
    private void UpdateTimerText(int sec)
    {
        Debug.Log($"TimerPanel: UpdateTimerText called with sec={sec}");
        timerText.text = $"{sec / 60:00}:{sec % 60:00}";
    }
}
