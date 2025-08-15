using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

//�v���C���̓���ɕ\�������UI�𐧌䂷��N���X
public class PlayerWorldUIController : NetworkBehaviour
{
    [Header("World-Space Canvas ��� UI �v�f")]
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private Slider hpBar;


    private PlayerNetworkState pNState;

    public override void Spawned()
    {
        //�l�b�g���[�N��ԃR���|�[�l���g�擾
        pNState = GetComponentInParent<PlayerNetworkState>();
        if (pNState == null)
        {
            Debug.LogError("PlayerNetworkState not found!");
        }

        //�v���C���[�����Z�b�g
        nameLabel.text = $"Player({Object.InputAuthority.PlayerId})";

        // �����̃L�����Ȃ��\���A����ȊO�͕\��
        bool isLocal = HasInputAuthority;
        nameLabel.gameObject.SetActive(!isLocal);
        hpBar.gameObject.SetActive(!isLocal);

        //�`�[���J���[��K�p
        nameLabel.color = pNState.Team.GetColor(); 
        // �C�x���g�o�^
        InitializeSubscriptions();
        // �����l�����f
        hpBar.value = pNState.HpNormalized;
    }


    void InitializeSubscriptions()
    {
        // ��ɉ������Ă���
        pNState.OnHPChanged -= UpdateWorldHPBar;
        pNState.OnTeamChanged -= UpdateNemeColor;

        // ���߂ēo�^
        pNState.OnHPChanged += UpdateWorldHPBar;
        pNState.OnTeamChanged += UpdateNemeColor;
    }


    //HP���ω������������Ă΂��HP�o�[���X�V����
    public void UpdateWorldHPBar(float normalized, PlayerRef _)
    {
        hpBar.value = normalized;
    }


    private void UpdateNemeColor(TeamType team)
    {
        nameLabel.color = team.GetColor();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (pNState != null)
        {
            pNState.OnHPChanged -= UpdateWorldHPBar;

        }
    }
}
