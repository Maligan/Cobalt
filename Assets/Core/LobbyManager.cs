using UnityEngine;
using Cobalt.Net;
using System.Collections;
using UnityEngine.Networking;
using Cobalt.UI;

public class LobbyManager : MonoBehaviour
{
    private LANServer local;

    public void LocalScan()
    {
        StartCoroutine(LocalScan_Coroutine());
    }

    private IEnumerator LocalScan_Coroutine()
    {
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
                var bytes = request.downloadHandler.data;
                App.UI.Get<UILobby>().Close();
                App.MatchManager.Connect(bytes);
            }
        }
    }

    public void LocalHost(bool autoConnect)
    {
        local = new LANServer();
        local.Start(new ShardOptions());
        if (autoConnect) App.MatchManager.Connect(local.Options.GetToken(0));
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