using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Won't be more instance of it: static
//Won't apply it on other objects:no inheritation of MonoBehaviour
public static class Noise
{

	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
	{
		//we want this to have a method for generating a noise map
		//and we want that to return essentially a grid of values between 0 and 1
		float[,] noiseMap = new float[mapWidth, mapHeight];

		System.Random prng = new System.Random(seed);
		Vector2[] octaveOffsets = new Vector2[octaves];
		for (int i = 0; i < octaves; i++)
		{
			float offsetX = prng.Next(-100000, 100000) + offset.x;
			float offsetY = prng.Next(-100000, 100000) + offset.y;
			octaveOffsets[i] = new Vector2(offsetX, offsetY);
		}

		//If scale = 0, we would divide by 0 :(
		if (scale <= 0)
		{
			scale = 0.0001f;
		}

		float maxNoiseHeight = float.MinValue;
		float minNoiseHeight = float.MaxValue;

		//just so when we zoom in, woo zoom in the center, not the top right corner
		float halfWidth = mapWidth / 2f;
		float halfHeight = mapHeight / 2f;


		for (int y = 0; y < mapHeight; y++)
		{
			for (int x = 0; x < mapWidth; x++)
			{

				float amplitude = 1;
				float frequency = 1;
				//current heightValue
				float noiseHeight = 0;

				//loop through all of our octaves
				for (int i = 0; i < octaves; i++)
				{
					//now we want to figure out at which point we'll be sampling our height values from
					//the higher the frequency the  further apart the sample points will be
					float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
					float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;


					//by default this will be betweeen 0 and 1, to get more interesting noises we change it to be between -1 and 1
					float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

					//instead of just setting the noise map directly equal to the perlin noise
					//we'll want to increase the noise height by the perlin value of each octave so
					noiseHeight += perlinValue * amplitude;

				amplitude *= persistance;
				frequency *= lacunarity;
				}

				if (noiseHeight > maxNoiseHeight)
				{
					maxNoiseHeight = noiseHeight;
				}
				else if (noiseHeight < minNoiseHeight)
				{
					minNoiseHeight = noiseHeight;
				}
				noiseMap[x, y] = noiseHeight;
			}
		}

		for (int y = 0; y < mapHeight; y++)
		{
			for (int x = 0; x < mapWidth; x++)
			{
				noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
			}
		}

		return noiseMap;
	}

}
