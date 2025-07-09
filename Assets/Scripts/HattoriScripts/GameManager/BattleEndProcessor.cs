using System.Collections;
using System;
using UnityEngine;
using Fusion;

public class BattleEndProcessor : NetworkBehaviour
{
    //シングルトン化
    public static BattleEndProcessor Instance { get; private set; }
    [SerializeField] private SceneChangeFade fadeUI;
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
        GameManager2.OnManagerInitialized -= SubscribeEvent;
        GameManager2.OnManagerInitialized += SubscribeEvent;
    }

    public override void Spawned()
    {
        //fadeUI = FindObjectOfType<SceneChangeFade>();
        //if (fadeUI == null)
        //{
        //    Debug.LogError("FadeUI component not found in the scene. Please ensure it is present.");
        //    return;
        //}
        
    }

    private void SubscribeEvent()
    {
        GameManager2.Instance.OnTimeUp -= HandleBattleEnd;
        GameManager2.Instance.OnTimeUp += HandleBattleEnd;

    }


    private void OnDisable()
    {
        GameManager2.OnManagerInitialized -= SubscribeEvent;
        if (GameManager2.Instance == null) return;
        GameManager2.Instance.OnTimeUp -= HandleBattleEnd;
    }

    private void HandleBattleEnd()
    {
        //ホスト環境でのみ実行
        if (!Runner.IsServer) return;
        //GameManagerのスコア辞書を取得
        var scoreList = GameManager2.Instance.GetSortedUserData();

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
        NetworkObject myPlayer = GameManager2.Instance.GetMyPlayer();
        PlayerAvatar playerAvatar = myPlayer.GetComponent<PlayerAvatar>();
        playerAvatar.IsDuringWeaponAction = true;
        playerAvatar.IsImmobilized = true;
        StartCoroutine(FadeSceneChange(3f));
    }

    //コルーチン
    private IEnumerator FadeSceneChange(float duration)
    {
        yield return fadeUI.FadeAlpha(0f, 1f, duration);

        //暗転中の処理
        yield return Resources.UnloadUnusedAssets();        
        GC.Collect();
        GC.WaitForPendingFinalizers();

        //リザルト画面へ遷移
        if (Runner.IsServer)
        {
            string sceneName = nextScene.ToSceneName();
            //シーン遷移
            yield return Runner.SetActiveScene(sceneName);
        }
        
    }

}
