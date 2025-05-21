using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

//プレイヤの頭上に表示されるUIを制御するクラス
public class PlayerHUDController : NetworkBehaviour
{
    [Header("World-Space Canvas 上の UI 要素")]
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private Slider hpBar;

    private PlayerNetworkState state;

    public override void Spawned()
    {
        //ネットワーク状態コンポーネント取得
        state = GetComponent<PlayerNetworkState>();
        if (state == null)
        {
            Debug.LogError("PlayerNetworkState not found!");
        }

        //プレイヤー名をセット
        nameLabel.text = $"Player({Object.InputAuthority.PlayerId})";

        // 自分のキャラなら非表示、それ以外は表示
        bool isLocal = HasInputAuthority;
        nameLabel.gameObject.SetActive(!isLocal);
        hpBar.gameObject.SetActive(!isLocal);
        // イベント登録
        InitializeSubscriptions();
        // 初期値も反映
        hpBar.value = state.HpNormalized;
    }


    void InitializeSubscriptions()
    {
        // 先に解除してから
        state.OnHPChanged -= UpdateHPBar;

        // 改めて登録
        state.OnHPChanged += UpdateHPBar;
    }


    //HPが変化した時だけ呼ばれてHPバーを更新する
    public void UpdateHPBar(float normalized)
    {
        hpBar.value = normalized;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (state != null)
            state.OnHPChanged -= UpdateHPBar;
    }
}
