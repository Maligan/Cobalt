using System.Collections;
using Netouch;
using Netouch.Unity;
using UnityEngine;

public class App : MonoBehaviour
{
    public static App Instance { get; private set; }

    public static UIManager UI => Instance.GetComponent<UIManager>();
    public static HookManager Hook = new HookManager();
    public static LibraryManager Library = new LibraryManager();
    public static MatchManager Match => Instance.GetComponent<MatchManager>();
    public static LobbyManager Lobby => Instance.GetComponent<LobbyManager>();

    public Stats Stats;

    public App()
    {
        Instance = this;
    }

    private void Update()
    {
        Stats.Set("latency", Match.Latency);
        Stats.Set("latency.fw", Match.LatencyForward);
        Stats.Set("latency.bw", Match.LatencyBackward);
    }

    public IEnumerator Start()
    {
        // Initialize
        Gesture.Dpi = (int)Screen.dpi;
        Gesture.Add(new UnityInput(false));
        Gesture.Add(new UnityRaycasterHitTester());

        

        // Input.multiTouchEnabled = true;
        // Input.simulateMouseWithTouches = true;
        // Input.touchSupported = true;

        // Debug.Log("Screen.dip = " + Screen.dpi);
        // Debug.Log("Input.multiTouchEnabled = " + Input.multiTouchEnabled);
        // Debug.Log("Input.simulateMouseWithTouches = " + Input.simulateMouseWithTouches);
        // Debug.Log("Input.touchSupported = " + Input.touchSupported);

        // var tap2 = new TapGesture() { NumTapsRequired = 2 }.On(GestureState.Accepted, x => Debug.Log("Double Tap"));
        // var tap1 = new TapGesture() { NumTapsRequired = 1 }.On(GestureState.Accepted,
        //     x => {
        //         Debug.Log("Tap");
        //     }
        // );
        // tap1.Require(tap2);

        // new LongPressGesture().On(GestureState.Accepted, x => Debug.Log("LongPress"));
        // new SwipeGesture().On(GestureState.Accepted, x => Debug.Log("Swipe " + x.Direction));

        // Start
        App.UI.Get<UIMenu>().Show();

        yield break;
    }
}