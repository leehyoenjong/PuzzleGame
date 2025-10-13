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

    // Phase 8.2: 경계 조건
    [Test]
    public void ShouldHandleEmptyGrid()
    {
        // Arrange: 완전히 빈 그리드
        var blockmover = new BlockMover();
        var gridmanager = new GridManager();
        gridmanager.Initialize(5, 8);

        // 모든 슬롯을 명시적으로 null로 초기화
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                gridmanager.SetBlock((x, y), null);
            }
        }

        // Act: 빈 그리드에 중력 적용
        Assert.DoesNotThrow(() => blockmover.MoveBlocksDown(gridmanager));

        // Assert: 여전히 모든 슬롯이 null이어야 함
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Assert.IsNull(gridmanager.GetBlock((x, y)), $"({x}, {y})는 여전히 null이어야 함");
            }
        }
    }

    [Test]
    public void ShouldNotMoveBlocksAlreadyAtBottom()
    {
        // Arrange: 모든 블록이 이미 바닥에 있는 상태
        var blockmover = new BlockMover();
        var gridmanager = new GridManager();
        gridmanager.Initialize(3, 5);

        // 모든 슬롯 초기화
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                gridmanager.SetBlock((x, y), null);
            }
        }

        // 각 열의 최하단(y=4)에 블록 배치
        var block1 = CreateBlock(0, 4, EBLOCKCOLORTYPE.RED);
        var block2 = CreateBlock(1, 4, EBLOCKCOLORTYPE.BLUE);
        var block3 = CreateBlock(2, 4, EBLOCKCOLORTYPE.GREEN);

        gridmanager.SetBlock((0, 4), block1);
        gridmanager.SetBlock((1, 4), block2);
        gridmanager.SetBlock((2, 4), block3);

        // Act: 중력 적용
        blockmover.MoveBlocksDown(gridmanager);

        // Assert: 블록들이 그대로 최하단에 있어야 함
        Assert.AreEqual(block1, gridmanager.GetBlock((0, 4)), "block1은 (0, 4)에 그대로 있어야 함");
        Assert.AreEqual(block2, gridmanager.GetBlock((1, 4)), "block2는 (1, 4)에 그대로 있어야 함");
        Assert.AreEqual(block3, gridmanager.GetBlock((2, 4)), "block3은 (2, 4)에 그대로 있어야 함");

        // 위쪽 슬롯들은 여전히 비어있어야 함
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                Assert.IsNull(gridmanager.GetBlock((x, y)), $"({x}, {y})는 비어있어야 함");
            }
        }

        // Cleanup
        Object.DestroyImmediate(block1.gameObject);
        Object.DestroyImmediate(block2.gameObject);
        Object.DestroyImmediate(block3.gameObject);
    }

    // Phase 8.1: 복잡한 중력 시나리오
    [Test]
    public void ShouldHandleMultipleColumnsSimultaneously()
    {
        // Arrange: 여러 열에서 동시에 블록이 떨어지는 상황
        var blockmover = new BlockMover();
        var gridmanager = new GridManager();
        gridmanager.Initialize(5, 6);

        // 모든 슬롯 초기화
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 6; y++)
            {
                gridmanager.SetBlock((x, y), null);
            }
        }

        // 여러 열에 블록 배치 (서로 다른 높이)
        var block1 = CreateBlock(0, 1, EBLOCKCOLORTYPE.RED);    // 열 0, 높이 1
        var block2 = CreateBlock(1, 0, EBLOCKCOLORTYPE.BLUE);   // 열 1, 높이 0
        var block3 = CreateBlock(2, 2, EBLOCKCOLORTYPE.GREEN);  // 열 2, 높이 2
        var block4 = CreateBlock(3, 0, EBLOCKCOLORTYPE.YELLOW); // 열 3, 높이 0
        var block5 = CreateBlock(4, 3, EBLOCKCOLORTYPE.PINK);   // 열 4, 높이 3

        gridmanager.SetBlock((0, 1), block1);
        gridmanager.SetBlock((1, 0), block2);
        gridmanager.SetBlock((2, 2), block3);
        gridmanager.SetBlock((3, 0), block4);
        gridmanager.SetBlock((4, 3), block5);

        // Act: 중력 적용
        blockmover.MoveBlocksDown(gridmanager);

        // Assert: 모든 블록이 각자의 열 바닥에 도달해야 함 (충돌 없이)
        Assert.AreEqual(block1, gridmanager.GetBlock((0, 5)), "block1은 (0, 5)로 이동해야 함");
        Assert.AreEqual(block2, gridmanager.GetBlock((1, 5)), "block2는 (1, 5)로 이동해야 함");
        Assert.AreEqual(block3, gridmanager.GetBlock((2, 5)), "block3은 (2, 5)로 이동해야 함");
        Assert.AreEqual(block4, gridmanager.GetBlock((3, 5)), "block4는 (3, 5)로 이동해야 함");
        Assert.AreEqual(block5, gridmanager.GetBlock((4, 5)), "block5는 (4, 5)로 이동해야 함");

        // Cleanup
        Object.DestroyImmediate(block1.gameObject);
        Object.DestroyImmediate(block2.gameObject);
        Object.DestroyImmediate(block3.gameObject);
        Object.DestroyImmediate(block4.gameObject);
        Object.DestroyImmediate(block5.gameObject);
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
