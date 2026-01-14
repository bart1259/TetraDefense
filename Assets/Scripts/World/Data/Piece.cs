
/// <summary>
/// An actual instance of a piece in the world with location and rotation.
/// </summary>
public class Piece
{
    public PieceShape LocalShape { get; private set; }
    private int _rotation = 0;
    public int Rotation {
        get
        {
            return _rotation;
        } 
        set { 
            if (value % 30 != 0)
                throw new System.ArgumentException("Rotation must be a multiple of 90 degrees.");
            _rotation = (360 + value) % 360; 
        } 
    }
    public PieceShape PieceShape
    {
        get { return LocalShape.GetRotation(Rotation); }
    }
    public GridCoord OriginCell { get; set; }
    public int Width { get { return PieceShape.Width; } }
    public int Height { get { return PieceShape.Height; } }

    public Piece(PieceShape shape, GridCoord originCell, int rotation = 0)
    {
        this.LocalShape = shape;
        this.OriginCell = originCell;
        this.Rotation = rotation;
    }

    public Piece(PieceShape shape) : this(shape, GridCoord.zero, 0) {}
    public Piece(PieceShape shape, GridCoord originCell) : this(shape, originCell, 0) {}
}