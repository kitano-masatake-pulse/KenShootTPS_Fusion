using System;

public enum SceneType
{
    Title,
    Lobby,
    Battle,
    Result,
    Test_Loaded
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
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"未定義のSceneTypeです: {type}")
        };
    }
}