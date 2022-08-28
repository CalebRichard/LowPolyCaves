/*
 * 
 * 
 * 
 */

#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#endregion // Using

public class InfiniteTerrain : MonoBehaviour {

	#region Constants

	private const int c_blockRenderDistance = 4;
	private const string c_blockContainerName = "Terrain";
	private const int c_lodStep = 4;

	#endregion // Constants

	#region Fields

	// Public
	public Transform Character;
	public Material BlockMaterial;

	// Blocks
	private Vector3Int currentCharacterBlockCoord;
	private Vector3Int previousCharacterBlockCoord;
	private GameObject blockContainer;
	private Dictionary<Vector3Int, Block> visibleBlocks;
	private Queue<Block> blockRecycling;

	// Level of Detail
	private Dictionary<Vector3Int, int> visibleMeshLODs;

	// Editor
	public SettingsTerrain TerrainSettings;
	[HideInInspector]
	public bool foldout;

	#endregion // Fields

	#region Methods

	// Inherited
	private void Awake() {
        
		InitializeBlockFields();
		previousCharacterBlockCoord = GetBlockCoordinate(Character.position);
    }

    private void Update() {

		currentCharacterBlockCoord = GetBlockCoordinate(Character.position);

		if(currentCharacterBlockCoord != previousCharacterBlockCoord) {

			UpdateTerrainAroundCharacter();
			previousCharacterBlockCoord = currentCharacterBlockCoord;
        }
    }

    private void OnValidate() {
        

    }

    // Public
    public Vector3Int GetBlockCoordinate(Vector3 vector) {

		return Vector3Int.FloorToInt(vector / Block.BlockSize);
	}

	public Vector3Int GetBlockCenter(Vector3Int vec) {

		return vec * Block.BlockSize + Vector3Int.one * (int)(Block.BlockSize * 0.5f);
    }

	public Vector3Int GetBlockCenter(Vector3 vec) {

		return GetBlockCenter(GetBlockCoordinate(vec));
    }


	// Private

	void InitializeBlockFields() {

		visibleBlocks = new();
		blockRecycling = new();
		visibleMeshLODs = new();
		// TBI: Saving/Loading world data

		blockContainer = (GameObject.Find(c_blockContainerName)) ? GameObject.Find(c_blockContainerName) : new GameObject(c_blockContainerName);
		UpdateTerrainAroundCharacter();
	}

	public void UpdateTerrainAroundCharacter() {

		// Update list of visible mesh LODs near character
		BlockCoordinatesLoop(UpdateLODSAroundCharacter);

		RecycleBlocks();

		// Update visible blocks list based on LODs
		// Generate/Regen meshes for visible blocks
		// Show new blocks added to visible blocks list
		UpdateVisibleBlocks();

		// Destroy blocks still in recycling
		ClearRecycling();

		// Hide block renderers out of character view
	}

	void UpdateLODSAroundCharacter(Vector3Int blockCoord) {

		var lod = Mathf.FloorToInt((currentCharacterBlockCoord - blockCoord).magnitude / c_lodStep);
		if (visibleMeshLODs.ContainsKey(blockCoord))
			visibleMeshLODs[blockCoord] = lod;
		else
			visibleMeshLODs.Add(blockCoord, lod);

		//print($"Block{blockCoord} lod: {lod}");
	}

	void UpdateVisibleBlocks() {

        foreach (KeyValuePair<Vector3Int, int> lodMesh in visibleMeshLODs) {

			Block block;
			var blockCoord = lodMesh.Key;
			var lod = lodMesh.Value;

			// If block exists but does not have the LOD mesh, add LOD mesh
			if (visibleBlocks.ContainsKey(blockCoord)) {

				if (visibleBlocks[blockCoord].ContainsLOD(lod))
					continue;
				else
					block = visibleBlocks[blockCoord];
			}
			// If block is in recycling, remove, reassign coordinate, add to visible blocks, add LOD mesh if it doesn't exist
			else if (blockRecycling.Count > 0) {

				block = blockRecycling.Dequeue();
				block.BlockCoordinate = blockCoord;
				visibleBlocks.Add(blockCoord, block);

				if (!block.ContainsLOD(lod))
					block.AddLODMesh(lod);
			}
			// Create new block and add to visible blocks
			else {

				block = GenerateBlock(blockCoord);
				visibleBlocks.Add(blockCoord, block);
				block.Material = BlockMaterial;
				block.AddLODMesh(lod);
			}

			RegenBlockMeshData(block);
			block.RegenerateMesh(lod);
			block.ShowMesh(lod);
		}
	}

	void BlockCoordinatesLoop(Action<Vector3Int> action) {

		// Loop through coordinates
		for (int z = currentCharacterBlockCoord.z - c_blockRenderDistance; z < currentCharacterBlockCoord.z + c_blockRenderDistance; z++) {
			for (int y = currentCharacterBlockCoord.y - c_blockRenderDistance; y < currentCharacterBlockCoord.y + c_blockRenderDistance; y++) {
				for (int x = currentCharacterBlockCoord.x - c_blockRenderDistance; x < currentCharacterBlockCoord.x + c_blockRenderDistance; x++) {

					// cut off corners of cube of blocks?
					var blockCoord = new Vector3Int(x, y, z);

					action(blockCoord);
				}
			}
		}
	}

	void RecycleBlocks() {

		// Recycle old blocks that are out of range
		// Problem: Can't modify collection I'm using to iterate
		foreach (Vector3Int blockCoord in visibleBlocks.Keys) {

			Block block = visibleBlocks[blockCoord];

			var maxCoord = Mathf.Max(blockCoord.x, blockCoord.y, blockCoord.z);
			var minCoord = Mathf.Min(blockCoord.x, blockCoord.y, blockCoord.z);
			var maxLimit = Mathf.Max(currentCharacterBlockCoord.x, currentCharacterBlockCoord.y, currentCharacterBlockCoord.z) + c_blockRenderDistance;
			var minLimit = Mathf.Min(currentCharacterBlockCoord.x, currentCharacterBlockCoord.y, currentCharacterBlockCoord.z) - c_blockRenderDistance;

			if (maxCoord > maxLimit || minCoord < minLimit) {

				block.HideMesh(visibleMeshLODs[blockCoord]);
				visibleMeshLODs.Remove(blockCoord);
                visibleBlocks.Remove(blockCoord);
				blockRecycling.Enqueue(block);
			}
		}
	}

	Block GenerateBlock(Vector3Int coord) {

		GameObject block = new($"Block({coord.x},{coord.y},{coord.z})");
		block.transform.parent = blockContainer.transform;

		Block newBlock = block.AddComponent<Block>();
		newBlock.BlockCoordinate = coord;
		newBlock.CenterWorldCoordinate = GetBlockCenter(coord);

		return newBlock;
	}

	void RegenBlockMeshData(Block block) {

		var size = Block.BlockSize;
		var blockCoord = block.BlockCoordinate * size;
		var bx = blockCoord.x;
		var by = blockCoord.y;
		var bz = blockCoord.z;

		// Get Noise map
		float[,,] noise = NoiseMap.NoiseValues(block.CenterWorldCoordinate, size + 1, size + 1, size + 1, TerrainSettings.noiseScale, TerrainSettings.octave,
			TerrainSettings.persistance, TerrainSettings.lacunarity, TerrainSettings.noiseOffset, false);

		block.TriangleList.Clear();

		// Loop through noise values
		for (int z = 0; z < size; z++) {
			for (int y = 0; y < size; y++) {
				for (int x = 0; x < size; x++) {

					var cx = bx + x;
					var cy = by + y;
					var cz = bz + z;

					Cube cube = new(
						new Vector4(cx, cy, cz, noise[x, y, z]),
						new Vector4(cx + 1, cy, cz, noise[x + 1, y, z]),
						new Vector4(cx + 1, cy, cz + 1, noise[x + 1, y, z + 1]),
						new Vector4(cx, cy, cz + 1, noise[x, y, z + 1]),
						new Vector4(cx, cy + 1, cz, noise[x, y + 1, z]),
						new Vector4(cx + 1, cy + 1, cz, noise[x + 1, y + 1, z]),
						new Vector4(cx + 1, cy + 1, cz + 1, noise[x + 1, y + 1, z + 1]),
						new Vector4(cx, cy + 1, cz + 1, noise[x, y + 1, z + 1])
					);

					block.TriangleList.AddRange(MarchingCubes.GetTriangles(cube, TerrainSettings.surfaceValue));
				}
			}
		}
	}

	void ClearRecycling() {

		foreach (Block block in blockRecycling) {
			Destroy(block);
		}

		blockRecycling.Clear();
	}

	public void OnSettingsUpdated() {


	}

	//void RegenBlockMesh(Block block, int LOD) {

	//	block.MeshContainers[LOD].RegenerateMesh();
	//}

	//void ShowBlockMesh(Block block, int LOD) {

	//	block.MeshContainers[LOD].ShowMesh(blockMaterial, false);		
	//}

	#endregion // Methods
}