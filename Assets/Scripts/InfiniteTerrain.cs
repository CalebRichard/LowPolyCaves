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

    private const string c_blockContainerName = "Terrain";

    #endregion // Constants

    #region Fields

    private int blockVisibleRadius = 10;
    private int blockLoadRadius = 20;

    public Transform Character;
    public static Vector3 characterPosition;
    //private Vector3Int currentCharacterBlockCoord;
    private Vector3Int previousCharacterBlockCoord;
    public static TerrainGenerator terrainGenerator;
    public bool drawGizmos = true;

    public static int BlockSize = 16;
    private GameObject blockContainer;
    private Dictionary<Vector3Int, MeshBlock> loadedBlocks;
    private Dictionary<Vector3Int, int> visibleBlocksLevelOfDetail;
    private Queue<MeshBlock> blockRecycling;

    #endregion // Fields

    #region Methods

    // Built-in
    private void Awake() {

        blockContainer = GameObject.Find(c_blockContainerName) ? GameObject.Find(c_blockContainerName) : new GameObject(c_blockContainerName);
        loadedBlocks = new();
        visibleBlocksLevelOfDetail = new();
        blockRecycling = new();
        terrainGenerator = FindObjectOfType<TerrainGenerator>();
    }

    private void Start() {

        characterPosition = Character.position;
        previousCharacterBlockCoord = GetBlockCoordinate(characterPosition);
        UpdateBlocksLevelOfDetail(previousCharacterBlockCoord);
        LoadVisibleBlocks();
        ShowVisibleBlocks();
    }

    private void Update() {

        characterPosition = Character.position;
        var blockCoord = GetBlockCoordinate(characterPosition);

        if (blockCoord != previousCharacterBlockCoord) {

            visibleBlocksLevelOfDetail.Clear();
            UpdateBlocksLevelOfDetail(blockCoord);
            HideBlocks();
            RecycleBlocks();
            LoadVisibleBlocks();
            ShowVisibleBlocks();
            EmptyRecycling();

            previousCharacterBlockCoord = blockCoord;
        }
    }

    private void OnDrawGizmos() {

        if (Application.isPlaying && drawGizmos) {

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(characterPosition, BlockSize * blockVisibleRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(characterPosition, BlockSize * blockLoadRadius);
        }
    }

    // General
    void UpdateBlocksLevelOfDetail(Vector3Int blockCoord) {

        int xIter = 1, yIter = 1, zIter = 1;

        for (int z = blockCoord.z; z < blockCoord.z + blockVisibleRadius; z++) {
            for (int y = blockCoord.y; y < blockCoord.y + blockVisibleRadius; y++) {
                for (int x = blockCoord.x; x < blockCoord.x + blockVisibleRadius; x++) {
                    // trying to think of a way to iterate over coords near player first
                    // so blocks near player update before all others
                    // Get direction character is looking and start in that quadrant

                    var newBlockCoord = new Vector3Int(x, y, z);
                    var dist = Vector3Int.Distance(newBlockCoord, blockCoord);
                    if (dist < blockVisibleRadius)
                        visibleBlocksLevelOfDetail.Add(newBlockCoord, Mathf.FloorToInt(dist));

                    newBlockCoord = new Vector3Int(x - xIter, y, z);
                    dist = Vector3Int.Distance(newBlockCoord, blockCoord);
                    if (dist < blockVisibleRadius)
                        visibleBlocksLevelOfDetail.Add(newBlockCoord, Mathf.FloorToInt(dist));

                    newBlockCoord = new Vector3Int(x, y, z - zIter);
                    dist = Vector3Int.Distance(newBlockCoord, blockCoord);
                    if (dist < blockVisibleRadius)
                        visibleBlocksLevelOfDetail.Add(newBlockCoord, Mathf.FloorToInt(dist));

                    newBlockCoord = new Vector3Int(x - xIter, y, z - zIter);
                    dist = Vector3Int.Distance(newBlockCoord, blockCoord);
                    if (dist < blockVisibleRadius)
                        visibleBlocksLevelOfDetail.Add(newBlockCoord, Mathf.FloorToInt(dist));

                    newBlockCoord = new Vector3Int(x, y - yIter, z);
                    dist = Vector3Int.Distance(newBlockCoord, blockCoord);
                    if (dist < blockVisibleRadius)
                        visibleBlocksLevelOfDetail.Add(newBlockCoord, Mathf.FloorToInt(dist));

                    newBlockCoord = new Vector3Int(x - xIter, y - yIter, z);
                    dist = Vector3Int.Distance(newBlockCoord, blockCoord);
                    if (dist < blockVisibleRadius)
                        visibleBlocksLevelOfDetail.Add(newBlockCoord, Mathf.FloorToInt(dist));

                    newBlockCoord = new Vector3Int(x, y - yIter, z - zIter);
                    dist = Vector3Int.Distance(newBlockCoord, blockCoord);
                    if (dist < blockVisibleRadius)
                        visibleBlocksLevelOfDetail.Add(newBlockCoord, Mathf.FloorToInt(dist));

                    newBlockCoord = new Vector3Int(x - xIter, y - yIter, z - zIter);
                    dist = Vector3Int.Distance(newBlockCoord, blockCoord);
                    if (dist < blockVisibleRadius)
                        visibleBlocksLevelOfDetail.Add(newBlockCoord, Mathf.FloorToInt(dist));

                    xIter += 2;
                }
                yIter += 2;
            }
            zIter += 2;
        }

        //for (int z = blockCoord.z - blockVisibleRadius; z < blockCoord.z + blockVisibleRadius; z++) {
        //    for (int y = blockCoord.y - blockVisibleRadius; y < blockCoord.y + blockVisibleRadius; y++) {
        //        for (int x = blockCoord.x - blockVisibleRadius; x < blockCoord.x + blockVisibleRadius; x++) {

        //            var newBlockCoord = new Vector3Int(x, y, z);

        //            if (Vector3Int.Distance(newBlockCoord, blockCoord) < blockVisibleRadius) {

        //                var lod = Mathf.FloorToInt(Vector3Int.Distance(newBlockCoord, blockCoord));
        //                visibleBlocksLevelOfDetail.Add(newBlockCoord, lod);
        //            }
        //        }
        //    }
        //}
    }

    private void HideBlocks() {

        foreach (MeshBlock block in loadedBlocks.Values) {

            if (!visibleBlocksLevelOfDetail.ContainsKey(block.BlockCoordinate))
                block.SetVisible(false);
        }
    }

    private void RecycleBlocks() {

        MeshBlock block;

        foreach(KeyValuePair<Vector3Int, MeshBlock> pair in loadedBlocks) {

            var coord = pair.Key;
            block = pair.Value;

            if (Vector3Int.Distance(GetBlockCoordinate(characterPosition), coord) > blockLoadRadius) {

                blockRecycling.Enqueue(block);
                block.SetVisible(false);
            }
        }
    }

    private void LoadVisibleBlocks() {

        foreach (KeyValuePair<Vector3Int, int> pair in visibleBlocksLevelOfDetail) {

            Vector3Int coord = pair.Key;
            int lod = pair.Value;

            if (!loadedBlocks.ContainsKey(coord)) {

                loadedBlocks.Add(coord, new(coord, blockContainer.transform, lod));
            }
            else if (loadedBlocks[coord].LOD != lod){

                loadedBlocks[coord].LOD = lod;
            }
        }
    }

    private void ShowVisibleBlocks() {

        foreach(Vector3Int vec in visibleBlocksLevelOfDetail.Keys) {

            loadedBlocks[vec].DrawCurrentMesh();
        }
    }

    private void EmptyRecycling() {

        MeshBlock block;

        while (blockRecycling.Count > 0) {
            block = blockRecycling.Dequeue();
            loadedBlocks.Remove(block.BlockCoordinate);
            Destroy(block.meshObject);
        }
    }

    // Utilities
    private void BlockCoordinatesLoop(int loopRadius, Vector3Int blockCoord, Action<Vector3Int, Vector3Int> action) {

        for (int z = blockCoord.z - loopRadius; z < blockCoord.z + loopRadius; z++) {
            for (int y = blockCoord.y - loopRadius; y < blockCoord.y + loopRadius; y++) {
                for (int x = blockCoord.x - loopRadius; x < blockCoord.x + loopRadius; x++) {

                    var newBlockCoord = new Vector3Int(x, y, z);

                    // do action only for blocks inside a sphere of radius loopRadius
                    if ( Vector3Int.Distance(newBlockCoord, blockCoord) < loopRadius)
                        action(blockCoord, newBlockCoord);
                }
            }
        }
    }

    private Vector3Int GetBlockCoordinate(Vector3 vector) {

        return Vector3Int.FloorToInt(vector / BlockSize);
    }

    #endregion // Methods

    #region Classes

    public class MeshBlock {

        #region Fields

        public GameObject meshObject;
        public Bounds bounds;
        public Vector3Int BlockCoordinate;

        private Dictionary<int, MeshData> _meshes;
        private bool _meshDataRecieved = false;
        private int _lod;
        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;

        #endregion // Fields

        #region Constructor

        public MeshBlock(Vector3Int coord, Transform parent, int lod) {

            BlockCoordinate = coord;
            bounds = new(GetWorldPosition(), Vector3.one * TerrainGenerator.BlockSize);

            _meshes = new();

            meshObject = new GameObject() { name = "Block" + coord };
            meshObject.transform.parent = parent;

            _meshFilter = meshObject.AddComponent<MeshFilter>();

            _meshRenderer = meshObject.AddComponent<MeshRenderer>();
            _meshRenderer.material = terrainGenerator.BlockMaterial;

            LOD = lod;
            SetVisible(false);
        }

        #endregion // Constructor

        #region Properties

        public int LOD {

            get { return _lod; }

            set {

                if (value != _lod)
                    _lod = value;

                if (!_meshes.ContainsKey(value))
                    GenMesh();
                else
                    DrawCurrentMesh();
                
            }
        }

        #endregion // Properties

        #region Methods

        public void DrawCurrentMesh() {

            if (_meshDataRecieved) {
                _meshFilter.mesh = _meshes[_lod].Mesh;
                _meshRenderer.material = terrainGenerator.BlockMaterial;
                SetVisible(true);
            }
        }

        public void GenMesh() {

            _meshDataRecieved = false;
            terrainGenerator.RequestTerrainData(GetWorldPosition(), _lod, OnTerrainDataReceived);
        }

        private void OnTerrainDataReceived(TerrainData data) {

            terrainGenerator.RequestMeshData(data, OnMeshDataReceived);
        }

        private void OnMeshDataReceived(MeshData data) {

            int lod = data.LOD;

            if (!_meshes.ContainsKey(lod)) {

                _meshes.Add(lod, data);
            }
            else {
                _meshes[lod] = data;
            }

            _meshDataRecieved = true;
            LOD = lod;
        }

        public bool ContainsLOD(int index) {

            return _meshes.ContainsKey(index);
        }

        public void SetVisible(bool visible) {

            meshObject.SetActive(visible);
        }

        public bool IsVisible() {

            return meshObject.activeSelf;
        }

        public Vector3Int GetWorldPosition() {
            return BlockCoordinate * TerrainGenerator.BlockSize;
        }

        #endregion // Methods
    }

    #endregion // Classes
}