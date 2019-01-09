using System.Collections;
using System.Linq;
using Cobalt.Math;
using Cobalt.Math.Net;
using Cobalt.UI;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Networking;

public class MenuPanel : UIPanel
{
    public GameObject lobbyPrefab;
    public RectTransform lobbyList;
    public RectTransform lobbyEmpty;

    private SpotServiceFinder finder;

    public IEnumerator Start()
    {
        finder = new SpotServiceFinder(8888);
        yield return null;
        OnRefreshClick();
    }

    private void RebuildList(Spot selected)
    {
        foreach (Transform lobbyItem in lobbyList)
            if (lobbyItem != lobbyEmpty)
                Destroy(lobbyItem.gameObject);

        foreach (var spot in finder.Spots)
        {
            var lobbyItem = Instantiate(lobbyPrefab, lobbyList);
            var lobby = lobbyItem.GetComponent<MenuSpotItem>();
            lobby.Spot = spot;
            if (spot == selected) lobby.Select();
        }

        lobbyEmpty.gameObject.SetActive(finder.Spots.Count == 0);
    }

    public void OnRefreshClick()
    {
        StopLAN();
        StartCoroutine(OnRefreshClickCoroutine(true));
    }

    private IEnumerator OnRefreshClickCoroutine(bool rebuild)
    {
        // Refresh Data
        if (finder.IsRunning) yield break;
        finder.Refresh();
        while (finder.IsRunning) yield return null;

        // Refresh List
        if (rebuild) RebuildList(null);
    }

    public void OnHostClick()
    {
        StopLAN();

        StartCoroutine(OnHostClickCoroutine());
    }

    private IEnumerator OnHostClickCoroutine()
    {
        App.ShardService.Start(new ShardOptions());

        yield return OnRefreshClickCoroutine(false);

        var ips = App.ShardService.Options.ips;

        var spot = finder.Spots.FirstOrDefault(s => {
            var ip = s.EndPoint.Address;
            var ipInOptions = ips.Any(x => x.Equals(ip));
            return ipInOptions;
        });

        RebuildList(spot);
    }

    private void StopLAN()
    {
        App.ShardService.Stop();
    }
}

public class SpotInfo
{
    public Spot Spot;
}