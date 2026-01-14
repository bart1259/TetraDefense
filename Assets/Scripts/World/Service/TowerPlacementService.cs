
public enum TowerPlacementResult
{
    Success,
    OutOfBounds,
    Collision,
    BlocksPathfinding
}

public class TowerPlacementService
{
    private readonly World _world;
    private readonly PathFindingService _pathFindingService;

    public TowerPlacementService(World world, PathFindingService pathFindingService)
    {
        _world = world;
        _pathFindingService = pathFindingService;
    }

    public bool TryPlacePiece (Piece piece, CellContent cell)
    {
        if (CanPlacePiece(piece, cell) == TowerPlacementResult.Success)
        {
            _world.AddPiece(piece, cell);
            return true;
        }
        return false;
    }

    /// <summary>
    /// The big functionality of this service lives in this method.
    /// </summary>
    public TowerPlacementResult CanPlacePiece(Piece piece, CellContent cell)
    {
        // 1. Check bounds
        if (!PieceInBounds(piece.PieceShape, piece.OriginCell))
            return TowerPlacementResult.OutOfBounds;
        // 2. Check for collisions
        if (PieceCollides(piece.PieceShape, piece.OriginCell, cell))
            return TowerPlacementResult.Collision;
        // 3. Check for pathfinding
        if (_pathFindingService.PathExists(piece) == false)
            return TowerPlacementResult.BlocksPathfinding;

        return TowerPlacementResult.Success;
    }

    public TowerPlacementResult CanUpgradeTower(GridCoord upgradeCoord)
    {
        GridCell cell = _world.GetCell(upgradeCoord);
        if (cell.Content != CellContent.Platform)
            return TowerPlacementResult.Collision;

        return TowerPlacementResult.Success;
    }

    private bool PieceInBounds(PieceShape shape, GridCoord coord)
    {
        if (coord.X < 0 || coord.Y < 0)
            return false;
        if (coord.X + shape.Width > _world.Width)
            return false;
        if (coord.Y + shape.Height > _world.Height)
            return false;
        return true;
    }

    private bool PieceCollides(PieceShape shape, GridCoord coord, CellContent cellType)
    {
        foreach (var cell in shape.LocalCells)
        {
            GridCoord worldCell = new GridCoord(coord.X + cell.X, coord.Y + cell.Y);
            GridCell gridCell = _world.GetCell(worldCell);
            // If we're placing a platform, we need an empty cell
            if (cellType == CellContent.Platform && gridCell.Content != CellContent.Empty)
                return true;
            // If we're placing a tower, we need a platform cell
            if (cellType == CellContent.Tower && gridCell.Content != CellContent.Platform)
                return true;
        }
        return false;
    }
}