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
            DontDestroyOnLoad(gameObject); // �V�[�����܂����ŕێ�
        }
        else
        {
            Destroy(gameObject); // ������������Ȃ��悤��
        }
    }
}
