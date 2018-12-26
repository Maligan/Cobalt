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
		client.Connect(shard.GetToken());
		client.Tickrate = 30;

		unit.GetComponent<TransformInterpolator>().Timeline = timeline;
	}

    private void OnMessageReceived(byte[] payload, int payloadSize)
    {
		client.Send(new byte[1] { 255 }, 1);

		try
		{
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

		UpdateInput();
		// UpdateClient();
	}

	private void UpdateClient()
	{
		// // Выход по опережению
		// if (timeline.Count < 3) return;

		// // Ускорение по отставани
		// // TODO: ---

		// // Нормальный просчёт
		// timeline.Time += Time.deltaTime;

		// while (timeline.Count > 2 && timeline.Time > timeline[1].timestamp)
        //     timeline.States.RemoveAt(0);

		// Update all Interpolators()
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
