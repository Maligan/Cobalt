using UnityEngine;
using Cobalt.Net;
using System.Collections;
using UnityEngine.Networking;
using Cobalt;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour
{
    private NetcodeClient client;
    private LanServer server;
    private LanSpotFinder finder = new LanSpotFinder(LanServer.DEFAULT_SPOT_PORT);

    /// All discovered LAN spots
    public List<LanSpotInfo> Spots => finder.Spots;

    /// Now search for spots
    public bool IsScanning => finder.IsRunnig;

    private LobbyState state = LobbyState.None;
    public LobbyState State
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

    public void Scan(int timemout = 8000)
    {
        if (timemout >= 0)
            finder.Start(timemout);
        else
            finder.Stop();
    }

    public LobbyJoinToken Join(LanSpotInfo spotInfo)
    {
        var token = new LobbyJoinToken();
        StartCoroutine(Join_Coroutine(spotInfo, token));
        return token;
    }

    private IEnumerator Join_Coroutine(LanSpotInfo spotInfo, LobbyJoinToken token)
    {
        // Auth

        var authUrl = $"http://{spotInfo.EndPoint}/auth";
        var authRequest = UnityWebRequest.Get(authUrl);
        authRequest.timeout = 4;

        Log.Info(this, $"Connect to '{authUrl}'");
        yield return authRequest.SendWebRequest();
        var httpCode = authRequest.responseCode;
        var httpBody = authRequest.downloadHandler.data;

        if (httpCode == 200)
        {
            token.Code = LobbyJoinCode.Success;
            token.Token = httpBody;
        }
        else
        {
            token.Code = LobbyJoinCode.Fail_Auth + (int)authRequest.responseCode;
        }

        // Connect (TODO: Error while Connect)

        if (token.Code == LobbyJoinCode.Success)
        {
            App.Match.Connect(client = new NetcodeClient(token.Token));
        }
    }

    public void Host(bool autoConnect)
    {
        server = new LanServer();
        server.Start(new ShardOptions());

        if (autoConnect)
            App.Match.Connect(client = new NetcodeClient(server.Options.GetToken(0)));
    }

    private void Update()
    {
        if (server != null)
            server.Update(Time.unscaledTime);

        if (client != null)
            client.Update(Time.unscaledTime);
    }

    private void OnDestroy()
    {
        if (server != null)
            server.Stop();
    }
}

public class LobbyJoinToken : CustomYieldInstruction
{
    public LobbyJoinCode Code;
    public byte[] Token;

    public override bool keepWaiting => Code == LobbyJoinCode.Unknown;
}

public enum LobbyJoinCode : int
{
    Unknown = -1,
    Success =  0,
    Fail_Auth = 1000,
}

public enum LobbyState
{
    None = 0,
    Await,
    Countdown,
    Play
}