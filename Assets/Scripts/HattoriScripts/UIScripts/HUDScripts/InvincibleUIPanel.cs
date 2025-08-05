using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InvincibleUIPanel : MonoBehaviour,IHUDPanel
{
    [SerializeField]
    private Image invincibleIcon; // 無敵状態を示すアイコン
    private PlayerNetworkState playerState;
    private Coroutine invincibleCoroutine;

    public void Initialize(PlayerNetworkState pState, PlayerAvatar _)
    {
        playerState = pState;
        // イベント登録
        playerState.OnInvincibleChanged -= UpdateInvincibleStatus;
        playerState.OnInvincibleChanged += UpdateInvincibleStatus;
        // 初期値設定
        UpdateInvincibleStatus(playerState.IsInvincible, 0f);
    }

    public void Cleanup()
    {
        playerState.OnInvincibleChanged -= UpdateInvincibleStatus;
    }

    // Update is called once per frame
    private void UpdateInvincibleStatus(bool isInvincible, float remainTime)
    {
        // Invincibleの状態に応じてUIを更新
        if (isInvincible)
        {
            // Invincible状態のUI更新処理
            Debug.Log("Player is invincible.");
            invincibleIcon.gameObject.SetActive(true);
            if (invincibleCoroutine != null)
            {
                StopCoroutine(invincibleCoroutine);
            }
            invincibleCoroutine = StartCoroutine(InvincibleCountdown(remainTime));
        }
        else
        {
            invincibleIcon.gameObject.SetActive(false);
            if (invincibleCoroutine != null)
            {
                StopCoroutine(invincibleCoroutine);
                invincibleCoroutine = null;
            }
        }
    }

    private IEnumerator InvincibleCountdown(float remainTime)
    {
        // 無敵時間のカウントダウン処理
        while (remainTime > 0f)
        {
            // UIの更新処理（例: 残り時間を表示するなど）
            Debug.Log($"Invincible time remaining: {remainTime}");
            yield return new WaitForSeconds(1f);
            remainTime -= 1f; // 1秒ずつ減少
        }
        // 無敵時間が終了したらUIを非表示にする
        invincibleIcon.gameObject.SetActive(false);
    }
}
