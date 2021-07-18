using UnityEngine;
using Cobalt.Net;
using System.Collections;
using UnityEngine.Networking;
using Cobalt;
using System.Collections.Generic;
using System.Net;
using System;
using System.Globalization;

public class LobbyManager : MonoBehaviour
{
    private LanSpotFinder _finder = new LanSpotFinder(LanServer.DEFAULT_SPOT_PORT);

    /// All discovered LAN spots
    public IList<LanSpotInfo> Spots => _finder.Spots;

    /// Now is searching for spots
    public bool IsScanning => _finder.IsRunnig;

    /// Start/Stop lan scanning
    public void Scan(int timemout = 8000)
    {
        if (timemout > 0)
            _finder.Start(timemout);
        else
            _finder.Stop();
    }

    // --------------------------

    private LanServer _server;

    public void Host()
    {
        _server = new LanServer();
        _server.Start();
    }

    // ---------------------------

    public LobbyListToken List(IPEndPoint endPoint)
    {
        var token = new LobbyListToken();
        StartCoroutine(List_Coroutine(token, endPoint));
        return token;
    }

    private IEnumerator List_Coroutine(LobbyListToken token, IPEndPoint endPoint)
    {
        // token.Status = LobbyListStatus.Http

        var authRequest = UnityWebRequest.Get($"http://{endPoint}/list");
        authRequest.timeout = 4;
        yield return authRequest.SendWebRequest();

        var httpCode = authRequest.responseCode;
        if (httpCode == 200)
        {
            token.Status = LobbyListStatus.Success;
            token.Lobbies = new List<LobbyInfo>();

            var httpBody = authRequest.downloadHandler.text;
            if (httpBody != string.Empty)
            {
                var httpRows = httpBody.Split('\n');

                foreach (var row in httpRows)
                {
                    var values = row.Split(',');
                    var lobby = new LobbyInfo()
                    {
                        Spot = new LanSpotInfo()
                        {
                            EndPoint = new IPEndPoint(
                                IPAddress.Parse(values[0]),
                                int.Parse(values[1])
                            )
                        },
                        NumPlayers = int.Parse(values[2]),
                        TotalPlayers = int.Parse(values[3])
                    };

                    token.Lobbies.Add(lobby);
                }
            }
        }
        else
        {
            token.Status = (LobbyListStatus)httpCode;
        }
    }

    // ---------------------------

    private LobbyJoinToken _token;
    
    public LobbyJoinToken Join(LanSpotInfo spotInfo)
    {
        StartCoroutine(Join_Coroutine(spotInfo));
        return _token;
    }

    private IEnumerator Join_Coroutine(LanSpotInfo spotInfo)
    {
        _token = new LobbyJoinToken();
        // Auth

        _token.Status = LobbyJoinStatus.Auth;

        var authUrl = $"http://{spotInfo.EndPoint}/auth";
        var authRequest = UnityWebRequest.Get(authUrl);
        authRequest.timeout = 4;

        yield return authRequest.SendWebRequest();

        var httpBody = authRequest.downloadHandler.data;
        var httpCode = authRequest.responseCode;
        if (httpCode == 200)
        {
            Log.Info(this, "Auth success");
            _token.Status = LobbyJoinStatus.AuthSuccess;
            _token.Token = httpBody;
            yield return JoinConnect();
        }
        else
        {
            _token.Status = LobbyJoinStatus.Fail_Auth + (int)authRequest.responseCode;
            Log.Info(this, "Auth error");
        }
    }

    private IEnumerator JoinConnect()
    {
        _token.Status = LobbyJoinStatus.Connect;
        _token.Client = new NetcodeClient(_token.Token);
        _token.Client.Connect();

        while (_token.Client.IsConnecting)
        {
            _token.Status = LobbyJoinStatus.Connect + _token.Client.State;
            yield return null;
        }

        if (_token.Client.IsConnected)
        {
            _token.Status = LobbyJoinStatus.ConnectSuccess;
            // TODO: Why is it here?
            App.Match.Connect(_token.Client);
        }
        else
        {
            _token.Status = LobbyJoinStatus.Fail_Connect - _token.Client.State;
        }

        // Завершение сессии игры
        while (_token.Client.IsConnected)
            yield return null;
        
        _token = null;
    }


    
    public LobbyState State
    {
        get
        {
            if (_token == null)
                return LobbyState.None;
            
            if (_token.Status == LobbyJoinStatus.ConnectSuccess)
                return LobbyState.Connected;
            
            return LobbyState.Connecting;
        }
    }

    private void Update()
    {
        if (_server != null)
            _server.Tick();

        if (_token != null && _token.Client != null)
            _token.Client.Tick();
    }

    private void OnDestroy()
    {
        if (_server != null)
            _server.Dispose();
    }
}











public enum LobbyState
{
    None = 0,
    Connecting,
    Connected
}