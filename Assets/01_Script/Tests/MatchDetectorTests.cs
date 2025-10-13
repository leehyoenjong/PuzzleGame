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

    // Phase 1: MatchDetector 경계 조건 테스트
    // Test 1.1: 그리드 경계 검증

    [Test]
    public void ShouldDetectMatchAtLeftBoundary()
    {
        // Arrange: Create horizontal match at left boundary (x=0)
        // Grid: [R][R][R][ ][ ]
        //        x=0 (left boundary)
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED);

        // Act: Detect horizontal match from left boundary position
        var result = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 0));

        // Assert: Should detect 3-match at left boundary
        Assert.IsNotNull(result, "Should detect match at left boundary");
        Assert.AreEqual(3, result.Count, "Should have 3 blocks in match");
        Assert.Contains((0, 0), result, "Should contain left boundary block");
        Assert.Contains((1, 0), result, "Should contain block at x=1");
        Assert.Contains((2, 0), result, "Should contain block at x=2");
    }

    [Test]
    public void ShouldDetectMatchAtRightBoundary()
    {
        // Arrange: Create horizontal match at right boundary
        // Grid: [ ][ ][B][B][B]
        //                    x=4 (right boundary)
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(3, 0, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(4, 0, EBLOCKCOLORTYPE.BLUE);

        // Act: Detect horizontal match from right boundary position
        var result = _matchdetector.DetectHorizontalMatch(_testgrid, (4, 0));

        // Assert: Should detect 3-match at right boundary
        Assert.IsNotNull(result, "Should detect match at right boundary");
        Assert.AreEqual(3, result.Count, "Should have 3 blocks in match");
        Assert.Contains((2, 0), result, "Should contain block at x=2");
        Assert.Contains((3, 0), result, "Should contain block at x=3");
        Assert.Contains((4, 0), result, "Should contain right boundary block");
    }

    [Test]
    public void ShouldDetectMatchAtBottomBoundary()
    {
        // Arrange: Create vertical match at bottom boundary (y=0)
        // Grid:
        // [Y] y=2
        // [Y] y=1
        // [Y] y=0 (bottom boundary)
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(0, 1, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(0, 2, EBLOCKCOLORTYPE.YELLOW);

        // Act: Detect vertical match from bottom boundary position
        var result = _matchdetector.DetectVerticalMatch(_testgrid, (0, 0));

        // Assert: Should detect 3-match at bottom boundary
        Assert.IsNotNull(result, "Should detect match at bottom boundary");
        Assert.AreEqual(3, result.Count, "Should have 3 blocks in match");
        Assert.Contains((0, 0), result, "Should contain bottom boundary block");
        Assert.Contains((0, 1), result, "Should contain block at y=1");
        Assert.Contains((0, 2), result, "Should contain block at y=2");
    }

    [Test]
    public void ShouldDetectMatchAtTopBoundary()
    {
        // Arrange: Create vertical match at top boundary (y=max)
        // Grid:
        // [G] y=4 (top boundary)
        // [G] y=3
        // [G] y=2
        // [ ] y=1
        // [ ] y=0
        CreateBlockAt(0, 2, EBLOCKCOLORTYPE.GREEN);
        CreateBlockAt(0, 3, EBLOCKCOLORTYPE.GREEN);
        CreateBlockAt(0, 4, EBLOCKCOLORTYPE.GREEN);

        // Act: Detect vertical match from top boundary position
        var result = _matchdetector.DetectVerticalMatch(_testgrid, (0, 4));

        // Assert: Should detect 3-match at top boundary
        Assert.IsNotNull(result, "Should detect match at top boundary");
        Assert.AreEqual(3, result.Count, "Should have 3 blocks in match");
        Assert.Contains((0, 2), result, "Should contain block at y=2");
        Assert.Contains((0, 3), result, "Should contain block at y=3");
        Assert.Contains((0, 4), result, "Should contain top boundary block");
    }

    [Test]
    public void ShouldReturnNullWhenPositionOutsideGridBounds()
    {
        // Arrange: Create small grid with blocks at (0,0) to (2,2)
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED);

        // Act: Try to detect match from positions outside grid bounds
        var resultNegativeX = _matchdetector.DetectHorizontalMatch(_testgrid, (-1, 0));
        var resultNegativeY = _matchdetector.DetectVerticalMatch(_testgrid, (0, -1));
        var resultLargeX = _matchdetector.DetectHorizontalMatch(_testgrid, (100, 0));
        var resultLargeY = _matchdetector.DetectVerticalMatch(_testgrid, (0, 100));

        // Assert: All should return null (position doesn't exist in grid)
        Assert.IsNull(resultNegativeX, "Should return null for negative x position");
        Assert.IsNull(resultNegativeY, "Should return null for negative y position");
        Assert.IsNull(resultLargeX, "Should return null for out-of-bounds x position");
        Assert.IsNull(resultLargeY, "Should return null for out-of-bounds y position");
    }

    [Test]
    public void ShouldReturnNullWhenPositionNotInSparseGrid()
    {
        // Arrange: Create sparse grid with gaps
        // Grid layout:
        // [R][ ][R][ ][R]
        //  0  1  2  3  4  (x coordinates)
        // Position (1,0) and (3,0) have no blocks (gaps in grid)
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(4, 0, EBLOCKCOLORTYPE.RED);

        // Act: Try to detect match from positions that exist in coordinate space
        // but don't have blocks in the grid dictionary
        var resultGap1 = _matchdetector.DetectHorizontalMatch(_testgrid, (1, 0));
        var resultGap2 = _matchdetector.DetectHorizontalMatch(_testgrid, (3, 0));

        // Assert: Should return null (position not in grid dictionary)
        Assert.IsNull(resultGap1, "Should return null for position (1,0) - not in grid");
        Assert.IsNull(resultGap2, "Should return null for position (3,0) - not in grid");
    }

    // Test 1.2: 빈 블록 및 null 처리

    [Test]
    public void ShouldReturnNullWhenGridIsEmpty()
    {
        // Arrange: Empty grid (no blocks added)
        // _testgrid is empty dictionary

        // Act: Try to detect match in empty grid
        var resultHorizontal = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 0));
        var resultVertical = _matchdetector.DetectVerticalMatch(_testgrid, (0, 0));
        var resultCross = _matchdetector.DetectCrossMatch(_testgrid, (0, 0));

        // Assert: All should return null (grid is empty)
        Assert.IsNull(resultHorizontal, "Should return null for horizontal match in empty grid");
        Assert.IsNull(resultVertical, "Should return null for vertical match in empty grid");
        Assert.IsFalse(resultCross.HasValue, "Should return null for cross match in empty grid");
    }

    [Test]
    public void ShouldReturnNullWhenStartPositionIsNullBlock()
    {
        // Arrange: Grid with null block at start position
        // [R][R][ ][R][R]
        //  0  1  2  3  4
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        _testgrid[(2, 0)] = null; // Explicitly add null block
        CreateBlockAt(3, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(4, 0, EBLOCKCOLORTYPE.RED);

        // Act: Try to detect match from null block position
        var result = _matchdetector.DetectHorizontalMatch(_testgrid, (2, 0));

        // Assert: Should return null (start position has null block)
        Assert.IsNull(result, "Should return null when start position is null block");
    }

    [Test]
    public void ShouldStopMatchWhenNullBlockInMiddle()
    {
        // Arrange: Grid with null block breaking potential match
        // [R][R][ ][R][R]
        //  0  1  2  3  4
        // Without null at (2,0), this would be a 5-match
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        _testgrid[(2, 0)] = null; // null block breaks the match
        CreateBlockAt(3, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(4, 0, EBLOCKCOLORTYPE.RED);

        // Act: Detect match from left side
        var resultLeft = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 0));
        var resultRight = _matchdetector.DetectHorizontalMatch(_testgrid, (3, 0));

        // Assert: Should only detect 2-block segments (not enough for match)
        Assert.IsNull(resultLeft, "Should return null - only 2 blocks before null");
        Assert.IsNull(resultRight, "Should return null - only 2 blocks after null");
    }

    [Test]
    public void ShouldTreatBlocksAfterNullAsSeparateMatch()
    {
        // Arrange: Grid with null block separating two valid matches
        // [R][R][R][ ][R][R][R]
        //  0  1  2  3  4  5  6
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED);
        _testgrid[(3, 0)] = null; // Separator
        CreateBlockAt(4, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(5, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(6, 0, EBLOCKCOLORTYPE.RED);

        // Act: Detect matches on both sides
        var resultLeft = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 0));
        var resultRight = _matchdetector.DetectHorizontalMatch(_testgrid, (4, 0));

        // Assert: Should detect two separate 3-matches
        Assert.IsNotNull(resultLeft, "Should detect left 3-match");
        Assert.AreEqual(3, resultLeft.Count, "Left match should have 3 blocks");
        Assert.Contains((0, 0), resultLeft);
        Assert.Contains((1, 0), resultLeft);
        Assert.Contains((2, 0), resultLeft);

        Assert.IsNotNull(resultRight, "Should detect right 3-match");
        Assert.AreEqual(3, resultRight.Count, "Right match should have 3 blocks");
        Assert.Contains((4, 0), resultRight);
        Assert.Contains((5, 0), resultRight);
        Assert.Contains((6, 0), resultRight);
    }

    [Test]
    public void ShouldReturnNullWhenAllBlocksAreNull()
    {
        // Arrange: Grid with only null blocks
        _testgrid[(0, 0)] = null;
        _testgrid[(1, 0)] = null;
        _testgrid[(2, 0)] = null;
        _testgrid[(0, 1)] = null;
        _testgrid[(1, 1)] = null;

        // Act: Try to detect matches
        var resultH1 = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 0));
        var resultH2 = _matchdetector.DetectHorizontalMatch(_testgrid, (1, 0));
        var resultV1 = _matchdetector.DetectVerticalMatch(_testgrid, (0, 0));

        // Assert: All should return null
        Assert.IsNull(resultH1, "Should return null - start block is null");
        Assert.IsNull(resultH2, "Should return null - start block is null");
        Assert.IsNull(resultV1, "Should return null - start block is null");
    }

    // Test 1.3: 불연속 매치 처리

    [Test]
    public void ShouldNotDetectSplitMatchesWith2BlockSegments()
    {
        // Arrange: Pattern with different color breaking match into 2+2 segments
        // [R][R][B][R][R]
        //  0  1  2  3  4
        // Two 2-block segments don't count as matches (need 3+)
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.BLUE); // Breaker
        CreateBlockAt(3, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(4, 0, EBLOCKCOLORTYPE.RED);

        // Act: Try to detect matches from each position
        var result0 = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 0));
        var result1 = _matchdetector.DetectHorizontalMatch(_testgrid, (1, 0));
        var result2 = _matchdetector.DetectHorizontalMatch(_testgrid, (2, 0));
        var result3 = _matchdetector.DetectHorizontalMatch(_testgrid, (3, 0));
        var result4 = _matchdetector.DetectHorizontalMatch(_testgrid, (4, 0));

        // Assert: All should return null (only 2-block segments, not enough)
        Assert.IsNull(result0, "Should return null - only 2 RED blocks on left");
        Assert.IsNull(result1, "Should return null - only 2 RED blocks on left");
        Assert.IsNull(result2, "Should return null - BLUE block doesn't match neighbors");
        Assert.IsNull(result3, "Should return null - only 2 RED blocks on right");
        Assert.IsNull(result4, "Should return null - only 2 RED blocks on right");
    }

    [Test]
    public void ShouldNotDetectMatchWithGapsBetweenBlocks()
    {
        // Arrange: Pattern with gaps (empty/null positions) between same-color blocks
        // [R][ ][R][ ][R]
        //  0  1  2  3  4
        // Three RED blocks but not continuous - should NOT match
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        // (1, 0) is gap - no block added
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED);
        // (3, 0) is gap - no block added
        CreateBlockAt(4, 0, EBLOCKCOLORTYPE.RED);

        // Act: Try to detect matches from each RED block position
        var result0 = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 0));
        var result2 = _matchdetector.DetectHorizontalMatch(_testgrid, (2, 0));
        var result4 = _matchdetector.DetectHorizontalMatch(_testgrid, (4, 0));

        // Assert: All should return null (blocks not continuous)
        Assert.IsNull(result0, "Should return null - gap at (1,0)");
        Assert.IsNull(result2, "Should return null - gaps at (1,0) and (3,0)");
        Assert.IsNull(result4, "Should return null - gap at (3,0)");
    }

    [Test]
    public void ShouldDetectTwoSeparateMatchesIndependently()
    {
        // Arrange: Pattern with breaker separating two valid 3-matches
        // [R][R][R][B][R][R][R]
        //  0  1  2  3  4  5  6
        // Should detect two separate 3-matches, not one 7-match
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(3, 0, EBLOCKCOLORTYPE.BLUE); // Breaker
        CreateBlockAt(4, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(5, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(6, 0, EBLOCKCOLORTYPE.RED);

        // Act: Detect from each side
        var resultLeft = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 0));
        var resultRight = _matchdetector.DetectHorizontalMatch(_testgrid, (4, 0));

        // Assert: Should detect two independent 3-matches
        Assert.IsNotNull(resultLeft, "Should detect left 3-match");
        Assert.AreEqual(3, resultLeft.Count, "Left match should have exactly 3 blocks");
        Assert.Contains((0, 0), resultLeft);
        Assert.Contains((1, 0), resultLeft);
        Assert.Contains((2, 0), resultLeft);
        Assert.IsFalse(resultLeft.Contains((3, 0)), "Should NOT include BLUE breaker");
        Assert.IsFalse(resultLeft.Contains((4, 0)), "Should NOT cross breaker");

        Assert.IsNotNull(resultRight, "Should detect right 3-match");
        Assert.AreEqual(3, resultRight.Count, "Right match should have exactly 3 blocks");
        Assert.Contains((4, 0), resultRight);
        Assert.Contains((5, 0), resultRight);
        Assert.Contains((6, 0), resultRight);
        Assert.IsFalse(resultRight.Contains((3, 0)), "Should NOT include BLUE breaker");
        Assert.IsFalse(resultRight.Contains((2, 0)), "Should NOT cross breaker");
    }

    [Test]
    public void ShouldDetectLongMatchAsOneMatch()
    {
        // Arrange: Pattern with 7 continuous same-color blocks
        // [R][R][R][R][R][R][R]
        //  0  1  2  3  4  5  6
        // Should detect as one 7-match, not multiple smaller matches
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(3, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(4, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(5, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(6, 0, EBLOCKCOLORTYPE.RED);

        // Act: Detect from different positions
        var resultStart = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 0));
        var resultMiddle = _matchdetector.DetectHorizontalMatch(_testgrid, (3, 0));
        var resultEnd = _matchdetector.DetectHorizontalMatch(_testgrid, (6, 0));

        // Assert: All positions should return the same 7-match
        Assert.IsNotNull(resultStart, "Should detect match from start");
        Assert.AreEqual(7, resultStart.Count, "Should detect all 7 blocks as one match");

        Assert.IsNotNull(resultMiddle, "Should detect match from middle");
        Assert.AreEqual(7, resultMiddle.Count, "Should detect all 7 blocks from middle");

        Assert.IsNotNull(resultEnd, "Should detect match from end");
        Assert.AreEqual(7, resultEnd.Count, "Should detect all 7 blocks from end");

        // All results should contain all 7 positions
        for (int i = 0; i < 7; i++)
        {
            Assert.Contains((i, 0), resultStart, $"Start result should contain ({i},0)");
            Assert.Contains((i, 0), resultMiddle, $"Middle result should contain ({i},0)");
            Assert.Contains((i, 0), resultEnd, $"End result should contain ({i},0)");
        }
    }

    // Test 1.4: 극단적인 그리드 크기

    [Test]
    public void ShouldReturnNullForSingleBlockGrid()
    {
        // Arrange: 1x1 grid with single block
        // [R]
        // Only one block - impossible to match (need 3+)
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);

        // Act: Try to detect matches
        var resultHorizontal = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 0));
        var resultVertical = _matchdetector.DetectVerticalMatch(_testgrid, (0, 0));
        var resultCross = _matchdetector.DetectCrossMatch(_testgrid, (0, 0));

        // Assert: All should return null (only 1 block)
        Assert.IsNull(resultHorizontal, "Should return null - only 1 block horizontally");
        Assert.IsNull(resultVertical, "Should return null - only 1 block vertically");
        Assert.IsFalse(resultCross.HasValue, "Should return null - no cross possible with 1 block");
    }

    [Test]
    public void ShouldDetectOnlyVerticalMatchIn1xNGrid()
    {
        // Arrange: 1xN grid (single column) with 5 vertical blocks
        // [R] y=4
        // [R] y=3
        // [R] y=2
        // [R] y=1
        // [R] y=0
        // Only one column - horizontal match impossible
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(0, 1, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(0, 2, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(0, 3, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(0, 4, EBLOCKCOLORTYPE.RED);

        // Act: Try to detect matches
        var resultVertical = _matchdetector.DetectVerticalMatch(_testgrid, (0, 0));
        var resultHorizontal = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 2));

        // Assert: Should detect vertical match but not horizontal
        Assert.IsNotNull(resultVertical, "Should detect vertical match");
        Assert.AreEqual(5, resultVertical.Count, "Should detect all 5 vertical blocks");
        Assert.Contains((0, 0), resultVertical);
        Assert.Contains((0, 1), resultVertical);
        Assert.Contains((0, 2), resultVertical);
        Assert.Contains((0, 3), resultVertical);
        Assert.Contains((0, 4), resultVertical);

        Assert.IsNull(resultHorizontal, "Should return null for horizontal - only 1 block wide");
    }

    [Test]
    public void ShouldDetectOnlyHorizontalMatchInNx1Grid()
    {
        // Arrange: Nx1 grid (single row) with 5 horizontal blocks
        // [B][B][B][B][B]
        // x=0 1  2  3  4
        // Only one row - vertical match impossible
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(3, 0, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(4, 0, EBLOCKCOLORTYPE.BLUE);

        // Act: Try to detect matches
        var resultHorizontal = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 0));
        var resultVertical = _matchdetector.DetectVerticalMatch(_testgrid, (2, 0));

        // Assert: Should detect horizontal match but not vertical
        Assert.IsNotNull(resultHorizontal, "Should detect horizontal match");
        Assert.AreEqual(5, resultHorizontal.Count, "Should detect all 5 horizontal blocks");
        Assert.Contains((0, 0), resultHorizontal);
        Assert.Contains((1, 0), resultHorizontal);
        Assert.Contains((2, 0), resultHorizontal);
        Assert.Contains((3, 0), resultHorizontal);
        Assert.Contains((4, 0), resultHorizontal);

        Assert.IsNull(resultVertical, "Should return null for vertical - only 1 block tall");
    }

    [Test]
    public void ShouldHandleLargeGridWithoutPerformanceIssues()
    {
        // Arrange: Create 100x100 grid (10,000 blocks) with a match pattern
        // This tests that the algorithm scales well with large grids
        // Place scattered match patterns throughout the large grid

        // Fill large grid with random blocks
        var random = new System.Random(42); // Fixed seed for reproducibility
        var colors = new[] { EBLOCKCOLORTYPE.RED, EBLOCKCOLORTYPE.BLUE, EBLOCKCOLORTYPE.GREEN, EBLOCKCOLORTYPE.YELLOW };

        for (int x = 0; x < 100; x++)
        {
            for (int y = 0; y < 100; y++)
            {
                var color = colors[random.Next(colors.Length)];
                CreateBlockAt(x, y, color);
            }
        }

        // Create a known 5-match pattern at specific location (50, 50)
        CreateBlockAt(48, 50, EBLOCKCOLORTYPE.PINK);
        CreateBlockAt(49, 50, EBLOCKCOLORTYPE.PINK);
        CreateBlockAt(50, 50, EBLOCKCOLORTYPE.PINK);
        CreateBlockAt(51, 50, EBLOCKCOLORTYPE.PINK);
        CreateBlockAt(52, 50, EBLOCKCOLORTYPE.PINK);

        // Act: Detect match at known location and measure time
        var startTime = System.DateTime.Now;
        var result = _matchdetector.DetectHorizontalMatch(_testgrid, (50, 50));
        var elapsedTime = (System.DateTime.Now - startTime).TotalMilliseconds;

        // Assert: Should detect match quickly (< 100ms) and correctly
        Assert.IsNotNull(result, "Should detect horizontal match in large grid");
        Assert.AreEqual(5, result.Count, "Should detect 5 PINK blocks");
        Assert.Contains((48, 50), result);
        Assert.Contains((49, 50), result);
        Assert.Contains((50, 50), result);
        Assert.Contains((51, 50), result);
        Assert.Contains((52, 50), result);

        // Performance check: Should complete in reasonable time
        // On modern hardware, this should be nearly instant (< 10ms typical)
        // Allow up to 100ms to account for slower CI environments
        Assert.Less(elapsedTime, 100.0, $"Detection should be fast even in large grid, took {elapsedTime}ms");

        Debug.Log($"[Performance] 100x100 grid match detection: {elapsedTime}ms");
    }

    [Test]
    public void ShouldDetectMatchInIrregularLShapeGrid()
    {
        // Arrange: Create L-shaped grid (불규칙한 그리드)
        // [R][R][R]     x=0,1,2 y=2
        // [R]           x=0 y=1
        // [R]           x=0 y=0
        // L자형 그리드에서 수직 매치 감지

        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(0, 1, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(0, 2, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(1, 2, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(2, 2, EBLOCKCOLORTYPE.RED);

        // Act: Detect vertical match on left edge
        var resultVertical = _matchdetector.DetectVerticalMatch(_testgrid, (0, 0));

        // Detect horizontal match on top edge
        var resultHorizontal = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 2));

        // Assert: Should detect both matches in L-shaped grid
        Assert.IsNotNull(resultVertical, "Should detect 3 vertical RED blocks on left edge");
        Assert.AreEqual(3, resultVertical.Count);
        Assert.Contains((0, 0), resultVertical);
        Assert.Contains((0, 1), resultVertical);
        Assert.Contains((0, 2), resultVertical);

        Assert.IsNotNull(resultHorizontal, "Should detect 3 horizontal RED blocks on top edge");
        Assert.AreEqual(3, resultHorizontal.Count);
        Assert.Contains((0, 2), resultHorizontal);
        Assert.Contains((1, 2), resultHorizontal);
        Assert.Contains((2, 2), resultHorizontal);
    }

    [Test]
    public void ShouldDetectMatchInIrregularUShapeGrid()
    {
        // Arrange: Create ㄷ-shaped grid (U-shape)
        // [B]     [B]     x=0,2 y=3
        // [B]           x=0 y=2
        // [B][B][B]       x=0,1,2 y=1
        // ㄷ자형 그리드에서 여러 매치 패턴 감지

        // Left vertical line (3 blocks)
        CreateBlockAt(0, 1, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(0, 2, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(0, 3, EBLOCKCOLORTYPE.BLUE);

        // Bottom horizontal line (3 blocks)
        CreateBlockAt(1, 1, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(2, 1, EBLOCKCOLORTYPE.BLUE);

        // Right vertical line (only 2 blocks - NOT enough for match)
        CreateBlockAt(2, 3, EBLOCKCOLORTYPE.BLUE);

        // Act: Detect matches at various positions
        var resultLeftVertical = _matchdetector.DetectVerticalMatch(_testgrid, (0, 2));
        var resultBottomHorizontal = _matchdetector.DetectHorizontalMatch(_testgrid, (1, 1));

        // Assert: Should detect left vertical match (3 blocks)
        Assert.IsNotNull(resultLeftVertical, "Should detect 3 vertical BLUE blocks on left");
        Assert.AreEqual(3, resultLeftVertical.Count);

        // Should detect bottom horizontal match (3 blocks)
        Assert.IsNotNull(resultBottomHorizontal, "Should detect 3 horizontal BLUE blocks on bottom");
        Assert.AreEqual(3, resultBottomHorizontal.Count);

        // Right edge only has 2 blocks - should NOT match
        var resultRightVertical = _matchdetector.DetectVerticalMatch(_testgrid, (2, 3));
        Assert.IsNull(resultRightVertical, "Should NOT detect match - only 2 blocks on right edge");
    }

    [Test]
    public void ShouldHandleSparseGridWithGaps()
    {
        // Arrange: Sparse grid with large gaps (불규칙한 배치)
        // [G]                 x=0 y=10
        // [G]                 x=0 y=5
        // [G]                 x=0 y=0
        //           [Y][Y][Y] x=20,21,22 y=0
        // 큰 간격이 있는 sparse grid에서도 올바르게 동작

        // Vertical match at x=0 with large Y gaps
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.GREEN);
        CreateBlockAt(0, 5, EBLOCKCOLORTYPE.GREEN);
        CreateBlockAt(0, 10, EBLOCKCOLORTYPE.GREEN);

        // Horizontal match at y=0 far away (x=20~22)
        CreateBlockAt(20, 0, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(21, 0, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(22, 0, EBLOCKCOLORTYPE.YELLOW);

        // Act
        var resultVertical = _matchdetector.DetectVerticalMatch(_testgrid, (0, 0));
        var resultHorizontal = _matchdetector.DetectHorizontalMatch(_testgrid, (20, 0));

        // Assert: Should NOT detect vertical (gaps between blocks)
        Assert.IsNull(resultVertical, "Should NOT detect vertical - blocks have gaps (y=0,5,10)");

        // Should detect horizontal (continuous blocks)
        Assert.IsNotNull(resultHorizontal, "Should detect horizontal match at x=20-22");
        Assert.AreEqual(3, resultHorizontal.Count);
    }

    private void CreateBlockAt(int x, int y, EBLOCKCOLORTYPE color)
    {
        // If block already exists at this position, destroy it first
        if (_testgrid.ContainsKey((x, y)))
        {
            var existingblock = _testgrid[(x, y)];
            if (existingblock != null)
            {
                Object.DestroyImmediate(existingblock.gameObject);
            }
            _testgrid.Remove((x, y));
        }

        var gameobject = new GameObject($"Block_{x}_{y}");
        var block = gameobject.AddComponent<UI_Match_Block>();

        // Use reflection to set private fields for testing
        var colorfield = typeof(UI_Match_Block).GetField("_colortypes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        colorfield?.SetValue(block, color);

        _testgrid.Add((x, y), block);
    }
}