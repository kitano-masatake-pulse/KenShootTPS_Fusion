using TMPro;
using UnityEngine;
using UnityEngine.UI;

// プレイヤーの 画面上のHUD を制御するクラス
public class HUDManager : MonoBehaviour
{
    // プレイヤーの HP や武器種を表示するUI
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text reserveAmmoText;
    [SerializeField] private TMP_Text magazineAmmoText;
    [SerializeField] private TMP_Text killScoreText;
    [SerializeField] private TMP_Text deathScoreText;
    [Header("WeaponType に対応するスプライト(順番を enum と合わせる)")]
    [Tooltip("0:Sword, 1:AssaultRifle, 2:SemiAutoRifle, 3:GrenadeLauncher")]
    [SerializeField] private Sprite[] weaponSprites = new Sprite[4];
    [SerializeField] private Image targetImage;

    // タイマー表示用UI
    [SerializeField] private TMP_Text timerText;

    // プレイヤーの状態を保持するクラス
    private PlayerNetworkState localState;

    //WeaponIconの大きさ
    // AssaultRifle, SemiAutoRifle の大きさ
    private const float RifleScale = 0.53f;
    private const float GrenadeLauncherScale = 0.53f;

    private void OnEnable()
    {
        // ローカルプレイヤー生成通知
        PlayerNetworkState.OnLocalPlayerSpawned += HandlePlayerSpawn;
        // タイマー通知
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
        //イベント登録
        InitializeSubscriptions();
        // 初期値反映
        // HPの初期値を反映
        OnLocalHPChanged(localState.HpNormalized);
        // 武器種の初期値を反映
        OnLocalWeaponChanged(localState.CurrentWeapon);
        // スコアの初期値を反映
        OnLocalScoreChanged(localState.KillScore, localState.DeathScore);
        // 弾薬の初期値を反映
        OnLocalAmmoChanged(localState.ReserveAmmo, localState.MagazineAmmo);
        // Weaponに登録したSpliteの長さチェック
        if (weaponSprites == null || weaponSprites.Length != System.Enum.GetValues(typeof(WeaponType)).Length)
        {
            Debug.LogError($"weaponSprites の要素数は {System.Enum.GetValues(typeof(WeaponType)).Length} にしてください");
        }
    }
  

    //初期化処理
    void InitializeSubscriptions()
    {
        // 先に解除してから
        localState.OnHPChanged -= OnLocalHPChanged;
        localState.OnWeaponChanged -= OnLocalWeaponChanged;
        localState.OnScoreChanged -= OnLocalScoreChanged;
        localState.OnAmmoChanged -= OnLocalAmmoChanged;
        // …

        // 改めて登録
        localState.OnHPChanged += OnLocalHPChanged;
        localState.OnWeaponChanged += OnLocalWeaponChanged;
        localState.OnScoreChanged += OnLocalScoreChanged;
        localState.OnAmmoChanged += OnLocalAmmoChanged;
        // …
    }

    //HPイベント
    //HPが変化した時だけ呼ばれてHPバーを更新する
    private void OnLocalHPChanged(float normalized)
    {
        hpSlider.value = normalized;
    }

    //WeaponTypeイベント
    // 武器種が変化した時だけ呼ばれて武器種表示を更新する
    void OnLocalWeaponChanged(WeaponType type)
    {
        var idx = (int)type;       
        if (idx < 0 || idx >= weaponSprites.Length || weaponSprites[idx] == null)
        {
            Debug.LogWarning("対応するスプライトがありません");
            return;
        }

        Sprite sp = weaponSprites[idx];
        targetImage.sprite = sp;

        // スケールは元のまま
        float scale = 1f;
        if (type == WeaponType.AssaultRifle || type == WeaponType.SemiAutoRifle)
            scale = RifleScale;
        else if (type == WeaponType.GrenadeLauncher)
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


    //タイマーイベント(厳密にはイベントではなく単に受け取ったままに時間を更新する部分)
    // タイマーの時間が変化した時に呼ばれる
    private void HandleTimeChanged(int sec)
    {
        int m = sec / 60, s = sec % 60;
        timerText.text = $"{m:00}:{s:00}";
    }

}
