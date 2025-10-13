using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[TestFixture]
public class GridManagerTests
{
    private GridManager _gridmanager;
    private Dictionary<(int, int), UI_Match_Block> _createdblocks;

    [SetUp]
    public void SetUp()
    {
        _gridmanager = new GridManager();
        _createdblocks = new Dictionary<(int, int), UI_Match_Block>();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var block in _createdblocks.Values)
        {
            if (block != null && block.gameObject != null)
            {
                Object.DestroyImmediate(block.gameObject);
            }
        }
        _createdblocks.Clear();
    }

    [Test]
    public void ShouldInitializeGridWithCorrectDimensions()
    {
        // Arrange
        int width = 5;
        int height = 8;

        // Act
        _gridmanager.Initialize(width, height);

        // Assert
        Assert.AreEqual(width, _gridmanager.Width);
        Assert.AreEqual(height, _gridmanager.Height);
    }

    [Test]
    public void ShouldAddBlockAtPosition()
    {
        // Arrange
        _gridmanager.Initialize(5, 8);
        var block = CreateBlock(2, 3, EBLOCKCOLORTYPE.RED);

        // Act
        _gridmanager.AddBlock((2, 3), block);

        // Assert
        Assert.IsTrue(_gridmanager.HasBlock((2, 3)));
        Assert.AreEqual(block, _gridmanager.GetBlock((2, 3)));
    }

    [Test]
    public void ShouldRemoveBlockAtPosition()
    {
        // Arrange
        _gridmanager.Initialize(5, 8);
        var block = CreateBlock(2, 3, EBLOCKCOLORTYPE.RED);
        _gridmanager.AddBlock((2, 3), block);

        // Act
        _gridmanager.RemoveBlock((2, 3));

        // Assert
        Assert.IsFalse(_gridmanager.HasBlock((2, 3)));
        Assert.IsNull(_gridmanager.GetBlock((2, 3)));
    }

    [Test]
    public void ShouldCheckIfPositionHasBlock()
    {
        // Arrange
        _gridmanager.Initialize(5, 8);
        var block = CreateBlock(2, 3, EBLOCKCOLORTYPE.RED);
        _gridmanager.AddBlock((2, 3), block);

        // Act & Assert
        Assert.IsTrue(_gridmanager.HasBlock((2, 3)));
        Assert.IsFalse(_gridmanager.HasBlock((0, 0)));
    }

    [Test]
    public void ShouldGetBlockAtPosition()
    {
        // Arrange
        _gridmanager.Initialize(5, 8);
        var block = CreateBlock(2, 3, EBLOCKCOLORTYPE.RED);
        _gridmanager.AddBlock((2, 3), block);

        // Act
        var retrievedblock = _gridmanager.GetBlock((2, 3));

        // Assert
        Assert.AreEqual(block, retrievedblock);
        Assert.IsNull(_gridmanager.GetBlock((0, 0)));
    }

    [Test]
    public void ShouldValidatePositionWithinMapBounds()
    {
        // Arrange
        _gridmanager.Initialize(5, 8);

        // Act & Assert
        Assert.IsTrue(_gridmanager.IsValidPosition((0, 0)));
        Assert.IsTrue(_gridmanager.IsValidPosition((4, 7)));
        Assert.IsFalse(_gridmanager.IsValidPosition((-1, 0)));
        Assert.IsFalse(_gridmanager.IsValidPosition((5, 0)));
        Assert.IsFalse(_gridmanager.IsValidPosition((0, 8)));
        Assert.IsFalse(_gridmanager.IsValidPosition((0, -1)));
    }

    [Test]
    public void ShouldLoadMapDataCorrectly()
    {
        // Arrange
        var mapdata = new string[]
        {
            "1 1 1 1 1",
            "1 1 1 1 1",
            "1 1 0 1 1",
            "1 1 1 1 1"
        };

        // Act
        _gridmanager.LoadMapData(mapdata);

        // Assert
        Assert.AreEqual(5, _gridmanager.Width);
        Assert.AreEqual(4, _gridmanager.Height);
        Assert.IsTrue(_gridmanager.IsValidSlot((0, 0)));
        Assert.IsTrue(_gridmanager.IsValidSlot((4, 3)));
        Assert.IsFalse(_gridmanager.IsValidSlot((2, 2))); // "0" in map data
    }

    [Test]
    public void ShouldTrackTopSlotsForSpawning()
    {
        // Arrange
        var mapdata = new string[]
        {
            "1 1 0 1 1",
            "1 1 1 1 1",
            "1 1 1 1 1"
        };
        _gridmanager.LoadMapData(mapdata);

        // Act
        var topslots = _gridmanager.GetTopSlots();

        // Assert
        Assert.AreEqual(5, topslots.Count);
        Assert.IsTrue(topslots.Contains((0, 0)));
        Assert.IsTrue(topslots.Contains((1, 0)));
        Assert.IsTrue(topslots.Contains((2, 1))); // Column 2의 최상단 슬롯은 (2,1)
        Assert.IsTrue(topslots.Contains((3, 0)));
        Assert.IsTrue(topslots.Contains((4, 0)));
    }

    // Phase 7.2: 맵 검증 로직
    [Test]
    public void ShouldHandleNullMapDataGracefully()
    {
        // Arrange & Act: null 맵 데이터 로드 시도
        Assert.DoesNotThrow(() => _gridmanager.LoadMapData(null));

        // Assert: 그리드가 초기화되지 않았어야 함 (Width, Height는 0)
        Assert.AreEqual(0, _gridmanager.Width);
        Assert.AreEqual(0, _gridmanager.Height);
    }

    [Test]
    public void ShouldHandleEmptyMapDataGracefully()
    {
        // Arrange
        var emptymapdata = new string[0];

        // Act & Assert: 빈 맵 데이터 로드 시도 시 예외 발생하지 않음
        Assert.DoesNotThrow(() => _gridmanager.LoadMapData(emptymapdata));

        // Assert: 그리드가 초기화되지 않았어야 함
        Assert.AreEqual(0, _gridmanager.Width);
        Assert.AreEqual(0, _gridmanager.Height);
    }

    [Test]
    public void ShouldHandleMalformedMapDataGracefully()
    {
        // Arrange: 형식이 잘못된 맵 데이터 (공백 없음)
        var malformedmapdata = new string[]
        {
            "111",  // 공백 없이 붙어있음
            "101",
            "111"
        };

        // Act & Assert: 예외 발생하지 않아야 함
        Assert.DoesNotThrow(() => _gridmanager.LoadMapData(malformedmapdata));

        // Assert: 각 행이 하나의 문자열로 처리됨 (Width = 1)
        Assert.AreEqual(1, _gridmanager.Width);
        Assert.AreEqual(3, _gridmanager.Height);
    }

    [Test]
    public void ShouldHandleZeroSizeMapGracefully()
    {
        // Arrange: 빈 문자열만 포함된 맵 데이터
        var zerosizemap = new string[]
        {
            ""
        };

        // Act & Assert: 예외 발생하지 않아야 함
        Assert.DoesNotThrow(() => _gridmanager.LoadMapData(zerosizemap));

        // Assert: Width가 0이거나 1 (빈 문자열 split 결과)
        Assert.AreEqual(1, _gridmanager.Height);
    }

    [Test]
    public void ShouldHandleIrregularMapDataGracefully()
    {
        // Arrange: 각 행의 열 개수가 다른 맵 데이터
        var irregularmapdata = new string[]
        {
            "1 1 1 1 1",      // 5개
            "1 1 1",          // 3개
            "1 1 1 1 1 1 1"  // 7개
        };

        // Act & Assert: 예외 발생하지 않아야 함
        Assert.DoesNotThrow(() => _gridmanager.LoadMapData(irregularmapdata));

        // Assert: Width는 첫 번째 행 기준 (5)
        Assert.AreEqual(5, _gridmanager.Width);
        Assert.AreEqual(3, _gridmanager.Height);

        // 첫 번째 행의 모든 슬롯은 유효해야 함
        Assert.IsTrue(_gridmanager.IsValidSlot((0, 0)));
        Assert.IsTrue(_gridmanager.IsValidSlot((4, 0)));
    }

    // Phase 7.1: 동시성 및 상태 일관성
    [Test]
    public void ShouldOverwriteBlockWhenAddingAtSamePosition()
    {
        // Arrange: 그리드 초기화 및 첫 번째 블록 추가
        _gridmanager.Initialize(5, 8);
        var firstblock = CreateBlock(2, 3, EBLOCKCOLORTYPE.RED);
        _gridmanager.AddBlock((2, 3), firstblock);

        // Act: 같은 위치에 다른 블록 추가
        var secondblock = CreateBlock(2, 3, EBLOCKCOLORTYPE.BLUE);
        _gridmanager.AddBlock((2, 3), secondblock);

        // Assert: 두 번째 블록으로 덮어써져야 함
        Assert.AreEqual(secondblock, _gridmanager.GetBlock((2, 3)));
        Assert.AreNotEqual(firstblock, _gridmanager.GetBlock((2, 3)));
    }

    [Test]
    public void ShouldNotThrowExceptionWhenRemovingNonExistentPosition()
    {
        // Arrange: 빈 그리드
        _gridmanager.Initialize(5, 8);

        // Act & Assert: 존재하지 않는 위치 제거 시 예외 발생하지 않아야 함
        Assert.DoesNotThrow(() => _gridmanager.RemoveBlock((2, 3)));
        Assert.DoesNotThrow(() => _gridmanager.RemoveBlock((10, 10)));
    }

    [Test]
    public void ShouldReturnNullWhenGettingBlockFromEmptyGrid()
    {
        // Arrange: 빈 그리드
        _gridmanager.Initialize(5, 8);

        // Act
        var block = _gridmanager.GetBlock((2, 3));

        // Assert: null 반환
        Assert.IsNull(block);
    }

    [Test]
    public void ShouldHandleBoundaryAccessSafely()
    {
        // Arrange: 그리드 초기화
        _gridmanager.Initialize(5, 8);

        // Act & Assert: 경계 밖 위치 접근 시 안전하게 처리
        Assert.IsFalse(_gridmanager.IsValidPosition((-1, 0)));
        Assert.IsFalse(_gridmanager.IsValidPosition((5, 0)));
        Assert.IsFalse(_gridmanager.IsValidPosition((0, -1)));
        Assert.IsFalse(_gridmanager.IsValidPosition((0, 8)));

        // GetBlock도 경계 밖에서 null 반환
        Assert.IsNull(_gridmanager.GetBlock((-1, 0)));
        Assert.IsNull(_gridmanager.GetBlock((10, 10)));
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

        _createdblocks[(x, y)] = block;
        return block;
    }
}
