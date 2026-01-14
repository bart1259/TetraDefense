using NUnit.Framework;
using UnityEngine;

using System.Collections.Generic;

public class PathFindingTests
{
    [Test]
    public void WorldLinePathfinding()
    {
        World world = new World(5, 1);
        PathFindingService pathFindingService = new PathFindingService(world);

        GridCoord start = new GridCoord(0, 0);
        GridCoord end = new GridCoord(4, 0);

        GridPath path = pathFindingService.FindBestPath(new List<GridCoord> { start }, new List<GridCoord> { end });
        Assert.IsNotNull(path);
        Assert.AreEqual(5, path.Cells.Count);
        for (int i = 0; i < 5; i++)
        {
            Assert.AreEqual(new GridCoord(i, 0), path.Cells[i]);
        }        
    }

    [Test]

    public void World2DPathfinding()
    {
        World world = new World(7,7);
        PathFindingService pathFindingService = new PathFindingService(world);
        PieceShape maze = PieceShape.FromString(
            "1111111\n" +
            "0000001\n" +
            "1011101\n" +
            "1010101\n" +
            "1010111\n" +
            "1000000\n" +
            "1111111");

        world.AddPiece(new Piece(maze, new GridCoord(0, 0)), CellContent.Platform);

        GridCoord start = new GridCoord(0, 5);
        GridCoord end = new GridCoord(6,1);
        GridPath path = pathFindingService.FindBestPath(new List<GridCoord> { start }, new List<GridCoord> { end });
        Assert.IsNotNull(path);
        Assert.AreEqual(11, path.Cells.Count);

        List<GridCoord> expectedPath = new List<GridCoord>
        {
            new GridCoord(0,5),
            new GridCoord(1,5),
            new GridCoord(1,4),
            new GridCoord(1,3),
            new GridCoord(1,2),
            new GridCoord(1,1),
            new GridCoord(2,1),
            new GridCoord(3,1),
            new GridCoord(4,1),
            new GridCoord(5,1),
            new GridCoord(6,1)
        };

        for (int i = 0; i < expectedPath.Count; i++)
        {
            Assert.AreEqual(expectedPath[i], path.Cells[i]);
        }
    }

    [Test]
    public void FindBestPath()
    {
        World world = new World(7,7);
        PathFindingService pathFindingService = new PathFindingService(world);
        PieceShape maze = PieceShape.FromString(
            "0011100\n" +
            "1000001\n" +
            "1111111\n" +
            "1111111\n" +
            "1111111\n" +
            "0000000\n" +
            "1111111");

        world.AddPiece(new Piece(maze, new GridCoord(0, 0)), CellContent.Platform);
        GridCoord start1 = new GridCoord(0, 6);
        GridCoord start2 = new GridCoord(0, 1);
        GridCoord end1 = new GridCoord(6,6);
        GridCoord end2 = new GridCoord(6,1);

        GridPath path = pathFindingService.FindBestPath(new List<GridCoord> { start1, start2 }, new List<GridCoord> { end1, end2 });
        Assert.IsNotNull(path);
        Assert.AreEqual(7, path.Cells.Count);
        List<GridCoord> expectedPath = new List<GridCoord>
        {
            new GridCoord(0,1),
            new GridCoord(1,1),
            new GridCoord(2,1),
            new GridCoord(3,1),
            new GridCoord(4,1),
            new GridCoord(5,1),
            new GridCoord(6,1)
        };

        for (int i = 0; i < expectedPath.Count; i++)
        {
            Assert.AreEqual(expectedPath[i], path.Cells[i]);
        }
    }

}