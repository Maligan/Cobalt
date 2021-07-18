using System;
using System.Collections;
using System.Net;
using Cobalt;
using Cobalt.Net;
using TMPro;
using UnityEngine;

[ExecuteAlways]
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

    private void OnGUI()
    {
        GUIExtensions.Actions(
            ("standalone", HostAndJoin),
            ("localhost", ScanAndJoin),
            ("malhost.ru", delegate
            {
                Join(new LanSpotInfo()
                {
                    EndPoint = new IPEndPoint(
                        IPAddress.Parse("178.128.234.24"),
                        LanServer.DEFAULT_AUTH_PORT
                    )
                });
            })
        );
    }

    public void OnPlayClick()
    {
        Get<UIDiscovery>().Show();
    }

    private void HostAndJoin()
    {
        App.Lobby.Host();
        ScanAndJoin();
    }

    private void ScanAndJoin() => App.Instance.StartCoroutine(ScanAndJoin_Coroutine());
    private IEnumerator ScanAndJoin_Coroutine()
    {
        Log.Info(this, "Start ScanAndJoin()...");
        
        App.Lobby.Scan(1000);

        while (App.Lobby.IsScanning && App.Lobby.Spots.Count == 0)
            yield return null;

        App.Lobby.Scan(0);

        if (App.Lobby.Spots.Count == 0)
        {
            Log.Warning(this, "There is no spots");
            yield break;
        }

        Join(App.Lobby.Spots[0]);
    }

    private void Join(LanSpotInfo spot) => App.Instance.StartCoroutine(Join_Coroutine(spot));
    private IEnumerator Join_Coroutine(LanSpotInfo spot)
    {
        Log.Info(this, "Join to " + spot);
        yield return App.Lobby.Join(spot);

        if (App.Lobby.State == LobbyState.Connected)
            Hide();

        while (App.Lobby.State == LobbyState.Connected)
            yield return null;

        App.Match.Disconnect();
        Show();
    }
}