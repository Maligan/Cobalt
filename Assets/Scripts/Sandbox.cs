using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sandbox : MonoBehaviour
{
	public void Start()
	{
		var board = new Board();
		var bullet = board.Produce<Bullet>();

		board.Put(bullet, new Board.Cell());
	}

	private void LoopServer()
	{
		// Apply controll messages
		// Update
		// Send state messages
	}

	private void LoopClient()
	{
		// Apply state messages
		// Update
		// Send controll messages
	}
}

public class Board : Board.IFactory
{
	public IFactory Factory { get; set; }

	public void Put(IBob bob, Cell cell) { }
	public T Get<T>(Cell cell) { return default(T); }
	
	public T Produce<T>() where T : IBob { return Factory.Produce<T>(); }
	public void Recycle(IBob bob) { Factory.Recycle(bob); }

	public struct Cell
	{
		public short x;
		public short y;

		public Cell Up { get { return new Board.Cell { x = x, y = (short)(y+1) }; } }
		public Cell Down { get { return new Board.Cell { x = x, y = (short)(y-1) }; } }
		public Cell Right { get { return new Board.Cell { x = (short)(x+1), y = y }; } }
		public Cell Left { get { return new Board.Cell { x = (short)(x-1), y = y }; } }
	}

	public interface IFactory
	{
		T Produce<T>() where T : IBob;
		void Recycle(IBob bob);
	}

	public interface IBob
	{
		// void OnProduce();
		// void OnRecycle();

		Board Board { get; }
		Board.Cell Cell { get; }

		void Update(int ms);
		void Affect(IAffect args);
	}

    // Маркерный класс
	public interface IAffect
	{

	}
}













public class Bullet : Board.IBob, Board.IAffect
{
	public Board Board { get; }
    public Board.Cell Cell { get; }

    public void Update(int ms)
    {
		var bob = Board.Get<Board.IBob>(Cell.Up);
	
		if (bob != null) bob.Affect(this);
		else Board.Put(this, Cell.Up);
    }
	
    public void Affect(Board.IAffect args)
    {
		// Is unaffectable right now
    }
}

public class Enemy : Board.IBob, Board.IAffect
{
    public Board Board { get; set; }
    public Board.Cell Cell { get; }

    public void Update(int ms)
	{
		Board.Put(this, Cell.Down);
	}

    public void Affect(Board.IAffect args)
	{
		if (args is Bullet)
		{
			Board.Recycle(this);
		}
	}
}