using TMPro;
using Fusion;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// �v���C���[�� ��ʏ��HUD �𐧌䂷��N���X
public class HUDManagerOld : MonoBehaviour
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
    [SerializeField] private TMP_Text respawnKillerText; 
    [SerializeField] private Button respawnButton;
    [SerializeField] private float fadeDuration = 1f;     // �t�F�[�h�ɂ�����b��
    [SerializeField] private float respawnDelay = 5f;
    private float _remainingRespawnTime;

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
        else if (currentWeapon == WeaponType.Grenade)
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

    //�v���C�����S�C�x���g
    // �v���C���[�����S�������Ɏ��S��UI��\������
    private void OnLocalPlayerDied(PlayerRef victim, PlayerRef killer)
    {
        
        //respawnCountdown.text = $"Respawn in {_remainingRespawnTime:F0}";
        respawnButton.gameObject.SetActive(false);
        respawnUI.alpha = 1;
        StartCoroutine(RespawnCountdown(killer));
    }

    private IEnumerator RespawnCountdown(PlayerRef killer)
    {
       
        respawnUI.blocksRaycasts = true;
        respawnUI.alpha = 0; // �����͔�\��
        respawnKillerText.text = $"You were killed by Player{killer.PlayerId}";

        _remainingRespawnTime = respawnDelay;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            respawnUI.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);

            yield return null;
        }
        respawnUI.alpha = 1f; // �t�F�[�h�������Ɋm���ɕ\��

        _remainingRespawnTime = respawnDelay;
        while (_remainingRespawnTime > 0f)
        {
            // ���X�|�[���܂ł̃J�E���g�_�E��
            respawnCountdown.text = $"Respawn in {_remainingRespawnTime:F0}";
            // 1�b�ҋ@
            yield return new WaitForSeconds(1f);
            _remainingRespawnTime -= 1f;
        }

        // �����Ń��X�|�[���{�^����\���A�����ƃ��X�|�[���������z�X�g�ɗv��
        respawnCountdown.text = "";
        respawnButton.gameObject.SetActive(true);
    }
    // ���X�|�[���{�^���������ꂽ���̏���
    public void OnRespawnButtonClicked()
    {
        // ���X�|�[���v�����z�X�g�ɑ���
       
        StopAllCoroutines();
        respawnUI.alpha = 0;
        respawnUI.blocksRaycasts = false;
    }

    //�^�C�}�[�C�x���g(�����ɂ̓C�x���g�ł͂Ȃ��P�Ɏ󂯎�����܂܂Ɏ��Ԃ��X�V���镔��)
    // �^�C�}�[�̎��Ԃ��ω��������ɌĂ΂��
    private void HandleTimeChanged(int sec)
    {
        int m = sec / 60, s = sec % 60;
        timerText.text = $"{m:00}:{s:00}";
    }

}
