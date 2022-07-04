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

[ExecuteInEditMode]
public class IsoSurfaceGen : MonoBehaviour {

	#region Constants

	public const int c_maxBlocksInView = 4;
	const string c_blockContainerName = "Block Container";

	#endregion // Constants

	#region Fields

	// Private
	Vector3 m_playerPosition;
	[Range(1f, 8f)]
	float m_resolution = 1f;
	bool m_settingsUpdated = false;

	float noiseScale = 0.6f;
	int octave = 2;
	float persistance = 3.4f;
	float lacunarity = 0.43f;
	Vector3 noiseOffset = Vector3.zero;

	GameObject m_blockContainer;
	Block m_block;
	List<Block> m_blocks;
	Dictionary<Vector3Int, Block> m_existingBlocks;
	Queue<Block> m_queue;

	// Public
	public Transform player;
	public IsoSurface[] surfaces;

	#endregion // Fields

	#region Constructors



	#endregion // Constructors

	#region Properties



	#endregion // Properties

	#region Methods

	// Private
	private void Awake() {

		InitializeBlockFields();

		m_playerPosition = player.position;
		//var voxelSize = Block.BlockSize / m_resolution;

    }

    private void Update() {

		m_playerPosition = player.position;
		//Generate();
	}

    private void OnValidate() {

		//m_settingsUpdated = true;
	}

	private void OnDrawGizmosSelected() {

		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(Vector3Int.FloorToInt(player.position) + Vector3.one * 0.5f, Vector3Int.one);

		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(BlockCenterFromPlayer(), Vector3.one * Block.BlockSize);
	}

	void InitializeBlockContainer() {

		if (m_blockContainer == null) {
            if (GameObject.Find(c_blockContainerName))
				m_blockContainer = GameObject.Find(c_blockContainerName);
			else
				m_blockContainer = new GameObject(c_blockContainerName);
		}
    }

	void InitializeBlockFields() {

		m_blocks = new List<Block>();
		m_existingBlocks = new Dictionary<Vector3Int, Block>();
		m_queue = new Queue<Block>();
    }

	void InitializeBlocks() {


    }

	void InitializeVisibleBlocks() {

		InitializeBlockContainer();

		Vector3Int bp = BlockCenterFromPlayer();
		var size = Block.BlockSize;
		float sqrViewDistance = c_maxBlocksInView * c_maxBlocksInView * Block.BlockSize * Block.BlockSize;

		// Recycle old blocks
        for (int i = 0; i < m_blocks.Count; i++) {


        }

		// Loop through blocks
		for (int z = -c_maxBlocksInView; z < c_maxBlocksInView; z++) {
			for (int y = -c_maxBlocksInView; y < c_maxBlocksInView; y++) {
				for (int x = -c_maxBlocksInView; x < c_maxBlocksInView; x++) {

					// Get Noise map
					var noise = NoiseMap.NoiseMap3D(bp, size, size, size, noiseScale, octave, persistance, lacunarity, noiseOffset);

                    // Loop through voxels
                    for (int vz = 0; vz < size - 1; vz++) {
                        for (int vy = 0; vy < size - 1; vy++) {
                            for (int vx = 0; vx < size - 1; vx++) {

								Cube voxel = new Cube(
									new Vector4(x * size + vx	 , y * size + vy	, z * size + vz	   , noise[vx	 , vy	 , vz]),
									new Vector4(x * size + vx + 1, y * size + vy	, z * size + vz    , noise[vx + 1, vy	 , vz]),
									new Vector4(x * size + vx + 1, y * size + vy	, z * size + vz + 1, noise[vx + 1, vy	 , vz + 1]),
									new Vector4(x * size + vx	 , y * size + vy	, z * size + vz + 1, noise[vx	 , vy	 , vz + 1]),
									new Vector4(x * size + vx	 , y * size + vy + 1, z * size + vz    , noise[vx	 , vy + 1, vz]),
									new Vector4(x * size + vx + 1, y * size + vy + 1, z * size + vz    , noise[vx + 1, vy + 1, vz]),
									new Vector4(x * size + vx + 1, y * size + vy + 1, z * size + vz + 1, noise[vx + 1, vy + 1, vz + 1]),
									new Vector4(x * size + vx	 , y * size + vy + 1, z * size + vz + 1, noise[vx	 , vy + 1, vz + 1])
									);

                                // Loop through surfaces
                                for (int i = 0; i < surfaces.Length; i++) {


                                }

								//noise[vx, vy, vz];
							}
                        }
                    }
				}
			}
		}
	}

	Block GenerateBlock(Vector3Int coord) {

		GameObject block = new($"Block-{coord.x}.{coord.y}.{coord.z}");
		block.transform.parent = m_blockContainer.transform;

		Block newBlock = block.AddComponent<Block>();
		newBlock.BlockCoordinate = coord;
		newBlock.CenterWorldCoordinate = BlockCenterFromPlayer();

		return newBlock;
	}

	// Public
	public void Generate() {

		InitializeVisibleBlocks();
	}

	public Vector3Int BlockCoordinate() {

		var playerPos = player.position;
		return Vector3Int.FloorToInt(playerPos / Block.BlockSize);
	}

	public Vector3Int BlockCenterFromPlayer() {

		return BlockCoordinate() * Block.BlockSize + Vector3Int.one * (int)(Block.BlockSize * 0.5f);
	}

    #endregion // Methods

}

[System.Serializable]
public struct IsoSurface {

	public float value;
	public Color color;
}
