using TMPro;
using UnityEngine;
using UnityEngine.UI;

// �v���C���[�� ��ʏ��HUD �𐧌䂷��N���X
public class HUDManager : MonoBehaviour
{
    // �v���C���[�� HP �═����\������UI
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text totalAmmoText;
    [SerializeField] private TMP_Text magazineAmmoText;
    [SerializeField] private TMP_Text killScoreText;
    [SerializeField] private TMP_Text deathScoreText;
    [Header("WeaponType �ɑΉ�����X�v���C�g(���Ԃ� enum �ƍ��킹��)")]
    [Tooltip("0:Sword, 1:AssaultRifle, 2:SemiAutoRifle, 3:GrenadeLauncher")]
    [SerializeField] private Sprite[] weaponSprites = new Sprite[4];
    [SerializeField] private Image targetImage;

    // �^�C�}�[�\���pUI
    [SerializeField] private TMP_Text timerText;

    // �v���C���[�̏�Ԃ�ێ�����N���X
    private PlayerNetworkState localState;

    //WeaponIcon�̑傫��
    private const float RifleScale = 0.625f;
    private const float GrenadeLauncherScale = 0.531f;

    private void OnEnable()
    {
        // ���[�J���v���C���[�����ʒm
        PlayerNetworkState.OnLocalPlayerSpawned += HandlePlayerSpawn;
        // �^�C�}�[�ʒm
        GameTimeManager.OnTimeChanged += HandleTimeChanged;
    }

    private void OnDisable()
    {
        PlayerNetworkState.OnLocalPlayerSpawned -= HandlePlayerSpawn;
        GameTimeManager.OnTimeChanged -= HandleTimeChanged;
        if (localState != null)
        {
            localState.OnHPChanged -= OnLocalHPChanged;
            localState.OnWeaponChanged -= OnLocalWeaponChanged;
            localState.OnScoreChanged -= OnLocalScoreChanged;
            localState.OnAmmoChanged -= OnLocalAmmoChanged;
        }
    }

    public void HandlePlayerSpawn(PlayerNetworkState state)
    {
        localState = state;
        //�C�x���g�o�^
        InitializeSubscriptions();
        // �����l���f
        // HP�̏����l�𔽉f
        OnLocalHPChanged(localState.HpNormalized);
        // �����̏����l�𔽉f
        OnLocalWeaponChanged(localState.CurrentWeapon);
        // �X�R�A�̏����l�𔽉f
        OnLocalScoreChanged(localState.KillScore, localState.DeathScore);
        // �e��̏����l�𔽉f
        OnLocalAmmoChanged(localState.TotalAmmo, localState.MagazineAmmo);
        // Weapon�ɓo�^����Splite�̒����`�F�b�N
        if (weaponSprites == null || weaponSprites.Length != System.Enum.GetValues(typeof(WeaponType)).Length)
        {
            Debug.LogError($"weaponSprites �̗v�f���� {System.Enum.GetValues(typeof(WeaponType)).Length} �ɂ��Ă�������");
        }
    }
  

    //����������
    void InitializeSubscriptions()
    {
        // ��ɉ������Ă���
        localState.OnHPChanged -= OnLocalHPChanged;
        localState.OnWeaponChanged -= OnLocalWeaponChanged;
        localState.OnScoreChanged -= OnLocalScoreChanged;
        localState.OnAmmoChanged -= OnLocalAmmoChanged;
        // �c

        // ���߂ēo�^
        localState.OnHPChanged += OnLocalHPChanged;
        localState.OnWeaponChanged += OnLocalWeaponChanged;
        localState.OnScoreChanged += OnLocalScoreChanged;
        localState.OnAmmoChanged += OnLocalAmmoChanged;
        // �c
    }

    //HP�C�x���g
    //HP���ω������������Ă΂��HP�o�[���X�V����
    private void OnLocalHPChanged(float normalized)
    {
        hpSlider.value = normalized;
    }

    //WeaponType�C�x���g
    // ����킪�ω������������Ă΂�ĕ����\�����X�V����
    void OnLocalWeaponChanged(WeaponType type)
    {
        float scale = 1f;

        

    Sprite sp = weaponSprites[(int)type];
        targetImage.sprite = sp;

        if (weaponSprites == null || (int)type >= weaponSprites.Length || weaponSprites[(int)type] == null)
        {
            Debug.LogWarning("�Ή�����X�v���C�g������܂���");
            return;
        }

        if (type == WeaponType.AssaultRifle || type == WeaponType.SemiAutoRifle)
        {
            scale = RifleScale; // ���C�t���n�̓T�C�Y������������
        }
        else if (type == WeaponType.GrenadeLauncher)
        {
            scale = GrenadeLauncherScale; // GrenadeLauncher �݂̂��T�C�Y������������   
        }
        // RectTransform �擾
        RectTransform rt = targetImage.rectTransform;

        // �X�v���C�g�̌��s�N�Z���T�C�Y �~ scale �{ �� sizeDelta �ɃZ�b�g
        Vector2 native = sp.rect.size;       
        rt.sizeDelta = native * scale;        

    }

    // Score�C�x���g
    // �X�R�A���ω������������Ă΂�ăX�R�A�\�����X�V����
    private void OnLocalScoreChanged(int killScore, int deathScore)
    {
        killScoreText.text = killScore.ToString();
        deathScoreText.text = deathScore.ToString();
    }

    // Ammo�C�x���g
    // �e�򂪕ω������������Ă΂�Ēe��\�����X�V����
    private void OnLocalAmmoChanged(int totalAmmo, int magazineAmmo)
    {
        totalAmmoText.text = totalAmmo.ToString();
        magazineAmmoText.text = magazineAmmo.ToString();
    }


    //�^�C�}�[�C�x���g(�����ɂ̓C�x���g�ł͂Ȃ��P�Ɏ󂯎�����܂܂Ɏ��Ԃ��X�V���镔��)
    // �^�C�}�[�̎��Ԃ��ω��������ɌĂ΂��
    private void HandleTimeChanged(int sec)
    {
        int m = sec / 60, s = sec % 60;
        timerText.text = $"{m:00}:{s:00}";
    }

}
