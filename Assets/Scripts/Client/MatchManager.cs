using System;
using System.IO;
using Cobalt.Core;
using NetcodeIO.NET;
using ProtoBuf;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    [SerializeField] private GameObject unit;
    [SerializeField] private GameObject wall;

    private MatchTimeline timeline;
    private Client client;

    public void Connect(byte[] token)
    {
        timeline = new MatchTimeline();

        client = new Client();
		client.OnStateChanged += x => Debug.Log("[Client] " + x); 
		client.OnMessageReceived += OnMessageReceived;
		client.Connect(token, false);

		unit.GetComponent<TransformInterpolator>().Timeline = timeline;

		var data = MatchBuilder.Random(21, 19);

		var w = data.GetLength(0);
		var h = data.GetLength(1);
		for (var x = 0; x < w; x++)
		{
			for (var y = 0; y < h; y++)
			{
				if (data[x, y])
				{
					var t = Instantiate(wall, unit.transform.parent);
					t.transform.localPosition = new Vector2(x-w/2, y-h/2);
				}
			}
		}

    }

	private void Update()
	{
		UnitInput input = null;

		if (Input.GetKeyDown(KeyCode.Space)) input = new UnitInput { move = Unit.Direction.None };
		if (Input.GetKeyDown(KeyCode.W)) input = new UnitInput { move = Unit.Direction.Top };
		if (Input.GetKeyDown(KeyCode.S)) input = new UnitInput { move = Unit.Direction.Bottom };
		if (Input.GetKeyDown(KeyCode.D)) input = new UnitInput { move = Unit.Direction.Right };
		if (Input.GetKeyDown(KeyCode.A)) input = new UnitInput { move = Unit.Direction.Left };

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
}