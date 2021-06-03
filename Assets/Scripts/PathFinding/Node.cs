using UnityEngine;
using System.Collections;

public class Node
{
	private bool walkable;
	private Vector3 worldPos;

	public bool Walkable
	{
		get { return this.walkable; }
	}
	public Vector3 WorldPos
	{
		get { return this.worldPos; }
	}

	public Node(bool walkable, Vector3 worldPos)
	{
		this.walkable = walkable;
		this.worldPos = worldPos;
	}
}
