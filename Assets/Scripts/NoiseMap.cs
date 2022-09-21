using UnityEngine;

public static class NoiseMap {

    // Lacunarity - Amount of detail - 1 or higher
    // Persistance - Amplitude culling - between 0 and 1

    #region Constants

    private const float maxNoise = 2f;
    private const float minNoise = -2f;

    #endregion // Constants

    #region Methods

    public static float[] NoiseValues(float pos, int mapWidth, float noiseScale, int octaves, float persistance, float lacunarity, float offset, bool octaveOffset) {

        if (noiseScale <= 0)
            noiseScale = 0.0001f;
        if (octaves <= 0)
            octaves = 1;

        float halfWidth = mapWidth * 0.5f;

        if (octaveOffset) {
            for (int i = 0; i < octaves; i++) {

                float randW = Random.value;

                offset += randW;
            }
        }

        float[] noiseValues = new float[mapWidth];

        for (int x = 0; x < mapWidth; x++) {

            float amplitude = 1f;
            float frequency = 1f;
            float noiseHeight = 0;

            for (int i = 0; i < octaves; i++) {

                float pointX = x - halfWidth + pos;

                float sampleX = pointX / noiseScale * frequency + offset;
                float noiseValue = NoiseGen.Perlin(sampleX);
                noiseHeight += noiseValue * amplitude;

                amplitude *= persistance;
                frequency *= lacunarity;
            }

            noiseValues[x] = noiseHeight;
        }

        // Normalize output value
        for (int x = 0; x < mapWidth; x++) {

            noiseValues[x] = Mathf.InverseLerp(minNoise, maxNoise, noiseValues[x]);
        }

        return noiseValues;
    }

    public static float[,] NoiseValues(Vector2 pos, int mapWidth, int mapDepth, float noiseScale,
        int octaves, float persistance, float lacunarity, Vector2 offsets, bool octaveOffsets) {

        if (noiseScale <= 0)
            noiseScale = 0.0001f;
        if (octaves <= 0)
            octaves = 1;

        float halfWidth = mapWidth * 0.5f;
        float halfDepth = mapDepth * 0.5f;

        if (octaveOffsets) {
            for (int i = 0; i < octaves; i++) {

                float randW = Random.value;
                float randD = Random.value;

                offsets.x += randW;
                offsets.y += randD;
            }
        }

        float[,] noiseValues = new float[mapWidth, mapDepth];

        for (int d = 0; d < mapDepth; d++) {
            for (int w = 0; w < mapWidth; w++) {

                float amplitude = 1f;
                float frequency = 1f;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++) {

                    float pointW = w - halfWidth + pos.x;
                    float pointD = d - halfDepth + pos.y;

                    float sampleW = pointW / noiseScale * frequency + offsets.x;
                    float sampleD = pointD / noiseScale * frequency + offsets.y;
                    float noiseValue = NoiseGen.Perlin(sampleW, sampleD);
                    noiseHeight += noiseValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                noiseValues[w, d] = noiseHeight;
            }
        }

        // Normalize output value
        for (int d = 0; d < mapDepth; d++) {
            for (int w = 0; w < mapWidth; w++) {

                noiseValues[w, d] = Mathf.InverseLerp(minNoise, maxNoise, noiseValues[w, d]);
            }
        }

        return noiseValues;
    }

    public static float[,,] NoiseValues(Vector3 pos, int mapWidth, int mapHeight, int mapDepth, int seed, NoiseSettings settings) {

        var noiseScale = settings.scale;
        var octaves = settings.octaves;
        var octaveOffsets = settings.octaveOffsets;
        var persistance = settings.persistance;
        var offsets = settings.offsets;
        var lacunarity = settings.lacunarity;

        if (noiseScale <= 0)
            noiseScale = 0.0001f;
        if (octaves <= 0)
            octaves = 1;

        System.Random nrand = new System.Random(seed);
        Vector3[] totalOffsets = new Vector3[octaves];

        if (octaveOffsets) {
            for (int i = 0; i < octaves; i++) {

                float randW = nrand.Next(-100000, 100000);
                float randH = nrand.Next(-100000, 100000);
                float randD = nrand.Next(-100000, 100000);

                totalOffsets[i] = new Vector3(offsets.x + randW, offsets.y + randH, offsets.z + randD);
            }
        }

        float halfWidth = mapWidth * 0.5f;
        float halfHeight = mapHeight * 0.5f;
        float halfDepth = mapDepth * 0.5f;
        float[,,] noiseValues = new float[mapWidth, mapHeight, mapDepth];

        for (int w = 0; w < mapDepth; w++) {
            for (int h = 0; h < mapHeight; h++) {
                for (int l = 0; l < mapWidth; l++) {

                    float amplitude = 1f;
                    float frequency = 1f;
                    float noiseHeight = 0;

                    float pointL = l - halfWidth + pos.x;
                    float pointH = h - halfHeight + pos.y;
                    float pointW = w - halfDepth + pos.z;

                    for (int i = 0; i < octaves; i++) {

                        float sampleL = pointL / noiseScale * frequency + totalOffsets[i].x;
                        float sampleH = pointH / noiseScale * frequency + totalOffsets[i].y;
                        float sampleW = pointW / noiseScale * frequency + totalOffsets[i].z;

                        float noiseValue = NoiseGen.Perlin(sampleL, sampleH, sampleW);
                        noiseHeight += noiseValue * amplitude;

                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    noiseValues[l, h, w] = noiseHeight;
                }
            }
        }

        // Normalize output value
        for (int w = 0; w < mapDepth; w++) {
            for (int h = 0; h < mapHeight; h++) {
                for (int l = 0; l < mapWidth; l++) {

                    noiseValues[l, h, w] = Mathf.InverseLerp(minNoise, maxNoise, noiseValues[l, h, w]);
                }
            }
        }

        return noiseValues;
    }

    // Thinking about trying to keep the coordinate data input into the noise function for use later, but unsure if necessary
    public static Vector4[,,] NoiseValues4(Vector3 pos, int mapWidth, int mapHeight, int mapDepth, float noiseScale,
    int octaves, float persistance, float lacunarity, Vector3 offsets, bool octaveOffsets) {

        if (noiseScale <= 0)
            noiseScale = 0.0001f;
        if (octaves <= 0)
            octaves = 1;

        float halfWidth = mapWidth * 0.5f;
        float halfHeight = mapHeight * 0.5f;
        float halfDepth = mapDepth * 0.5f;

        if (octaveOffsets) {
            for (int i = 0; i < octaves; i++) {

                float randW = Random.value;
                float randH = Random.value;
                float randD = Random.value;

                offsets.x += randW;
                offsets.y += randH;
                offsets.z += randD;
            }
        }

        Vector4[,,] noiseValues = new Vector4[mapWidth, mapHeight, mapDepth];

        for (int w = 0; w < mapDepth; w++) {
            for (int h = 0; h < mapHeight; h++) {
                for (int l = 0; l < mapWidth; l++) {

                    float amplitude = 1f;
                    float frequency = 1f;
                    float noiseHeight = 0;

                    float pointL = l - halfWidth + pos.x;
                    float pointH = h - halfHeight + pos.y;
                    float pointW = w - halfDepth + pos.z;

                    for (int i = 0; i < octaves; i++) {

                        float sampleL = pointL / noiseScale * frequency + offsets.x;
                        float sampleH = pointH / noiseScale * frequency + offsets.y;
                        float sampleW = pointW / noiseScale * frequency + offsets.z;
                        float noiseValue = NoiseGen.Perlin(sampleL, sampleH, sampleW);
                        noiseHeight += noiseValue * amplitude;

                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    noiseValues[l, h, w] = new Vector4(pointL, pointH, pointW, noiseHeight);
                }
            }
        }

        // Normalize output value
        for (int w = 0; w < mapDepth; w++) {
            for (int h = 0; h < mapHeight; h++) {
                for (int l = 0; l < mapWidth; l++) {

                    noiseValues[l, h, w].w = Mathf.InverseLerp(minNoise, maxNoise, noiseValues[l, h, w].w);
                }
            }
        }

        return noiseValues;
    }

    #endregion // Methods

}

public struct NoiseSettings {

    public float scale;
    public int octaves;
    public float persistance;
    public float lacunarity;
    public Vector3 offsets;
    public bool octaveOffsets;

    public NoiseSettings(float scale, int octaves, float persistance, float lacunarity, Vector3 offsets, bool octaveOffsets) {

        this.scale = scale;
        this.octaves = octaves;
        this.persistance = persistance;
        this.lacunarity = lacunarity;
        this.offsets = offsets;
        this.octaveOffsets = octaveOffsets;

    }
}