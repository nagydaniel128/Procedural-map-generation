using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float [,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale, bool falloffIsOn = true)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        //needs the scale to be greater than 0
        if(scale <= 0)
        {
            scale = 0.0001f;
        }

        //randomize the noisemap with PerlinNoise
        int random = Random.Range(0, 100000);
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float sampleX = (x + random) / scale;
                float sampleY = (y + random) / scale;

                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                noiseMap[x, y] = perlinValue;
            }
        }

        if (falloffIsOn)
        {
            //falloff
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    float sampleX = y / (float)mapHeight * 2 - 1;
                    float sampleY = x / (float)mapWidth * 2 - 1;

                    float value = (Mathf.Max(Mathf.Abs(sampleX), Mathf.Abs(sampleY))) * 1f;

                    //evaluate
                    float a = 3;
                    float b = 2.2f;

                    value = Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));

                    noiseMap[x, y] -= value;
                }
            }
        }

        return noiseMap;
    }
}
