using System;
using Cobalt.Ecs;
using Cobalt.Net;
using Cobalt.Unity;
using Netouch;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public GameObject root;
    public Prefabs prefabs;

    private MatchTimeline timeline;
    private NetcodeClient client;

    private UnitInput input;

    private TapGesture tap;
    private SwipeGesture swipe;

    public void Connect(NetcodeClient client)
    {
        Debug.Assert(client.IsConnected, "Client must be connected");

        root.SetActive(true);

        timeline = new MatchTimeline();

        this.client = client;
        this.client.OnMessage += OnMessage;

        input = new UnitInput() { move = Direction.None };

        tap = new TapGesture().On(GestureState.Accepted, OnTap);
        swipe = new SwipeGesture().On(GestureState.Accepted, OnSwipe);
    }

    public void Disconnect()
    {
        root.SetActive(false);
    }

    private void OnTap(Gesture tap)
    {
        input.move = Direction.None;
    }

    private void OnSwipe(Gesture swipe)
    {
        var swipeGesture = (SwipeGesture)swipe;

        switch (swipeGesture.Direction)
        {
            case SwipeGestureDirection.Up: input.move = Direction.Top; break;
            case SwipeGestureDirection.Right: input.move = Direction.Right; break;
            case SwipeGestureDirection.Down: input.move = Direction.Bottom; break;
            case SwipeGestureDirection.Left: input.move = Direction.Left; break;
        }
    }

    private void Init()
    {
        // Юнит
        for (int i = 0; i < timeline.NumUnits; i++)
        {
            var unit = Instantiate(prefabs.Unit, Vector3.zero, Quaternion.identity, root.transform);
            unit.GetComponent<MatchUnitSync>().Timeline = timeline;
            unit.GetComponent<MatchUnitSync>().UnitIndex = i;   
        }

        // Стены (TODO: Получать список)
        var data = CellularAutomata.Cave_1;
        var w = data.GetLength(0);
        var h = data.GetLength(1);

        for (var x = 0; x < w; x++)
        {
            for (var y = 0; y < h; y++)
            {
                if (data[x, y])
                {
                    var wall = Instantiate(prefabs.Wall, Vector3.zero, Quaternion.identity, root.transform);
                    wall.transform.localPosition = new Vector2(x - w/2, y - h/2);
                }
            }
        }
    }

    private void Update()
    {
        if (timeline != null)
        {
            var started = timeline.IsStarted;
            timeline.AdvanceTime(Time.unscaledDeltaTime);
            if (started != timeline.IsStarted) Init();

            var h = Input.GetAxisRaw("Horizontal");
            var v = Input.GetAxisRaw("Vertical");

            if (v > 0) input.move = Direction.Top;
            if (v < 0) input.move = Direction.Bottom;
            if (h > 0) input.move = Direction.Right;
            if (h < 0) input.move = Direction.Left;
            if (Input.GetKeyDown(KeyCode.Space)) input.move = Direction.None;
        }

        if (client != null)
        {
            var tmp = new UnitInput();
            tmp.move = input.move;

            if (root.transform.localRotation != Quaternion.identity)
            {
                switch (input.move)
                {
                    case Direction.Top: tmp.move = Direction.Left; break;
                    case Direction.Right: tmp.move = Direction.Top; break;
                    case Direction.Bottom: tmp.move = Direction.Right; break;
                    case Direction.Left: tmp.move = Direction.Bottom; break;
                }
            }

            if (client.IsConnected)
                client.Send(new NetcodeMessageInput() { input = tmp });            
        }
    }

    private void OnMessage(object message)
    {
        var state = (NetcodeMessageState)message;
        timeline.Add(state.state);
    }

    [Serializable]
    public class Prefabs
    {
        public GameObject Unit;
        public GameObject Wall;
    }
}