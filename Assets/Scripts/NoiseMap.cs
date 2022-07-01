using UnityEngine;

public static class NoiseMap {

    #region Methods

    public static float[,] NoiseMap2D(Vector2Int pos, int mapWidth, int mapHeight, float noiseScale, int octaves, float persistance, float lacunarity, Vector2 offsets) {

        if (noiseScale <= 0)
            noiseScale = 0.0001f;
        if (octaves <= 0)
            octaves = 1;

        int halfWidth = (int)(mapWidth * 0.5f);
        int halfHeight = (int)(mapHeight * 0.5f);

        Vector2Int minVec = new(pos.x - halfWidth, pos.y - halfHeight);
        Vector2Int maxVec = new(pos.x + halfWidth, pos.y + halfHeight);

        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++) {

            float randx = Random.value;
            float randy = Random.value;
            octaveOffsets[i] = new Vector2(randx, randy);
        }

        float maxNoise = float.MinValue;
        float minNoise = float.MaxValue;

        float[,] noiseMap = new float[mapWidth, mapHeight];

        for (int y = minVec.y; y < maxVec.y; y++) {
            for (int x = minVec.x; x < maxVec.x; x++) {

                float amplitude = 1f;
                float frequency = 1f;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++) {

                    float sampleX = x /  noiseScale * frequency + octaveOffsets[i].x + offsets.x;
                    float sampleY = y /  noiseScale * frequency + octaveOffsets[i].y + offsets.y;
                    float noiseValue = NoiseGen.Perlin(sampleX, sampleY);
                    noiseHeight += noiseValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if(noiseHeight > maxNoise)
                    maxNoise = noiseHeight;
                else if(noiseHeight < minNoise)
                    minNoise = noiseHeight;

                noiseMap[x, y] = noiseHeight;
            }
        }

        // Normalize output value
        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {

                noiseMap[x, y] = Mathf.InverseLerp(minNoise, maxNoise, noiseMap[x,y]);
            }
        }

        return noiseMap;
    }

    public static float[,,] NoiseMap3D(Vector3 pos, int mapWidth, int mapHeight, int mapDepth, float noiseScale, int octaves, float persistance, float lacunarity, Vector3 offsets) {

        if (noiseScale <= 0)
            noiseScale = 0.0001f;
        if (octaves <= 0)
            octaves = 1;

        int halfWidth = (int)(mapWidth * 0.5f);
        int halfHeight = (int)(mapHeight * 0.5f);
        int halfDepth = (int)(mapDepth * 0.5f);

        //Vector3[] octaveOffsets = new Vector3[octaves];

        //for (int i = 0; i < octaves; i++) {

        //    float randx = Random.value;
        //    float randy = Random.value;
        //    float randz = Random.value;

        //    octaveOffsets[i] = new Vector3(randx, randy, randz);
        //}

        float maxNoise = 2f;
        float minNoise = -2f;

        float[,,] noiseMap = new float[mapWidth, mapHeight, mapDepth];

        for (int z = 0; z < mapDepth; z++) {
            for (int y = 0; y < mapHeight; y++) {
                for (int x = 0; x < mapWidth; x++) {

                    float amplitude = 1f;
                    float frequency = 1f;
                    float noiseHeight = 0;

                    for (int i = 0; i < octaves; i++) {

                        float pointX = x - halfWidth + pos.x;
                        float pointY = y - halfHeight + pos.y;
                        float pointZ = z - halfDepth + pos.z;

                        float sampleX = pointX / noiseScale * frequency /*+ octaveOffsets[i].x*/ + offsets.x;
                        float sampleY = pointY / noiseScale * frequency /*+ octaveOffsets[i].y*/ + offsets.y;
                        float sampleZ = pointZ / noiseScale * frequency /*+ octaveOffsets[i].z*/ + offsets.z;
                        float noiseValue = NoiseGen.Perlin(sampleX, sampleY, sampleZ);
                        noiseHeight += noiseValue * amplitude;

                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    //if (noiseHeight > maxNoise)
                    //    maxNoise = noiseHeight;
                    //else if (noiseHeight < minNoise)
                    //    minNoise = noiseHeight;

                    noiseMap[x, y, z] = noiseHeight;
                }
            }
        }

        // Normalize output value
        for (int z = 0; z < mapDepth; z++) {
            for (int y = 0; y < mapHeight; y++) {
                for (int x = 0; x < mapWidth; x++) {

                    noiseMap[x, y, z] = Mathf.InverseLerp(minNoise, maxNoise, noiseMap[x, y, z]);
                }
            }
        }

        return noiseMap;
    }

    public static float[,,] NoiseMap3D(Vector3 pos, int mapWidth, int mapHeight, int mapDepth, float noiseScale, int octaves, float persistance, float lacunarity) {

        return NoiseMap3D(pos, mapWidth, mapHeight, mapDepth, noiseScale, octaves, persistance, lacunarity, new Vector3(0, 0, 0));
    }

    #endregion // Methods

}
