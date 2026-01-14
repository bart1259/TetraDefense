using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;

public class PieceTests
{
    [Test]
    public void SanityCheck()
    {
        Assert.AreEqual(4, 2 + 2);
    }


    [Test]
    public void PieceCreationTest()
    {
        PieceShape pieceShape3x3 = PieceShape.FromString("111\n101\n111");
        PieceShape pieceShape5x1 = PieceShape.FromString("11111");
        PieceShape pieceShape1x5 = PieceShape.FromString("1\n1\n1\n1\n1");
        Assert.AreEqual(3, pieceShape3x3.Width);
        Assert.AreEqual(3, pieceShape3x3.Height);

        Assert.AreEqual(5, pieceShape5x1.Width);
        Assert.AreEqual(1, pieceShape5x1.Height);
        Assert.AreEqual(1, pieceShape1x5.Width);
        Assert.AreEqual(5, pieceShape1x5.Height);
    }

    [Test]
    public void PieceStringRepresentationTest()
    {
        PieceShape pieceShape = PieceShape.FromString("1001\n1111\n1001");
        Assert.AreEqual(pieceShape.Width, 4);
        Assert.AreEqual(pieceShape.Height, 3);
        Assert.AreEqual(pieceShape.GetVisualRepresentation(), "1001\n1111\n1001");
    }

    [Test]
    public void PieceRotationTest()
    {
        PieceShape pieceShape3x2 = PieceShape.FromString("111\n101");
        Piece piece = new Piece(pieceShape3x2);

        Assert.AreEqual(3, piece.PieceShape.Width);
        Assert.AreEqual(2, piece.PieceShape.Height);

        piece.Rotation = 90;
        Assert.AreEqual(2, piece.PieceShape.Width);
        Assert.AreEqual(3, piece.PieceShape.Height);
        Assert.AreEqual("11\n01\n11", piece.PieceShape.GetVisualRepresentation());

        piece.Rotation = 180;
        Assert.AreEqual(3, piece.PieceShape.Width);
        Assert.AreEqual(2, piece.PieceShape.Height);
        Assert.AreEqual("101\n111", piece.PieceShape.GetVisualRepresentation());

        piece.Rotation = 270;
        Assert.AreEqual(2, piece.PieceShape.Width);
        Assert.AreEqual(3, piece.PieceShape.Height);
        Assert.AreEqual("11\n10\n11", piece.PieceShape.GetVisualRepresentation());
    }

    [Test]
    public void PieceRotationTest_InvalidRotation()
    {
        PieceShape pieceShape = PieceShape.FromString("11\n11");
        Piece piece = new Piece(pieceShape);
        Assert.Throws<System.ArgumentException>(() => {
            piece.Rotation = 45; // Invalid rotation
        });
    }
}