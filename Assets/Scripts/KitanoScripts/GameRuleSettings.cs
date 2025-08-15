using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GameRule
{
    DeathMatch,
    TeamDeathMatch,
    None
}


public class GameRuleSettings : MonoBehaviour
{
    public static GameRuleSettings Instance;

    public GameRule selectedRule=GameRule.None;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            this.transform.SetParent(null); // 親オブジェクトを解除
            DontDestroyOnLoad(gameObject); // シーンをまたいで保持
        }
        else
        {
            Destroy(gameObject); // 複数生成されないように
        }
    }
}
