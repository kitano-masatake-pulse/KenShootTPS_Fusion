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
        if(GameManager2.Instance != null)
        {
            GameManager2.Instance.OnAnyPlayerDied += HandleDeath;
            Debug.Log("PlayerDeathHandler: Registered to GameManager2.OnAnyPlayerDied");
        }
        
    }

    void OnDisable()
    {
        if (GameManager2.Instance != null)
        {
            GameManager2.Instance.OnAnyPlayerDied -= HandleDeath;
            Debug.Log("PlayerDeathHandler: Unregistered from GameManager2.OnAnyPlayerDied");
        }
    }

    public void HandleDeath( float hostTimeStamp, PlayerRef victim, PlayerRef killer)
    {
        Debug.Log($"PlayerDeathHandlerBefore:victim:{victim}, killer: {killer}, hostTimeStamp: {hostTimeStamp},InputAuthority:{Object.InputAuthority}");
        //このプレイヤーでない場合は何もしない
        if (victim != Object.InputAuthority)
        {
            return;
        }
        Debug.Log($"PlayerDeathHandler: Player{victim.PlayerId} has died at {hostTimeStamp} by {killer.PlayerId}");
        //プレイヤーアバターに死亡アニメーションを設定

        playerAvatar.SetActionAnimationPlayList(ActionType.Dead,hostTimeStamp);
        //行動不能化
       
        playerAvatar.CurrentWeaponActionState= WeaponActionState.Stun;
        playerAvatar.SetReturnTimeToIdle(0f);
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