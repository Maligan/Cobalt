using System;
using System.IO;
using Cobalt;
using Cobalt.Core;
using Cobalt.Net;
using Cobalt.Unity;
using NetcodeIO.NET;
using ProtoBuf;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    [SerializeField]
    private GameObject root;

    [SerializeField]
    private Prefab prefab;

    private GameObject unit;
    private MatchTimeline timeline;
    private Client client;

    public void Connect(byte[] token)
    {
        root.SetActive(true);

        timeline = new MatchTimeline();

        client = new Client();
        client.OnStateChanged += x => Log.Info(client, x.ToString());
        client.OnMessageReceived += OnMessageReceived;
        client.Connect(token, false);

        // Юнит
        unit = Instantiate(prefab.Unit, Vector3.zero, Quaternion.identity, root.transform);
        unit.GetComponent<TransformInterpolator>().Timeline = timeline;

        // Стены (TODO: Получать список)
        var data = MatchBuilder.Random(21, 19);
        var w = data.GetLength(0);
        var h = data.GetLength(1);

        for (var x = 0; x < w; x++)
            for (var y = 0; y < h; y++)
                if (data[x, y])
                    Instantiate(
                        prefab.Wall,
                        new Vector2(x - w/2, y - h/2) * 0.5f,
                        Quaternion.identity,
                        root.transform);
    }

    private void Update()
    {
        UnitInput input = null;

        if (Input.GetKeyDown(KeyCode.Space)) input = new UnitInput { move = Direction.None };
        if (Input.GetKeyDown(KeyCode.W)) input = new UnitInput { move = Direction.Top };
        if (Input.GetKeyDown(KeyCode.S)) input = new UnitInput { move = Direction.Bottom };
        if (Input.GetKeyDown(KeyCode.D)) input = new UnitInput { move = Direction.Right };
        if (Input.GetKeyDown(KeyCode.A)) input = new UnitInput { move = Direction.Left };

        if (client != null)
        {
            if (client.State == ClientState.Connected && input != null)
                client.Send(input);

            client.Tick(Time.time);
        }
    }

    private void OnMessageReceived(byte[] payload, int payloadSize)
    {
        try
        {
            // var rnd = new System.Random();
            // if (rnd.Next(0, 100) > 95) return;

            timeline.Add(NetcodeUtils.Read<MatchState>(payload, payloadSize));
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    [Serializable]
    public class Prefab
    {
        public GameObject Unit;
        public GameObject Wall;
    }
}