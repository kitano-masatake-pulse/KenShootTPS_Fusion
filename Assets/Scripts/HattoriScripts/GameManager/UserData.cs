using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

public struct UserData : INetworkStruct
{
    // 16バイトのハッシュを固定長バッファで定義
    //public fixed byte userID[16];
    public Guid userGuid;
    //public long userID_high ;
    //public long userID_low;
    public PlayerRef playerRef;
    public PlayerScore userScore;
    public TeamType userTeam;
    public NetworkString<_16> userName;
    public ConnectionState userConnectionState;

    public UserData(Guid id, PlayerRef player, PlayerScore score = default, TeamType team = TeamType.None, string name = "Player", ConnectionState connectionState = ConnectionState.Dummy)
    {
        //fixed{ }
        userGuid = id;
        playerRef = player;
        userScore = score;
        userTeam = team;
        userName = name;
        userConnectionState = connectionState; // 初期状態はダミー
    }

   
    public  void DisplayUserInfo()
    {
       Debug.Log($"Display UserInfo. User ID:{userGuid.ToString() } , Player Ref: {playerRef}, Score: {userScore.Kills}/{userScore.Deaths}, Team: {userTeam.GetName()} ");
    }

}

public enum ConnectionState
{
    Dummy,        // 誰でもないダミースロット
    Connected,    // 現在オンライン中
    Disconnected  // 過去に接続していたが、現在は切断中
}