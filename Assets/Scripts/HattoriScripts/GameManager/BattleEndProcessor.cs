using System.Collections;
using System;
using UnityEngine;
using Fusion;

public class BattleEndProcessor : NetworkBehaviour
{
    //シングルトン化
    public static BattleEndProcessor Instance { get; private set; }
    [Header("試合終了時、フェードアウトするまでの待機時間")]
    [SerializeField] private float waitingTime = 3f;
    [Header("次に遷移するシーン(デフォルトBattleScene)")]
    public SceneType nextScene = SceneType.Result;
    public event Action OnBattleEnd;


    private void Awake()
    {
        //シングルトンのインスタンスを設定
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }   
    private void OnEnable()
    {
        GameManager.OnManagerInitialized -= SubscribeEvent;
        GameManager.OnManagerInitialized += SubscribeEvent;
    }

    public override void Spawned()
    {
        
    }

    private void SubscribeEvent()
    {
        GameManager.Instance.OnTimeUp -= HandleBattleEnd;
        GameManager.Instance.OnTimeUp += HandleBattleEnd;

    }


    private void OnDisable()
    {
        GameManager.OnManagerInitialized -= SubscribeEvent;
        if (GameManager.Instance == null) return;
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
        OnBattleEnd?.Invoke();
        //試合終了時のプレイヤー処理
        //全プレイヤー(自分のプレイヤー)の行動を停止
        NetworkObject myPlayer = GameManager.Instance.GetMyPlayer();
        PlayerAvatar playerAvatar = myPlayer.GetComponent<PlayerAvatar>();
        playerAvatar.IsDuringWeaponAction = true;
        playerAvatar.IsImmobilized = true;
        StartCoroutine(watingCoroutine(waitingTime));
    }

    //コルーチン
    private IEnumerator watingCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        if(Runner.IsServer)
        {
            SceneTransitionManager.Instance.ChangeScene(nextScene);
        }
    }

}
