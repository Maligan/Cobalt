using System.Collections;
using Cobalt.UI;
using UnityEngine;

public class App : MonoBehaviour
{
    public static App Instance { get; private set; }

    public static T UI<T>() where T : UIElement => Instance.GetComponent<UIManager>().Get<T>();
    public static HookManager Hook = new HookManager();
    public static MatchManager MatchManager { get { return Instance.GetComponent<MatchManager>(); } }
    public static LobbyManager LobbyManager { get { return Instance.GetComponent<LobbyManager>(); } }

    public App()
    {
        Instance = this;
    }

    public IEnumerator Start()
    {
        //*
        App.LobbyManager.LocalHost(true);
        /*/
        // Search
        App.LobbyManager.LocalScan();
        //*/

        yield break;
    }
}
