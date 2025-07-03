using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct UserData: INetworkStruct
{

    
    public NetworkString<_32> userID;
    public PlayerRef playerRef;
    public PlayerScore userScore;
    public TeamType userTeam;
    public NetworkString<_32> userName;
    public ConnectionState userConnectionState;
    public UserData(string id, PlayerRef player, PlayerScore score = default,TeamType team = TeamType.None, string name = "Player",ConnectionState connectionState=ConnectionState.Dummy)
    {
        userID = id;
        playerRef = player;
        userScore = score;
        userTeam = team;
        userName = name;
        userConnectionState = connectionState; // 初期状態はダミー
    }
    
    public void DisplayUserInfo()
    {
       Debug.Log($"User ID: {userID}, Player Ref: {playerRef}, Score: {userScore.Kills}/{userScore.Deaths}, Team: {userTeam.GetName()}, Name: {userName}");
    }
}

public enum ConnectionState
{
    Dummy,        // 誰でもないダミースロット
    Connected,    // 現在オンライン中
    Disconnected  // 過去に接続していたが、現在は切断中
}
