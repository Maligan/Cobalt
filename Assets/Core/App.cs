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

    public IEnumerator Start()
    {
        yield return null;

        /*
        App.LobbyManager.LocalHost(true);
        /*/
        // Search
        App.LobbyManager.LocalScan();
        //*/
    }
}
