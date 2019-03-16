using System;
using System.Collections;
using System.Linq;
using Cobalt.Core;
using Cobalt.Net;
using Cobalt.UI;
using TMPro;
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
        finder.Refresh();

        StartCoroutine(RebuildListCoroutine());
        yield return null;
    }

    public void OnDisable()
    {
        App.ShardService.Stop();
        finder.Stop();
    }

    private IEnumerator RebuildListCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            RebuildList(null);
        }
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
            // if (spot == selected) lobby.Select();
        }

        lobbyEmpty.gameObject.SetActive(finder.Spots.Count == 0);
    }

    public void OnRefreshClick()
    {
        StopLAN();
    }

    public void OnHostClick()
    {
        StopLAN();
        
        App.ShardService.Start(new ShardOptions());

        var ips = App.ShardService.Options.ips;
        var spot = finder.Spots.FirstOrDefault(s => {
            var ip = s.EndPoint.Address;
            var ipInOptions = ips.Any(x => x.Equals(ip));
            return ipInOptions;
        });
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


