using Cobalt.Net;
using Cobalt.UI;
using UnityEngine;

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

    public void Start()
    {
        App.UI.Get<UILobby>().Open();
    }
}
