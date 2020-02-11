using System.Collections;
using Cobalt.UI;
using Netouch;
using Netouch.Unity;
using UnityEngine;

public class App : MonoBehaviour
{
    public static App Instance { get; private set; }

    public static T UI<T>() where T : UIPanel => Instance.GetComponent<UIManager>().Get<T>();
    public static Canvas Canvas => Instance.GetComponent<UIManager>().root.GetComponentInParent<Canvas>();
    public static Camera Camera => Instance.GetComponent<Camera>();
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
        Gesture.Add(new UnityTouchInput());

        Input.simulateMouseWithTouches = true;
        Input.multiTouchEnabled = true;
        // Input.touchSupported = true;

        Debug.Log("Input.multiTouchEnabled = " + Input.multiTouchEnabled);
        Debug.Log("Input.simulateMouseWithTouches = " + Input.simulateMouseWithTouches);
        Debug.Log("Input.touchSupported = " + Input.touchSupported);

        new TapGesture().Recognized += x => Debug.Log("Tap");
        new LongPressGesture().Recognized += x => Debug.Log("LongPress");
        new SwipeGesture().Recognized += x => Debug.Log("Swipe: " + ((SwipeGesture)x).Direction);

        // Start
        // App.UI<UILobby>().Open();
        yield break;
    }
}
