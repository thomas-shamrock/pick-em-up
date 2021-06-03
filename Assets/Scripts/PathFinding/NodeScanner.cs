using UnityEngine;
using System.Collections;
using UniRx;

public class NodeScanner : MonoBehaviour
{
	//public Transform terrain;
	public IntReactiveProperty currentLOD = new IntReactiveProperty();
	public FloatReactiveProperty gridHeight = new FloatReactiveProperty();
	public int gridSizeX = 30;
	public int gridSizeY = 30;

	public Node[,] nodes;
	public float NodeDiameter
	{
		get { return this.nodeDiameter; }
	}

	public Vector3 WorldBottomLeft
	{
		get { return this.worldBottomLeft; }
	}

	private int lowestLOD = -1;
	private float nodeDiameter;
	private float nodeRadius;
	private Vector2 gridWorldSize;
	private Vector3 gridPos;
	private Vector3 worldBottomLeft;


	// Use this for initialization
	void Start ()
	{
		lowestLOD = currentLOD.Value;

		Mesh mesh = this.GetComponent<MeshFilter>().mesh;
		gridWorldSize = new Vector2(mesh.bounds.size.x, mesh.bounds.size.z);
		nodeDiameter = gridWorldSize.x / gridSizeX; 
		nodeRadius = nodeDiameter / 2;

		this.currentLOD.TakeUntilDestroy(this).Subscribe(RefreshNodes);
		this.gridHeight.TakeUntilDestroy(this).Subscribe(CreateNodes);
	}


	private void RefreshNodes(int _currentLOD)
	{
		if (_currentLOD < lowestLOD) //detail increases
		{
			lowestLOD = _currentLOD;
			CreateNodes(this.gridHeight.Value);
		}
	}

	private void CreateNodes(float _gridHeight) {
		nodes = new Node[gridSizeX,gridSizeY];
		gridPos = this.transform.position + Vector3.up * _gridHeight;
		worldBottomLeft = gridPos - Vector3.right * gridWorldSize.x/2 - Vector3.forward * gridWorldSize.y/2;

		for (int x = 0; x < gridSizeX; x ++) {
			for (int y = 0; y < gridSizeY; y ++) {
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
				bool walkable = !(Physics.CheckSphere(worldPoint,nodeRadius));
				nodes[x,y] = new Node(walkable,worldPoint);
			}
		}
	}


}