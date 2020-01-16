using System.Collections;
using Cobalt.UI;
using GestureKit;
using GestureKit.Unity;
using UnityEngine;

public class App : MonoBehaviour
{
    public static App Instance { get; private set; }

    public static T UI<T>() where T : UIPanel => Instance.GetComponent<UIManager>().Get<T>();
    public static HookManager Hook = new HookManager();
    public static DataManager Data = new DataManager();
    public static MatchManager Match => Instance.GetComponent<MatchManager>();
    public static LobbyManager Lobby => Instance.GetComponent<LobbyManager>();

    public App()
    {
        Instance = this;
    }

    public IEnumerator Start()
    {
        // Initialize
        Gesture.Dpi = (int)Screen.dpi;
        Gesture.Add(new UnityRaycasterHitTester());
        Gesture.Add(new UnityMouseInput());

        // Start
        App.UI<UILobby>().Open();
        yield break;
    }
}
