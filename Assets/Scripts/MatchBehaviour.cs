using System;
using System.IO;
using Cobalt.Core;
using NetcodeIO.NET;
using ProtoBuf;
using UnityEngine;

public class MatchBehaviour : MonoBehaviour
{
    [SerializeField]
    private GameObject unit;

    private MatchTimeline timeline;
    private Client client;

    public void Connect(byte[] token)
    {
        timeline = new MatchTimeline();

        client = new Client();
		client.OnMessageReceived += OnMessageReceived;
		client.Connect(token, false);

		unit.GetComponent<TransformInterpolator>().Timeline = timeline;
    }

	private void Update()
	{
		if (client != null)
			client.Tick(Time.time);
	}

    private void OnMessageReceived(byte[] payload, int payloadSize)
    {
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
}