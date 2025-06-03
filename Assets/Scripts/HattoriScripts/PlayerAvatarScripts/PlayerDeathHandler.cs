using UnityEngine;
using System.Collections;
using Fusion;
using TMPro;

public class PlayerDeathHandler : NetworkBehaviour
{
    [SerializeField] private PlayerAvatar playerAvatar;
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
        //PlayerAvatarの行動不能フラグを有効化する
        //Collider 無効化(レイヤー切り替え)
        foreach (var col in GetComponentsInChildren<Collider>())
            col.gameObject.layer = LayerMask.NameToLayer("DeadPlayer");
        //Hitbox 無効化(レイヤー切り替え)
        foreach (var hitbox in GetComponentsInChildren<PlayerHitbox>())
            hitbox.gameObject.layer = LayerMask.NameToLayer("DeadHitbox");
        //ネームタグ非表示
        WorldUICanvas.SetActive(false);
        //プレイヤーアバターに死亡アニメーションを設定
        playerAvatar.SetActionAnimationPlayList(ActionType.Dead, Runner.SimulationTime);
    }


}