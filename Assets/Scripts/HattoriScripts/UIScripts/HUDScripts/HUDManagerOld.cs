using TMPro;
using Fusion;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// プレイヤーの 画面上のHUD を制御するクラス
public class HUDManagerOld : MonoBehaviour
{
    // プレイヤーの HP や武器種を表示するUI
    [Header("HP用UI")]
    [SerializeField] private Slider hpSlider;

    [Header("武器表示用UI")]
    [SerializeField] private TMP_Text reserveAmmoText;
    [SerializeField] private TMP_Text magazineAmmoText;
    [Header("WeaponType に対応するスプライト(順番を enum と合わせる)")]
    [Tooltip("0:Sword, 1:AssaultRifle, 2:SemiAutoRifle, 3:GrenadeLauncher")]
    [SerializeField] private Sprite[] weaponSprites = new Sprite[4];
    [SerializeField] private Image targetImage;

    [Header("スコア表示用UI")]
    [SerializeField] private TMP_Text killScoreText;
    [SerializeField] private TMP_Text deathScoreText;

    [Header("リスポーンUI")]
    [SerializeField] private CanvasGroup respawnUI;
    [SerializeField] private TMP_Text respawnCountdown;
    [SerializeField] private TMP_Text respawnKillerText; 
    [SerializeField] private Button respawnButton;
    [SerializeField] private float fadeDuration = 1f;     // フェードにかける秒数
    [SerializeField] private float respawnDelay = 5f;
    private float _remainingRespawnTime;

    [Header("タイマー表示用UI")]
    [SerializeField] private TMP_Text timerText;

    // プレイヤーの状態を保持するクラス
    private PlayerNetworkState pNState;
    //武器の内容を保持するクラス
    private WeaponLocalState wLState;

    //WeaponIconの大きさ
    // AssaultRifle, SemiAutoRifle の大きさ
    private const float RifleScale = 0.53f;
    private const float GrenadeLauncherScale = 0.53f;

    private void OnEnable()
    {
        // ローカルプレイヤー生成通知
        PlayerNetworkState.OnLocalPlayerSpawned += HandlePlayerSpawn;
        // 武器生成通知
        WeaponLocalState.OnWeaponSpawned += HandleWeaponSpawn;
        // タイマー通知
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
        //イベント登録
        // 先に解除してから
        pNState.OnHPChanged -= OnLocalHPChanged;
        pNState.OnScoreChanged -= OnLocalScoreChanged;
        pNState.OnPlayerDied -= OnLocalPlayerDied;
        pNState.OnHPChanged += OnLocalHPChanged;
        pNState.OnScoreChanged += OnLocalScoreChanged;
        pNState.OnPlayerDied += OnLocalPlayerDied;
        // 初期値反映
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
        // 初期表示
        OnLocalWeaponChanged(wLState.CurrentWeapon);
        var ammo = wLState.GetCurrentAmmo();
        OnLocalAmmoChanged(ammo.magazine, ammo.reserve);
        // Weaponに登録したSpliteの長さチェック
        if (weaponSprites == null || weaponSprites.Length != System.Enum.GetValues(typeof(WeaponType)).Length)
        {
            Debug.LogError($"weaponSprites の要素数は {System.Enum.GetValues(typeof(WeaponType)).Length} にしてください");
        }
    }

    //HPイベント
    //HPが変化した時だけ呼ばれてHPバーを更新する
    private void OnLocalHPChanged(float normalized)
    {
        hpSlider.value = normalized;
    }

    //WeaponTypeイベント
    // 武器種が変化した時だけ呼ばれて武器種表示を更新する
    void OnLocalWeaponChanged(WeaponType currentWeapon)
    {
        var idx = (int)currentWeapon;       
        if (idx < 0 || idx >= weaponSprites.Length || weaponSprites[idx] == null)
        {
            Debug.LogWarning("対応するスプライトがありません");
            return;
        }

        Sprite sp = weaponSprites[idx];
        targetImage.sprite = sp;

        // スケールは元のまま
        float scale = 1f;
        if (currentWeapon == WeaponType.AssaultRifle || currentWeapon == WeaponType.SemiAutoRifle)
            scale = RifleScale;
        else if (currentWeapon == WeaponType.Grenade)
            scale = GrenadeLauncherScale;

        var rt = targetImage.rectTransform;
        rt.sizeDelta = sp.rect.size * scale;

    }

    // Scoreイベント
    // スコアが変化した時だけ呼ばれてスコア表示を更新する
    private void OnLocalScoreChanged(int killScore, int deathScore)
    {
        killScoreText.text = killScore.ToString();
        deathScoreText.text = deathScore.ToString();
    }

    // Ammoイベント
    // 弾薬が変化した時だけ呼ばれて弾薬表示を更新する
    private void OnLocalAmmoChanged(int magazineAmmo, int reserveAmmo )
    {
        reserveAmmoText.text = reserveAmmo.ToString();
        magazineAmmoText.text = magazineAmmo.ToString();
    }

    //プレイヤ死亡イベント
    // プレイヤーが死亡した時に死亡時UIを表示する
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
        respawnUI.alpha = 0; // 初期は非表示
        respawnKillerText.text = $"You were killed by Player{killer.PlayerId}";

        _remainingRespawnTime = respawnDelay;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            respawnUI.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);

            yield return null;
        }
        respawnUI.alpha = 1f; // フェード完了時に確実に表示

        _remainingRespawnTime = respawnDelay;
        while (_remainingRespawnTime > 0f)
        {
            // リスポーンまでのカウントダウン
            respawnCountdown.text = $"Respawn in {_remainingRespawnTime:F0}";
            // 1秒待機
            yield return new WaitForSeconds(1f);
            _remainingRespawnTime -= 1f;
        }

        // ここでリスポーンボタンを表示、押すとリスポーン処理をホストに要求
        respawnCountdown.text = "";
        respawnButton.gameObject.SetActive(true);
    }
    // リスポーンボタンが押された時の処理
    public void OnRespawnButtonClicked()
    {
        // リスポーン要求をホストに送る
       
        StopAllCoroutines();
        respawnUI.alpha = 0;
        respawnUI.blocksRaycasts = false;
    }

    //タイマーイベント(厳密にはイベントではなく単に受け取ったままに時間を更新する部分)
    // タイマーの時間が変化した時に呼ばれる
    private void HandleTimeChanged(int sec)
    {
        int m = sec / 60, s = sec % 60;
        timerText.text = $"{m:00}:{s:00}";
    }

}
