using UnityEngine;
using UnityEngine.UI;
using Fusion;

// HP �p�p�l��
public class HPPanel : MonoBehaviour, IHUDPanel
{
    [SerializeField] private Slider hpSlider;
    private PlayerNetworkState playerState;

    public void Initialize(PlayerNetworkState pState, PlayerAvatar _)
    {
        playerState = pState;
        // �C�x���g�o�^
        playerState.OnHPChanged -= UpdateHPBar;
        playerState.OnHPChanged += UpdateHPBar;
        playerState.OnHPChanged += OrderTestHP; // �e�X�g�p�C�x���g�o�^
        if (GameManager2.Instance != null)
        {
            GameManager2.Instance.OnScoreChanged += OrderTestScore; // GameManager�̃C�x���g�o�^
            GameManager2.Instance.OnAnyPlayerDied += OrderTestDeath; // Player���S�C�x���g�o�^
        }
        // �����l�ݒ�
        UpdateHPBar(playerState.HpNormalized, PlayerRef.None);
    }
    public void Cleanup()
    {
        playerState.OnHPChanged -= UpdateHPBar;
    }
    private void UpdateHPBar(float hpNormalized,PlayerRef _) => hpSlider.value = hpNormalized;

    private void OrderTestHP(float hpNormalized, PlayerRef _)
    {
        // �����Ƀe�X�g�R�[�h��ǉ����邱�Ƃ��ł��܂��B

        Debug.Log("OrderTest:HPPanel OrderTest called with HPChanged");
    }
    private void OrderTestScore()
    {
        // �����Ƀe�X�g�R�[�h��ǉ����邱�Ƃ��ł��܂��B
        Debug.Log("OrderTest:HPPanel OrderTest called with ScoreChanged");
    }
    private void OrderTestDeath(float timeStamp, PlayerRef victim, PlayerRef killer)
    {
        // �����Ƀe�X�g�R�[�h��ǉ����邱�Ƃ��ł��܂��B
        Debug.Log($"OrderTest:HPPanel OrderTest called with PlayerDied");
    }
}