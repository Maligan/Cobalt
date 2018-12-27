using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cobalt.Core;
using NetcodeIO.NET;
using ProtoBuf;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public Shard shard;

	public Client client;
	public MatchTimeline timeline = new MatchTimeline();
	public GameObject unit;

	public void Start()
	{
		shard = new Shard(new Shard.Options());
		shard.Start();

		client = new Client();
		client.OnStateChanged += x => Debug.Log(x);
		client.OnMessageReceived += OnMessageReceived;
		client.Connect(shard.GetToken(), false);

		unit.GetComponent<TransformInterpolator>().Timeline = timeline;
	}

    private void OnMessageReceived(byte[] payload, int payloadSize)
    {
		client.Send(new byte[1] { 255 }, 1);

		try
		{
			var rnd = new System.Random();
			if (rnd.Next(0, 100) > 95) return;

			var stream = new MemoryStream(payload, 0, payloadSize);
			var state = Serializer.Deserialize<MatchState>(stream);
			timeline.Add(state);
		}
		catch (Exception e)
		{
			Debug.LogError(e);
		}
    }

    public void Update()
	{
		shard.Tick(Time.deltaTime);

		client.Tick(Time.time);
		UpdateInput();
	}

	private void UpdateInput()
	{
		if (Input.GetKeyDown(KeyCode.Space))
			shard.match.State.inputs[0].move = Unit.Rotation.None;
			
		if (Input.GetKeyDown(KeyCode.W))
			shard.match.State.inputs[0].move = Unit.Rotation.Top;
			
		if (Input.GetKeyDown(KeyCode.S))
			shard.match.State.inputs[0].move = Unit.Rotation.Bottom;

		if (Input.GetKeyDown(KeyCode.D))
			shard.match.State.inputs[0].move = Unit.Rotation.Right;
			
		if (Input.GetKeyDown(KeyCode.A))
			shard.match.State.inputs[0].move = Unit.Rotation.Left;
	}
}
