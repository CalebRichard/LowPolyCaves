


using UnityEngine;

[CreateAssetMenu]
public class SettingsTerrain : ScriptableObject {

    [Range(0,1)]
    public float surfaceValue = 0.5f;

	public float noiseScale = 6f; // Greater than 0
	public int octaves = 1; // Greater than 1
	[Range(0, 1)]
	public float persistance = 0.6f;
	public float lacunarity = 1.5f; // Greater than 1
	public Vector3 noiseOffset = Vector3.zero;
	public bool octaveOffsets = false;

	public NoiseSettings GetSettings() {

		return new()
		{
			scale = noiseScale,
			octaves = octaves,
			persistance = persistance,
			lacunarity = lacunarity,
			offsets = noiseOffset,
			octaveOffsets = octaveOffsets
		};
	}
}
