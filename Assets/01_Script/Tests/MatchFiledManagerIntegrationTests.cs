using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[TestFixture]
public class MatchFiledManagerIntegrationTests
{
    [Test]
    public void GridManager_ShouldHandleAllGridOperations()
    {
        // Arrange
        var gridmanager = new GridManager();
        gridmanager.Initialize(5, 8);

        var block = CreateBlock(2, 3, EBLOCKCOLORTYPE.RED);

        // Act: Add
        gridmanager.AddBlock((2, 3), block);

        // Assert: Has and Get
        Assert.IsTrue(gridmanager.HasBlock((2, 3)));
        Assert.AreEqual(block, gridmanager.GetBlock((2, 3)));

        // Act: Remove
        gridmanager.RemoveBlock((2, 3));

        // Assert: Removed
        Assert.IsFalse(gridmanager.HasBlock((2, 3)));

        // Cleanup
        Object.DestroyImmediate(block.gameObject);
    }

    [Test]
    public void GridManager_ShouldDelegateChangeIDX()
    {
        // Arrange
        var gridmanager = new GridManager();
        gridmanager.Initialize(5, 8);

        var block1 = CreateBlock(0, 0, EBLOCKCOLORTYPE.RED);
        var block2 = CreateBlock(1, 0, EBLOCKCOLORTYPE.BLUE);

        gridmanager.AddBlock((0, 0), block1);
        gridmanager.AddBlock((1, 0), block2);

        // Act: Simulate ChangeIDX operation
        gridmanager.RemoveBlock((0, 0));
        gridmanager.AddBlock((1, 1), block1);

        // Assert
        Assert.IsFalse(gridmanager.HasBlock((0, 0)));
        Assert.IsTrue(gridmanager.HasBlock((1, 1)));
        Assert.AreEqual(block1, gridmanager.GetBlock((1, 1)));

        // Cleanup
        Object.DestroyImmediate(block1.gameObject);
        Object.DestroyImmediate(block2.gameObject);
    }

    [Test]
    public void MatchFiledManager_ShouldUseGridManagerForBlockOperations()
    {
        // Arrange
        var gameobject = new GameObject("MatchFiledManager");
        var manager = gameobject.AddComponent<MatchFiledManager>();

        // GridManager가 초기화되었는지 확인
        var gridmanagerfield = typeof(MatchFiledManager).GetField("_gridmanager",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        Assert.IsNotNull(gridmanagerfield, "MatchFiledManager should have _gridmanager field");

        var gridmanager = gridmanagerfield.GetValue(manager) as GridManager;
        Assert.IsNotNull(gridmanager, "GridManager should be initialized");

        // Cleanup
        Object.DestroyImmediate(gameobject);
    }

    [Test]
    public void MatchFiledManager_ChangeIDX_ShouldUseGridManager()
    {
        // Arrange
        var gameobject = new GameObject("MatchFiledManager");
        var manager = gameobject.AddComponent<MatchFiledManager>();

        var gridmanagerfield = typeof(MatchFiledManager).GetField("_gridmanager",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var gridmanager = new GridManager();
        gridmanager.Initialize(5, 8);
        gridmanagerfield.SetValue(manager, gridmanager);

        // 두 블록 생성: block1은 (1, 2)에, block2는 (2, 3)에
        var block1 = CreateBlock(1, 2, EBLOCKCOLORTYPE.RED);
        var block2 = CreateBlock(2, 3, EBLOCKCOLORTYPE.BLUE);
        gridmanager.SetBlock((1, 2), block1);
        gridmanager.SetBlock((2, 3), block2);

        // Act: ChangeIDX 호출 - block1을 (2, 3)으로 이동 (block2와 교환)
        var changeidxmethod = typeof(MatchFiledManager).GetMethod("ChangeIDX",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        changeidxmethod.Invoke(manager, new object[] { 2, 3, block1 });

        // Assert: GridManager에서 블록이 교환되었는지 확인
        Assert.AreEqual(block1, gridmanager.GetBlock((2, 3)), "block1 should be at (2, 3)");
        Assert.AreEqual(block2, gridmanager.GetBlock((1, 2)), "block2 should be at old position (1, 2)");

        // Cleanup
        Object.DestroyImmediate(block1.gameObject);
        Object.DestroyImmediate(block2.gameObject);
        Object.DestroyImmediate(gameobject);
    }

    [Test]
    public void MatchFiledManager_RemoveIDX_ShouldUseGridManager()
    {
        // Arrange
        var gameobject = new GameObject("MatchFiledManager");
        var manager = gameobject.AddComponent<MatchFiledManager>();

        var gridmanagerfield = typeof(MatchFiledManager).GetField("_gridmanager",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var gridmanager = new GridManager();
        gridmanager.Initialize(5, 8);
        gridmanagerfield.SetValue(manager, gridmanager);

        var block = CreateBlock(2, 3, EBLOCKCOLORTYPE.RED);
        gridmanager.SetBlock((2, 3), block);

        // Act: RemoveIDX 호출 - 블록을 null로 설정
        var removeidxmethod = typeof(MatchFiledManager).GetMethod("RemoveIDX",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        removeidxmethod.Invoke(manager, new object[] { block });

        // Assert: GridManager에서 블록이 null로 설정되었는지 확인
        Assert.IsNull(gridmanager.GetBlock((2, 3)), "Block should be null at (2, 3)");
        Assert.IsFalse(gridmanager.HasBlock((2, 3)), "HasBlock should return false for null block");

        // Cleanup
        Object.DestroyImmediate(block.gameObject);
        Object.DestroyImmediate(gameobject);
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
