using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using System.Linq;

public class HPObserver : NetworkBehaviour, IAfterSpawned
{
    public static HPObserver Instance { get; private set; }

    private Dictionary<PlayerRef, float> playerHPDict = new Dictionary<PlayerRef, float>();
    private Dictionary<PlayerRef, float> playerHPDictRed = new Dictionary<PlayerRef, float>();
    private Dictionary<PlayerRef, float> playerHPDictBlue = new Dictionary<PlayerRef, float>();
    public IReadOnlyDictionary<PlayerRef, float> PlayerHPDict => playerHPDict;
    public event Action OnAnyHPChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        GameManager.OnManagerInitialized -= SubscribeAllPlayers;
        GameManager.OnManagerInitialized += SubscribeAllPlayers;
    }
    private void OnDisable()
    {
        GameManager.OnManagerInitialized -= SubscribeAllPlayers;
    }

    public void AfterSpawned()
    {
        Debug.Log($"MyTest:AfterSpawned called.{Runner.Tick}");
    }

    public void SubscribeAllPlayers()
    {
        foreach (var playerRef in Runner.ActivePlayers)
        {
            if (!Runner.TryGetPlayerObject(playerRef, out NetworkObject playerObj))
            {
                Debug.LogWarning($"HPObsrver:Player object for {playerRef} not found.");
                continue;
            }
            var networkState = playerObj.GetComponent<PlayerNetworkState>();
            if (networkState == null)
            {
                Debug.LogWarning($"HPObserver:PlayerNetworkState component not found for player {playerRef}.");
                continue;
            }

            networkState.OnHPChanged += UpdateHPDict;

            playerHPDict[playerRef] = 1f;

            UserData? found = GameManager2.Instance.UserDataArray.FirstOrDefault(u => u.playerRef == playerRef);
            TeamType team = found.HasValue ? found.Value.userTeam : TeamType.None;


            if (team == TeamType.Red)
            {
                playerHPDictRed[playerRef] = 1f;
            }
            else if (team== TeamType.Blue)
            {
                playerHPDictBlue[playerRef] = 1f;
            }
        }
        LogAllPlayerHP();
    }

    private void UpdateHPDict(float hpNormalized, PlayerRef playerRef)
    {
        Debug.Log($"HPChangeTick：{Runner.Tick}");
        playerHPDict[playerRef] = hpNormalized;
        // 必要に応じてここで追加の処理を行う

        UserData? found = GameManager2.Instance.UserDataArray.FirstOrDefault(u => u.playerRef == playerRef);
        TeamType team = found.HasValue ? found.Value.userTeam : TeamType.None;

        if (team == TeamType.Red)
        {
            playerHPDictRed[playerRef] = hpNormalized;
        }
        else if (team == TeamType.Blue)
        {
            playerHPDictBlue[playerRef] = hpNormalized;
        }
        OnAnyHPChanged?.Invoke();

    }

    //デバッグ用
    //全プレイヤーのHPをログに出力
    public void LogAllPlayerHP()
    {
        foreach (var kvp in playerHPDict)
        {
            Debug.Log($"Player {kvp.Key.PlayerId}: HP = {kvp.Value}");
        }
    }
}
