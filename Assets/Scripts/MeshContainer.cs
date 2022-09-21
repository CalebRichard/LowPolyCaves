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