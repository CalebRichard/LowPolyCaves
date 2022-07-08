using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockSurfaceTest : MonoBehaviour {

	#region Constants

	const string c_blockContainerName = "BlockTest";

	#endregion // Constants

	#region Fields

	// Public
	public float noiseScale = 6f;
	public int octave = 2;
	public float persistance = 3.4f;
	public float lacunarity = 0.43f;
	public Vector3 noiseOffset = Vector3.zero;

	public Material blockMaterial;

	public IsoSurface[] surfaces;

	// Private
	GameObject m_blockContainer;
	Block m_block;

    #endregion // Fields

    #region Methods

    private void Awake() {

		InitializeBlockContainer();
		m_block = GenerateBlock(Vector3Int.zero);
	}

    private void Update() {

		RegenBlockMesh(m_block);
	}

 //   private void OnValidate() {

	//	if(Application.isPlaying)
	//		RegenBlockMesh(m_block);
	//}

	public Vector3Int BlockCenterFromCoord(Vector3Int vector) {

		return vector * Block.BlockSize + Vector3Int.one * (int)(Block.BlockSize * 0.5f);
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
		newBlock.CenterWorldCoordinate = BlockCenterFromCoord(coord);

		return newBlock;
	}

	void RegenBlockMesh(Block block) {

		var size = Block.BlockSize;
		var blockCoord = block.BlockCoordinate;
		var bx = blockCoord.x;
		var by = blockCoord.y;
		var bz = blockCoord.z;

		// Get Noise map
		var noise = NoiseMap.NoiseMap3D(block.CenterWorldCoordinate, size + 1, size + 1, size + 1, noiseScale, octave, persistance, lacunarity, noiseOffset);

		// Loop through surfaces
		for (int i = 0; i < surfaces.Length; i++) {

			if (block.MeshContainers.Count <= i) {
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

			block.MeshContainers[i].RegenerateMesh();
			blockMaterial.color = surfaces[i].color;
			block.MeshContainers[i].ShowMesh(blockMaterial, false);
		}
	}

	#endregion // Methods

}
