using UnityEngine;

public class CubeSurfaceTest : MonoBehaviour {

    #region Fields

    // Public
    [Range(0,1)]
    public float Level = 0.5f;
    [Range(0, 1)]
    public float c1 = 0.6f;
    [Range(0, 1)]
    public float c2 = 0.3f;
    [Range(0, 1)]
    public float c3 = 0.1f;
    [Range(0, 1)]
    public float c4 = 0.1f;
    [Range(0, 1)]
    public float c5 = 0.2f;
    [Range(0, 1)]
    public float c6 = 0.55f;
    [Range(0, 1)]
    public float c7 = 0.56f;
    [Range(0, 1)]
    public float c8 = 0.6f;

    public bool settingsChanged = false;

    // Private
    Cube cube;
    Mesh m_mesh;
    MeshFilter meshFilter;

    #endregion // Fields

    #region Methods

    private void Awake() {

        meshFilter = gameObject.GetComponent<MeshFilter>();
        meshFilter.mesh = new() { name = "Surface" };
    }

    private void Update() {

        if (settingsChanged)
            UpdateMesh();
    }

    private void OnValidate() {

        settingsChanged = true;
    }

    private void OnDrawGizmosSelected() {

        // Draw cube
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector3(0.5f, 0.5f, 0.5f), Vector3.one);
    }

    void UpdateMesh() {

        cube = new(
            new Vector4(0, 0, 0, c1),
            new Vector4(1, 0, 0, c2),
            new Vector4(1, 0, 1, c3),
            new Vector4(0, 0, 1, c4),
            new Vector4(0, 1, 0, c5),
            new Vector4(1, 1, 0, c6),
            new Vector4(1, 1, 1, c7),
            new Vector4(0, 1, 1, c8)
        );

        Mesh mesh = meshFilter.mesh;
        mesh.Clear();

        var tris = MarchingCubes.GetTriangles(cube, Level);
        var verticies = new Vector3[tris.Count * 3];
        var triangles = new int[tris.Count * 3];

        for (int j = 0; j < tris.Count; j++) {
            for (int k = 0; k < 3; k++) {

                Vector3[] vec = tris[j].Vectors();
                verticies[j * 3 + k] = vec[k];
                triangles[j * 3 + k] = j * 3 + k;
            }
        }

        mesh.vertices = verticies;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    #endregion // Methods

}
