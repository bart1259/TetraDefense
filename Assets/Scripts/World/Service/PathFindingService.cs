using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFindingService
{
    private readonly World _world;
    private List<GridCoord> defaultStartingCells;
    private List<GridCoord> defaultEndingCells;

    public PathFindingService(World world)
    {
        _world = world;
        defaultStartingCells = new List<GridCoord>();
        defaultEndingCells = new List<GridCoord>();

        for (int y = 0; y < _world.Height; y++)
        {
            defaultStartingCells.Add(new GridCoord(0, y));
            defaultEndingCells.Add(new GridCoord(_world.Width - 1, y));
        }

        // Prioritization
        defaultStartingCells = defaultStartingCells.OrderBy(x => Mathf.Abs(x.Y - (_world.Height / 2))).ToList();
    }

    public void SetStartingCells(List<GridCoord> startingCells)
    {
        defaultStartingCells.Clear();
        defaultStartingCells.AddRange(startingCells);
    }

    private bool IsCellWalkable(GridCoord coord, Piece additionalPiece = null)
    {
        if (coord.X < 0 || coord.Y < 0 || coord.X >= _world.Width || coord.Y >= _world.Height)
            return false;

        var cell = _world.GetCell(coord.X, coord.Y);
        if (cell.Content == CellContent.Tower || cell.Content == CellContent.Platform)
            return false;

        if (additionalPiece != null)
        {
            foreach (var localCell in additionalPiece.PieceShape.LocalCells)
            {
                GridCoord worldCell = new GridCoord(additionalPiece.OriginCell.X + localCell.X, additionalPiece.OriginCell.Y + localCell.Y);
                if (worldCell.Equals(coord))
                {
                    return false;
                }
            }
        }

        return true;
    }

    // TODO: Optimization: Cache this result
    private GridPath FindPathFromTo(GridCoord start, List<GridCoord> endCells, Piece additionalPiece = null)
    {
        // Bredth-first search implementation
        List<GridCoord> visited = new List<GridCoord>();
        Queue<GridCoord> frontier = new Queue<GridCoord>();
        Dictionary<GridCoord, GridCoord?> cameFrom = new Dictionary<GridCoord, GridCoord?>();

        frontier.Enqueue(start);
        cameFrom[start] = null;

        while (frontier.Count > 0)
        {
            GridCoord? current = frontier.Peek();

            if (current == null)
                return null; // We couldn't find a path
            if (endCells.Contains(current.Value))
            {
                // Reconstruct path
                List<GridCoord> pathCells = new List<GridCoord>();
                GridCoord? step = current.Value;
                while (step.HasValue)
                {
                    pathCells.Add(step.Value);
                    step = cameFrom[step.Value];
                }
                pathCells.Reverse();
                return new GridPath(pathCells);
            }

            frontier.Dequeue();

            // Explore neighbors
            List<GridCoord> neighbors = new List<GridCoord>
            {
                new GridCoord(current.Value.X + 1, current.Value.Y),
                new GridCoord(current.Value.X - 1, current.Value.Y),
                new GridCoord(current.Value.X, current.Value.Y + 1),
                new GridCoord(current.Value.X, current.Value.Y - 1)
            };

            visited.Add(current.Value);

            foreach (var neighbor in neighbors)
            {
                // Check bounds
                if (neighbor.X < 0 || neighbor.Y < 0 || neighbor.X >= _world.Width || neighbor.Y >= _world.Height)
                    continue;



                if (!IsCellWalkable(neighbor, additionalPiece))
                    continue;

                var cell = _world.GetCell(neighbor.X, neighbor.Y);

                if (!visited.Contains(neighbor) && !frontier.Contains(neighbor))
                {
                    frontier.Enqueue(neighbor);
                    cameFrom[neighbor] = current.Value;
                }
            }

        }
        return null;
    }


    /// <summary>
    /// Returns if a path exists between any of the startCells and any of the endCells.
    /// </summary>
    /// <param name="startCells">In priority order, the starting cells to search from.</param>
    public GridPath FindBestPath(List<GridCoord> startCells, List<GridCoord> endCells, Piece additionalPiece=null)
    {
        GridPath bestPath = null;
        int minPathLength = int.MaxValue;

        foreach (var start in startCells)
        {
            if (!IsCellWalkable(start, additionalPiece))
                continue;

            GridPath path = FindPathFromTo(start, endCells, additionalPiece);
            if ((path != null && bestPath == null) || (path != null && path.Cells.Count < minPathLength))
            {
                bestPath = path;
                minPathLength = path.Cells.Count;
            }
        }
        return bestPath;
    }


    public GridPath FindBestPath(Piece additionalPiece=null)
    {
        return FindBestPath(defaultStartingCells, defaultEndingCells, additionalPiece);
    }

    public bool PathExists(List<GridCoord> startCells, List<GridCoord> endCells, Piece additionalPiece=null)
    {
        foreach (var start in startCells)
        {
            if (!IsCellWalkable(start, additionalPiece))
                continue;

            GridPath path = FindPathFromTo(start, endCells, additionalPiece);
            if(path != null)
            {
                return true;
            }
        }
        return false;
    }

    public bool PathExists(Piece additionalPiece)
    {
        return PathExists(defaultStartingCells, defaultEndingCells, additionalPiece);
    }
}