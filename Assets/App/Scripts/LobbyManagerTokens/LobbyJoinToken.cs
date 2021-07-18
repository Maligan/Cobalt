using Cobalt.Net;
using UnityEngine;

public class LobbyJoinToken : CustomYieldInstruction
{
    public LobbyJoinStatus Status { get; set; } = LobbyJoinStatus.Unknown;
    public byte[] Token { get; set; }
    public NetcodeClient Client { get; set; }

    public override bool keepWaiting => Status < LobbyJoinStatus.ConnectSuccess;
}

public enum LobbyJoinStatus : int
{
    Unknown = -1,
    
    Auth = 0,
    AuthSuccess = 999,
    Connect = 1000,
    ConnectSuccess =  1999,

    Fail_Auth = 2000,
    Fail_Connect = 3000
}