using UnityEngine;
using System.Collections;
using Fusion;
using TMPro;

public class PlayerDeathHandler : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Collider[] colliders;
    [SerializeField] private GameObject[] weaponModels;
    [SerializeField] private GameObject playerHUDCanvas;
    [SerializeField] private CanvasGroup respawnUI;
    [SerializeField] private TMP_Text respawnCountdown;
    [SerializeField] private float respawnDelay = 5f;

    private bool _isDead;

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

        _isDead = true;
        // 1. 入力無効（PlayerAvatar 側にもフラグを送るか共有）
        GetComponent<PlayerAvatar>().enabled = false;

        // 2. Collider 無効化(レイヤー切り替え)
        foreach (var col in GetComponentsInChildren<Collider>())
            col.gameObject.layer = LayerMask.NameToLayer("DeadLayer");

        // 3. 死亡アニメ再生
        animator.SetTrigger("Die");

        // 4. 武器モデル／ネームタグ非表示
        foreach (var w in weaponModels) w.SetActive(false);
        playerHUDCanvas.SetActive(false);


        //死亡プレイヤーなら実行
        if (victim == Object.InputAuthority)
        {
            // 5. リスポーンUI 表示
            StartCoroutine(RespawnCountdown());

        }
        //殺害プレイヤーなら実行
        else if (killer == Object.InputAuthority)
        {
            // 6. キル演出
            // ここにキル演出のコードを追加（例：エフェクト、サウンドなど）

        }



    }

    private IEnumerator RespawnCountdown()
    {
        
        respawnUI.alpha = 1;
        float t = respawnDelay;
        while (t > 0)
        {
            respawnCountdown.text = Mathf.CeilToInt(t).ToString();
            yield return new WaitForSeconds(1f);
            t -= 1f;
        }
        respawnUI.alpha = 0;
        // ここでリスポーン処理をトリガー（RPC or イベントでホストに依頼）
    }
}