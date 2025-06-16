using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//死亡したプレイヤーのリスポーン処理を行うクラス
//アニメーションプレイリストのクリア、弾薬の初期化、リスポーンUIのリセット、フェード処理などを行う

public class LocalRespawnHandler: MonoBehaviour
{
    [SerializeField] private Image fadePanel;
    [Header("リスポーン時のフェード時間設定")]
    [SerializeField] private float respawnFadeOutTime = 3f;
    [SerializeField] private float respawnFadeInTime = 1f;
    //硬直時間
    [SerializeField] private float respawnStunDuration = 3f;
    [SerializeField] private RespawnPanel respawnPanel;

    private Coroutine currentFade;
    private NetworkObject myPlayer;
    private PlayerAvatar playerAvatar;
    public event Action<float> OnRespawnStuned;


    public void RespawnStart()
    {
        // プレイヤー情報取得
        myPlayer = GameManager.Instance.GetMyPlayer();
        playerAvatar = myPlayer.GetComponent<PlayerAvatar>();

        // 流れを一つのコルーチンで実行
        StartCoroutine(RespawnCoroutine());
    }
    private IEnumerator RespawnCoroutine()
    {
        // 1. リスポーン前処理
        InitializeBeforeFade();

        // 2. フェードアウト
        yield return StartCoroutine(FadeAlpha(0f, 1f, respawnFadeOutTime));

        // 3. フェードアウト後処理
        Debug.Log("LocalRespawnHandler: リスポーン処理を開始します。");
        InitializeAfterFade();

        // 4. フェードイン
        yield return StartCoroutine(FadeAlpha(1f, 0f, respawnFadeInTime));

        // 5. フェードイン後処理
        OnRespawnStuned?.Invoke(respawnStunDuration);

        // 6. スタン時間待機
        yield return new WaitForSeconds(respawnStunDuration);

        // 7. リスポーン後の処理
        AfterRespawn();
    }

    private void InitializeBeforeFade()
    {
        //Animationプレイリストをクリア
        //(リスポーン猶予後に呼ばれるから後から入ってこないと判断)
        playerAvatar.ClearActionAnimationPlayList();
    }

    private void InitializeAfterFade()
    {
        respawnPanel.ResetUI(); // UIをリセット
        playerAvatar.InitializeAllAmmo();// 弾薬初期化

        playerAvatar.IsHoming = false;
        playerAvatar.IsFollowingCameraForward = true; //カメラ追従を有効化
        playerAvatar.TeleportToInitialSpawnPoint(GetRespawnPoint());

        //ホストにリスポーン処理を要求
        RespawnManager.Instance.RPC_RequestRespawn(myPlayer);
    }

    private void AfterRespawn()
    {
        //行動制限を解除
        playerAvatar.IsDuringWeaponAction = false;
        playerAvatar.IsImmobilized = false;

        //ホストにリスポーン後の処理を要求
        RespawnManager.Instance.RPC_RespawnEnd(myPlayer);
    }



    // from→to のアルファ値を duration 秒かけて補間するコルーチン
    private IEnumerator FadeAlpha(float from, float to, float duration)
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeRoutine(from, to, duration));
        yield return currentFade;
    }

    // 実際のフェード処理
    private IEnumerator FadeRoutine(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;
        Color panelColor = fadePanel.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            panelColor.a = Mathf.Lerp(fromAlpha, toAlpha, t);
            fadePanel.color = panelColor;
            yield return null;
        }
        panelColor.a = toAlpha;
        fadePanel.color = panelColor;
        currentFade = null;
    }

    public Vector3 GetRespawnPoint()
    {
        var randomValue = UnityEngine.Random.insideUnitCircle * 5f;
        return new Vector3(randomValue.x, 5f, randomValue.y);
    }

}
