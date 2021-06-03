using UnityEngine;
using System.Collections;
using UniRx;

public class EndlessTerrainTest : MonoBehaviour {

	public Transform viewer;
	public Transform[] seekers;
	public EndlessTerrain endlessTerrain;
	//public Grid grid;

	//private Vector2[][] paths;
	//private Vector2[] walkablePos;

	private float raycastHeight = 20f;

	public GameObject[] itemsToSpawn;
	public float spawnDistance = 10f;
	public float spawnHeight = 5f;

	// Use this for initialization
	void Start () {

		var seed = UnityEngine.Random.Range (int.MinValue, int.MaxValue);
		endlessTerrain.Init(seed, () => {
			Debug.Log("Init completed");
			Vector3 pos = new Vector3(viewer.position.x, viewer.position.y, viewer.position.z);
			do {
				Ray ray = new Ray(pos, transform.up * -1.0f);
				RaycastHit hitInfo;

				if (Physics.Raycast(ray, out hitInfo, 100.0f)) {
					Debug.Log("Player has a place to land!");
					viewer.gameObject.SetActive(true);
					break;
				} else {
					Debug.Log("Player need to land on another place...");
					pos = pos + new Vector3(Random.Range(1.0f, 5.0f), 0f, Random.Range(1.0f, 5.0f));
					viewer.position = pos;

				}
			} while (true);

			int i = 0;
			do {
				GameObject item = itemsToSpawn [i];
				Vector3 rayOrg = new Vector3 (Random.Range (viewer.position.x - spawnDistance, viewer.position.x + spawnDistance), raycastHeight, Random.Range (viewer.position.z - spawnDistance, viewer.position.z + spawnDistance));
				Ray ray = new Ray (rayOrg, transform.up * -1.0f);
				RaycastHit hitInfo;

				if (Physics.Raycast (ray, out hitInfo, 100.0f)) {
					if (hitInfo.point.y > 3.2f) {
						continue;
					}
					Vector3 spawnPos = hitInfo.point + transform.up * spawnHeight;
					Instantiate (item, spawnPos, Random.rotation);
					Debug.Log (item.name + " spawned at " + spawnPos);
					i++;
				}
			} while (i < itemsToSpawn.Length);
		});
		endlessTerrain.viewer = viewer;

		//paths = new Vector2[seekers.Length][];
	}

	void Update()
	{
		/*if (grid.grid != null)
		{
			for (int i = 0; i < seekers.Length; i++)
			{
				paths[i] = grid.FindPath(seekers[i].position, viewer.position);
			}

			walkablePos = grid.GetWalkablePos(seekers[1].position, 3);
		}*/
	}

	/*void OnDrawGizmos()
	{
		Vector3 terrainSize = Vector3.zero;

		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(viewer.position, endlessTerrain.detailLevels[0].visibleDstThreshold);

		for (int i = 0; i < endlessTerrain.transform.childCount; i++)
		{
			Transform child = endlessTerrain.transform.GetChild(i);
			if (child.GetComponent<MeshFilter>() != null)
			{
				terrainSize = child.GetComponent<MeshFilter>().mesh.bounds.size;
				Gizmos.DrawWireCube(child.position, terrainSize);
			}
		}

		if (grid.grid != null)
		{
			int[] viewerCoord = grid.ResolveGridPos(viewer.position);
			int viewerX = viewerCoord[0];
			int viewerY = viewerCoord[1];
			Node viewerNode = grid.grid[viewerX, viewerY];
			Gizmos.color = Color.blue;
			Gizmos.DrawCube(viewerNode.WorldPos, Vector3.one * grid.NodeDiameter);

			for (int i = 0; i < seekers.Length; i++)
			{
				int[] seekerCoord = grid.ResolveGridPos(seekers[i].position);
				int seekerX = seekerCoord[0];
				int seekerY = seekerCoord[1];
				Node seekerNode = grid.grid[seekerX, seekerY];
				Gizmos.color = Color.yellow;
				Gizmos.DrawCube(seekerNode.WorldPos, Vector3.one * grid.NodeDiameter);

				if (paths[i] != null)
				{
					for (int j = 0; j < paths[i].Length-1; j++)
					{
						Vector3 startPoint = new Vector3(paths[i][j].x, 20, paths[i][j].y);
						Vector3 endPoint = new Vector3(paths[i][j + 1].x, 20, paths[i][j + 1].y);
						Gizmos.color = Color.black;
						Gizmos.DrawLine(startPoint, endPoint);
					}
				}
			}

			if (walkablePos != null)
			{
				Gizmos.color = new Color(1f, 0f, 1f, 0.5f);
				for (int i = 0; i < walkablePos.Length; i++)
				{
					Gizmos.DrawCube(new Vector3(walkablePos[i].x, viewerNode.WorldPos.y, walkablePos[i].y), Vector3.one * grid.NodeDiameter);
				}
			}
		}
	}*/
}
