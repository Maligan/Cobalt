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
        Gesture.Dpi = (int)Screen.dpi;
        Gesture.Add(new UnityRaycasterHitTester());
        Gesture.Add(new UnityMouseInput());

        var sprite = GameObject.Find("Sprite_2");
        var tap = new TapGesture(sprite);
        tap.Recognized += _ => Debug.Log("Recognized #2");

        sprite = GameObject.Find("Sprite_1");
        var swipe = new SwipeGesture(sprite);
        swipe.Recognized += _ => Debug.Log("Recognized #1");

        yield break;
    }
}
