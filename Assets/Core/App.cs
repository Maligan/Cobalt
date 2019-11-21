using System;
using System.Collections;
using System.Net;
using Cobalt;
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
        // App.UI.Get<UILobby>().Open();


       var tree = new BTree(); 
       tree.Do(1)
           .If(() => true)
                .Do(2)
                .If(() => true)
                    .Do(3)
                    .Do(4)
                    .Do(5)
                    .End()
                .Do(6)
                .End()
           .While(() => Time.unscaledTime < 4.0f)
                .Do(7)
                .End()
           .Do(8);

        for (int i = 0; i < 1000; i++)
        {
            tree.Tick();
            yield return null; // new WaitForSecondsRealtime(0.1f);
        }
        
        yield break;

        //*
        App.LobbyManager.LocalHost(true);
        /*/
        // Search
        // App.LobbyManager.LocalHost(false);
        
        var finder = new LANSpotFinder(Constants.PORT);
        finder.Start();
        yield return new WaitForSeconds(1f);
        var spot = finder.Spots[0];
        finder.Stop();

        Log.Info(this, "Find " + spot);

        // Auth
        var request = UnityWebRequest.Get("http://" + spot.EndPoint + "/auth");
        yield return request.SendWebRequest();

        // Connect
        MatchManager.Connect(request.downloadHandler.data);
        //*/
    }

    public void Update()
    {
        if (server != null)
            server.Tick(Time.time);
    }
}
