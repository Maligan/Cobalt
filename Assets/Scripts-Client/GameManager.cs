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
	public MatchBehaviour match;

	public void Start()
	{
		shard = new Shard(new Shard.Options());
		shard.Start();

		match.Connect(new Shard(new Shard.Options()).GetToken());
	}

    public void Update()
	{
		if (shard != null)
		{
			shard.Tick(Time.deltaTime);
			UpdateInput();
		}
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
