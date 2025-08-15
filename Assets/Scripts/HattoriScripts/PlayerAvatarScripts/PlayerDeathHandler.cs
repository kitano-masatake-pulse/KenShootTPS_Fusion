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

        if (victim != Object.InputAuthority)
        {
            return;
        }
        Debug.Log($"PlayerDeathHandler: Player{victim.PlayerId} has died at {hostTimeStamp} by {killer.PlayerId}");
        //繝励Ξ繧､繝､繝ｼ繧｢繝舌ち繝ｼ縺ｫ豁ｻ莠｡繧｢繝九Γ繝ｼ繧ｷ繝ｧ繝ｳ繧定ｨｭ螳

        playerAvatar.SetActionAnimationPlayList(ActionType.Dead,hostTimeStamp);
        //陦悟虚荳崎�蛹
       
        playerAvatar.IsDead = true;
        playerAvatar.CurrentWeaponActionState= WeaponActionState.Stun;
        playerAvatar.TimeElapsedUntilHomingFinish = 0f;  
        playerAvatar.SetReturnTimeToIdle(0f);
        playerAvatar.IsImmobilized = true;
        //鬘斐�繧ｫ繝｡繝ｩ霑ｽ蠕薙ｒ蛻�ｋ
        playerAvatar.IsFollowingCameraForward = false;
        //Collider 辟｡蜉ｹ蛹(繝ｬ繧､繝､繝ｼ蛻�ｊ譖ｿ縺)
        foreach (var col in GetComponentsInChildren<Collider>())
            col.gameObject.layer = LayerMask.NameToLayer("DeadPlayer");
        //Hitbox 辟｡蜉ｹ蛹(繝ｬ繧､繝､繝ｼ蛻�ｊ譖ｿ縺)
        foreach (var hitbox in GetComponentsInChildren<PlayerHitbox>())
            hitbox.gameObject.layer = LayerMask.NameToLayer("DeadHitbox");
        //繝阪�繝繧ｿ繧ｰ髱櫁｡ｨ遉ｺ
        WorldUICanvas.SetActive(false);
        

    }

}