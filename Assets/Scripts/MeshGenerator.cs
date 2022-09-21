/*
 * 
 * 
 * 
 */

#region Using

using System.Collections.Generic;
using UnityEngine;

#endregion // Using

public static class MeshGenerator {

    #region Methods

    public static MeshData GenerateTerrainMesh(float[,,] noise, Vector3Int origin, float surfaceValue, int levelOfDetail) {

		int size = noise.GetLength(0) - 1;
		int halfSize = (int)(size * 0.5f);

		int div = (int)Mathf.Pow(2, levelOfDetail);
		int lodIncriment = (div > size) ? size : div;

		List<Triangle> triangleList = new();

		for (int z = 0; z < size; z += lodIncriment) {
			for (int y = 0; y < size; y += lodIncriment) {
				for (int x = 0; x < size; x += lodIncriment) {

                    int cx = origin.x + x - halfSize;
                    int cy = origin.y + y - halfSize;
					int cz = origin.z + z - halfSize;

					Cube cube = new(
						new Vector4(cx, cy, cz, noise[x, y, z]),
						new Vector4(cx + lodIncriment, cy, cz, noise[x + lodIncriment, y, z]),
						new Vector4(cx + lodIncriment, cy, cz + lodIncriment, noise[x + lodIncriment, y, z + lodIncriment]),
						new Vector4(cx, cy, cz + lodIncriment, noise[x, y, z + lodIncriment]),
						new Vector4(cx, cy + lodIncriment, cz, noise[x, y + lodIncriment, z]),
						new Vector4(cx + lodIncriment, cy + lodIncriment, cz, noise[x + lodIncriment, y + lodIncriment, z]),
						new Vector4(cx + lodIncriment, cy + lodIncriment, cz + lodIncriment, noise[x + lodIncriment, y + lodIncriment, z + lodIncriment]),
						new Vector4(cx, cy + lodIncriment, cz + lodIncriment, noise[x, y + lodIncriment, z + lodIncriment])
					);

					triangleList.AddRange(MarchingCubes.GetTriangles(cube, surfaceValue));
				}
			}
		}

		return new MeshData(triangleList, $"{origin}LOD{levelOfDetail}", levelOfDetail);
	}

    #endregion // Methods
}

public class MeshData {

    #region Fields

    private readonly List<Triangle> triangleList;
	private readonly Vector3[] verts;
	private readonly int[] tris;
	private readonly Vector2[] uvs;
	private readonly string name;
	private readonly Mesh mesh;

	public int LOD;

	#endregion // Fields

    #region Constructor

    public MeshData(List<Triangle> triangleList, string name, int lod) {

        this.triangleList = triangleList;
        this.name = name;
        verts = new Vector3[triangleList.Count * 3];
        tris = new int[triangleList.Count * 3];
        uvs = new Vector2[triangleList.Count * 3];
        LOD = lod;
    }

    #endregion // Constructor

    #region Properties

    public Mesh Mesh {
		get {
			if (mesh == null)
				return GenMesh();
			else
				return mesh;
		}
    }

    #endregion // Properties

    #region Methods

    private Mesh GenMesh() {

		Mesh mesh = new() { name = $"Mesh{name}" };

		for (int j = 0; j < triangleList.Count; j++) {
			for (int k = 0; k < 3; k++) {

				Triangle tri = triangleList[j];
				verts[j * 3 + k] = tri[k];
				tris[j * 3 + k] = j * 3 + k;
			}
		}

		mesh.vertices = verts;
		mesh.triangles = tris;
		mesh.uv = uvs;
		mesh.RecalculateNormals();

		return mesh;
	}

    #endregion // Methods
}