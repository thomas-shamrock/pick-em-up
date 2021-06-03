using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UniRx;
using EpPathFinding.cs;

public class Grid : MonoBehaviour
{
	private EndlessTerrain endlessTerrain;
	private GameObject currentStandingChunk;
	private const int centerChunkIndex = 4;
	private const int chunkSizeX = 3;
	private const int chunkSizeY = 3;
	private float nodeDiameter;
	private Vector3 gridBottomLeft;
	private BaseGrid searchGrid;

	public Node[,] grid;
	public float NodeDiameter
	{
		get { return this.nodeDiameter; }
	}
	public bool showVisualClue = true;
	public Color unwalkableColor = new Color(1f, 0.2f, 0.2f, 0.5f);

	public void Init ()
	{
		endlessTerrain = this.GetComponentInParent<EndlessTerrain>();

		endlessTerrain.terrainObjectsForGrid.TakeUntilDestroy(this).Subscribe(terrainObjects =>
		{
			if (terrainObjects != null)
			{
				NodeScanner ns = terrainObjects[0].GetComponent<NodeScanner>();
				grid = new Node[chunkSizeX * ns.gridSizeX,
								chunkSizeY * ns.gridSizeY];
				nodeDiameter = ns.NodeDiameter;
				gridBottomLeft = ns.WorldBottomLeft;

				searchGrid = new StaticGrid(grid.GetLength(0), grid.GetLength(1));
				
				for (int i = 0; i < terrainObjects.Length; i++)
				{
					NodeScanner nodeScanner = terrainObjects[i].GetComponent<NodeScanner>();
					int xStart = nodeScanner.gridSizeX * (i % chunkSizeX);
					int xEnd = xStart + nodeScanner.gridSizeX;
					int yStart = nodeScanner.gridSizeY * (i / chunkSizeY);
					int yEnd = yStart + nodeScanner.gridSizeY;

					for (int x = xStart; x < xEnd; x++)
					{
						for (int y = yStart; y < yEnd; y++)
						{
							grid[x,y] = nodeScanner.nodes[x % nodeScanner.gridSizeX, y % nodeScanner.gridSizeY];
							searchGrid.SetWalkableAt(x,y, grid[x, y].Walkable);
						}
					}
				}
			}
		});
	}

	void OnDrawGizmos()
	{
		if (showVisualClue)
		{
			if (grid != null)
			{
				foreach (Node n in grid)
				{
					if (!n.Walkable)
					{
						Gizmos.color = unwalkableColor;
						Gizmos.DrawCube(n.WorldPos, Vector3.one * nodeDiameter);
					}
				}
			}
		}
	}

	public int[] ResolveGridPos(Vector3 targetPos)
	{
		int x = Mathf.RoundToInt((targetPos.x - gridBottomLeft.x) / nodeDiameter);
		int y = Mathf.RoundToInt((targetPos.z - gridBottomLeft.z) / nodeDiameter);

		return new int[] { x, y };
	}

	public Vector2[] FindPath(Vector3 seekerPos, Vector3 targetPos)
	{
		int[] seekerGridPos = this.ResolveGridPos(seekerPos);
		int[] targetGridPos = this.ResolveGridPos(targetPos);
		GridPos startPos = new GridPos(seekerGridPos[0], seekerGridPos[1]);
		GridPos endPos = new GridPos(targetGridPos[0], targetGridPos[1]);
		JumpPointParam jpParam = new JumpPointParam(searchGrid, true, true, false);
		jpParam.Reset(startPos, endPos);
		GridPos[] gridWayPoints = JumpPointFinder.FindPath(jpParam).ToArray();

		if (gridWayPoints.Length > 0)
		{
			Vector2[] wayPoints = new Vector2[gridWayPoints.Length];
			for (int i = 0; i < wayPoints.Length; i++)
			{
				int x = gridWayPoints[i].x;
				int y = gridWayPoints[i].y;
				Node wayPointNode = grid[x, y];
				wayPoints[i] = new Vector2(wayPointNode.WorldPos.x, wayPointNode.WorldPos.z);
			}
			return wayPoints;
		} else
		{
			Debug.Log("No path found");
			return null;
		}

	}

	public Vector2[] GetWalkablePos(Vector3 targetPos, int range)
	{
		List<Node> walkableNodes = new List<Node>();
		int[] targetGridPos = this.ResolveGridPos(targetPos);
		for (int x = targetGridPos[0] - range; x < targetGridPos[0] + range + 1; x++)
		{
			for (int y = targetGridPos[1] - range; y < targetGridPos[1] + range + 1; y++)
			{
				if (x > grid.GetLength(0) - 1 || y > grid.GetLength(1) - 1)
				{
					continue;
				}
				if (x < 0 || y < 0)
				{
					continue;
				}
				if (x == targetGridPos[0] && y == targetGridPos[1])
				{
					continue;
				}
				Node node = grid[x, y];
				if (node != null && node.Walkable)
					walkableNodes.Add(node);
			}
		}
		Vector2[] walkablePos = new Vector2[walkableNodes.Count];
		for (int i = 0; i < walkablePos.Length; i++)
		{
			walkablePos[i] = new Vector2(walkableNodes[i].WorldPos.x, walkableNodes[i].WorldPos.z);
		}

		return walkablePos;
	}
}
