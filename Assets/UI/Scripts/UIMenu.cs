﻿using System.Collections;
using Cobalt;
using TMPro;
using UnityEngine;

public class UIMenu : UIPanel
{
    private const string HINT_0 = "Collect all <#ffff00>coins</color> before <#ff0000>enemies</color>";
    private const string HINT_1 = "Tap Play for connect to <#ffff00>{0}</color>";
    private const string HINT_2 = "Tap Play for host new game";

    private const string EVENT_CLICK = "EVENT_CLICK";
    private const string EVENT_UPDATE = "EVENT_UPDATE";

    public TextMeshProUGUI Version;

    private void Start()
    {
        Version.text = $"v{Application.version}";
    }

    public void OnPlayClick()
    {
        // Get<UIDiscovery>().Show();

        // Hide();
        // App.Lobby.Host(true);
        
        App.Instance.StartCoroutine(ScanAndJoin());
    }

    private IEnumerator ScanAndJoin()
    {
        //*
        Log.Info(this, "Start ScanAndJoin()...");
        
        App.Lobby.Scan(2500);

        while (App.Lobby.IsScanning && App.Lobby.Spots.Count == 0)
            yield return null;

        App.Lobby.Scan(0);

        if (App.Lobby.Spots.Count == 0)
        {
            Log.Warning(this, "There is no spots");
            yield break;
        }

        var spot = App.Lobby.Spots[0];
        Log.Info(this, $"Connect to {spot.EndPoint}");
        yield return App.Lobby.Join(spot);

        /*/

        yield return App.Lobby.Join("178.128.234.24:8889");

        //*/

        if (App.Lobby.State == LobbyState.Connected)
            Hide();

        while (App.Lobby.State == LobbyState.Connected)
            yield return null;

        App.Match.Disconnect();
        Show();
    }
}    