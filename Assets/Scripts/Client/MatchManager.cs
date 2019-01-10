using System;
using System.IO;
using Cobalt.Core;
using NetcodeIO.NET;
using ProtoBuf;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    [SerializeField]
    private GameObject unit;

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