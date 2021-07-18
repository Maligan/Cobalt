using System.Collections.Generic;
using Cobalt.Net;
using UnityEngine;

public class LobbyListToken : CustomYieldInstruction
{
    public List<LobbyInfo> Lobbies { get; set; }
    public LobbyListStatus Status { get; set; } = LobbyListStatus.Unknown;

    public override bool keepWaiting => Lobbies == null || Status > 0;
}

public struct LobbyInfo
{
    public LanSpotInfo Spot;
    public int NumPlayers;
    public int TotalPlayers;

    public new string ToString()
    {
        return Spot + "-" + NumPlayers + "/" + TotalPlayers;
    }
}

public enum LobbyListStatus : int
{
    Unknown = -1,
    Success = 0
}