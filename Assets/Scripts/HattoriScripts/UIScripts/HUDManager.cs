using TMPro;
using Fusion;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// �v���C���[�� ��ʏ��HUD �𐧌䂷��N���X
public class HUDManager : MonoBehaviour
{
    // �v���C���[�� HP �═����\������UI
    [Header("HP�pUI")]
    [SerializeField] private Slider hpSlider;

    [Header("����\���pUI")]
    [SerializeField] private TMP_Text reserveAmmoText;
    [SerializeField] private TMP_Text magazineAmmoText;
    [Header("WeaponType �ɑΉ�����X�v���C�g(���Ԃ� enum �ƍ��킹��)")]
    [Tooltip("0:Sword, 1:AssaultRifle, 2:SemiAutoRifle, 3:GrenadeLauncher")]
    [SerializeField] private Sprite[] weaponSprites = new Sprite[4];
    [SerializeField] private Image targetImage;

    [Header("�X�R�A�\���pUI")]
    [SerializeField] private TMP_Text killScoreText;
    [SerializeField] private TMP_Text deathScoreText;

    [Header("���X�|�[��UI")]
    [SerializeField] private CanvasGroup respawnUI;
    [SerializeField] private TMP_Text respawnCountdown;
    [SerializeField] private float respawnDelay = 5f;

    [Header("�^�C�}�[�\���pUI")]
    [SerializeField] private TMP_Text timerText;

    // �v���C���[�̏�Ԃ�ێ�����N���X
    private PlayerNetworkState pNState;
    //����̓��e��ێ�����N���X
    private WeaponLocalState wLState;

    //WeaponIcon�̑傫��
    // AssaultRifle, SemiAutoRifle �̑傫��
    private const float RifleScale = 0.53f;
    private const float GrenadeLauncherScale = 0.53f;

    private void OnEnable()
    {
        // ���[�J���v���C���[�����ʒm
        PlayerNetworkState.OnLocalPlayerSpawned += HandlePlayerSpawn;
        // ���퐶���ʒm
        WeaponLocalState.OnWeaponSpawned += HandleWeaponSpawn;
        // �^�C�}�[�ʒm
        GameTimeManager.OnTimeChanged += HandleTimeChanged;
  
    }

    private void OnDisable()
    {
        PlayerNetworkState.OnLocalPlayerSpawned -= HandlePlayerSpawn;
        WeaponLocalState.OnWeaponSpawned -= HandleWeaponSpawn;
        GameTimeManager.OnTimeChanged -= HandleTimeChanged;
        if (pNState != null)
        {
            pNState.OnHPChanged -= OnLocalHPChanged;
            pNState.OnScoreChanged -= OnLocalScoreChanged;
        }
        if(wLState != null)
        {
            wLState.OnWeaponChanged -= OnLocalWeaponChanged;
            wLState.OnAmmoChanged -= OnLocalAmmoChanged;
        }
    }

    public void HandlePlayerSpawn(PlayerNetworkState state)
    {
        pNState = state;
        //�C�x���g�o�^
        // ��ɉ������Ă���
        pNState.OnHPChanged -= OnLocalHPChanged;
        pNState.OnScoreChanged -= OnLocalScoreChanged;
        pNState.OnPlayerDied -= OnLocalPlayerDied;
        pNState.OnHPChanged += OnLocalHPChanged;
        pNState.OnScoreChanged += OnLocalScoreChanged;
        pNState.OnPlayerDied += OnLocalPlayerDied;
        // �����l���f
        OnLocalHPChanged(pNState.HpNormalized);
        OnLocalScoreChanged(pNState.KillScore, pNState.DeathScore);



    }

    private void HandleWeaponSpawn(WeaponLocalState weaponState)
    {
        wLState = weaponState;
        wLState.OnWeaponChanged -= OnLocalWeaponChanged;
        wLState.OnAmmoChanged -= OnLocalAmmoChanged;
        wLState.OnWeaponChanged += OnLocalWeaponChanged;
        wLState.OnAmmoChanged += OnLocalAmmoChanged;
        // �����\��
        OnLocalWeaponChanged(wLState.CurrentWeapon);
        var ammo = wLState.GetCurrentAmmo();
        OnLocalAmmoChanged(ammo.magazine, ammo.reserve);
        // Weapon�ɓo�^����Splite�̒����`�F�b�N
        if (weaponSprites == null || weaponSprites.Length != System.Enum.GetValues(typeof(WeaponType)).Length)
        {
            Debug.LogError($"weaponSprites �̗v�f���� {System.Enum.GetValues(typeof(WeaponType)).Length} �ɂ��Ă�������");
        }
    }

    //HP�C�x���g
    //HP���ω������������Ă΂��HP�o�[���X�V����
    private void OnLocalHPChanged(float normalized)
    {
        hpSlider.value = normalized;
    }

    //WeaponType�C�x���g
    // ����킪�ω������������Ă΂�ĕ����\�����X�V����
    void OnLocalWeaponChanged(WeaponType currentWeapon)
    {
        var idx = (int)currentWeapon;       
        if (idx < 0 || idx >= weaponSprites.Length || weaponSprites[idx] == null)
        {
            Debug.LogWarning("�Ή�����X�v���C�g������܂���");
            return;
        }

        Sprite sp = weaponSprites[idx];
        targetImage.sprite = sp;

        // �X�P�[���͌��̂܂�
        float scale = 1f;
        if (currentWeapon == WeaponType.AssaultRifle || currentWeapon == WeaponType.SemiAutoRifle)
            scale = RifleScale;
        else if (currentWeapon == WeaponType.GrenadeLauncher)
            scale = GrenadeLauncherScale;

        var rt = targetImage.rectTransform;
        rt.sizeDelta = sp.rect.size * scale;

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
    private void OnLocalAmmoChanged(int magazineAmmo, int reserveAmmo )
    {
        reserveAmmoText.text = reserveAmmo.ToString();
        magazineAmmoText.text = magazineAmmo.ToString();
    }

    // �v���C���[�����S�������̏���
    private void OnLocalPlayerDied(PlayerRef victim, PlayerRef killer)
    {
        StartCoroutine(RespawnCountdown());
    }

    private IEnumerator RespawnCountdown()
    {

        respawnUI.alpha = 1;
        float t = respawnDelay;
        while (t > 0)
        {
            respawnCountdown.text = Mathf.CeilToInt(t).ToString();
            yield return new WaitForSeconds(1f);
            t -= 1f;
        }
        respawnUI.alpha = 0;
        // �����Ń��X�|�[���{�^����\���A�����ƃ��X�|�[���������z�X�g�ɗv��
    }

    //�^�C�}�[�C�x���g(�����ɂ̓C�x���g�ł͂Ȃ��P�Ɏ󂯎�����܂܂Ɏ��Ԃ��X�V���镔��)
    // �^�C�}�[�̎��Ԃ��ω��������ɌĂ΂��
    private void HandleTimeChanged(int sec)
    {
        int m = sec / 60, s = sec % 60;
        timerText.text = $"{m:00}:{s:00}";
    }

}
