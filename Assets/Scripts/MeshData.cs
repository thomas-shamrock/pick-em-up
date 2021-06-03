using UnityEngine;
using System.Collections;

public class MeshData {
	public Vector3[] vertices;
	public int[] triangles;
	//public Vector2[] uvs;

	int triangleIndex;

	public MeshData(int meshWidth, int meshHeight) {
		vertices = new Vector3[meshWidth * meshHeight];
		//uvs = new Vector2[meshWidth * meshHeight];
		triangles = new int[(meshWidth-1)*(meshHeight-1)*6];
	}

	public void AddTriangle(int a, int b, int c) {
		triangles [triangleIndex] = a;
		triangles [triangleIndex + 1] = b;
		triangles [triangleIndex + 2] = c;
		triangleIndex += 3;
	}

//	public Mesh CreateMesh() {
//		Mesh mesh = new Mesh ();
//		mesh.vertices = vertices;
//		mesh.triangles = triangles;
//		mesh.uv = uvs;
//		mesh.RecalculateNormals ();
//		return mesh;
//	}

	public Mesh CreateMesh()
	{
		Mesh mesh = new Mesh();
		Vector3[] newVerts = new Vector3[triangles.Length];
		for (int i = 0; i < triangles.Length; i++)
		{
			newVerts[i] = vertices[triangles[i]];
			triangles[i] = i;
		}

		Vector2[] uvs = new Vector2[newVerts.Length];
		for(var i = 0; i < newVerts.Length; i++)
		{
			uvs[i] = new Vector2(newVerts[i].x, newVerts[i].y);
		}

		mesh.vertices = newVerts;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();

		return mesh;
	}
}