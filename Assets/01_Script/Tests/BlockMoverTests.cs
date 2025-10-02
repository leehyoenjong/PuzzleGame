using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class BlockMoverTests
{
    [Test]
    public void ShouldMoveBlockToNearestEmptySlotBelow()
    {
        // Arrange
        var blockmover = new BlockMover();
        var gridmanager = new GridManager();
        gridmanager.Initialize(3, 5);

        // 모든 슬롯 초기화
        gridmanager.SetBlock((1, 0), null);
        gridmanager.SetBlock((1, 1), null);
        gridmanager.SetBlock((1, 2), null);
        gridmanager.SetBlock((1, 3), null);
        gridmanager.SetBlock((1, 4), null);

        // (1, 1) 위치에 블록 생성
        var block = CreateBlock(1, 1, EBLOCKCOLORTYPE.RED);
        gridmanager.SetBlock((1, 1), block);

        // Act
        blockmover.MoveBlocksDown(gridmanager);

        // Assert: 블록이 (1, 4)로 이동해야 함
        Assert.IsNull(gridmanager.GetBlock((1, 1)), "원래 위치 (1, 1)는 null이어야 함");
        Assert.AreEqual(block, gridmanager.GetBlock((1, 4)), "블록이 (1, 4)로 이동해야 함");

        // Cleanup
        Object.DestroyImmediate(block.gameObject);
    }

    [Test]
    public void ShouldSkipNonEmptySlots()
    {
        // Arrange
        var blockmover = new BlockMover();
        var gridmanager = new GridManager();
        gridmanager.Initialize(3, 5);

        // 모든 슬롯 초기화
        gridmanager.SetBlock((1, 0), null);
        gridmanager.SetBlock((1, 1), null);
        gridmanager.SetBlock((1, 2), null);
        gridmanager.SetBlock((1, 3), null);
        gridmanager.SetBlock((1, 4), null);

        // (1, 1)에 블록1 생성
        var block1 = CreateBlock(1, 1, EBLOCKCOLORTYPE.RED);
        gridmanager.SetBlock((1, 1), block1);

        // (1, 4)에 블록2 생성 (바닥에 있는 장애물)
        var block2 = CreateBlock(1, 4, EBLOCKCOLORTYPE.BLUE);
        gridmanager.SetBlock((1, 4), block2);

        // Act
        blockmover.MoveBlocksDown(gridmanager);

        // Assert: block1은 (1, 3)으로 이동해야 함 (block2 바로 위까지만)
        Assert.IsNull(gridmanager.GetBlock((1, 1)), "원래 위치 (1, 1)는 null이어야 함");
        Assert.IsNull(gridmanager.GetBlock((1, 2)), "(1, 2)는 비어있어야 함");
        Assert.AreEqual(block1, gridmanager.GetBlock((1, 3)), "block1은 (1, 3)으로 이동해야 함 (block2 위)");
        Assert.AreEqual(block2, gridmanager.GetBlock((1, 4)), "block2는 (1, 4)에 그대로 있어야 함");

        // Cleanup
        Object.DestroyImmediate(block1.gameObject);
        Object.DestroyImmediate(block2.gameObject);
    }

    [Test]
    public void ShouldHandleMultipleBlocksInSameColumn()
    {
        // Arrange
        var blockmover = new BlockMover();
        var gridmanager = new GridManager();
        gridmanager.Initialize(3, 6);

        // 모든 슬롯 초기화
        for (int y = 0; y < 6; y++)
        {
            gridmanager.SetBlock((1, y), null);
        }

        // (1, 0), (1, 2), (1, 4)에 블록 생성 - 빈 공간이 있는 여러 블록
        var block1 = CreateBlock(1, 0, EBLOCKCOLORTYPE.RED);
        var block2 = CreateBlock(1, 2, EBLOCKCOLORTYPE.BLUE);
        var block3 = CreateBlock(1, 4, EBLOCKCOLORTYPE.GREEN);

        gridmanager.SetBlock((1, 0), block1);
        gridmanager.SetBlock((1, 2), block2);
        gridmanager.SetBlock((1, 4), block3);

        // Act
        blockmover.MoveBlocksDown(gridmanager);

        // Assert: 모든 블록이 아래로 쌓여야 함
        Assert.IsNull(gridmanager.GetBlock((1, 0)), "(1, 0)는 비어있어야 함");
        Assert.IsNull(gridmanager.GetBlock((1, 1)), "(1, 1)는 비어있어야 함");
        Assert.IsNull(gridmanager.GetBlock((1, 2)), "(1, 2)는 비어있어야 함");
        Assert.AreEqual(block1, gridmanager.GetBlock((1, 3)), "block1은 (1, 3)으로 이동해야 함");
        Assert.AreEqual(block2, gridmanager.GetBlock((1, 4)), "block2는 (1, 4)로 이동해야 함");
        Assert.AreEqual(block3, gridmanager.GetBlock((1, 5)), "block3은 (1, 5)로 이동해야 함");

        // Cleanup
        Object.DestroyImmediate(block1.gameObject);
        Object.DestroyImmediate(block2.gameObject);
        Object.DestroyImmediate(block3.gameObject);
    }

    [Test]
    public void ShouldRespectMapBoundaries()
    {
        // Arrange
        var blockmover = new BlockMover();
        var gridmanager = new GridManager();
        gridmanager.Initialize(3, 5);

        // 모든 슬롯 초기화
        for (int y = 0; y < 5; y++)
        {
            gridmanager.SetBlock((1, y), null);
        }

        // 가장 아래(1, 4)에 블록 생성
        var block = CreateBlock(1, 4, EBLOCKCOLORTYPE.RED);
        gridmanager.SetBlock((1, 4), block);

        // Act
        blockmover.MoveBlocksDown(gridmanager);

        // Assert: 블록이 맵 경계를 넘어가지 않고 그대로 있어야 함
        Assert.AreEqual(block, gridmanager.GetBlock((1, 4)), "블록은 맵 경계에서 멈춰야 함");

        // Cleanup
        Object.DestroyImmediate(block.gameObject);
    }

    private UI_Match_Block CreateBlock(int x, int y, EBLOCKCOLORTYPE colortype)
    {
        var gameobject = new GameObject($"Block_{x}_{y}");
        var block = gameobject.AddComponent<UI_Match_Block>();

        var colorfield = typeof(UI_Match_Block).GetField("_colortypes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        colorfield?.SetValue(block, colortype);

        var xfield = typeof(UI_Match_Block).GetField("_x",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        xfield?.SetValue(block, x);

        var yfield = typeof(UI_Match_Block).GetField("_y",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        yfield?.SetValue(block, y);

        return block;
    }
}
