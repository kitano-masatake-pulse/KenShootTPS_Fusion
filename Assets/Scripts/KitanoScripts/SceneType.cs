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
    /// SceneType �� ���ۂ̃V�[�����iBuild Settings�ɓo�^���ꂽ���O�j�ւ̕ϊ�
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
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"����`��SceneType�ł�: {type}")
        };
    }
}