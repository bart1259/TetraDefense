using NUnit.Framework;
using UnityEngine;

public class TowerPlacementTests
{
    [Test]
    public void WorldDebugOutputTest()
    {
        World world = new World(6, 5); // 6 wide, 5 high

        string debugOutput = world.GetVisualRepresentation();

        Assert.AreEqual(world.Width, 6);
        Assert.AreEqual(world.Height, 5);

        Assert.AreEqual(debugOutput, "......\n" +
                                     "......\n" +
                                     "......\n" +
                                     "......\n" +
                                     "......");
    }

    [Test]
    public void WorldDebugOutputOrientation()
    {
        World world = new World(6, 5);
        PieceShape pieceShape1 = PieceShape.FromString("101");
        PieceShape pieceShape2 = PieceShape.FromString("1");
        world.AddPiece(new Piece(pieceShape1, new GridCoord(2, 0)), CellContent.Platform);
        world.AddPiece(new Piece(pieceShape2, new GridCoord(4, 0)), CellContent.Tower);

        string debugOutput = world.GetVisualRepresentation();

        Assert.AreEqual(debugOutput, "......\n" +
                                     "......\n" +
                                     "......\n" +
                                     "......\n" +
                                     "..P.T.");
    }

    [Test]
    public void PiecePlaceOutOfBoundsTest()
    {
        World world = new World(5, 5);
        TowerPlacementService placementService = new TowerPlacementService(world, new PathFindingService(world));

        PieceShape pieceShape = PieceShape.FromString("111\n111\n111");
        Piece piece1 = new Piece(pieceShape, new GridCoord(5, 5)); // This will go OOB
        Piece piece2 = new Piece(pieceShape, new GridCoord(4, 4)); // This will go OOB
        Piece piece3 = new Piece(pieceShape, new GridCoord(3, 3)); // This will go OOB
        Piece piece4 = new Piece(pieceShape, new GridCoord(2, 2)); // This will fit
        Piece piece5 = new Piece(pieceShape, new GridCoord(-1, 0)); // This will go OOB
        Piece piece6 = new Piece(pieceShape, new GridCoord(0, -1)); // This will go OOB
        Piece piece7 = new Piece(pieceShape, new GridCoord(3, 0));  // This will go OOB
        Piece piece8 = new Piece(pieceShape, new GridCoord(0, 3));  // This will go OOB

        TowerPlacementResult result1 = placementService.CanPlacePiece(piece1, CellContent.Platform);
        TowerPlacementResult result2 = placementService.CanPlacePiece(piece2, CellContent.Platform);
        TowerPlacementResult result3 = placementService.CanPlacePiece(piece3, CellContent.Platform);
        TowerPlacementResult result4 = placementService.CanPlacePiece(piece4, CellContent.Platform);
        TowerPlacementResult result5 = placementService.CanPlacePiece(piece5, CellContent.Platform);
        TowerPlacementResult result6 = placementService.CanPlacePiece(piece6, CellContent.Platform);
        TowerPlacementResult result7 = placementService.CanPlacePiece(piece7, CellContent.Platform);
        TowerPlacementResult result8 = placementService.CanPlacePiece(piece8, CellContent.Platform);

        Assert.AreEqual(TowerPlacementResult.OutOfBounds, result1);
        Assert.AreEqual(TowerPlacementResult.OutOfBounds, result2);
        Assert.AreEqual(TowerPlacementResult.OutOfBounds, result3);
        Assert.AreEqual(TowerPlacementResult.Success, result4);
        Assert.AreEqual(TowerPlacementResult.OutOfBounds, result5);
        Assert.AreEqual(TowerPlacementResult.OutOfBounds, result6);
        Assert.AreEqual(TowerPlacementResult.OutOfBounds, result7);
        Assert.AreEqual(TowerPlacementResult.OutOfBounds, result8);
    }

    [Test]
    public void CanPlaceIntersection()
    {
        PieceShape pieceShape = PieceShape.FromString("010\n111\n010\n");
        Piece piece = new Piece(pieceShape, new GridCoord(1, 1));
        World world = new World(5, 5);
        TowerPlacementService placementService = new TowerPlacementService(world, new PathFindingService(world));
        bool result = placementService.TryPlacePiece(piece, CellContent.Platform);

        Assert.IsTrue(result);
    
        PieceShape corner = PieceShape.FromString("10\n11");
        Piece cornerPiece1 = new Piece(corner, new GridCoord(0, 0)); // Fit
        Piece cornerPiece2 = new Piece(corner, new GridCoord(1, 1)); // Will not fit
        Piece cornerPiece3 = new Piece(corner, new GridCoord(1, 0)); // Fit
        Piece cornerPiece4 = new Piece(corner, new GridCoord(0, 1)); // Fit

        Piece cornerPiece5 = new Piece(corner, new GridCoord(3, 3), 180); // Fit
        Piece cornerPiece6 = new Piece(corner, new GridCoord(2, 2), 180); // Will not fit
        Piece cornerPiece7 = new Piece(corner, new GridCoord(2, 3), 180); // Fit
        Piece cornerPiece8 = new Piece(corner, new GridCoord(3, 2), 180); // Fit

        Assert.AreEqual(cornerPiece6.PieceShape.GetVisualRepresentation(), "11\n01");
        // Debug.Log(cornerPiece6.PieceShape.GetVisualRepresentation());
        // Debug.Log(world.GetVisualRepresentation());

        TowerPlacementResult res1 = placementService.CanPlacePiece(cornerPiece1, CellContent.Platform);
        TowerPlacementResult res2 = placementService.CanPlacePiece(cornerPiece2, CellContent.Platform);
        TowerPlacementResult res3 = placementService.CanPlacePiece(cornerPiece3, CellContent.Platform);
        TowerPlacementResult res4 = placementService.CanPlacePiece(cornerPiece4, CellContent.Platform);
        TowerPlacementResult res5 = placementService.CanPlacePiece(cornerPiece5, CellContent.Platform);
        TowerPlacementResult res6 = placementService.CanPlacePiece(cornerPiece6, CellContent.Platform); 
        TowerPlacementResult res7 = placementService.CanPlacePiece(cornerPiece7, CellContent.Platform);
        TowerPlacementResult res8 = placementService.CanPlacePiece(cornerPiece8, CellContent.Platform);

        Assert.AreEqual(TowerPlacementResult.Success, res1);
        Assert.AreEqual(TowerPlacementResult.Collision, res2);
        Assert.AreEqual(TowerPlacementResult.Success, res3);
        Assert.AreEqual(TowerPlacementResult.Success, res4);

        Assert.AreEqual(TowerPlacementResult.Success, res5);
        Assert.AreEqual(TowerPlacementResult.Collision, res6);
        Assert.AreEqual(TowerPlacementResult.Success, res7);
        Assert.AreEqual(TowerPlacementResult.Success, res8);
    }

    [Test]
    public void TowerVsPlatformPlace()
    {
        World world = new World(5, 5);
        TowerPlacementService placementService = new TowerPlacementService(world, new PathFindingService(world));

        PieceShape pieceShape1 = PieceShape.FromString("11\n11");
        PieceShape pieceShape2 = PieceShape.FromString("1\n1");
        Piece platformPiece = new Piece(pieceShape1, new GridCoord(1, 1));
        Piece towerPiece1 = new Piece(pieceShape2, new GridCoord(1, 1));
        Piece towerPiece2 = new Piece(pieceShape2, new GridCoord(1, 0));

        TowerPlacementResult platformResult = placementService.CanPlacePiece(platformPiece, CellContent.Platform);
        TowerPlacementResult towerResult1 = placementService.CanPlacePiece(towerPiece1, CellContent.Tower);
        TowerPlacementResult towerResult2 = placementService.CanPlacePiece(towerPiece2, CellContent.Tower);

        Assert.AreEqual(TowerPlacementResult.Success, platformResult);
        Assert.AreEqual(TowerPlacementResult.Collision, towerResult1);
        Assert.AreEqual(TowerPlacementResult.Collision, towerResult2);

        bool platformPlaced = placementService.TryPlacePiece(platformPiece, CellContent.Platform);
        Assert.IsTrue(platformPlaced);

        TowerPlacementResult towerResultAfterPlace1 = placementService.CanPlacePiece(towerPiece1, CellContent.Tower);
        TowerPlacementResult towerResultAfterPlace2 = placementService.CanPlacePiece(towerPiece2, CellContent.Tower);
        
        Assert.AreEqual(TowerPlacementResult.Success, towerResultAfterPlace1);
        Assert.AreEqual(TowerPlacementResult.Collision, towerResultAfterPlace2);

        bool towerPlaced = placementService.TryPlacePiece(towerPiece1, CellContent.Tower);
        Assert.IsTrue(towerPlaced);
    }

    [Test]
    public void BlocksPathfindingTest()
    {
        World world = new World(5, 5);
        TowerPlacementService placementService = new TowerPlacementService(world, new PathFindingService(world));

        PieceShape blockingShape = PieceShape.FromString("1\n1\n1\n1\n1");
        Piece blockingPiece1 = new Piece(blockingShape, new GridCoord(0, 0));
        Piece blockingPiece2 = new Piece(blockingShape, new GridCoord(1, 0));
        Piece blockingPiece3 = new Piece(blockingShape, new GridCoord(4, 0));

        TowerPlacementResult result = placementService.CanPlacePiece(blockingPiece1, CellContent.Platform);
        Assert.AreEqual(TowerPlacementResult.BlocksPathfinding, result);

        TowerPlacementResult result2 = placementService.CanPlacePiece(blockingPiece2, CellContent.Platform);
        Assert.AreEqual(TowerPlacementResult.BlocksPathfinding, result2);

        TowerPlacementResult result3 = placementService.CanPlacePiece(blockingPiece3, CellContent.Platform);
        Assert.AreEqual(TowerPlacementResult.BlocksPathfinding, result3);

        PieceShape nonBlockingShape = PieceShape.FromString("11111\n10001\n11111");
        Piece nonBlockingPiece = new Piece(nonBlockingShape, new GridCoord(0, 1));

        TowerPlacementResult result4 = placementService.CanPlacePiece(nonBlockingPiece, CellContent.Platform);
        Assert.AreEqual(TowerPlacementResult.Success, result4);
    }
}