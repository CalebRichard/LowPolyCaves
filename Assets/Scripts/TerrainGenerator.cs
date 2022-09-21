/*
 * 
 * 
 * 
 */

#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#endregion // Using

public class TerrainGenerator : MonoBehaviour {

    #region Constants

	public static int BlockSize = 32;

	#endregion // Constants

	#region Fields

	public enum DrawMode { Gizmos, Terrain};
	public DrawMode drawMode = DrawMode.Terrain;

	public int seed;
	public Material BlockMaterial;

	// Editor
	public SettingsTerrain TerrainSettings;
	[HideInInspector]
	public bool foldout;

	// Threading
	private Queue<ThreadInfo<TerrainData>> terrainDataThreadInfo = new();
	private Queue<ThreadInfo<MeshData>> meshDataThreadInfo = new();

    #endregion // Fields

    #region Methods

    private void OnValidate() {
        
		if(TerrainSettings != null) {
			if (TerrainSettings.lacunarity < 1)
				TerrainSettings.lacunarity = 1;
			if (TerrainSettings.octaves < 0)
				TerrainSettings.octaves = 0;
        }
    }

    private void Update() {

		if (terrainDataThreadInfo.Count > 0) {

			ThreadInfo<TerrainData> threadInfo = terrainDataThreadInfo.Dequeue();
			threadInfo.callback(threadInfo.param);
		}

		if (meshDataThreadInfo.Count > 0) {

			ThreadInfo<MeshData> threadInfo = meshDataThreadInfo.Dequeue();
			threadInfo.callback(threadInfo.param);
		}
    }

    public void RequestTerrainData(Vector3Int pos, int lod, Action<TerrainData> callback) {

		new Thread(() => TerrainDataThread(pos, lod, callback)).Start();
	}

	void TerrainDataThread(Vector3Int pos, int lod, Action<TerrainData> callback) {

		TerrainData terrainData = GenerateTerrain(pos, lod);

		lock (terrainDataThreadInfo) {
			terrainDataThreadInfo.Enqueue(new ThreadInfo<TerrainData> (callback, terrainData));
		}
	}

	public void RequestMeshData(TerrainData data, Action<MeshData> callback) {
		
		new Thread(() => MeshDataThread(data, callback)).Start();
    }

	void MeshDataThread(TerrainData data, Action<MeshData> callback) {

		MeshData meshData = MeshGenerator.GenerateTerrainMesh(data.noiseData, data.origin, data.surfaceValue, data.levelOfDetail);

        lock (meshDataThreadInfo) {
			meshDataThreadInfo.Enqueue(new ThreadInfo<MeshData>(callback, meshData));
        }
	}

    private TerrainData GenerateTerrain(Vector3Int pos, int lod) {

		NoiseSettings noiseSettings = TerrainSettings.GetSettings();

		// Get Noise map
		float[,,] noise = NoiseMap.NoiseValues(pos, BlockSize + 1, BlockSize + 1, BlockSize + 1, seed, noiseSettings);

		return new TerrainData(noise, pos, TerrainSettings.surfaceValue, lod);
	}

	#endregion // Methods

	struct ThreadInfo<T> {
		public readonly Action<T> callback;
		public readonly T param;

		public ThreadInfo(Action<T> callback, T param) {

			this.callback = callback;
			this.param = param;
        }
    }
}

public struct TerrainData {

	public readonly float[,,] noiseData;
	public readonly Vector3Int origin;
	public readonly float surfaceValue;
	public readonly int levelOfDetail;

    public TerrainData(float[,,] noiseData, Vector3Int origin, float surfaceValue, int levelOfDetail) {

		this.noiseData = noiseData;
		this.origin = origin;
		this.surfaceValue = surfaceValue;
		this.levelOfDetail = levelOfDetail;
    }
}