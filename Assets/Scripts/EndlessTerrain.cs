using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using System;

public class EndlessTerrain : MonoBehaviour {

	const float scale = 1f;

	const float viewerMoveThresholdForChunkUpdate = 25f;
	const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

	public LODInfo[] detailLevels;
	public static float maxViewDst;

	public Transform viewer;
	public Material mapMaterial;

	public static Vector2 viewerPosition;
	Vector2 viewerPositionOld;
	static MapGenerator mapGenerator;
	int chunkSize;
	int chunksVisibleInViewDst;
	private int initVisibleChunkCount;
	static IntReactiveProperty readyChunkCount = new IntReactiveProperty(0);

	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();


	static ReactiveProperty<Vector2> currentStandingTerrainChunkCoord = new ReactiveProperty<Vector2>(new Vector2(float.MinValue, float.MinValue));
	private const int TerrainObjectsForGridSize = 9;
	public ReactiveProperty<GameObject[]> terrainObjectsForGrid = new ReactiveProperty<GameObject[]>();

	public void Init(int seed, System.Action OnInitialized = null) {
		terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk> ();
		this.viewer = viewer;
		mapGenerator = FindObjectOfType<MapGenerator> ();
		mapGenerator.seed = seed;

		maxViewDst = detailLevels [detailLevels.Length - 1].visibleDstThreshold;
		chunkSize = MapGenerator.mapChunkSize - 1;
		chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);

		UpdateVisibleChunks();

		currentStandingTerrainChunkCoord.TakeUntilDestroy(this).Subscribe(UpdateTerrainObjectsForGrid);
		this.transform.Find("Grid").GetComponent<Grid>().Init();

		initVisibleChunkCount = Mathf.RoundToInt(Mathf.Pow(maxViewDst / chunkSize * 2, 2));
		 
		readyChunkCount.TakeWhile(_ => readyChunkCount.Value <= initVisibleChunkCount).Subscribe(count =>
		{
			if (count == initVisibleChunkCount)
			{
				if (OnInitialized != null)
					OnInitialized();
			}
		});
	}

	void Update() {
		if (viewer != null)
		{
			viewerPosition = new Vector2 (viewer.position.x, viewer.position.z) / scale;

			if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate) {
				viewerPositionOld = viewerPosition;
				UpdateVisibleChunks ();
			}	
		}
	}

	void OnDestroy()
	{
//		foreach (var item in terrainChunkDictionary)
//		{
//			var chunk = item.Value;
//			Destroy (item.Value);
//		}
		Debug.Log("Endless terrain destroyed");
		terrainChunkDictionary = null;
		terrainChunksVisibleLastUpdate.Clear ();
	}

	private void UpdateVisibleChunks() {

		for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) {
			terrainChunksVisibleLastUpdate [i].SetVisible (false);
		}
		terrainChunksVisibleLastUpdate.Clear ();

		int currentChunkCoordX = Mathf.RoundToInt (viewerPosition.x / chunkSize);
		int currentChunkCoordY = Mathf.RoundToInt (viewerPosition.y / chunkSize);
		Vector2 currentChunkCoord = new Vector2(currentChunkCoordX, currentChunkCoordY);

		for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++) {
			for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++) {
				Vector2 viewedChunkCoord = new Vector2 (currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

				if (terrainChunkDictionary.ContainsKey (viewedChunkCoord)) {
					terrainChunkDictionary[viewedChunkCoord].centerCoord = currentChunkCoord;
					terrainChunkDictionary [viewedChunkCoord].UpdateTerrainChunk ();
				} else {
					terrainChunkDictionary.Add (viewedChunkCoord, new TerrainChunk(viewedChunkCoord, currentChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
				}

			}
		}
	}

	private void UpdateTerrainObjectsForGrid(Vector2 centerCoord)
	{
		if (centerCoord == new Vector2(float.MinValue, float.MinValue))
			return;

		GameObject[] terrainObjects = new GameObject[TerrainObjectsForGridSize];

		if (terrainChunkDictionary.ContainsKey(centerCoord + Vector2.down + Vector2.left))
			terrainObjects[0] = terrainChunkDictionary[centerCoord + Vector2.down + Vector2.left].meshObject;
		if (terrainChunkDictionary.ContainsKey(centerCoord + Vector2.down))
			terrainObjects[1] = terrainChunkDictionary[centerCoord + Vector2.down].meshObject;
		if (terrainChunkDictionary.ContainsKey(centerCoord + Vector2.down + Vector2.right))
			terrainObjects[2] = terrainChunkDictionary[centerCoord + Vector2.down + Vector2.right].meshObject;
		if (terrainChunkDictionary.ContainsKey(centerCoord + Vector2.left))
			terrainObjects[3] = terrainChunkDictionary[centerCoord + Vector2.left].meshObject;
		if (terrainChunkDictionary.ContainsKey(centerCoord))
			terrainObjects[4] = terrainChunkDictionary[centerCoord].meshObject;
		if (terrainChunkDictionary.ContainsKey(centerCoord + Vector2.right))
			terrainObjects[5] = terrainChunkDictionary[centerCoord + Vector2.right].meshObject;
		if (terrainChunkDictionary.ContainsKey(centerCoord + Vector2.up + Vector2.left))
			terrainObjects[6] = terrainChunkDictionary[centerCoord + Vector2.up + Vector2.left].meshObject;
		if (terrainChunkDictionary.ContainsKey(centerCoord + Vector2.up))
			terrainObjects[7] = terrainChunkDictionary[centerCoord + Vector2.up].meshObject;
		if (terrainChunkDictionary.ContainsKey(centerCoord + Vector2.up + Vector2.right))
			terrainObjects[8] = terrainChunkDictionary[centerCoord + Vector2.up + Vector2.right].meshObject;

		bool allNodesReady = true;
		for (int i = 0; i < TerrainObjectsForGridSize; i++)
		{
			if (terrainObjects[i] == null || terrainObjects[i].GetComponent<NodeScanner>() == null)
			{
				allNodesReady = false;
				break;
			}
		}

		if (allNodesReady)
		{
			terrainObjectsForGrid.Value = terrainObjects;
		} else
		{
			Observable.Timer(TimeSpan.FromMilliseconds(200)).Subscribe(_ =>
			{
				UpdateTerrainObjectsForGrid(centerCoord);
			});
		}
	}

	public class TerrainChunk
	{
		public GameObject meshObject;
		public Vector2 centerCoord;
		Vector2 position;
		Bounds bounds;

		MeshRenderer meshRenderer;
		MeshFilter meshFilter;

		LODInfo[] detailLevels;
		LODMesh[] lodMeshes;

		MapData mapData;
		bool mapDataReceived;
		int previousLODIndex = -1;

		public TerrainChunk(Vector2 coord, Vector2 centerCoord, int size, LODInfo[] detailLevels, Transform parent, Material material) {
			this.centerCoord = centerCoord;
			this.detailLevels = detailLevels;

			position = coord * size;
			bounds = new Bounds(position,Vector2.one * size);
			Vector3 positionV3 = new Vector3(position.x,0,position.y);

			meshObject = new GameObject("Terrain Chunk " + coord);
			meshRenderer = meshObject.AddComponent<MeshRenderer>();
			meshFilter = meshObject.AddComponent<MeshFilter>();
			meshRenderer.material = material;
			meshObject.transform.position = positionV3 * scale;
			meshObject.transform.parent = parent;
			meshObject.transform.localScale = Vector3.one * scale;

			meshObject.layer = LayerMask.NameToLayer("Terrain");
			SetVisible(false);

			lodMeshes = new LODMesh[detailLevels.Length];
			for (int i = 0; i < detailLevels.Length; i++) {
				lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
			}

			mapGenerator.RequestMapData(position,OnMapDataReceived);
		}

		void OnMapDataReceived(MapData mapData) {
			this.mapData = mapData;
			mapDataReceived = true;

			if (mapGenerator.drawMode == MapGenerator.DrawMode.ColorMesh || mapGenerator.drawMode == MapGenerator.DrawMode.ColourMap)
			{
				Texture2D texture = TextureGenerator.TextureFromColourMap(mapData.colourMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
				meshRenderer.material.mainTexture = texture;
			}
			UpdateTerrainChunk ();
		}

		public void UpdateTerrainChunk()
		{
			if (mapDataReceived) {
				float viewerDstFromNearestEdge = Mathf.Sqrt (bounds.SqrDistance (viewerPosition));
				bool visible = viewerDstFromNearestEdge <= maxViewDst;

				if (visible) {
					int lodIndex = 0;
					for (int i = 0; i < detailLevels.Length - 1; i++) {
						if (viewerDstFromNearestEdge > detailLevels [i].visibleDstThreshold) {
							lodIndex = i + 1;
						} else {
							break;
						}
					}

					if (lodIndex != previousLODIndex) {
						LODMesh lodMesh = lodMeshes [lodIndex];
						if (lodMesh.hasMesh) {
							previousLODIndex = lodIndex;
							meshFilter.mesh = lodMesh.mesh;
							meshFilter.mesh.RecalculateBounds();
						} else if (!lodMesh.hasRequestedMesh) {
							lodMesh.RequestMesh (mapData);
						}
					}

					terrainChunksVisibleLastUpdate.Add (this);

					if ((lodIndex == 0 || lodIndex == 1) && lodMeshes[lodIndex].hasMesh)
					{
						MeshCollider meshCollider = meshObject.GetComponent<MeshCollider>();
						if (meshCollider == null)
						{
							meshCollider = meshObject.AddComponent<MeshCollider>();
						}
						meshCollider.sharedMesh = meshFilter.mesh;

						NodeScanner nodeScanner = meshObject.GetComponent<NodeScanner>();
						if (nodeScanner == null)
						{
							nodeScanner = meshObject.AddComponent<NodeScanner>();
						}
						nodeScanner.currentLOD.Value = detailLevels[lodIndex].lod;
						nodeScanner.gridHeight.Value = 10f;
					}

					if ((lodIndex == 0) && lodMeshes[lodIndex].hasMesh)
					{
						EndlessTerrain.currentStandingTerrainChunkCoord.Value = centerCoord;
					}
			}
				SetVisible (visible);
			}
		}

		public void SetVisible(bool visible) {
			meshObject.SetActive (visible);
		}

		public bool IsVisible() {
			return meshObject.activeSelf;
		}
	}

	class LODMesh {

		public Mesh mesh;
		public bool hasRequestedMesh;
		public bool hasMesh;
		int lod;
		System.Action updateCallback;

		public LODMesh(int lod, System.Action updateCallback) {
			this.lod = lod;
			this.updateCallback = updateCallback;
		}

		void OnMeshDataReceived(MeshData meshData) {
			mesh = meshData.CreateMesh ();
			mesh.RecalculateBounds();
			hasMesh = true;
			EndlessTerrain.readyChunkCount.Value++;

			updateCallback ();
		}

		public void RequestMesh(MapData mapData) {
			hasRequestedMesh = true;
			mapGenerator.RequestMeshData (mapData, lod, OnMeshDataReceived);
		}

	}

	[System.Serializable]
	public struct LODInfo {
		public int lod;
		public float visibleDstThreshold;
	}

}