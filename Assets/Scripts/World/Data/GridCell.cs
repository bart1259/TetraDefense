
public class GridCell
{
    public GridCoord Coord { get; private set; }
    public CellContent Content { get; set; }

    public GridCell(int x, int y)
    {
        Coord = new GridCoord(x, y);
        Content = CellContent.Empty;
    }

    public Piece platformInstance { get; set; }
    public Piece towerInstance { get; set; }
}