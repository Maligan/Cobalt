using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cobalt.Core;
using Cobalt.Core.Net;
using Cobalt.UI;
using NetcodeIO.NET;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Networking;

public class App : MonoBehaviour
{
	public static App Instance { get; private set; }
	public static UIManager UI { get { return Instance.GetComponent<UIManager>(); } }
	public static MatchManager MatchManager { get { return Instance.GetComponent<MatchManager>(); } }
	public static ShardService ShardService { get; private set; }

	public App()
	{
		Instance = this;
		ShardService = new ShardService();
	}

	public void Start()
	{
		DoMenu();
	}

	public void Update()
	{
		ShardService.Update(Time.deltaTime);
	}

	public static void DoConnect(byte[] token)
	{
		App.UI.Get<MenuPanel>().Require(Instance, 0);
		App.MatchManager.Connect(token);
		App.Instance.transform.Find("Match").gameObject.SetActive(true);

	}

	public static void DoMenu()
	{
		App.UI.Get<MenuPanel>().Require(Instance, +1);
		App.Instance.transform.Find("Match").gameObject.SetActive(false);
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
