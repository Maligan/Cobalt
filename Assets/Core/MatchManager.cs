using System;
using System.IO;
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
    private Client client;

    private UnitInput input;

    public void Connect(byte[] token)
    {
        root.SetActive(true);

        timeline = new MatchTimeline();

        client = new Client();
        client.OnStateChanged += x => Log.Info(client, x.ToString());
        client.OnMessageReceived += OnMessageReceived;
        client.Connect(token, false);

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

            if (Input.GetKeyDown(KeyCode.Space)) input.move = Direction.None;
            if (Input.GetKeyDown(KeyCode.W)) input.move = Direction.Top;
            if (Input.GetKeyDown(KeyCode.S)) input.move = Direction.Bottom;
            if (Input.GetKeyDown(KeyCode.D)) input.move = Direction.Right;
            if (Input.GetKeyDown(KeyCode.A)) input.move = Direction.Left;
        }

        if (client != null)
        {
            if (client.State == ClientState.Connected)
                client.Send(input);

            client.Tick(Time.time);
        }
    }

    private void OnMessageReceived(byte[] payload, int payloadSize)
    {
        try
        {
            var state = NetcodeUtils.Read<MatchState>(payload, payloadSize);
            timeline.Add(state);
        }
        catch (Exception e)
        {
            Log.Error(this, e);
        }
    }

    [Serializable]
    public class Prefabs
    {
        public GameObject Unit;
        public GameObject Wall;
    }
}