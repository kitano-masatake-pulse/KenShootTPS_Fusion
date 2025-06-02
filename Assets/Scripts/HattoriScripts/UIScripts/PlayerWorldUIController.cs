using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

//プレイヤの頭上に表示されるUIを制御するクラス
public class PlayerWorldUIController : NetworkBehaviour
{
    [Header("World-Space Canvas 上の UI 要素")]
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private Slider hpBar;
    [Header("WeaponType に対応するスプライト(順番を enum と合わせる)")]
    [Tooltip("0:Sword, 1:AssaultRifle, 2:SemiAutoRifle, 3:GrenadeLauncher")]
    [SerializeField] private Sprite[] weaponSprites = new Sprite[4];
    [SerializeField] private Image weaponImage;


    private PlayerNetworkState pNState;

    public override void Spawned()
    {
        //ネットワーク状態コンポーネント取得
        pNState = GetComponent<PlayerNetworkState>();
        if (pNState == null)
        {
            Debug.LogError("PlayerNetworkState not found!");
        }

        //プレイヤー名をセット
        nameLabel.text = $"Player({Object.InputAuthority.PlayerId})";

        // 自分のキャラなら非表示、それ以外は表示
        bool isLocal = HasInputAuthority;
        nameLabel.gameObject.SetActive(!isLocal);
        hpBar.gameObject.SetActive(true);
        //weaponImage.gameObject.SetActive(!isLocal);
        // イベント登録
        InitializeSubscriptions();
        // 初期値も反映
        hpBar.value = pNState.HpNormalized;
    }


    void InitializeSubscriptions()
    {
        // 先に解除してから
        pNState.OnHPChanged -= UpdateWorldHPBar;
        pNState.OnWeaponChanged_Network -= UpdateWorldWeapon;

        // 改めて登録
        pNState.OnHPChanged += UpdateWorldHPBar;
        pNState.OnWeaponChanged_Network += UpdateWorldWeapon;
    }


    //HPが変化した時だけ呼ばれてHPバーを更新する
    public void UpdateWorldHPBar(float normalized)
    {
        hpBar.value = normalized;
    }
    void UpdateWorldWeapon(WeaponType type)
    {
        var idx = (int)type;
        if (idx < 0 || idx >= weaponSprites.Length || weaponSprites[idx] == null)
        {
            Debug.LogWarning("対応するスプライトがありません");
            return;
        }

        Sprite sp = weaponSprites[idx];
        weaponImage.sprite = sp;

        // スケールは元のまま
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
