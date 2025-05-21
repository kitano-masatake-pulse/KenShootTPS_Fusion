using TMPro;
using UnityEngine;
using UnityEngine.UI;

// プレイヤーの 画面上のHUD を制御するクラス
public class HUDManager : MonoBehaviour
{
    // プレイヤーの HP や武器種を表示するUI
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text totalAmmoText;
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
    private const float RifleScale = 0.625f;
    private const float GrenadeLauncherScale = 0.531f;

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
        OnLocalAmmoChanged(localState.TotalAmmo, localState.MagazineAmmo);
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
        float scale = 1f;

        

    Sprite sp = weaponSprites[(int)type];
        targetImage.sprite = sp;

        if (weaponSprites == null || (int)type >= weaponSprites.Length || weaponSprites[(int)type] == null)
        {
            Debug.LogWarning("対応するスプライトがありません");
            return;
        }

        if (type == WeaponType.AssaultRifle || type == WeaponType.SemiAutoRifle)
        {
            scale = RifleScale; // ライフル系はサイズを小さくする
        }
        else if (type == WeaponType.GrenadeLauncher)
        {
            scale = GrenadeLauncherScale; // GrenadeLauncher のみよりサイズを小さくする   
        }
        // RectTransform 取得
        RectTransform rt = targetImage.rectTransform;

        // スプライトの元ピクセルサイズ × scale 倍 を sizeDelta にセット
        Vector2 native = sp.rect.size;       
        rt.sizeDelta = native * scale;        

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
    private void OnLocalAmmoChanged(int totalAmmo, int magazineAmmo)
    {
        totalAmmoText.text = totalAmmo.ToString();
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
