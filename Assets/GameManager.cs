using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Server;
using UnityEngine;

public class StageView : MonoBehaviour
{
	public GameObject Prefab;
	public GameObject Root;
	public Dictionary<int, GameObject> Objects = new Dictionary<int, GameObject>();

	public void Apply(Data[] datas)
	{
		foreach (var data in datas)
		{
			if (Objects.ContainsKey(data.ID) == false)
				Objects[data.ID] = Instantiate(Prefab);

			var obj = Objects[data.ID];
			obj.transform.localPosition = new Vector2(data.X, data.Y);
		}
	}
}

public class Channel
{
	public event Action<Message> OnMessage;

	public void Send(Message message)
	{
		if (OnMessage != null)
			OnMessage(message);
	}
}

public class Message
{
	public string Type;
	public Data[] Data;
	public Vector2Int Dir;
}












public class GameManager : MonoBehaviour
{



	public static GameManager Current { get; private set; }

	// Server
	public Stage Stage;
	public int time;

	// Server-Client
	public Channel Channel;

	// Client #1
	public StageView StageView;
	public GameObject StageViewPrefab;


	private void Start()
	{
		return;

		Stage = new Stage();
		Stage.Objects.Add(new StageUnit(Stage));
		Stage.Objects[0].Data.ID = 1;

		// Stage.Objects.Add(new StageUnit(Stage));
		// Stage.Objects[1].Data.ID = 2;
		// Stage.Objects[1].Data.Y  = 2;

		StageView = new StageView();
		StageView.Root = new GameObject();
		StageView.Prefab = StageViewPrefab;

		Channel = new Channel();
		Channel.OnMessage += OnMessage;
	}

	private void FixedUpdate()
	{
		return;
		
		// Server
		var ms = (int)(Time.fixedDeltaTime * 1000);
		time += ms;

		if (time > 300)
		{
			time %= 300;
			Stage.Update(300);
			Channel.Send(new Message() { Type = "Update", Data = Stage.Objects.Select(
				x => JsonUtility.FromJson<Data>(JsonUtility.ToJson(x.Data))
			).ToArray() });
		}
	}

	private void OnMessage(Message message)
	{
		switch (message.Type)
		{
			case "Update":
				StageView.Apply(message.Data);
				break;
			case "Move":
				Stage.Objects[0].Data.DX = message.Dir.x;
				Stage.Objects[0].Data.DY = message.Dir.y;
				break;
			default:
				Debug.Log("Unknown message type: " + message.Type);
				break;
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.UpArrow)) Channel.Send(new Message() { Type = "Move", Dir = Vector2Int.up });
		if (Input.GetKeyDown(KeyCode.RightArrow)) Channel.Send(new Message() { Type = "Move", Dir = Vector2Int.right });
		if (Input.GetKeyDown(KeyCode.DownArrow)) Channel.Send(new Message() { Type = "Move", Dir = Vector2Int.down });
		if (Input.GetKeyDown(KeyCode.LeftArrow)) Channel.Send(new Message() { Type = "Move", Dir = Vector2Int.left });
		if (Input.GetKeyDown(KeyCode.Space)) Channel.Send(new Message() { Type = "Move", Dir = Vector2Int.zero });
	}













	/*

	// Prefabs
	public List<GameObject> Prefabs;
	public Arena Arena;

	public GameObject ArenaRoot;
	public GameObject UIRoot;

	private void Awake()
	{
		Current = this;
	}

	private void Start()
	{
		Arena = new Arena(Prefabs, ArenaRoot.transform);
		OnPlayClick();
	}

	private void CreateArena()
	{
		CreateLevel();
		StartCoroutine(InputCoroutine());
	}



	private IEnumerator InputCoroutine()
	{
		var unitCount = Arena.Objects.Count(x => x.Type == ArenaObjectType.Unit);

		while (true)
		{
			var units = Arena.Objects.Where(x => x.Type == ArenaObjectType.Unit);
			var exit = Arena.Objects.FirstOrDefault(x => x.Type == ArenaObjectType.Exit);
			var win = unitCount != units.Count();

			if (exit != null)
			{
				foreach (var unit in units)
				{
					if (Vector2.Distance(unit.transform.position, exit.transform.position) < 1/3f)
					{
						win = true;
						break;
					}
				}
			}
			

			if (win)
			{
				Arena.Clear();
				UIRoot.SetActive(true);
				StopAllCoroutines();
			}

			yield return new WaitForSeconds(0.1f);
		}
	}

	public void CreateLevel()
	{
		var hw = 20;
		var hh = 10;

		var result = ArenaGenerator.Create(hw*2, hh*2);
		result[0, 0] = false;
		result[hw*2-1, hh*2-1] = false;

		for (var x = 0; x < hw*2; x++)
			for (var y = 0; y < hh*2; y++)
				if (result[x, y])
						Arena.Pool.CreateWall(Arena.GetCell(x-hw, y-hh))
						.name = string.Format("Wall ({0}; {1})", x, y);

		var poses = FindObjectsOfType<Wall>().Where(x => x.WallType == WallType.Box).Select(x => x.Position).ToList();
		var pos = poses[Random.Range(0, poses.Count)];
		Arena.Pool.CreateExit(pos);

		CreateUnit(-hw, -hh, false);
		CreateUnit(+hw, +hh, true);
	}

	public void CreateUnit(int x, int y, bool ai)
	{
		var unit = Arena.Pool.CreateUnit(Arena.GetCell(x, y));

		if (ai) unit.GetComponent<UnitAI>().enabled = true;
        else unit.GetComponent<UnitUserInput>().enabled = true;
	}

	#region Handlers 

	public void OnPlayClick()
	{
		UIRoot.SetActive(false);
		CreateArena();
	}

	#endregion

	 */
}
