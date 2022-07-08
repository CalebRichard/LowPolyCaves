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
	public List<MeshContainer> MeshContainers = new();

	#endregion // Fields

	#region Methods

    public void GenerateContainer(int index) {

        if (MeshContainers.Count <= index) {

			GameObject container = new("Surface"+index);
			container.transform.parent = transform;

			MeshContainer newContainer = container.AddComponent<MeshContainer>();
			MeshContainers.Add(newContainer);
		}
    }

	#endregion // Methods
}