using System;
using UnityEngine.SceneManagement;


public enum SceneType
{

    None = 0,
    Lobby = 1 << 0,
    Battle = 1 << 1,
    Result = 1 << 2,
    HattoriLobby = 1 << 3,
    KitanoBattleTest = 1 << 4,
    NakajimaBattleTest = 1 << 5,
    HattoriBattleTest = 1 << 6,
    KitanoLobby = 1 << 7,
    NakajimaLobby = 1 << 8,
    KitanoResult = 1 << 9,
    HattoriResult = 1 << 10,
    KitanoTitle = 1 << 11, 
    HattoriTitle = 1 << 12,
    Title = 1 << 13,
    HattoriUITest = 1 << 14,

    All = ~0                // すべて ON

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
            SceneType.KitanoBattleTest => "KitanoBattleTestScene",
            SceneType.NakajimaBattleTest => "NakajimaBattleTestScene",
            SceneType.HattoriBattleTest => "HattoriBattleTestScene",
            SceneType.KitanoLobby => "KitanoLobbyTest",
            SceneType.NakajimaLobby => "NakajimaLobby",
            SceneType.KitanoResult => "KitanoResultTest",
            SceneType.HattoriResult => "HattoriResultScene",
            SceneType.KitanoTitle => "KitanoTitleTest",
            SceneType.HattoriTitle => "HattoriTitleScene",
            SceneType.HattoriLobby => "HattoriLobbyScene",
            SceneType HattoriUITest => "HattoriUITestScene",

            //_ => throw new ArgumentOutOfRangeException(nameof(type), $"未定義のSceneTypeです: {type}")
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



    // 逆変換「ファイル名 → enum」
    public static SceneType ToSceneType(this Scene scene)
    {
        return scene.name switch
        {
            "TitleScene"=> SceneType.Title,
            "LobbyScene" => SceneType.Lobby,
            "BattleScene" => SceneType.Battle,
            "ResultScene" => SceneType.Result,
            "KitanoBattleTestScene" => SceneType.KitanoBattleTest,
            "NakajimaBattleTestScene" => SceneType.NakajimaBattleTest,
            "HattoriBattleTestScene" => SceneType.HattoriBattleTest,
            "KitanoLobbyTest" => SceneType.KitanoLobby,
            "NakajimaLobby"=> SceneType.NakajimaLobby,
            "KitanoResultTest" => SceneType.KitanoResult,
            "HattoriResultScene" => SceneType.HattoriResult,
            "KitanoTitleTest"=> SceneType.KitanoTitle,
            "HattoriTitleScene"=> SceneType.HattoriTitle,
            "HattoriUITestScene" => SceneType.HattoriUITest,
            "HattoriLobbyScene" => SceneType.HattoriLobby,
            _ => SceneType.None

        };
    }
}