using UnityEngine;
using Cobalt.Net;
using System.Collections;
using UnityEngine.Networking;
using Cobalt.UI;
using Cobalt;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour
{
    private LobbyManagerState state = LobbyManagerState.None;
    private LanServer local;
    private LanSpotFinder finder = new LanSpotFinder(LanServer.DEFAULT_SPOT_PORT);

    public List<LanSpotInfo> Spots => finder.Spots;

    public LobbyManagerState State
    {
        get => state;
        set
        {
            if (state != value)
            {
                state = value;
                Log.Info(this, $"State to '{state}'");
            }
        }
    }

    public LobbyManager()
    {
        finder = new LanSpotFinder(LanServer.DEFAULT_SPOT_PORT);
    }

    public void Scan()
    {
        // 10 minutes
        finder.Start(60 * 10 * 1000);
    }

    public LobbyConnectToken Connect(LanSpotInfo spotInfo)
    {
        var token = new LobbyConnectToken();
        StartCoroutine(Connect_Coroutine(spotInfo, token));
        return token;
    }

    private IEnumerator Connect_Coroutine(LanSpotInfo spotInfo, LobbyConnectToken token)
    {
        var authUrl = $"http://{spotInfo.EndPoint}/auth";
        var authRequest = UnityWebRequest.Get(authUrl);

        yield return authRequest.SendWebRequest();
        
        if (authRequest.responseCode == 200)
        {
            Log.Info(this, "Connecting by token");
            App.Match.Connect(authRequest.downloadHandler.data);
            token.Code = LobbyConnectCode.Success;
        }
        else
        {
            token.Code = LobbyConnectCode.Fail_Auth + (int)authRequest.responseCode;
        }
    }

    public void Host(bool autoConnect)
    {
        local = new LanServer();
        local.Start(new ShardOptions());
        if (autoConnect) App.Match.Connect(local.Options.GetToken(0));
    }

    private void Update()
    {
        if (local != null)
            local.Tick(Time.time);
    }

    private void OnDestroy()
    {
        if (local != null)
            local.Stop();
    }
}

public class LobbyConnectToken : CustomYieldInstruction
{
    public LobbyConnectCode Code;

    public override bool keepWaiting => Code == LobbyConnectCode.Unknown;
}

public enum LobbyConnectCode : int
{
    Unknown = -1,
    Success =  0,
    Fail_Auth = 1000,
    Fail_Connect = 2000
}


public enum LobbyManagerState
{
    None = 0,
    Scan,
    AwaitAsClient,
    AwaitAsServer,
    // Countdown,
}