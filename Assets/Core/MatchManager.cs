using System;
using Cobalt;
using Cobalt.Ecs;
using Cobalt.Net;
using Cobalt.Unity;
using NetcodeIO.NET;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    [SerializeField]
    private GameObject root;

    [SerializeField]
    private Prefabs prefabs;

    private MatchTimeline timeline;
    private NetcodeClient client;

    private UnitInput input;

    public void Connect(byte[] token)
    {
        root.SetActive(true);

        timeline = new MatchTimeline();

        client = new NetcodeClient(token);
        client.OnMessage += OnMessage;
        client.Connect();

        input = new UnitInput() { move = Direction.None };
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
            for (var y = 0; y < h; y++)
                if (data[x, y])
                    Instantiate(prefabs.Wall, new Vector2(x - w/2, y - h/2) * 0.5f, Quaternion.identity, root.transform);
    }

    private void Update()
    {
        if (timeline != null)
        {
            var started = timeline.IsStarted;
            timeline.AdvanceTime(Time.unscaledDeltaTime);
            // timeline.AdvanceTime(Time.deltaTime);
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
            if (client.IsConnected)
                client.Send(new NetcodeMessageInput() { input = input });
            
            client.Update(Time.time);
        }
    }

    private void OnMessage(NetcodeMessage message)
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