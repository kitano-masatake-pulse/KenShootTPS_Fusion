using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class PlayerHUDController : NetworkBehaviour
{
    [Header("World-Space Canvas 上の UI 要素")]
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private Slider hpBar;

    private PlayerNetworkState state;

    public override void Spawned()
    {
        // 1) ネットワーク状態コンポーネント取得
        state = GetComponent<PlayerNetworkState>();
        if (state == null)
        {
            Debug.LogError("PlayerNetworkState not found!");
        }

        // 2) 名前セット
        nameLabel.text = $"Player({Object.InputAuthority.PlayerId})";

        // 3) 自分のキャラなら最初から非表示に
        bool isLocal = HasInputAuthority;
        //デバッグ用にコメントアウト中
        //nameLabel.gameObject.SetActive(!isLocal);
        //hpBar.gameObject.SetActive(!isLocal);
    }

    //HPが変化した時だけ呼ばれてHPバーを更新する
    public void UpdateHPBar(float v)
    {

        {
            hpBar.value = v;
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        // クリーンアップ（あれば）
        state = null;
    }
}
