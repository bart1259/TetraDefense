using System;
using System.Collections.Generic;
using UnityEngine;

public class GridPath
{
    public static readonly GridPath Empty = new GridPath(new List<GridCoord>());

    public int PathLength
    {
        get { return Cells.Count; }
    }
    public List<GridCoord> Cells { get; private set; }

    public GridPath(List<GridCoord> cells)
    {
        Cells = new List<GridCoord>(cells);
    }

    public Vector3 GetWorldPositionAt(float positionAlongPath)
    {
        if (Cells.Count == 0)
            throw new InvalidOperationException("Path is empty.");

        int index = (int)positionAlongPath;
        if (index >= Cells.Count - 1)
            return new Vector3(Cells[Cells.Count - 1].X, 0.0f, Cells[Cells.Count - 1].Y);

        float t = positionAlongPath - index;
        Vector3 start = new Vector3(Cells[index].X, 0.0f, Cells[index].Y);
        Vector3 end = new Vector3(Cells[index + 1].X, 0.0f, Cells[index + 1].Y);
        return Vector3.Lerp(start, end, t);
    }
}