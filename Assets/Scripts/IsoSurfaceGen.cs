//
//
//
//
//

#region Using

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#endregion // Using

//[ExecuteInEditMode]
public class IsoSurfaceGen : MonoBehaviour {

	#region Constants

	public const int c_maxBlocksInView = 2;
	const string c_blockContainerName = "Block Container";

	#endregion // Constants

	#region Fields

	// Private
	Vector3Int m_blockCoord;
	[Range(1f, 8f)]
	//float m_resolution = 1f;
	bool m_settingsUpdated = false;

	float noiseScale = 6f;
	int octave = 1;
	float persistance = 3.4f;
	float lacunarity = 0.43f;
	Vector3 noiseOffset = Vector3.zero;

	GameObject m_blockContainer;
	//Block m_block;
	List<Block> m_blocks;
	Dictionary<Vector3Int, Block> m_existingBlocks;
	Queue<Block> m_recycleBlocks;

	// Public
	public Material blockMaterial;
	public Transform player;
	public IsoSurface[] surfaces;

	#endregion // Fields

	#region Constructors



	#endregion // Constructors

	#region Properties



	#endregion // Properties

	#region Methods

	// Public
	public Vector3Int GetBlockCoordinate(Vector3 vector) {

		return Vector3Int.FloorToInt(vector / Block.BlockSize);
	}

	public Vector3Int GetBlockCenterFromVector(Vector3 vector) {

		return GetBlockCoordinate(vector) * Block.BlockSize + Vector3Int.one * (int)(Block.BlockSize * 0.5f);
	}

	public Vector3Int GetBlockCenterFromCoord(Vector3Int vector) {

		return vector * Block.BlockSize + Vector3Int.one * (int)(Block.BlockSize * 0.5f);
	}

	// Private

	void InitializeBlockFields() {

		m_blocks = new List<Block>();
		m_existingBlocks = new Dictionary<Vector3Int, Block>();
		m_recycleBlocks = new Queue<Block>();
	}

	void InitializeBlockContainer() {

		if (m_blockContainer == null) {
            m_blockContainer = (GameObject.Find(c_blockContainerName)) ? GameObject.Find(c_blockContainerName) : new GameObject(c_blockContainerName);
		}
	}

	Block GenerateBlock(Vector3Int coord) {

		GameObject block = new($"Block({coord.x},{coord.y},{coord.z})");
		block.transform.parent = m_blockContainer.transform;

		Block newBlock = block.AddComponent<Block>();
		newBlock.BlockCoordinate = coord;
		newBlock.CenterWorldCoordinate = GetBlockCenterFromCoord(coord);

		return newBlock;
	}

	void InitializeVisibleBlocks() {

		InitializeBlockContainer();

		Vector3Int playerBlockCoord = GetBlockCoordinate(player.position);

		// Recycle old blocks that are out of range
        for (int i = 0; i < m_blocks.Count; i++) {

			var block = m_blocks[i];
			var coord = block.BlockCoordinate;
			var maxCoord = Mathf.Max(coord.x, coord.y, coord.z);
			var minCoord = Mathf.Min(coord.x, coord.y, coord.z);
			var maxLimit = Mathf.Max(playerBlockCoord.x, playerBlockCoord.y, playerBlockCoord.z) + c_maxBlocksInView;
			var minLimit = Mathf.Min(playerBlockCoord.x, playerBlockCoord.y, playerBlockCoord.z) - c_maxBlocksInView;

            if (maxCoord > maxLimit || minCoord < minLimit) {

				m_existingBlocks.Remove(coord);
				m_recycleBlocks.Enqueue(block);
				m_blocks.RemoveAt(i);
			}
        }

		// Loop through coordinates
		for (int z = playerBlockCoord.z - c_maxBlocksInView; z < playerBlockCoord.z + c_maxBlocksInView; z++) {
			for (int y = playerBlockCoord.y - c_maxBlocksInView; y < playerBlockCoord.y + c_maxBlocksInView; y++) {
				for (int x = playerBlockCoord.x - c_maxBlocksInView; x < playerBlockCoord.x + c_maxBlocksInView; x++) {

					var blockCoord = new Vector3Int(x, y, z);

					if (m_existingBlocks.ContainsKey(blockCoord))
						continue;
					if (m_recycleBlocks.Count > 0) {

						Block block = m_recycleBlocks.Dequeue();
						block.BlockCoordinate = blockCoord;
						m_existingBlocks.Add(blockCoord, block);
						m_blocks.Add(block);
						RegenBlockMeshTriangles(block);
					}
                    else {

						Block block = GenerateBlock(blockCoord);
						block.BlockCoordinate = blockCoord;
						m_existingBlocks.Add(blockCoord, block);
						m_blocks.Add(block);
						RegenBlockMeshTriangles(block);
					}
				}
			}
		}
	}

	void UpdateBlocks() {

        foreach (Block block in m_blocks) {
			RegenBlockMeshTriangles(block);
        }
	}

	void RegenBlockMeshTriangles(Block block) {

		var size = Block.BlockSize;
		var blockCoord = block.BlockCoordinate;
		var bx = blockCoord.x;
		var by = blockCoord.y;
		var bz = blockCoord.z;

		// Get Noise map
		var noise = NoiseMap.NoiseMap3D(block.CenterWorldCoordinate, size + 1, size + 1, size + 1, noiseScale, octave, persistance, lacunarity, noiseOffset);

		// Loop through surfaces
		for (int i = 0; i < surfaces.Length; i++) {

			if(block.MeshContainers.Count <= i) {
				block.GenerateContainer(i);
            }

            block.MeshContainers[i].triangleList.Clear();

			// Loop through cubes
			for (int z = 0; z < size; z++) {
				for (int y = 0; y < size; y++) {
					for (int x = 0; x < size; x++) {

						Cube cube = new(
							new Vector4(bx * size + x, by * size + y, bz * size + z, noise[x, y, z]),
							new Vector4(bx * size + x + 1, by * size + y, bz * size + z, noise[x + 1, y, z]),
							new Vector4(bx * size + x + 1, by * size + y, bz * size + z + 1, noise[x + 1, y, z + 1]),
							new Vector4(bx * size + x, by * size + y, bz * size + z + 1, noise[x, y, z + 1]),
							new Vector4(bx * size + x, by * size + y + 1, bz * size + z, noise[x, y + 1, z]),
							new Vector4(bx * size + x + 1, by * size + y + 1, bz * size + z, noise[x + 1, y + 1, z]),
							new Vector4(bx * size + x + 1, by * size + y + 1, bz * size + z + 1, noise[x + 1, y + 1, z + 1]),
							new Vector4(bx * size + x, by * size + y + 1, bz * size + z + 1, noise[x, y + 1, z + 1])
						);

						block.MeshContainers[i].triangleList.AddRange(MarchingCubes.GetTriangles(cube, surfaces[i].value));
					}
				}
			}
		}

		RegenBlockMesh(block);
		ShowBlockMesh(block);
	}

	void RegenBlockMesh(Block block) {

		// Loop through surfaces
		for (int i = 0; i < surfaces.Length; i++) {

			block.MeshContainers[i].RegenerateMesh();
		}
	}

	void ShowBlockMesh(Block block) {

        for (int i = 0; i < block.MeshContainers.Count; i++) {

			block.MeshContainers[i].ShowMesh(blockMaterial, false);
        }
    }

	// Built-in
	private void Awake() {

		InitializeBlockFields();

		m_blockCoord = GetBlockCoordinate(player.position);
		InitializeVisibleBlocks();
		//var voxelSize = Block.BlockSize / m_resolution;
	}

	private void Update() {

		if (GetBlockCoordinate(player.position) != m_blockCoord || m_settingsUpdated) {
			m_blockCoord = GetBlockCoordinate(player.position);
			InitializeVisibleBlocks();
		}
	}

	private void OnValidate() {

		m_settingsUpdated = true;
	}

	private void OnDrawGizmosSelected() {

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3Int.FloorToInt(player.position) + Vector3.one * 0.5f, Vector3Int.one);

        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(GetBlockCenterFromVector(player.position), Vector3.one * Block.BlockSize);
    }

 //   private void OnApplicationQuit() {

	//	var oldBlocks = FindObjectsOfType<Block>();
	//	for (int i = 0; i < oldBlocks.Length; i++) {

	//		Destroy(oldBlocks[i].gameObject);
	//	}
	//}

    #endregion // Methods

}

[System.Serializable]
public struct IsoSurface {

    [Range(0,1)]
	public float value;
	public Color color;
}
