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


    private PlayerNetworkState pNState;

    public override void Spawned()
    {
        //ネットワーク状態コンポーネント取得
        pNState = GetComponentInParent<PlayerNetworkState>();
        if (pNState == null)
        {
            Debug.LogError("PlayerNetworkState not found!");
        }

        //プレイヤー名をセット
        nameLabel.text = $"Player({Object.InputAuthority.PlayerId})";

        // 自分のキャラなら非表示、それ以外は表示
        bool isLocal = HasInputAuthority;
        nameLabel.gameObject.SetActive(!isLocal);
        hpBar.gameObject.SetActive(!isLocal);

        //チームカラーを適用
        nameLabel.color = pNState.Team.GetColor(); 
        // イベント登録
        InitializeSubscriptions();
        // 初期値も反映
        hpBar.value = pNState.HpNormalized;
    }


    void InitializeSubscriptions()
    {
        // 先に解除してから
        pNState.OnHPChanged -= UpdateWorldHPBar;
        pNState.OnTeamChanged -= UpdateNemeColor;

        // 改めて登録
        pNState.OnHPChanged += UpdateWorldHPBar;
        pNState.OnTeamChanged += UpdateNemeColor;
    }


    //HPが変化した時だけ呼ばれてHPバーを更新する
    public void UpdateWorldHPBar(float normalized, PlayerRef _)
    {
        hpBar.value = normalized;
    }


    private void UpdateNemeColor(TeamType team)
    {
        nameLabel.color = team.GetColor();
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (pNState != null)
        {
            pNState.OnHPChanged -= UpdateWorldHPBar;

        }
    }
}
