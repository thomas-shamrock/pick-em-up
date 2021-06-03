using UnityEngine;
using System.Collections;

public static class TextureGenerator {

	public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height) {
		Texture2D texture = new Texture2D (width, height);
		texture.filterMode = FilterMode.Bilinear;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.SetPixels (colourMap);
		texture.Apply ();
		return texture;
	}


	public static Texture2D TextureFromHeightMap(float[,] heightMap) {
		int width = heightMap.GetLength (0);
		int height = heightMap.GetLength (1);

		Color[] colourMap = new Color[width * height];
		for (int y = 0; y < height; y++) {
			for (int x = 0; x < width; x++) {
				colourMap [y * width + x] = Color.Lerp (Color.black, Color.white, heightMap [x, y]);
			}
		}

		return TextureFromColourMap (colourMap, width, height);
	}

	public static Texture2D TiledTextureFromHeightMap(Texture2D[] tiles, int tilingX, int tilingY, float[,] heightMap, TerrainType[] regions)
	{
		Texture2D[] tiledTextures = new Texture2D[tiles.Length];
		for (int i = 0; i < tiledTextures.Length; i++)
		{
			tiledTextures[i] = TiledTexture(tiles[i], tilingX, tilingY);
		}
		int width = tiledTextures[0].width;
		int height = tiledTextures[0].height;
		int heightMapWidth = heightMap.GetLength(0);
		int heightMapHeight = heightMap.GetLength(1);
		int xMultiplier = width / heightMapWidth;
		int yMultiplier = height / heightMapHeight;
		Texture2D texture = new Texture2D(width, height);
		Color[] colorMap = texture.GetPixels();

		for (int i = 0; i < tiledTextures.Length; i++)
		{
			Color[] tiledTextureColorMap = tiledTextures[i].GetPixels();
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					float currentHeight = heightMap[x / xMultiplier, y / yMultiplier];
					if (currentHeight >= regions[i].height)
					{
						colorMap[y * width + x] = tiledTextureColorMap[y * width + x];
					}
				}
			}
		}
		texture.SetPixels(colorMap);
		texture.Apply();
		return texture;
	}

	private static Texture2D TiledTexture(Texture2D tile, int tilingX, int tilingY)
	{
		int width = tile.width * tilingX + 1;
		int height = tile.height * tilingY + 1;
		Color[] tileColorMap = tile.GetPixels();

		Texture2D texture = new Texture2D(width, height);
		for (int y = 0; y < tilingY; y++)
		{
			for (int x = 0; x < tilingX; x++)
			{
				texture.SetPixels(x * tile.width, y * tile.height, tile.width, tile.height, tileColorMap);
			}
		}
		texture.filterMode = FilterMode.Bilinear;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.Apply();
		return texture;
	}
}