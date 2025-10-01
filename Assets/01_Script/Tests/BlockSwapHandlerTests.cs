using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[TestFixture]
public class BlockSwapHandlerTests
{
    private BlockSwapHandler _blockswaphandler;
    private Dictionary<(int, int), UI_Match_Block> _testgrid;

    [SetUp]
    public void SetUp()
    {
        _blockswaphandler = new BlockSwapHandler();
        _testgrid = new Dictionary<(int, int), UI_Match_Block>();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var block in _testgrid.Values)
        {
            if (block != null && block.gameObject != null)
            {
                Object.DestroyImmediate(block.gameObject);
            }
        }
        _testgrid.Clear();
    }

    [Test]
    public void ShouldSwapBlocksAndUpdatePositions()
    {
        // Arrange: 두 블록 생성
        var block1 = CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        var block2 = CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE);

        // Act: 블록 교환
        _blockswaphandler.SwapBlocks(_testgrid, (0, 0), (1, 0));

        // Assert: 딕셔너리에서 위치가 교환되어야 함
        Assert.AreEqual(block2, _testgrid[(0, 0)]);
        Assert.AreEqual(block1, _testgrid[(1, 0)]);

        // Assert: 블록 객체의 내부 좌표도 업데이트되어야 함
        Assert.AreEqual((0, 0), block2.GetPoint());
        Assert.AreEqual((1, 0), block1.GetPoint());
    }

    [Test]
    public void ShouldRollbackSwapWhenNoMatch()
    {
        // Arrange: 두 블록 생성
        var block1 = CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        var block2 = CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE);

        // Act: 블록 교환 후 롤백
        _blockswaphandler.SwapBlocks(_testgrid, (0, 0), (1, 0));
        _blockswaphandler.RollbackSwap(_testgrid, (0, 0), (1, 0));

        // Assert: 원래 위치로 돌아가야 함
        Assert.AreEqual(block1, _testgrid[(0, 0)]);
        Assert.AreEqual(block2, _testgrid[(1, 0)]);

        // Assert: 블록 객체의 내부 좌표도 원래대로 돌아가야 함
        Assert.AreEqual((0, 0), block1.GetPoint());
        Assert.AreEqual((1, 0), block2.GetPoint());
    }

    [Test]
    public void ShouldMaintainDictionaryConsistencyDuringSwap()
    {
        // Arrange: 여러 블록 생성
        var block1 = CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        var block2 = CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE);
        var block3 = CreateBlockAt(2, 0, EBLOCKCOLORTYPE.GREEN);

        // Act: 블록 교환
        _blockswaphandler.SwapBlocks(_testgrid, (0, 0), (1, 0));

        // Assert: 딕셔너리에 모든 블록이 존재해야 함
        Assert.AreEqual(3, _testgrid.Count);
        Assert.IsTrue(_testgrid.ContainsKey((0, 0)));
        Assert.IsTrue(_testgrid.ContainsKey((1, 0)));
        Assert.IsTrue(_testgrid.ContainsKey((2, 0)));

        // Assert: 교환되지 않은 블록은 영향받지 않아야 함
        Assert.AreEqual(block3, _testgrid[(2, 0)]);
        Assert.AreEqual((2, 0), block3.GetPoint());
    }

    private UI_Match_Block CreateBlockAt(int x, int y, EBLOCKCOLORTYPE colortype)
    {
        var gameobject = new GameObject($"Block_{x}_{y}");
        var block = gameobject.AddComponent<UI_Match_Block>();

        // Use reflection to set private fields for testing
        var colorfield = typeof(UI_Match_Block).GetField("_colortypes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        colorfield?.SetValue(block, colortype);

        var xfield = typeof(UI_Match_Block).GetField("_x",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        xfield?.SetValue(block, x);

        var yfield = typeof(UI_Match_Block).GetField("_y",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        yfield?.SetValue(block, y);

        _testgrid[(x, y)] = block;
        return block;
    }
}
