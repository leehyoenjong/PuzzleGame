using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[TestFixture]
public class MatchTypeClassifierTests
{
    private MatchTypeClassifier _matchtypeclassifier;
    private Dictionary<(int, int), UI_Match_Block> _testgrid;

    [SetUp]
    public void SetUp()
    {
        _matchtypeclassifier = new MatchTypeClassifier();
        _testgrid = new Dictionary<(int, int), UI_Match_Block>();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var block in _testgrid.Values)
        {
            Object.DestroyImmediate(block.gameObject);
        }
        _testgrid.Clear();
    }

    [Test]
    public void ShouldClassify3MatchAsThree()
    {
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 0, EBLOCKCOLORTYPE.RED),
            CreateBlock(1, 0, EBLOCKCOLORTYPE.RED),
            CreateBlock(2, 0, EBLOCKCOLORTYPE.RED)
        };
        var ylist = new List<UI_Match_Block>();

        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        Assert.AreEqual(EMATCHTYPE.THREE, result);
    }

    [Test]
    public void ShouldClassify4HorizontalMatchAsForeLeftRight()
    {
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 0, EBLOCKCOLORTYPE.RED),
            CreateBlock(1, 0, EBLOCKCOLORTYPE.RED),
            CreateBlock(2, 0, EBLOCKCOLORTYPE.RED),
            CreateBlock(3, 0, EBLOCKCOLORTYPE.RED)
        };
        var ylist = new List<UI_Match_Block>();

        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        Assert.AreEqual(EMATCHTYPE.FORE_LEFTRIGHT, result);
    }

    [Test]
    public void ShouldClassify4VerticalMatchAsForeUpDown()
    {
        var xlist = new List<UI_Match_Block>();
        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(0, 0, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(0, 1, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(0, 2, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(0, 3, EBLOCKCOLORTYPE.BLUE)
        };

        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        Assert.AreEqual(EMATCHTYPE.FORE_UPDOWN, result);
    }

    [Test]
    public void ShouldClassify5MatchAsFive()
    {
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 0, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(1, 0, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(2, 0, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(3, 0, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(4, 0, EBLOCKCOLORTYPE.YELLOW)
        };
        var ylist = new List<UI_Match_Block>();

        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        Assert.AreEqual(EMATCHTYPE.FIVE, result);
    }

    [Test]
    public void ShouldClassify6MatchAsFive()
    {
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 0, EBLOCKCOLORTYPE.GREEN),
            CreateBlock(1, 0, EBLOCKCOLORTYPE.GREEN),
            CreateBlock(2, 0, EBLOCKCOLORTYPE.GREEN),
            CreateBlock(3, 0, EBLOCKCOLORTYPE.GREEN),
            CreateBlock(4, 0, EBLOCKCOLORTYPE.GREEN),
            CreateBlock(5, 0, EBLOCKCOLORTYPE.GREEN)
        };
        var ylist = new List<UI_Match_Block>();

        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        Assert.AreEqual(EMATCHTYPE.FIVE, result);
    }

    [Test]
    public void ShouldClassify3x3SameColorCrossAsCrossThree()
    {
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 1, EBLOCKCOLORTYPE.RED),
            CreateBlock(1, 1, EBLOCKCOLORTYPE.RED),
            CreateBlock(2, 1, EBLOCKCOLORTYPE.RED)
        };
        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(1, 0, EBLOCKCOLORTYPE.RED),
            CreateBlock(1, 1, EBLOCKCOLORTYPE.RED),
            CreateBlock(1, 2, EBLOCKCOLORTYPE.RED)
        };

        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        Assert.AreEqual(EMATCHTYPE.CROSS_THREE, result);
    }

    [Test]
    public void ShouldClassify4x4SameColorCrossAsCrossFour()
    {
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 1, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(1, 1, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(2, 1, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(3, 1, EBLOCKCOLORTYPE.BLUE)
        };
        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(1, 0, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(1, 1, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(1, 2, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(1, 3, EBLOCKCOLORTYPE.BLUE)
        };

        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        Assert.AreEqual(EMATCHTYPE.CROSS_FOUR, result);
    }

    [Test]
    public void ShouldClassify5x5SameColorCrossAsCrossFive()
    {
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 2, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(1, 2, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(2, 2, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(3, 2, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(4, 2, EBLOCKCOLORTYPE.YELLOW)
        };
        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(2, 0, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(2, 1, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(2, 2, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(2, 3, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(2, 4, EBLOCKCOLORTYPE.YELLOW)
        };

        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        Assert.AreEqual(EMATCHTYPE.CROSS_FIVE, result);
    }

    [Test]
    public void ShouldNotClassifyCrossWhenColorsDiffer()
    {
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 1, EBLOCKCOLORTYPE.RED),
            CreateBlock(1, 1, EBLOCKCOLORTYPE.RED),
            CreateBlock(2, 1, EBLOCKCOLORTYPE.RED)
        };
        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(1, 0, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(1, 1, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(1, 2, EBLOCKCOLORTYPE.BLUE)
        };

        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        Assert.AreEqual(EMATCHTYPE.THREE, result);
    }

    [Test]
    public void ShouldClassifyLShape5MatchAsFive()
    {
        // L-shape: 3 horizontal + 3 vertical with 1 overlap = 5 unique blocks
        // Layout:
        //   [R]       (1, 2)
        //   [R]       (1, 1)
        // [R][R][R]   (0,0) (1,0) (2,0)
        var cornerblock = CreateBlock(1, 0, EBLOCKCOLORTYPE.RED);
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 0, EBLOCKCOLORTYPE.RED),
            cornerblock,
            CreateBlock(2, 0, EBLOCKCOLORTYPE.RED)
        };
        var ylist = new List<UI_Match_Block>
        {
            cornerblock,
            CreateBlock(1, 1, EBLOCKCOLORTYPE.RED),
            CreateBlock(1, 2, EBLOCKCOLORTYPE.RED)
        };

        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        Assert.AreEqual(EMATCHTYPE.FIVE, result,
            "L-shape with 5 unique blocks should be classified as FIVE, not CROSS_THREE");
    }

    [Test]
    public void ShouldClassifyCrossOnlyWhenIntersectionExists()
    {
        // True CROSS: Must have exactly 1 intersection point
        var centerblock = CreateBlock(1, 1, EBLOCKCOLORTYPE.RED);
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 1, EBLOCKCOLORTYPE.RED),
            centerblock,
            CreateBlock(2, 1, EBLOCKCOLORTYPE.RED)
        };
        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(1, 0, EBLOCKCOLORTYPE.RED),
            centerblock,
            CreateBlock(1, 2, EBLOCKCOLORTYPE.RED)
        };

        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        Assert.AreEqual(EMATCHTYPE.CROSS_THREE, result,
            "3x3 pattern with center intersection should be CROSS_THREE");
    }

    private UI_Match_Block CreateBlock(int x, int y, EBLOCKCOLORTYPE color)
    {
        if (_testgrid.ContainsKey((x, y)))
        {
            return _testgrid[(x, y)];
        }

        var gameobject = new GameObject($"Block_{x}_{y}");
        var block = gameobject.AddComponent<UI_Match_Block>();

        var colorfield = typeof(UI_Match_Block).GetField("_colortypes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        colorfield?.SetValue(block, color);

        var xfield = typeof(UI_Match_Block).GetField("_x",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        xfield?.SetValue(block, x);

        var yfield = typeof(UI_Match_Block).GetField("_y",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        yfield?.SetValue(block, y);

        _testgrid.Add((x, y), block);
        return block;
    }

    // Test 2.1: 색상 검증 엣지 케이스

    [Test]
    public void ShouldHandleNullXListWithoutException()
    {
        // Arrange: xlist is null, ylist has valid blocks (3-match vertical)
        List<UI_Match_Block> xlist = null;
        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(0, 0, EBLOCKCOLORTYPE.RED),
            CreateBlock(0, 1, EBLOCKCOLORTYPE.RED),
            CreateBlock(0, 2, EBLOCKCOLORTYPE.RED)
        };

        // Act: Should handle null gracefully and return THREE for valid ylist
        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert: Should return THREE (3 vertical blocks) without throwing exception
        Assert.AreEqual(EMATCHTYPE.THREE, result,
            "Should handle null xlist gracefully and classify based on ylist alone");
    }

    [Test]
    public void ShouldHandleNullYListWithoutException()
    {
        // Arrange: xlist has valid blocks (4-match horizontal), ylist is null
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 0, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(1, 0, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(2, 0, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(3, 0, EBLOCKCOLORTYPE.BLUE)
        };
        List<UI_Match_Block> ylist = null;

        // Act: Should handle null gracefully and return FORE_LEFTRIGHT for 4 horizontal blocks
        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert: Should return FORE_LEFTRIGHT (4 horizontal blocks) without throwing exception
        Assert.AreEqual(EMATCHTYPE.FORE_LEFTRIGHT, result,
            "Should handle null ylist gracefully and classify based on xlist alone");
    }

    [Test]
    public void ShouldReturnThreeWhenBothListsAreNull()
    {
        // Arrange: Both xlist and ylist are null
        List<UI_Match_Block> xlist = null;
        List<UI_Match_Block> ylist = null;

        // Act: Should handle both nulls gracefully
        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert: Should return THREE as default fallback
        Assert.AreEqual(EMATCHTYPE.THREE, result,
            "Should return THREE as default when both lists are null");
    }

    [Test]
    public void ShouldClassifyBasedOnYListWhenXListIsEmpty()
    {
        // Arrange: xlist is empty, ylist has 4 vertical blocks
        var xlist = new List<UI_Match_Block>(); // Empty list
        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(0, 0, EBLOCKCOLORTYPE.GREEN),
            CreateBlock(0, 1, EBLOCKCOLORTYPE.GREEN),
            CreateBlock(0, 2, EBLOCKCOLORTYPE.GREEN),
            CreateBlock(0, 3, EBLOCKCOLORTYPE.GREEN)
        };

        // Act: Should classify based on ylist alone
        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert: Should return FORE_UPDOWN (4 vertical blocks)
        Assert.AreEqual(EMATCHTYPE.FORE_UPDOWN, result,
            "Should classify based on ylist when xlist is empty");
    }

    [Test]
    public void ShouldClassifyBasedOnXListWhenYListIsEmpty()
    {
        // Arrange: xlist has 5 horizontal blocks, ylist is empty
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 0, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(1, 0, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(2, 0, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(3, 0, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(4, 0, EBLOCKCOLORTYPE.YELLOW)
        };
        var ylist = new List<UI_Match_Block>(); // Empty list

        // Act: Should classify based on xlist alone
        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert: Should return FIVE (5 horizontal blocks, all same color)
        Assert.AreEqual(EMATCHTYPE.FIVE, result,
            "Should classify based on xlist when ylist is empty");
    }

    [Test]
    public void ShouldHandleListWithNullBlocksWithoutException()
    {
        // Arrange: xlist contains null blocks, ylist has valid blocks
        var xlist = new List<UI_Match_Block>
        {
            null,
            null,
            null
        };
        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(0, 0, EBLOCKCOLORTYPE.PINK),
            CreateBlock(0, 1, EBLOCKCOLORTYPE.PINK),
            CreateBlock(0, 2, EBLOCKCOLORTYPE.PINK)
        };

        // Act: Should filter out null blocks and classify based on valid ylist
        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert: Should return THREE based on valid ylist blocks
        Assert.AreEqual(EMATCHTYPE.THREE, result,
            "Should filter null blocks and classify based on valid blocks only");
    }

    // Test 2.2: L자형 vs T자형 vs 십자 구분

    [Test]
    public void ShouldClassifyTShapeAsCrossThree()
    {
        // Arrange: T-shape (3H + 3V, intersection at center of horizontal line)
        // Layout:
        //     [R]       (1, 2)
        //     [R]       (1, 1)
        // [R][R][R]     (0,1) (1,1) (2,1)
        // Intersection at (1,1) is center of both lines
        var centerblock = CreateBlock(1, 1, EBLOCKCOLORTYPE.RED);
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 1, EBLOCKCOLORTYPE.RED),
            centerblock,
            CreateBlock(2, 1, EBLOCKCOLORTYPE.RED)
        };
        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(1, 0, EBLOCKCOLORTYPE.RED),
            centerblock,
            CreateBlock(1, 2, EBLOCKCOLORTYPE.RED)
        };

        // Act
        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert: T-shape with center intersection should be CROSS_THREE
        Assert.AreEqual(EMATCHTYPE.CROSS_THREE, result,
            "T-shape with intersection at center of both lines should be CROSS_THREE");
    }

    [Test]
    public void ShouldClassifyInvertedTShapeAsCrossThree()
    {
        // Arrange: 역T자형 (3H + 3V, intersection at center of both lines)
        // Layout:
        // [B][B][B]     (0,1) (1,1) (2,1)
        //     [B]       (1, 0)
        //     [B]       (1, -1)
        // Intersection at (1,1) is center of both horizontal and vertical lines
        var centerblock = CreateBlock(1, 1, EBLOCKCOLORTYPE.BLUE);
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 1, EBLOCKCOLORTYPE.BLUE),
            centerblock,
            CreateBlock(2, 1, EBLOCKCOLORTYPE.BLUE)
        };
        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(1, 2, EBLOCKCOLORTYPE.BLUE),
            centerblock,
            CreateBlock(1, 0, EBLOCKCOLORTYPE.BLUE)
        };

        // Act
        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert: 역T-shape with center intersection should be CROSS_THREE
        Assert.AreEqual(EMATCHTYPE.CROSS_THREE, result,
            "Inverted T-shape with intersection at center of both lines should be CROSS_THREE");
    }

    [Test]
    public void ShouldClassifyReverseGShape4x3AsFive()
    {
        // Arrange: ㄱ자형 (4H + 3V, corner overlap at edge) = 6 unique blocks
        // Layout:
        //         [G]       (3, 2)
        //         [G]       (3, 1)
        // [G][G][G][G]      (0,0) (1,0) (2,0) (3,0)
        // Intersection at (3,0) is corner (edge of both lines)
        var cornerblock = CreateBlock(3, 0, EBLOCKCOLORTYPE.GREEN);
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 0, EBLOCKCOLORTYPE.GREEN),
            CreateBlock(1, 0, EBLOCKCOLORTYPE.GREEN),
            CreateBlock(2, 0, EBLOCKCOLORTYPE.GREEN),
            cornerblock
        };
        var ylist = new List<UI_Match_Block>
        {
            cornerblock,
            CreateBlock(3, 1, EBLOCKCOLORTYPE.GREEN),
            CreateBlock(3, 2, EBLOCKCOLORTYPE.GREEN)
        };

        // Act
        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert: ㄱ-shape with corner intersection (6 unique blocks) should be FIVE
        Assert.AreEqual(EMATCHTYPE.FIVE, result,
            "ㄱ-shape with 6 unique blocks should be classified as FIVE, not CROSS");
    }

    [Test]
    public void ShouldClassifyGShape3x4AsFive()
    {
        // Arrange: ㄴ자형 (3H + 4V, corner overlap at edge) = 6 unique blocks
        // Layout:
        // [Y]           (0, 3)
        // [Y]           (0, 2)
        // [Y]           (0, 1)
        // [Y][Y][Y]     (0,0) (1,0) (2,0)
        // Intersection at (0,0) is corner (edge of both lines)
        var cornerblock = CreateBlock(0, 0, EBLOCKCOLORTYPE.YELLOW);
        var xlist = new List<UI_Match_Block>
        {
            cornerblock,
            CreateBlock(1, 0, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(2, 0, EBLOCKCOLORTYPE.YELLOW)
        };
        var ylist = new List<UI_Match_Block>
        {
            cornerblock,
            CreateBlock(0, 1, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(0, 2, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(0, 3, EBLOCKCOLORTYPE.YELLOW)
        };

        // Act
        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert: ㄴ-shape with corner intersection (6 unique blocks) should be FIVE
        Assert.AreEqual(EMATCHTYPE.FIVE, result,
            "ㄴ-shape with 6 unique blocks should be classified as FIVE, not CROSS");
    }

    [Test]
    public void ShouldClassify5x5CrossAsCrossFive()
    {
        // Arrange: 십자형 (5H + 5V, center intersection) = 9 unique blocks
        // Layout:
        //     [P]           (2, 4)
        //     [P]           (2, 3)
        // [P][P][P][P][P]   (0,2) (1,2) (2,2) (3,2) (4,2)
        //     [P]           (2, 1)
        //     [P]           (2, 0)
        // Intersection at (2,2) is center of both lines
        var centerblock = CreateBlock(2, 2, EBLOCKCOLORTYPE.PINK);
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 2, EBLOCKCOLORTYPE.PINK),
            CreateBlock(1, 2, EBLOCKCOLORTYPE.PINK),
            centerblock,
            CreateBlock(3, 2, EBLOCKCOLORTYPE.PINK),
            CreateBlock(4, 2, EBLOCKCOLORTYPE.PINK)
        };
        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(2, 0, EBLOCKCOLORTYPE.PINK),
            CreateBlock(2, 1, EBLOCKCOLORTYPE.PINK),
            centerblock,
            CreateBlock(2, 3, EBLOCKCOLORTYPE.PINK),
            CreateBlock(2, 4, EBLOCKCOLORTYPE.PINK)
        };

        // Act
        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert: 5x5 cross with center intersection should be CROSS_FIVE
        Assert.AreEqual(EMATCHTYPE.CROSS_FIVE, result,
            "5x5 cross pattern with center intersection should be classified as CROSS_FIVE");
    }

    // Test 2.3: 복합 패턴 우선순위

    [Test]
    public void ShouldClassify3H5VOverlapAsFive()
    {
        // Arrange: 3H + 5V overlap pattern = 7 unique blocks (same color)
        // Layout:
        //     [R]           (1, 4)
        //     [R]           (1, 3)
        //     [R]           (1, 2)
        // [R][R][R]         (0,1) (1,1) (2,1)
        //     [R]           (1, 0)
        // Intersection at (1,1) - 3 horizontal + 5 vertical
        var centerblock = CreateBlock(1, 1, EBLOCKCOLORTYPE.RED);
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 1, EBLOCKCOLORTYPE.RED),
            centerblock,
            CreateBlock(2, 1, EBLOCKCOLORTYPE.RED)
        };
        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(1, 0, EBLOCKCOLORTYPE.RED),
            centerblock,
            CreateBlock(1, 2, EBLOCKCOLORTYPE.RED),
            CreateBlock(1, 3, EBLOCKCOLORTYPE.RED),
            CreateBlock(1, 4, EBLOCKCOLORTYPE.RED)
        };

        // Act
        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert: 3H + 5V with 7 unique blocks should be FIVE
        Assert.AreEqual(EMATCHTYPE.FIVE, result,
            "3H + 5V overlap with 7 unique blocks should be classified as FIVE");
    }

    [Test]
    public void ShouldClassify5H3VOverlapAsFive()
    {
        // Arrange: 5H + 3V overlap pattern = 7 unique blocks (same color)
        // Layout:
        //         [B]       (2, 2)
        // [B][B][B][B][B]   (0,1) (1,1) (2,1) (3,1) (4,1)
        //         [B]       (2, 0)
        // Intersection at (2,1) - 5 horizontal + 3 vertical
        var centerblock = CreateBlock(2, 1, EBLOCKCOLORTYPE.BLUE);
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 1, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(1, 1, EBLOCKCOLORTYPE.BLUE),
            centerblock,
            CreateBlock(3, 1, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(4, 1, EBLOCKCOLORTYPE.BLUE)
        };
        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(2, 0, EBLOCKCOLORTYPE.BLUE),
            centerblock,
            CreateBlock(2, 2, EBLOCKCOLORTYPE.BLUE)
        };

        // Act
        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert: 5H + 3V with 7 unique blocks should be FIVE
        Assert.AreEqual(EMATCHTYPE.FIVE, result,
            "5H + 3V overlap with 7 unique blocks should be classified as FIVE");
    }

    [Test]
    public void ShouldClassify4H4VCornerOverlapAsFive()
    {
        // Arrange: 4H + 4V corner overlap pattern = 7 unique blocks (same color)
        // Layout:
        //         [G]       (3, 3)
        //         [G]       (3, 2)
        //         [G]       (3, 1)
        // [G][G][G][G]      (0,0) (1,0) (2,0) (3,0)
        // Intersection at (3,0) - corner (edge of both lines)
        var cornerblock = CreateBlock(3, 0, EBLOCKCOLORTYPE.GREEN);
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 0, EBLOCKCOLORTYPE.GREEN),
            CreateBlock(1, 0, EBLOCKCOLORTYPE.GREEN),
            CreateBlock(2, 0, EBLOCKCOLORTYPE.GREEN),
            cornerblock
        };
        var ylist = new List<UI_Match_Block>
        {
            cornerblock,
            CreateBlock(3, 1, EBLOCKCOLORTYPE.GREEN),
            CreateBlock(3, 2, EBLOCKCOLORTYPE.GREEN),
            CreateBlock(3, 3, EBLOCKCOLORTYPE.GREEN)
        };

        // Act
        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert: 4H + 4V corner overlap with 7 unique blocks should be FIVE
        Assert.AreEqual(EMATCHTYPE.FIVE, result,
            "4H + 4V corner overlap with 7 unique blocks should be classified as FIVE");
    }

    [Test]
    public void ShouldClassifyDifferentColor4H4VBasedOnXList()
    {
        // Arrange: 4H (RED) + 4V (BLUE) with different colors
        // When colors differ, CROSS and FIVE are not applicable
        // Should classify based on xlist (4 blocks) → FORE_LEFTRIGHT
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 1, EBLOCKCOLORTYPE.RED),
            CreateBlock(1, 1, EBLOCKCOLORTYPE.RED),
            CreateBlock(2, 1, EBLOCKCOLORTYPE.RED),
            CreateBlock(3, 1, EBLOCKCOLORTYPE.RED)
        };
        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(1, 0, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(1, 1, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(1, 2, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(1, 3, EBLOCKCOLORTYPE.BLUE)
        };

        // Act
        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert: Different colors should classify based on xlist
        Assert.AreEqual(EMATCHTYPE.FORE_LEFTRIGHT, result,
            "4H + 4V with different colors should classify based on xlist (FORE_LEFTRIGHT)");
    }

    // Test 2.4: 교차점 검증

    [Test]
    public void ShouldClassifyParallelPatternsIndependently()
    {
        // Arrange: 교차점이 없는 평행선 패턴
        // xlist: y=0 라인에 3개 블록 (0,0) (1,0) (2,0)
        // ylist: y=2 라인에 3개 블록 (0,2) (1,2) (2,2)
        // 두 리스트가 전혀 교차하지 않음 → 각각 독립적으로 분류
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 0, EBLOCKCOLORTYPE.RED),
            CreateBlock(1, 0, EBLOCKCOLORTYPE.RED),
            CreateBlock(2, 0, EBLOCKCOLORTYPE.RED)
        };
        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(0, 2, EBLOCKCOLORTYPE.RED),
            CreateBlock(1, 2, EBLOCKCOLORTYPE.RED),
            CreateBlock(2, 2, EBLOCKCOLORTYPE.RED)
        };

        // Act
        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert: 교차점이 없으면 xlist 기준으로만 분류 → THREE
        Assert.AreEqual(EMATCHTYPE.THREE, result,
            "Parallel patterns with no intersection should classify based on xlist independently (THREE)");
    }

    [Test]
    public void ShouldHandleMultipleIntersectionPoints()
    {
        // Arrange: 교차점이 2개인 격자 패턴
        // xlist: 가로 (0,1) (1,1) (2,1) (3,1) - 4개
        // ylist: 인위적으로 두 개의 세로 라인 포함
        //        (1,0) (1,1) (1,2) + (2,0) (2,1) (2,2) - 6개
        // 교차점: (1,1), (2,1) - 2개
        // 기대값: CROSS가 아니므로 고유 블록 수로 판정 (8개 → FIVE)
        var intersection1 = CreateBlock(1, 1, EBLOCKCOLORTYPE.RED);
        var intersection2 = CreateBlock(2, 1, EBLOCKCOLORTYPE.RED);

        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 1, EBLOCKCOLORTYPE.RED),
            intersection1,
            intersection2,
            CreateBlock(3, 1, EBLOCKCOLORTYPE.RED)
        };

        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(1, 0, EBLOCKCOLORTYPE.RED),
            intersection1,
            CreateBlock(1, 2, EBLOCKCOLORTYPE.RED),
            CreateBlock(2, 0, EBLOCKCOLORTYPE.RED),
            intersection2,
            CreateBlock(2, 2, EBLOCKCOLORTYPE.RED)
        };

        // Act
        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert: 교차점이 2개이므로 CROSS가 아님, 고유 블록 8개 → FIVE
        Assert.AreEqual(EMATCHTYPE.FIVE, result,
            "Pattern with 2 intersection points should not be CROSS, classify as FIVE (8 unique blocks)");
    }

    [Test]
    public void ShouldClassifyAsNonCrossWhenIntersectionNearEdge()
    {
        // Arrange: 교차점이 끝에서 1칸 떨어진 위치
        // 5H + 3V 패턴에서 교차점이 중간에 있지만 한쪽이 너무 짧음
        // xlist: (0,1) (1,1) (2,1) (3,1) (4,1) - 5개
        // ylist: (1,0) (1,1) (1,2) - 3개
        // 교차점: (1,1) - 1개
        // xlist 인덱스: 1 (중간, 0 < 1 < 4)
        // ylist 인덱스: 1 (중간, 0 < 1 < 2)
        // 현재 로직: 5H + 3V는 불균형 → CROSS가 아님
        // 기대값: 고유 블록 7개 → FIVE
        var intersectionblock = CreateBlock(1, 1, EBLOCKCOLORTYPE.RED);

        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 1, EBLOCKCOLORTYPE.RED),
            intersectionblock,
            CreateBlock(2, 1, EBLOCKCOLORTYPE.RED),
            CreateBlock(3, 1, EBLOCKCOLORTYPE.RED),
            CreateBlock(4, 1, EBLOCKCOLORTYPE.RED)
        };

        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(1, 0, EBLOCKCOLORTYPE.RED),
            intersectionblock,
            CreateBlock(1, 2, EBLOCKCOLORTYPE.RED)
        };

        // Act
        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert: 5H + 3V는 불균형한 패턴이므로 CROSS가 아닌 FIVE로 분류
        Assert.AreEqual(EMATCHTYPE.FIVE, result,
            "5H + 3V with unbalanced arms should be FIVE, not CROSS (7 unique blocks)");
    }

    [Test]
    public void ShouldClassifyAsIndependentMatchesWhenIntersectionAtEdge()
    {
        // Arrange: 교차점이 한쪽 끝에 위치 (L자형 패턴)
        // xlist: (0,1) (1,1) (2,1) - 3개 (가로)
        // ylist: (0,0) (0,1) (0,2) - 3개 (세로)
        // 교차점: (0,1) - 1개
        // xlist 인덱스: 0 (끝, NOT 중간)
        // ylist 인덱스: 1 (중간)
        // 한쪽이 끝에 있으므로 CROSS가 아님 → FIVE
        var intersectionblock = CreateBlock(0, 1, EBLOCKCOLORTYPE.RED);

        var xlist = new List<UI_Match_Block>
        {
            intersectionblock,
            CreateBlock(1, 1, EBLOCKCOLORTYPE.RED),
            CreateBlock(2, 1, EBLOCKCOLORTYPE.RED)
        };

        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(0, 0, EBLOCKCOLORTYPE.RED),
            intersectionblock,
            CreateBlock(0, 2, EBLOCKCOLORTYPE.RED)
        };

        // Act
        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert: L자형 패턴은 CROSS가 아닌 FIVE로 분류 (고유 블록 5개)
        Assert.AreEqual(EMATCHTYPE.FIVE, result,
            "L-shape pattern with intersection at edge should be FIVE, not CROSS (5 unique blocks)");
    }
}