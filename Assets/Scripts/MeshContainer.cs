using System.Collections.Generic;
using UnityEngine;

public class MeshContainer : MonoBehaviour {

	#region Fields

	public Mesh mesh;
	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;
	public MeshCollider meshCollider;
	public bool generateCollider;

	#endregion // Fields

	#region Methods

	private void Awake() {

		meshFilter = gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = new() { name = gameObject.name, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
		mesh = meshFilter.mesh;
		meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.enabled = false;
    }

	// I don't like this
    public void Destroy() {

		Destroy(gameObject);
	}

	public void RegenerateMesh(List<Triangle> triangleList) {

		mesh.Clear();

		var verticies = new Vector3[triangleList.Count * 3];
		var triangles = new int[triangleList.Count * 3];

		for (int j = 0; j < triangleList.Count; j++) {
			for (int k = 0; k < 3; k++) {

				Triangle tri = triangleList[j];
				verticies[j * 3 + k] = tri[k];
				triangles[j * 3 + k] = j * 3 + k;
			}
		}

		mesh.vertices = verticies;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
	}

	public void ShowMesh(Material material, bool generateCollider) {

		meshRenderer.material = material;
		meshRenderer.enabled = true;

		if (generateCollider) {

			if (meshCollider == null)
				meshCollider = gameObject.AddComponent<MeshCollider>();
			else
				meshCollider = GetComponent<MeshCollider>();

			meshCollider.sharedMesh = mesh;
			meshCollider.enabled = false;
			meshCollider.enabled = true;
		}
		else
			Destroy(meshCollider);
	}

	public void HideMesh() {

		if(meshRenderer != null)
			meshRenderer.enabled = false;

		Destroy(meshCollider);
	}

    #endregion // Methods
}