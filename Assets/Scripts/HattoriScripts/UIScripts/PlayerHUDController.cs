using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

//�v���C���̓���ɕ\�������UI�𐧌䂷��N���X
public class PlayerHUDController : NetworkBehaviour
{
    [Header("World-Space Canvas ��� UI �v�f")]
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private Slider hpBar;

    private PlayerNetworkState state;

    public override void Spawned()
    {
        //�l�b�g���[�N��ԃR���|�[�l���g�擾
        state = GetComponent<PlayerNetworkState>();
        if (state == null)
        {
            Debug.LogError("PlayerNetworkState not found!");
        }

        //�v���C���[�����Z�b�g
        nameLabel.text = $"Player({Object.InputAuthority.PlayerId})";

        // �����̃L�����Ȃ��\���A����ȊO�͕\��
        bool isLocal = HasInputAuthority;
        nameLabel.gameObject.SetActive(!isLocal);
        hpBar.gameObject.SetActive(!isLocal);
        // �C�x���g�o�^
        InitializeSubscriptions();
        // �����l�����f
        hpBar.value = state.HpNormalized;
    }


    void InitializeSubscriptions()
    {
        // ��ɉ������Ă���
        state.OnHPChanged -= UpdateHPBar;

        // ���߂ēo�^
        state.OnHPChanged += UpdateHPBar;
    }


    //HP���ω������������Ă΂��HP�o�[���X�V����
    public void UpdateHPBar(float normalized)
    {
        hpBar.value = normalized;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (state != null)
            state.OnHPChanged -= UpdateHPBar;
    }
}
