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
    [Header("WeaponType �ɑΉ�����X�v���C�g(���Ԃ� enum �ƍ��킹��)")]
    [Tooltip("0:Sword, 1:AssaultRifle, 2:SemiAutoRifle, 3:GrenadeLauncher")]
    [SerializeField] private Sprite[] weaponSprites = new Sprite[4];
    [SerializeField] private Image targetImage;


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
        state.OnWeaponChanged -= UpdateWeapon;

        // ���߂ēo�^
        state.OnHPChanged += UpdateHPBar;
        state.OnWeaponChanged += UpdateWeapon;
    }


    //HP���ω������������Ă΂��HP�o�[���X�V����
    public void UpdateHPBar(float normalized)
    {
        hpBar.value = normalized;
    }
    void UpdateWeapon(WeaponType type)
    {
        var idx = (int)type;
        if (idx < 0 || idx >= weaponSprites.Length || weaponSprites[idx] == null)
        {
            Debug.LogWarning("�Ή�����X�v���C�g������܂���");
            return;
        }

        Sprite sp = weaponSprites[idx];
        targetImage.sprite = sp;

        // �X�P�[���͌��̂܂�
        float scale = 1f;
        var rt = targetImage.rectTransform;
        rt.sizeDelta = sp.rect.size * scale;

    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (state != null)
            state.OnHPChanged -= UpdateHPBar;
    }
}
