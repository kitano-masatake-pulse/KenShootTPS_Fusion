using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;



//ホスト環境でキルデスを検知→ここでスコアとして管理
//Networkedプロパティを使って、スコアの変更をネットワーク上で同期する
public class GameManager : NetworkBehaviour
{
    // シングルトンインスタンス
    public static GameManager Instance { get; private set; }


    //プレイヤーのスコアを管理する辞書
    //キー: PlayerRef, 値: PlayerScore
    [Networked(OnChanged = nameof(ManagerScoreCallback))]
    [Capacity(8)]
    private NetworkDictionary<PlayerRef, PlayerScore> PlayerScores { get; }
    //スコア変更イベント
    public event Action<IEnumerable<KeyValuePair<PlayerRef, PlayerScore>>> OnManagerScoreChanged;


    //===========================================
    //初期化処理
    //===========================================
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void Spawned()
    {
        if(Object.HasStateAuthority)
        {
            Debug.Log($"ScoreManager Spawned. Current Player Count: {Runner.ActivePlayers.Count()}");

            foreach (var playerRef in Runner.ActivePlayers)
            {
                InitializePlayerScore(playerRef);
            }
        }
    }
    private void InitializePlayerScore(PlayerRef playerRef)
    {
        PlayerScore playerScore = new PlayerScore(0, 0);
        PlayerScores.Set(playerRef, playerScore);
    }


    //===========================================
    //ホスト環境でのみ呼ばれるメソッド群
    //===========================================


    //プレイヤーが死亡したときに呼ばれ、色々な処理を行うメソッド
    public void NotifyDeath(PlayerRef victim, PlayerRef killer)
    {
        //ここでスコアの更新を行う
        AddScore(victim, killer);

        //RPCで全ローカル環境でのvictimに死亡処理を通知


    }

    //プレイヤーのスコアを更新するメソッド
    public void AddScore(PlayerRef victim, PlayerRef killer = default)
    {
        if (!Object.HasStateAuthority) return;
        // デスを追加
        if (PlayerScores.TryGet(victim, out PlayerScore victimScore))
        {
            victimScore.Deaths++;
            PlayerScores.Set(victim, victimScore);
        }
        // キルを追加
        if (PlayerScores.TryGet(killer, out PlayerScore killerScore))
        {
            killerScore.Kills++;
            PlayerScores.Set(killer, killerScore);
        }
    }


    //===========================================
    //全ローカル環境で呼ばれるメソッド群
    //===========================================
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_RaiseDeathEvent(PlayerRef victim, PlayerRef killer)
    {
        if(Runner.TryGetPlayerObject(victim, out var victimPlayer))
        {
            var victimState = victimPlayer.GetComponent<PlayerNetworkState>();
            if (victimState != null)
            {
                victimState.OnPlayerDeath(killer);
            }
        }
    }


    //スコアが変更されたときに呼ばれるコールバック
    public static void ManagerScoreCallback(Changed<GameManager> changed)
    {
        // スコアが変更されたときにイベントを発火
        changed.Behaviour.OnManagerScoreChanged?.Invoke(changed.Behaviour.GetAllScores());
        changed.Behaviour.PrintAllScore();
    }
    
    public bool TryGetPlayerScore(PlayerRef playerRef, out PlayerScore score)
    {
        return PlayerScores.TryGet(playerRef, out score);
    }

    public PlayerScore GetPlayerScore(PlayerRef playerRef)
    {
        if (PlayerScores.TryGet(playerRef, out var score))
        {
            return score;
        }
        return new PlayerScore(0, 0); // 存在しない場合はデフォルト値を返す
    }

    //全プレイヤー分の辞書を返す
    public　IEnumerable<KeyValuePair<PlayerRef, PlayerScore>> GetAllScores()
    {
        return PlayerScores;
    }

    //===========================================
    //-----------------デバッグ用----------------
    //===========================================

    //ホストが任意のプレイヤーのキル／デスを増減できるメソッド
    public void ModifyScore(PlayerRef target, int killsDelta, int deathsDelta)
    {
        if (!Object.HasStateAuthority) return;

        if (PlayerScores.TryGet(target, out var score))
        {
            // 0 未満にはしないように Clamp
            score.Kills = Mathf.Max(0, score.Kills + killsDelta);
            score.Deaths = Mathf.Max(0, score.Deaths + deathsDelta);
            PlayerScores.Set(target, score);
        }
    }
    //全プレイヤーのスコアをデバッグログに出力するメソッド
    public void PrintAllScore()
    {
        if (!Object.HasStateAuthority) return;
        foreach (var kvp in PlayerScores)
        {
            Debug.Log($"Player: {kvp.Key}, Kills: {kvp.Value.Kills}, Deaths: {kvp.Value.Deaths}");
        }
    }
}
