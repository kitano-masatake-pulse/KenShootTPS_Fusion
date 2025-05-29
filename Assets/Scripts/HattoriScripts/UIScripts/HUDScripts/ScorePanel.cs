using TMPro;
using UnityEngine;

// �X�R�A�p�p�l��
public class ScorePanel : MonoBehaviour, IHUDPanel
{
    [SerializeField] private TMP_Text killCount, deathCount;
    private PlayerNetworkState playerState;
    public void Initialize(PlayerNetworkState pState, WeaponLocalState _)
    {
        playerState = pState;
        //�C�x���g�o�^
        playerState.OnScoreChanged -= UpdateScoreText; 
        playerState.OnScoreChanged += UpdateScoreText;
        //�����l�ݒ�
        UpdateScoreText(playerState.KillScore, playerState.DeathScore);
    }
    public void Cleanup() {
        playerState.OnScoreChanged -= UpdateScoreText;
    }
    private void UpdateScoreText(int k, int d)
    {
        killCount.text = k.ToString();
        deathCount.text = d.ToString();
    }
}
