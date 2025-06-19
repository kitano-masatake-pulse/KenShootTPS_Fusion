using Fusion;


public struct PlayerScore : INetworkStruct
{
    public int Kills;
    public int Deaths;
    public PlayerScore(int kills, int deaths)
    {
        Kills = kills;
        Deaths = deaths;
    }
}