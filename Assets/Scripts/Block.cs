using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour {

	#region Constants

	public static int BlockSize = 32;

	#endregion // Constants

	#region Fields

	// Public
	public Vector3Int BlockCoordinate;
	public Vector3 CenterWorldCoordinate;
    [HideInInspector]
	public Mesh mesh;

	// Private
	MeshFilter meshFilter;
	MeshRenderer meshRenderer;
	MeshCollider meshCollider;
	bool generateCollider;

	#endregion // Fields

	#region Constructors



	#endregion // Constructors

	#region Properties



	#endregion // Properties

	#region Methods

	public void Generate(Material material, bool generateCollider) {

		this.generateCollider = generateCollider;

		meshFilter = GetComponent<MeshFilter>();
		if (meshFilter == null)
			meshFilter = gameObject.AddComponent<MeshFilter>();

		meshRenderer = GetComponent<MeshRenderer>();
		if (meshRenderer == null)
			meshRenderer = gameObject.AddComponent<MeshRenderer>();

		meshCollider = GetComponent<MeshCollider>();
		if(meshCollider == null && generateCollider)
			meshCollider = gameObject.AddComponent<MeshCollider>();
		if (meshCollider != null && !generateCollider)
			DestroyImmediate(meshCollider);

		mesh = meshFilter.sharedMesh;
		if(mesh == null) {

            mesh = new Mesh {
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
            };
            meshFilter.sharedMesh = mesh;
        }

        if (generateCollider) {

			if(meshCollider.sharedMesh == null)
				meshCollider.sharedMesh = mesh;
			meshCollider.enabled = false;
			meshCollider.enabled = true;
		}
    }

	#endregion // Methods
}
