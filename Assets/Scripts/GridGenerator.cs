#region Using

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#endregion // Using

public class GridGenerator : MonoBehaviour {

	#region Constants



	#endregion // Constants

	#region Fields

	int m_resolution;
	Vector3Int playerPos;

	#endregion // Fields

	#region Constructors



	#endregion // Constructors

	#region Properties

	public GameObject Player;
	public Dictionary<Vector3Int, float> Noise;
	public int Radius;

    #endregion // Properties

    #region Methods

    private void Awake() {

		Radius = 10;
		m_resolution = 11;
		playerPos = Vector3Int.FloorToInt(Player.transform.position);

		Noise = new();
		GenerateGrid();
	}

    private void Start() {


    }

    private void Update() {

		if (Vector3Int.FloorToInt(Player.transform.position) != playerPos)
			GenerateGrid();
    }

    public void GenerateGrid() {

		Noise.Clear();

		var res = m_resolution;
		var playerPos = Vector3Int.FloorToInt(Player.transform.position);
		var minVec = playerPos - Vector3Int.one * Radius;
		var maxVec = playerPos + Vector3Int.one * Radius;

        for (int z = minVec.z; z < maxVec.z; z++) {
            for (int y = minVec.y; y < maxVec.y; y++) {
                for (int x = minVec.x; x < maxVec.x; x++) {

					var pos = new Vector3Int(x, y, z);
					var magPos = playerPos - pos;
					var mag = magPos.magnitude;

					//Debug.Log(mag);
					if(mag < Radius) {

						float noise = (NoiseGen.Perlin((float)x / res, (float)y / res, (float)z / res) + 1) * 0.5f;
						//Debug.Log(noise);
						Noise.Add(pos, noise);
                    }
                }
            }
        }
    }

	private void OnDrawGizmos() {

        if (Noise != null) {
			foreach (KeyValuePair<Vector3Int, float> noisePair in Noise) {

				Gizmos.color = Color.Lerp(Color.black, new Color(1, 1, 1, 0), noisePair.Value);
				Gizmos.DrawSphere(noisePair.Key, 0.1f);
			}
		}
	}

    #endregion // Methods

}
