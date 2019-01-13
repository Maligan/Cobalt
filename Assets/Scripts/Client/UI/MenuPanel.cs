using System;
using System.Collections;
using System.Linq;
using Cobalt.Core;
using Cobalt.Core.Net;
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
    }

    public void OnDisable()
    {
        App.ShardService.Stop();
        finder.Stop();
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
        Debug.Log("IsHeld...");
        var isHeld = MulticastLock();
        Debug.Log("IsHeld: " + isHeld);

        StopLAN();
        StartCoroutine(OnRefreshClickCoroutine(true));
    }

    private IEnumerator OnRefreshClickCoroutine(bool rebuild)
    {
        // Refresh Data
        finder.Refresh();
        yield return new WaitForSeconds(1f);
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



    private AndroidJavaObject multicastLock;
    private bool MulticastLock()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR

        try
        {
            if (multicastLock == null)
            {
                using (AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (var wifiManager = activity.Call<AndroidJavaObject>("getSystemService", "wifi"))
                    {
                        multicastLock = wifiManager.Call<AndroidJavaObject>("createMulticastLock", "lock");
                        multicastLock.Call("acquire");
                    }
                }
            }

            return multicastLock.Call<bool>("isHeld");
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        #endif

        return false;
    }
}

public class SpotInfo
{
    public Spot Spot;
}


