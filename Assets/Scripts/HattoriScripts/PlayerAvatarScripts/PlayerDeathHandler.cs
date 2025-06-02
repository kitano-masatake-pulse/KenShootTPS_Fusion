using UnityEngine;
using System.Collections;
using Fusion;
using TMPro;

public class PlayerDeathHandler : NetworkBehaviour
{
    [SerializeField] private GameObject WorldUICanvas;

    void OnEnable()
    {
        // PlayerNetworkState の死亡通知を購読
        var state = GetComponent<PlayerNetworkState>();
        state.OnPlayerDied += HandleDeath;
    }

    void OnDisable()
    {
        GetComponent<PlayerNetworkState>().OnPlayerDied -= HandleDeath;
    }

    private void HandleDeath(PlayerRef victim, PlayerRef killer)
    {
        //入力無効（PlayerAvatar 側にもフラグを送るか共有）
        GetComponent<PlayerAvatar>().enabled = false;

        //Collider 無効化(レイヤー切り替え)
        foreach (var col in GetComponentsInChildren<Collider>())
            col.gameObject.layer = LayerMask.NameToLayer("DeadPlayer");
        //Hitbox 無効化(レイヤー切り替え)
        foreach (var hitbox in GetComponentsInChildren<PlayerHitbox>())
            hitbox.gameObject.layer = LayerMask.NameToLayer("DeadHitbox");

        //武器モデル／ネームタグ非表示
        WorldUICanvas.SetActive(false);

        //他にすること
        //死亡時アニメーション

        //死亡プレイヤーなら実行
        if (victim == Object.InputAuthority)
        {
           
        }
        //殺害プレイヤーなら実行
        else if (killer == Object.InputAuthority)
        {
            // 6. キル演出
            // ここにキル演出のコードを追加（例：エフェクト、サウンドなど）

        }

    }

   
}