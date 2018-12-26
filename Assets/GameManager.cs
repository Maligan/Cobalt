using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cobalt.Shard;
using ProtoBuf;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public Match match = new Match();
	public MatchTimeline timeline = new MatchTimeline();
	public GameObject unit;

	public void Start()
	{
		match.tps = 120;
		unit.GetComponent<TransformInterpolator>().Timeline = timeline;
	}

	public void Update()
	{
		UpdateInput();
		UpdateServer();
		UpdateClient();
	}

	private void UpdateServer()
	{
		// Update
		var change = match.Tick(Time.deltaTime);
		if (change)
		{
			var stream = new MemoryStream();
			Serializer.Serialize(stream, match.State);
			
			stream.Position = 0;
			var state = Serializer.Deserialize<MatchState>(stream);
			timeline.Add(state);
		}
	}

	private void UpdateClient()
	{
		// Выход по опережению
		if (timeline.Count < 3) return;

		// Ускорение по отставани
		// TODO: ---

		// Нормальный просчёт
		timeline.Time += Time.deltaTime;

		while (timeline.Count > 2 && timeline.Time > timeline[1].timestamp)
            timeline.States.RemoveAt(0);


		// Update all Interpolators()
	}

	private void UpdateInput()
	{
		if (Input.GetKeyDown(KeyCode.Space))
			match.State.inputs[0].move = Unit.Rotation.None;
			
		if (Input.GetKeyDown(KeyCode.W))
			match.State.inputs[0].move = Unit.Rotation.Top;
			
		if (Input.GetKeyDown(KeyCode.S))
			match.State.inputs[0].move = Unit.Rotation.Bottom;

		if (Input.GetKeyDown(KeyCode.D))
			match.State.inputs[0].move = Unit.Rotation.Right;
			
		if (Input.GetKeyDown(KeyCode.A))
			match.State.inputs[0].move = Unit.Rotation.Left;
	}

	public void OnDrawGizmos()
	{
		var state = match.State.units[0];
		var position = new Vector2(state.x, state.y);
		Gizmos.color = Color.magenta;
		Gizmos.DrawCube(position, Vector3.one/3);
	}
}
