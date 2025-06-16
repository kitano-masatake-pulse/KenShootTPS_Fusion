using TMPro;
using UnityEngine;

// �^�C�}�[�p�p�l��
public class TimerPanel : MonoBehaviour, IHUDPanel
{
    [SerializeField] private TMP_Text timerText;

    public void Initialize(PlayerNetworkState _, PlayerAvatar __)
    {
        GameManager.Instance.OnTimeChanged -= UpdateTimerText;
        GameManager.Instance.OnTimeChanged += UpdateTimerText;
        UpdateTimerText(GameManager.Instance.initialTimeSec);
    }
    public void Cleanup()
    {
        GameManager.Instance.OnTimeChanged -= UpdateTimerText;
    }
    private void UpdateTimerText(int sec)
    {
        //Debug.Log($"TimerPanel: UpdateTimerText called with sec={sec}");
        timerText.text = $"{sec / 60:00}:{sec % 60:00}";
    }
}
