using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cobalt.Core;
using NetcodeIO.NET;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Networking;

public class App : MonoBehaviour
{
	public static App Instance { get; private set; }
	public static MatchManager MatchManager { get { return Instance.GetComponent<MatchManager>(); } }
	public App() { Instance = this; }

	public void Start()
	{
		// byte[] token = null;

		// /*
		// shard = new Shard(new Shard.Options());
		// shard.Start();
		// token = new Shard(new Shard.Options()).GetToken();
 		// /*/
		// var www = UnityWebRequest.Get("localhost:8080/auth");
		// yield return www.SendWebRequest(); 
		// if (www.isNetworkError || www.isHttpError) yield break;
		// var key = www.downloadHandler.text;
		// token = Convert.FromBase64String(key);
		// //*/

		// match.Connect(token);
		// yield break;
	}

    public void Update()
	{
		// if (shard != null)
		// {
		// 	shard.Tick(Time.deltaTime);
		// 	UpdateInput();
		// }
	}

	private void UpdateInput()
	{
		// if (Input.GetKeyDown(KeyCode.Space))
		// 	shard.match.State.inputs[0].move = Unit.Rotation.None;
			
		// if (Input.GetKeyDown(KeyCode.W))
		// 	shard.match.State.inputs[0].move = Unit.Rotation.Top;
			
		// if (Input.GetKeyDown(KeyCode.S))
		// 	shard.match.State.inputs[0].move = Unit.Rotation.Bottom;

		// if (Input.GetKeyDown(KeyCode.D))
		// 	shard.match.State.inputs[0].move = Unit.Rotation.Right;
			
		// if (Input.GetKeyDown(KeyCode.A))
		// 	shard.match.State.inputs[0].move = Unit.Rotation.Left;
	}
}
