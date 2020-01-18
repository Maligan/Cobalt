using UnityEngine;
using Cobalt.Net;
using System.Collections;
using UnityEngine.Networking;
using Cobalt.UI;
using Cobalt;
using System.Collections.Generic;
using System.Net.Http;

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

        Log.Info(this, $"Connect to '{authUrl}'");

        /*
        yield return authRequest.SendWebRequest();
        var httpCode = authRequest.responseCode;
        var httpBody = authRequest.downloadHandler.data;
        /*/
        var http = new HttpClient();
        var httpResponseTask = http.GetAsync(authUrl);
        while (!httpResponseTask.IsCompleted) yield return null;
        var httpResponse = httpResponseTask.Result;
        var httpCode = (int)httpResponse.StatusCode;
        var httpBodyTask = httpResponse.Content.ReadAsByteArrayAsync();
        while (!httpBodyTask.IsCompleted) yield return null;
        var httpBody = httpBodyTask.Result;
        //*/

        Log.Info(this, $"Connect response - {httpCode}");


        if (httpCode == 200)
        {
            App.Match.Connect(httpBody);
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
            local.Tick(Time.unscaledTime);
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
}


public enum LobbyManagerState
{
    None = 0,
    Scan,
    AwaitAsClient,
    AwaitAsServer,
    // Countdown,
}