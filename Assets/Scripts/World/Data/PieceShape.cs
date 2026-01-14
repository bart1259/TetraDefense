using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PieceShape
{
    public IReadOnlyList<GridCoord> LocalCells { get; private set; }

    public int Width
    {
        get { return LocalCells.Max(c => c.X) + 1; }
    }

    public int Height
    {
        get { return LocalCells.Max(c => c.Y) + 1; }
    }

    /// <summary>
    /// Creates a PieceShape from a string representation. The origin
    /// Is in the bottom left.
    /// 1001
    /// 1111
    /// 1001
    /// ^ Origin
    /// </summary>
    public static PieceShape FromString(string str)
    {
        str = str.TrimEnd('\n'); // Remove trailing newlines
        str = Utils.NormalizeNewlines(str);

        string[] lines = str.Split("\n");
        if (lines.Length == 0)
            throw new System.ArgumentException("PieceShape string cannot be empty."); // This may be impossible to reach
        int height = lines.Length;
        int width = lines[0].Length;
        List<GridCoord> cells = new List<GridCoord>();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                char c = lines[y][x];
                if (c == '1')
                {
                    cells.Add(new GridCoord(x, height - y - 1));
                } else if (c != '0')
                {
                    throw new System.ArgumentException("PieceShape string can only contain '1', '0' and newlines. Found '" + c + "'.");
                }
            }
        }
        if (cells.Count == 0)
            throw new System.ArgumentException("PieceShape must have at least one cell. Input (" + width + "," + height + ") has none.");
        
        PieceShape shape = new PieceShape();
        shape.LocalCells = cells;

        if (shape.Width != width || shape.Height != height)
            throw new System.ArgumentException("PieceShape string is not tight");

        return shape;
    }

    public PieceShape GetRotation(int rotation)
    {
        if (rotation % 90 != 0)
            throw new System.ArgumentException("Rotation must be a multiple of 90 degrees.");

        rotation = (360 + rotation) % 360;
        if (rotation == 0)
            return this;

        PieceShape rotatedShape = new PieceShape();
        rotatedShape.LocalCells = this.LocalCells;
        for (int i = 0; i < rotation / 90; i++)
        {
            List<GridCoord> newCells = new List<GridCoord>();
            foreach (var cell in rotatedShape.LocalCells)
            {
                // 90 degree rotation clockwise: (x, y) -> (y, width - x - 1)
                newCells.Add(new GridCoord(cell.Y, rotatedShape.Width - cell.X - 1));
            }
            rotatedShape.LocalCells = newCells;
        }

        return rotatedShape;
    }

    public string GetVisualRepresentation()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int y = Height - 1; y >= 0; y--)
        {
            for (int x = 0; x < Width; x++)
            {
                if (LocalCells.Contains(new GridCoord(x, y)))
                    sb.Append('1');
                else
                    sb.Append('0');
            }
            if (y > 0)
                sb.AppendLine();
        }

        return Utils.NormalizeNewlines(sb.ToString());
    }
}