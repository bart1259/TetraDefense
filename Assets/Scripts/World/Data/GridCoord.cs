public struct GridCoord
{
    public readonly int X;
    public readonly int Y;

    public GridCoord(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public override bool Equals(object obj)
    {
        if (obj is GridCoord other)
        {
            return X == other.X && Y == other.Y;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return (X, Y).GetHashCode();
    }

    public static GridCoord zero => new GridCoord(0, 0);

    public override string ToString()
    {
        return $"({X}, {Y})";
    }
}