using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[TestFixture]
public class MatchDetectorTests
{
    private MatchDetector _matchdetector;
    private Dictionary<(int, int), UI_Match_Block> _testgrid;

    [SetUp]
    public void SetUp()
    {
        _matchdetector = new MatchDetector();
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
    public void ShouldDetect3HorizontalMatchingBlocks()
    {
        // Arrange: Create 3 horizontal blocks with same color at (0,0), (1,0), (2,0)
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED);

        // Act: Detect horizontal match starting from (0,0)
        var result = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 0));

        // Assert: Should return list with 3 blocks
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Count);
        Assert.Contains((0, 0), result);
        Assert.Contains((1, 0), result);
        Assert.Contains((2, 0), result);
    }

    [Test]
    public void ShouldDetect4HorizontalMatchingBlocks()
    {
        // Arrange: Create 4 horizontal blocks with same color at (0,0), (1,0), (2,0), (3,0)
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(3, 0, EBLOCKCOLORTYPE.BLUE);

        // Act: Detect horizontal match starting from (0,0)
        var result = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 0));

        // Assert: Should return list with 4 blocks
        Assert.IsNotNull(result);
        Assert.AreEqual(4, result.Count);
        Assert.Contains((0, 0), result);
        Assert.Contains((1, 0), result);
        Assert.Contains((2, 0), result);
        Assert.Contains((3, 0), result);
    }

    [Test]
    public void ShouldDetect5OrMoreHorizontalMatchingBlocks()
    {
        // Arrange: Create 5 horizontal blocks with same color
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(3, 0, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(4, 0, EBLOCKCOLORTYPE.YELLOW);

        // Act: Detect horizontal match starting from (0,0)
        var result = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 0));

        // Assert: Should return list with 5 blocks
        Assert.IsNotNull(result);
        Assert.AreEqual(5, result.Count);
    }

    [Test]
    public void ShouldNotDetectMatchWith2Blocks()
    {
        // Arrange: Create only 2 horizontal blocks with same color
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.GREEN);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.GREEN);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED); // Different color breaks match

        // Act: Detect horizontal match starting from (0,0)
        var result = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 0));

        // Assert: Should return null (not enough blocks)
        Assert.IsNull(result);
    }

    [Test]
    public void ShouldDetect3HorizontalMatchFromMiddlePosition()
    {
        // Arrange: Create 3 horizontal blocks [R][R][R]
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED);

        // Act: Detect from MIDDLE position (1,0) - not start position
        var result = _matchdetector.DetectHorizontalMatch(_testgrid, (1, 0));

        // Assert: Should find ALL 3 blocks including ones to the left
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Count);
        Assert.Contains((0, 0), result);
        Assert.Contains((1, 0), result);
        Assert.Contains((2, 0), result);
    }

    [Test]
    public void ShouldDetect3HorizontalMatchFromEndPosition()
    {
        // Arrange: Create 3 horizontal blocks [R][R][R]
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED);

        // Act: Detect from END position (2,0) - last block
        var result = _matchdetector.DetectHorizontalMatch(_testgrid, (2, 0));

        // Assert: Should find ALL 3 blocks including ones to the left
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Count);
        Assert.Contains((0, 0), result);
        Assert.Contains((1, 0), result);
        Assert.Contains((2, 0), result);
    }

    [Test]
    public void ShouldDetect4HorizontalMatchFromAllPositions()
    {
        // Arrange: Create 4 horizontal blocks [B][B][B][B]
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(3, 0, EBLOCKCOLORTYPE.BLUE);

        // Act & Assert: Test from position 0
        var result0 = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 0));
        Assert.IsNotNull(result0);
        Assert.AreEqual(4, result0.Count);

        // Act & Assert: Test from position 1
        var result1 = _matchdetector.DetectHorizontalMatch(_testgrid, (1, 0));
        Assert.IsNotNull(result1);
        Assert.AreEqual(4, result1.Count);

        // Act & Assert: Test from position 2
        var result2 = _matchdetector.DetectHorizontalMatch(_testgrid, (2, 0));
        Assert.IsNotNull(result2);
        Assert.AreEqual(4, result2.Count);

        // Act & Assert: Test from position 3
        var result3 = _matchdetector.DetectHorizontalMatch(_testgrid, (3, 0));
        Assert.IsNotNull(result3);
        Assert.AreEqual(4, result3.Count);
    }

    [Test]
    public void ShouldDetect5HorizontalMatchFromAllPositions()
    {
        // Arrange: Create 5 horizontal blocks [Y][Y][Y][Y][Y]
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(3, 0, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(4, 0, EBLOCKCOLORTYPE.YELLOW);

        // Act & Assert: Test from all 5 positions
        for (int i = 0; i < 5; i++)
        {
            var result = _matchdetector.DetectHorizontalMatch(_testgrid, (i, 0));
            Assert.IsNotNull(result, $"Failed at position {i}");
            Assert.AreEqual(5, result.Count, $"Failed at position {i}");
        }
    }

    [Test]
    public void ShouldDetect3VerticalMatchingBlocks()
    {
        // Arrange: Create 3 vertical blocks with same color at (0,0), (0,1), (0,2)
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(0, 1, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(0, 2, EBLOCKCOLORTYPE.RED);

        // Act: Detect vertical match starting from (0,0)
        var result = _matchdetector.DetectVerticalMatch(_testgrid, (0, 0));

        // Assert: Should return list with 3 blocks
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Count);
        Assert.Contains((0, 0), result);
        Assert.Contains((0, 1), result);
        Assert.Contains((0, 2), result);
    }

    [Test]
    public void ShouldDetect4VerticalMatchingBlocks()
    {
        // Arrange: Create 4 vertical blocks with same color
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(1, 1, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(1, 2, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(1, 3, EBLOCKCOLORTYPE.BLUE);

        // Act: Detect vertical match starting from (1,0)
        var result = _matchdetector.DetectVerticalMatch(_testgrid, (1, 0));

        // Assert: Should return list with 4 blocks
        Assert.IsNotNull(result);
        Assert.AreEqual(4, result.Count);
        Assert.Contains((1, 0), result);
        Assert.Contains((1, 1), result);
        Assert.Contains((1, 2), result);
        Assert.Contains((1, 3), result);
    }

    [Test]
    public void ShouldDetect5OrMoreVerticalMatchingBlocks()
    {
        // Arrange: Create 5 vertical blocks with same color
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(2, 1, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(2, 2, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(2, 3, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(2, 4, EBLOCKCOLORTYPE.YELLOW);

        // Act: Detect vertical match starting from (2,0)
        var result = _matchdetector.DetectVerticalMatch(_testgrid, (2, 0));

        // Assert: Should return list with 5 blocks
        Assert.IsNotNull(result);
        Assert.AreEqual(5, result.Count);
    }

    [Test]
    public void ShouldNotDetectVerticalMatchWith2Blocks()
    {
        // Arrange: Create only 2 vertical blocks with same color
        CreateBlockAt(3, 0, EBLOCKCOLORTYPE.GREEN);
        CreateBlockAt(3, 1, EBLOCKCOLORTYPE.GREEN);
        CreateBlockAt(3, 2, EBLOCKCOLORTYPE.PINK); // Different color breaks match

        // Act: Detect vertical match starting from (3,0)
        var result = _matchdetector.DetectVerticalMatch(_testgrid, (3, 0));

        // Assert: Should return null (not enough blocks)
        Assert.IsNull(result);
    }

    [Test]
    public void ShouldDetect3x3CrossPattern()
    {
        // Arrange: Create 3x3 cross pattern (+ shape) centered at (1,1)
        // Vertical line: (1,0), (1,1), (1,2)
        // Horizontal line: (0,1), (1,1), (2,1)
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(0, 1, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(1, 1, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(2, 1, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(1, 2, EBLOCKCOLORTYPE.RED);

        // Act: Detect cross match at center (1,1)
        var result = _matchdetector.DetectCrossMatch(_testgrid, (1, 1));

        // Assert: Should return cross match with horizontal and vertical lists
        Assert.IsTrue(result.HasValue);
        Assert.AreEqual(3, result.Value.horizontalmatches.Count);
        Assert.AreEqual(3, result.Value.verticalmatches.Count);
    }

    [Test]
    public void ShouldDetect4x4CrossPattern()
    {
        // Arrange: Create 4x4 cross pattern centered between (1,1) and (2,2)
        // Vertical line: (1,0), (1,1), (1,2), (1,3)
        // Horizontal line: (0,1), (1,1), (2,1), (3,1)
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(0, 1, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(1, 1, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(2, 1, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(3, 1, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(1, 2, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(1, 3, EBLOCKCOLORTYPE.BLUE);

        // Act: Detect cross match at center
        var result = _matchdetector.DetectCrossMatch(_testgrid, (1, 1));

        // Assert: Should return 4x4 cross match
        Assert.IsTrue(result.HasValue);
        Assert.AreEqual(4, result.Value.horizontalmatches.Count);
        Assert.AreEqual(4, result.Value.verticalmatches.Count);
    }

    [Test]
    public void ShouldDetect5x5CrossPattern()
    {
        // Arrange: Create 5x5 cross pattern
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(0, 2, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(1, 2, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(2, 1, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(2, 2, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(2, 3, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(3, 2, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(4, 2, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(2, 4, EBLOCKCOLORTYPE.YELLOW);

        // Act: Detect cross match at center (2,2)
        var result = _matchdetector.DetectCrossMatch(_testgrid, (2, 2));

        // Assert: Should return 5x5 cross match
        Assert.IsTrue(result.HasValue);
        Assert.GreaterOrEqual(result.Value.horizontalmatches.Count, 5);
        Assert.GreaterOrEqual(result.Value.verticalmatches.Count, 5);
    }

    [Test]
    public void ShouldReturnNullWhenNoCrossPattern()
    {
        // Arrange: Create only horizontal match (no cross)
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.GREEN);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.GREEN);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.GREEN);

        // Act: Detect cross match at (1,0)
        var result = _matchdetector.DetectCrossMatch(_testgrid, (1, 0));

        // Assert: Should return null (no cross pattern)
        Assert.IsFalse(result.HasValue);
    }

    private void CreateBlockAt(int x, int y, EBLOCKCOLORTYPE color)
    {
        var gameobject = new GameObject($"Block_{x}_{y}");
        var block = gameobject.AddComponent<UI_Match_Block>();

        // Use reflection to set private fields for testing
        var colorfield = typeof(UI_Match_Block).GetField("_colortypes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        colorfield?.SetValue(block, color);

        _testgrid.Add((x, y), block);
    }
}