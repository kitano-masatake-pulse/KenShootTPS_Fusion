using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct UserData
{
    public string userID;
    public PlayerRef playerRef;
    public PlayerScore userScore;
    public TeamType userTeam;
    public string userName;
    public UserData(string id, PlayerRef player, PlayerScore score = default,TeamType team = TeamType.None, string name = "Player")
    {
        userID = id;
        playerRef = player;
        userScore = score;
        userTeam = team;
        userName = name;
    }
    
    public void DisplayUserInfo()
    {
       Debug.Log($"User ID: {userID}, Player Ref: {playerRef}, Score: {userScore.Kills}/{userScore.Deaths}, Team: {userTeam.GetName()}, Name: {userName}");
    }
}
