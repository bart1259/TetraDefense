using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class TextureUtils {

    public static Texture2D CreatePieceTexture(PieceShape shape, Color color, Color backgroundColor, Color borderColor, int cellSize, int maxSize)
    {
        if (shape.Width > maxSize || shape.Height > maxSize)
            throw new ArgumentException("Shape dimensions exceed the maximum allowed size.");

        int MARGIN_CELLS = 1;

        int imageWidth = (maxSize + MARGIN_CELLS * 2) * cellSize;
        int imageHeight = (maxSize + MARGIN_CELLS * 2) * cellSize;

        Color32[] colors = new Color32[imageWidth * imageHeight];
        // Fill black
        for (int x = 0; x < imageWidth; x++)
        {
            for (int y = 0; y < imageHeight; y++)
            {
                colors[y * imageWidth + x] = backgroundColor;
            }
        }

        for (int cx = 0; cx < maxSize + MARGIN_CELLS * 2; cx++)
        {
            for (int cy = 0; cy < maxSize + MARGIN_CELLS * 2; cy++)
            {
                // Given cell cx, and cy, we need to first cast it to local coords, we'll do this naively
                int localX = cx - MARGIN_CELLS + shape.Width/2 - maxSize/2;
                int localY = cy - MARGIN_CELLS + shape.Height/2 - maxSize/2;

                if (localX < 0 || localY < 0 || localX >= shape.Width || localY >= shape.Height)
                    continue;

                GridCoord coord = new GridCoord(localX, localY);
                if (!shape.LocalCells.Contains(coord))
                    continue;

                for (int x = 0; x < cellSize; x++)
                {
                    for (int y = 0; y < cellSize; y++)
                    {
                        int pixelX = cx * cellSize + x;
                        int pixelY = cy * cellSize + y;
                        if (x == 0 || y == 0 || x == cellSize - 1 || y == cellSize - 1)
                        {
                            colors[pixelY * imageWidth + pixelX] = borderColor;
                        } else
                        {
                            colors[pixelY * imageWidth + pixelX] = color;
                        }
                    }
                }
            }
        }

        Texture2D tex = new Texture2D(imageWidth, imageHeight);
        tex.SetPixels32(colors);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return tex;
    }


}