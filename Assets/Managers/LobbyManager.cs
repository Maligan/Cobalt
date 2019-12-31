using UnityEngine;
using Cobalt.Net;
using System.Collections;
using UnityEngine.Networking;
using Cobalt.UI;
using Cobalt;

public class LobbyManager : MonoBehaviour
{
    private LobbyManagerState state;
    private LANServer local;

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

    public void LocalScan() { StartCoroutine(LocalScan_Coroutine()); }
    private IEnumerator LocalScan_Coroutine()
    {
        State = LobbyManagerState.Scan;

        LANSpotInfo spot = null;

        using (var finder = new LANSpotFinder(LANServer.DEFAULT_SPOT_PORT))
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
                App.MatchManager.Connect(bytes);
                
                State = LobbyManagerState.None;
            }
        }
    }

    public void LocalHost(bool autoConnect)
    {
        State = LobbyManagerState.AwaitAsServer;
        local = new LANServer();
        local.Start(new ShardOptions());
        if (autoConnect) App.MatchManager.Connect(local.Options.GetToken(0));
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