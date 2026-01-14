using System;
using System.Collections.Generic;

/// <summary>
/// Data container for the world grid and all pieces on the grid.
/// </summary>
public class World
{
    private readonly GridCell[,] _cells;
    public int Width { get; private set; }
    public int Height { get; private set;}
    public IReadOnlyList<Piece> Towers { get; private set; }
    public IReadOnlyList<Piece> Platforms { get; private set; } 

    public World(int width, int height)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentException("Width and Height must be positive integers.");

        Width = width;
        Height = height;

        Towers = new List<Piece>();
        Platforms = new List<Piece>();

        _cells = new GridCell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _cells[x, y] = new GridCell(x, y);
            }
        }
    }

    public void AddPiece(Piece piece, CellContent contentType)
    {
        foreach (var localCell in piece.PieceShape.LocalCells)
        {
            int worldX = piece.OriginCell.X + localCell.X;
            int worldY = piece.OriginCell.Y + localCell.Y;

            if (worldX < 0 || worldX >= Width || worldY < 0 || worldY >= Height)
                throw new ArgumentException("Piece placement is out of world bounds.");

            var cell = _cells[worldX, worldY];
            cell.Content = contentType;

            if (contentType == CellContent.Platform)
                cell.platformInstance = piece;
            else if (contentType == CellContent.Tower)
                cell.towerInstance = piece;
        }

        if (contentType == CellContent.Platform)
            ((List<Piece>)Platforms).Add(piece);
        else if (contentType == CellContent.Tower)
            ((List<Piece>)Towers).Add(piece);
        else
            throw new ArgumentException("Invalid content type for piece placement.");
    }

    /// <summary>
    /// Returns a string representing the state of the world
    /// for debugging purposes. (0,0) is bottom-left.
    /// </summary>
    public string GetVisualRepresentation()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int y = Height - 1; y >= 0; y--)
        {
            for (int x = 0; x < Width; x++)
            {
                var cell = _cells[x, y];
                char c = '.';
                if (cell.Content == CellContent.Platform)
                    c = 'P';
                else if (cell.Content == CellContent.Tower)
                    c = 'T';
                sb.Append(c);
            }
            sb.AppendLine();
        }
        return Utils.NormalizeNewlines(sb.ToString().TrimEnd());
    }

    public GridCell GetCell(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            throw new ArgumentOutOfRangeException("Coordinates are out of world bounds.");
        return _cells[x, y];
    }

    public GridCell GetCell(GridCoord coord)
    {
        return GetCell(coord.X, coord.Y);
    }
}