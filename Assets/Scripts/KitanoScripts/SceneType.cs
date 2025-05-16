using System;
using UnityEngine.SceneManagement;

public enum SceneType
{
    Title,
    Lobby,
    Battle,
    Result,
    Test_Loaded,
    Sample
}

public static class SceneTypeExtensions
{
    /// <summary>
    /// SceneType → 実際のシーン名（Build Settingsに登録された名前）への変換
    /// </summary>
    public static string ToSceneName(this SceneType type)
    {
        return type switch
        {
            SceneType.Title => "TitleScene",
            SceneType.Lobby => "LobbyScene",
            SceneType.Battle => "BattleScene",
            SceneType.Result => "ResultScene",
            SceneType.Test_Loaded => "SceneLoadTest_Loaded",
            SceneType.Sample => "SampleScene",
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"未定義のSceneTypeです: {type}")
        };
    }

    public static int ToSceneBuildIndex(this SceneType type)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneFileName = System.IO.Path.GetFileNameWithoutExtension(path);
            if (sceneFileName == type.ToSceneName())
            {
                return i;
            }
        }
        return -1; // not found
    }
}