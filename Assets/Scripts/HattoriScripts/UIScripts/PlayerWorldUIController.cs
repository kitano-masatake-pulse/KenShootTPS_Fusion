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
    [Header("WeaponType �ɑΉ�����X�v���C�g(���Ԃ� enum �ƍ��킹��)")]
    [Tooltip("0:Sword, 1:AssaultRifle, 2:SemiAutoRifle, 3:GrenadeLauncher")]
    [SerializeField] private Sprite[] weaponSprites = new Sprite[4];
    [SerializeField] private Image weaponImage;


    private PlayerNetworkState pNState;

    public override void Spawned()
    {
        //�l�b�g���[�N��ԃR���|�[�l���g�擾
        pNState = GetComponent<PlayerNetworkState>();
        if (pNState == null)
        {
            Debug.LogError("PlayerNetworkState not found!");
        }

        //�v���C���[�����Z�b�g
        nameLabel.text = $"Player({Object.InputAuthority.PlayerId})";

        // �����̃L�����Ȃ��\���A����ȊO�͕\��
        bool isLocal = HasInputAuthority;
        nameLabel.gameObject.SetActive(!isLocal);
        hpBar.gameObject.SetActive(true);
        //weaponImage.gameObject.SetActive(!isLocal);
        // �C�x���g�o�^
        InitializeSubscriptions();
        // �����l�����f
        hpBar.value = pNState.HpNormalized;
    }


    void InitializeSubscriptions()
    {
        // ��ɉ������Ă���
        pNState.OnHPChanged -= UpdateWorldHPBar;
        pNState.OnWeaponChanged_Network -= UpdateWorldWeapon;

        // ���߂ēo�^
        pNState.OnHPChanged += UpdateWorldHPBar;
        pNState.OnWeaponChanged_Network += UpdateWorldWeapon;
    }


    //HP���ω������������Ă΂��HP�o�[���X�V����
    public void UpdateWorldHPBar(float normalized)
    {
        hpBar.value = normalized;
    }
    void UpdateWorldWeapon(WeaponType type)
    {
        var idx = (int)type;
        if (idx < 0 || idx >= weaponSprites.Length || weaponSprites[idx] == null)
        {
            Debug.LogWarning("�Ή�����X�v���C�g������܂���");
            return;
        }

        Sprite sp = weaponSprites[idx];
        weaponImage.sprite = sp;

        // �X�P�[���͌��̂܂�
        float scale = 1f;
        var rt = weaponImage.rectTransform;
        rt.sizeDelta = sp.rect.size * scale;

    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (pNState != null)
            pNState.OnHPChanged -= UpdateWorldHPBar;
    }
}
