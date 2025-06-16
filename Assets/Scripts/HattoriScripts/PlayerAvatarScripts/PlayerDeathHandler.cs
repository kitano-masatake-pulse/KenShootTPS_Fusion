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
            GameManager.Instance.OnAnyPlayerDied += HandleDeath;
        }
        
    }

    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnAnyPlayerDied -= HandleDeath;
        }
    }

    private void HandleDeath(PlayerRef victim, PlayerRef killer, float hostTimeStamp)
    {
        //このプレイヤーでない場合は何もしない
        if (victim != Object.InputAuthority)
        {
            return;
        }
        Debug.Log($"PlayerDeathHandler: Player{victim.PlayerId} has died at {hostTimeStamp} by {killer.PlayerId}");
        //プレイヤーアバターに死亡アニメーションを設定
        playerAvatar.SetActionAnimationPlayList(ActionType.Dead, hostTimeStamp);
        //行動不能化
        playerAvatar.IsDuringWeaponAction = true;
        playerAvatar.IsImmobilized = true;
        //顔のカメラ追従を切る
        playerAvatar.IsFollowingCameraForward = false;
        //Collider 無効化(レイヤー切り替え)
        foreach (var col in GetComponentsInChildren<Collider>())
            col.gameObject.layer = LayerMask.NameToLayer("DeadPlayer");
        //Hitbox 無効化(レイヤー切り替え)
        foreach (var hitbox in GetComponentsInChildren<PlayerHitbox>())
            hitbox.gameObject.layer = LayerMask.NameToLayer("DeadHitbox");
        //ネームタグ非表示
        WorldUICanvas.SetActive(false);
        

    }

}