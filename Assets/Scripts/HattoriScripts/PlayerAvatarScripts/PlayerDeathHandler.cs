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
        if(GameManager.Instance != null)
        {
            GameManager.Instance.OnMyPlayerDied += HandleDeath;
        }
        
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMyPlayerDied -= HandleDeath;
        }
    }

    private void HandleDeath(PlayerRef killer, float hostTimeStamp)
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
        playerAvatar.SetActionAnimationPlayList(ActionType.Dead, hostTimeStamp);

    }

}