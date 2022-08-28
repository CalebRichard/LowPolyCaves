using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour {

	#region Constants

	public static int BlockSize = 16;

	#endregion // Constants

	#region Fields

	// Public
	public Vector3Int BlockCoordinate;
	public Vector3 CenterWorldCoordinate;
	public List<Triangle> TriangleList;
	public Material Material;
	public Dictionary<int, MeshContainer> Meshes;

    #endregion // Fields

    #region Methods

    private void Awake() {

		TriangleList = new();
		Meshes = new();
    }

	public MeshContainer this[int i] {

        get { return Meshes[i]; }
    }

	public bool ContainsLOD(int i) {

		return Meshes.ContainsKey(i);
    }

    public void AddLODMesh(int index) {

        if (!Meshes.ContainsKey(index)) {

			GameObject lodMesh = new("LOD"+index);
			lodMesh.transform.parent = transform;

			MeshContainer container = lodMesh.AddComponent<MeshContainer>();
			Meshes.Add(index, container);
		}
    }

	public void RegenerateMesh(int lod) {

		if (Meshes.ContainsKey(lod)) {

			Meshes[lod].RegenerateMesh(TriangleList);
		}
    }

	public void ShowMesh(int lod) {

		if (Meshes.ContainsKey(lod)) {

			Meshes[lod].ShowMesh(Material, false);
		}
	}

	public void HideMesh(int lod) {

		if (Meshes.ContainsKey(lod)) {

			Meshes[lod].HideMesh();
		}
	}

	// Expose MeshContainer methods?

	#endregion // Methods
}