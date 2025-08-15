using TMPro;
using UnityEngine;

// タイマー用パネル
public class TimerPanel : MonoBehaviour, IHUDPanel
{
    [SerializeField] private TMP_Text timerText;

    public void Initialize(PlayerNetworkState _, PlayerAvatar __)
    {
        GameManager2.Instance.OnTimeChanged -= UpdateTimerText;
        GameManager2.Instance.OnTimeChanged += UpdateTimerText;
        UpdateTimerText(GameManager2.Instance.initialTimeSec);
    }
    public void Cleanup()
    {
        GameManager2.Instance.OnTimeChanged -= UpdateTimerText;
    }
    private void UpdateTimerText(int sec)
    {
        //Debug.Log($"TimerPanel: UpdateTimerText called with sec={sec}");
        timerText.text = $"{sec / 60:00}:{sec % 60:00}";
    }
}
