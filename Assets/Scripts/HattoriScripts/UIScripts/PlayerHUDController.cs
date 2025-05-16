using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class PlayerHUDController : NetworkBehaviour
{
    [Header("World-Space Canvas ��� UI �v�f")]
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private Slider hpBar;

    private PlayerNetworkState state;

    public override void Spawned()
    {
        // 1) �l�b�g���[�N��ԃR���|�[�l���g�擾
        state = GetComponent<PlayerNetworkState>();
        if (state == null)
        {
            Debug.LogError("PlayerNetworkState not found!");
        }

        // 2) ���O�Z�b�g
        nameLabel.text = $"Player({Object.InputAuthority.PlayerId})";

        // 3) �����̃L�����Ȃ�ŏ������\����
        bool isLocal = HasInputAuthority;
        //�f�o�b�O�p�ɃR�����g�A�E�g��
        //nameLabel.gameObject.SetActive(!isLocal);
        //hpBar.gameObject.SetActive(!isLocal);
    }

    //HP���ω������������Ă΂��HP�o�[���X�V����
    public void UpdateHPBar(float v)
    {

        {
            hpBar.value = v;
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        // �N���[���A�b�v�i����΁j
        state = null;
    }
}
