using UnityEngine;
using Cobalt.Net;
using System.Collections;
using UnityEngine.Networking;
using Cobalt.UI;

public class LobbyManager : MonoBehaviour
{
    private ShardService shard;

    public void LocalScan()
    {
        StartCoroutine(LocalScan_Coroutine());
    }

    private IEnumerator LocalScan_Coroutine()
    {
        Spot spot = null;

        using (var finder = new SpotServiceFinder(Constants.PORT))
        {
            finder.Change += () => {
                spot = finder.Spots[0];
                finder.Stop();
            };

            finder.Refresh();
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

    public void LocalHost()
    {
        shard = new ShardService();
        shard.Start(new ShardOptions());
        App.MatchManager.Connect(shard.GetToken());
    }

    private void Update()
    {
        if (shard != null)
            shard.Tick(Time.deltaTime);
    }
}