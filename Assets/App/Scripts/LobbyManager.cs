﻿using UnityEngine;
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

    // private LobbyJoinToken Join(LanServer server)
    // {
    //     if (_token != null)
    //         return null;
            
    //     _token = new LobbyJoinToken()
    //     {
    //         Token = server.Auth(server.Add(new ShardOptions())),
    //         Status = LobbyJoinStatus.AuthSuccess
    //     };

    //     StartCoroutine(JoinConnect());
    //     return _token;
    // }
    
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

public class LobbyJoinToken : CustomYieldInstruction
{
    public LobbyJoinStatus Status = LobbyJoinStatus.Unknown;
    public byte[] Token;
    public NetcodeClient Client;

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

public enum LobbyState
{
    None = 0,
    Connecting,
    Connected
}