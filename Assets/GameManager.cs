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
	private Shard shard = new Shard();
	private float shardStep;

	private List<ShardState> clientStates = new List<ShardState>();
	private float clientTime;
	private Vector2 clientPosition;

	public GameObject unit;


	public void Update()
	{
		UpdateServer();
		UpdateClient();

		unit.transform.localPosition = clientPosition;
	}

	private void UpdateServer()
	{
		// Update
		shard.Tick(Time.deltaTime);

		// Send
		shardStep -= Time.deltaTime;
		if (shardStep > 0) return;
		shardStep = 1f;

		var stream = new MemoryStream();
		Serializer.Serialize(stream, shard.State);
		
		// --------------------------

		stream.Position = 0;
		var state = Serializer.Deserialize<ShardState>(stream);
		clientStates.Add(state);
	}

	private void UpdateClient()
	{
		if (clientStates.Count < 2) return;

		clientTime += Time.deltaTime;

		while (clientTime > clientStates[1].timestamp)
            clientStates.RemoveAt(0);

		var curr = new Vector2(clientStates[0].units[0].x, clientStates[0].units[0].y);
        var next = new Vector2(clientStates[1].units[0].x, clientStates[1].units[0].y);

		var delta = clientTime - clientStates[0].timestamp;
        var total = clientStates[1].timestamp - clientStates[0].timestamp;
        
        var t = delta / total; // [0; 1)

        clientPosition = Vector2.LerpUnclamped(curr, next, t);
	}

	public void OnDrawGizmos()
	{
		Gizmos.DrawCube(clientPosition, Vector3.one);
	}
}
