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
    private List<LanSpotInfo> spots = new List<LanSpotInfo>();
    
    private LanServer local;
    private LanSpotFinder finder = new LanSpotFinder(LanServer.DEFAULT_SPOT_PORT);

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
        finder.Change += () => {
            // if (state != LobbyManagerState.Scan)
            // {
            //     Log.Warning(this, "LanSpotFinder.Change while not in Scanning");
            //     return;
            // }

            spots = finder.Spots;
            // state = finder.IsRunning ? LobbyManagerState.Scan : LobbyManagerState.None;
        };
    }

    public void Scan()
    {
        finder.Start();
    }

    private IEnumerator Scan_Coroutine()
    {
        State = LobbyManagerState.Scan;

        LanSpotInfo spot = null;

        using (var finder = new LanSpotFinder(LanServer.DEFAULT_SPOT_PORT))
        {
            finder.Change += () => {
                spot = finder.Spots[0];
                finder.Stop();
            };

            finder.Start();
            yield return new WaitForSecondsRealtime(2);
        }

        if (spot != null)
        {
            var request = UnityWebRequest.Get($"http://{spot.EndPoint}/auth");
            yield return request.SendWebRequest();

            if (request.responseCode == 200)
            {
                State = LobbyManagerState.AwaitAsClient;

                var bytes = request.downloadHandler.data;
                App.UI<UILobby>().Close();
                App.Match.Connect(bytes);
                
                State = LobbyManagerState.None;
            }
        }
    }

    public void Host(bool autoConnect)
    {
        State = LobbyManagerState.AwaitAsServer;
        local = new LanServer();
        local.Start(new ShardOptions());
        if (autoConnect) App.Match.Connect(local.Options.GetToken(0));
        State = LobbyManagerState.None;
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

public enum LobbyManagerState
{
    None = 0,
    Scan,
    AwaitAsClient,
    AwaitAsServer,
    // Countdown,
}