using System;
using System.Collections;
using Cobalt.Net;
using Cobalt.UI;
using UnityEngine;
using UnityEngine.Networking;

public class App : MonoBehaviour
{
    public static App Instance { get; private set; }

    public static UIManager UI { get { return Instance.GetComponent<UIManager>(); } }
    public static MatchManager MatchManager { get { return Instance.GetComponent<MatchManager>(); } }
    public static LobbyManager LobbyManager { get { return Instance.GetComponent<LobbyManager>(); } }

    public App()
    {
        Instance = this;
    }

    private LANServer server;
    public IEnumerator Start()
    {
        yield return null;

        App.LobbyManager.LocalHost();

        // App.UI.Get<UILobby>().Open();

        // LANServer

        /*
        // Search
        var finder = new LANSpotFinder(Constants.PORT);
        finder.Start();
        yield return new WaitForSeconds(1f);
        var spot = finder.Spots[0];

        // Auth
        var request = UnityWebRequest.Get("http://" + spot.EndPoint + "/auth");
        yield return request.SendWebRequest();

        // Connect
        MatchManager.Connect(request.downloadHandler.data);
        */
    }

    public void Update()
    {
        if (server != null)
            server.Tick(Time.time);
    }
}
