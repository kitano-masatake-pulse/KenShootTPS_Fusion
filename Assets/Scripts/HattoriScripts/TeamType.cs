using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TeamType : byte
{
    None = 0, // チームなし
    Red,      // 赤チーム
    Blue,     // 青チーム
}

public static class TeamTypeExtensions
{
    public static string GetName(this TeamType teamType)
    {
        return teamType switch
        {
            TeamType.None => "None",
            TeamType.Red => "Red",
            TeamType.Blue => "Blue",
            _ => "Unknown"
        };
    }
    public static Color GetColor(this TeamType teamType)
    {
        return teamType switch
        {
            TeamType.None => Color.white, // チームなしは白
            TeamType.Red => Color.red,
            TeamType.Blue => Color.blue,
            _ => Color.gray // 不明なチームは灰色
        };
    }

}
