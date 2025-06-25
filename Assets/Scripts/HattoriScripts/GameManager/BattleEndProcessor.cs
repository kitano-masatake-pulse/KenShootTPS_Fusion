using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class BattleEndProcessor : NetworkBehaviour
{
    [SerializeField] private FadeUI fadeUI;
    [Header("次に遷移するシーン(デフォルトBattleScene)")]
    public SceneType nextScene = SceneType.Result;
    public event Action OnGameEnd;

    private void OnEnable()
    {
        GameManager.Instance.OnTimeUp -= HandleBattleEnd;
        GameManager.Instance.OnTimeUp += HandleBattleEnd;
    }
    private void OnDisable()
    {
        GameManager.Instance.OnTimeUp -= HandleBattleEnd;
    }

    private void HandleBattleEnd()
    {
        //ホスト環境でのみ実行
        if (!Runner.IsServer) return;
        //GameManagerのスコア辞書を取得
        var scoreList = GameManager.Instance.GetSortedScores();

        //データ移譲用オブジェクトを生成
        var scoreTransferObj = new GameObject("ScoreTransferObject");
        var scoreTransfer = scoreTransferObj.AddComponent<ScoreTransfer>();
        scoreTransfer.SetScores(scoreList);
        DontDestroyOnLoad(scoreTransferObj);

        RPC_EndBattle();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_EndBattle()
    {
        //試合終了イベントを発火
        OnGameEnd?.Invoke();
        //試合終了時のプレイヤー処理
        //全プレイヤー(自分のプレイヤー)の行動を停止
        NetworkObject myPlayer = GameManager.Instance.GetMyPlayer();
        PlayerAvatar playerAvatar = myPlayer.GetComponent<PlayerAvatar>();
        playerAvatar.IsDuringWeaponAction = true;
        playerAvatar.IsImmobilized = true;
        StartCoroutine(FadeToBlack(3f));
    }

    //コルーチン
    private IEnumerator FadeToBlack(float duration)
    {
        yield return fadeUI.FadeAlpha(0f, 1f, duration);
        //リザルト画面へ遷移
        if (Runner.IsServer)
        {
            string sceneName = nextScene.ToSceneName();
            //シーン遷移
            Runner.SetActiveScene(sceneName);
        }
        
    }

}
