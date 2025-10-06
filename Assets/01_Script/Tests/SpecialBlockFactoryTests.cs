using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[TestFixture]
public class SpecialBlockFactoryTests
{
    private SpecialBlockFactory _specialblockfactory;
    private MatchTypeClassifier _matchtypeclassifier;
    private Dictionary<(int, int), UI_Match_Block> _testgrid;

    [SetUp]
    public void SetUp()
    {
        _specialblockfactory = new SpecialBlockFactory();
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
    public void ShouldReturnNullFor3Match()
    {
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 0, EBLOCKCOLORTYPE.RED),
            CreateBlock(1, 0, EBLOCKCOLORTYPE.RED),
            CreateBlock(2, 0, EBLOCKCOLORTYPE.RED)
        };
        var ylist = new List<UI_Match_Block>();
        var matchtype = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        var result = _specialblockfactory.CreateRequest(xlist, ylist, matchtype, null);

        Assert.IsFalse(result.HasValue, $"Expected null for 3-match, but got Type={result?.Type}, Point={result?.Point}, Color={result?.Color}");
    }

    [Test]
    public void ShouldCreateForeLeftRightAtUserMovePosition()
    {
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 0, EBLOCKCOLORTYPE.RED),
            CreateBlock(1, 0, EBLOCKCOLORTYPE.RED),
            CreateBlock(2, 0, EBLOCKCOLORTYPE.RED),
            CreateBlock(3, 0, EBLOCKCOLORTYPE.RED)
        };
        var ylist = new List<UI_Match_Block>();
        var usermoveblock = xlist[1];
        var matchtype = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        var result = _specialblockfactory.CreateRequest(xlist, ylist, matchtype, usermoveblock);

        Assert.IsTrue(result.HasValue, "Expected special block creation for 4-match horizontal");
        Assert.AreEqual(EMATCHTYPE.FORE_LEFTRIGHT, result.Value.Type, $"Expected FORE_LEFTRIGHT but got {result.Value.Type}");
        Assert.AreEqual((1, 0), result.Value.Point, $"Expected point (1,0) but got {result.Value.Point}");
        Assert.AreEqual(EBLOCKCOLORTYPE.RED, result.Value.Color, $"Expected RED but got {result.Value.Color}");
    }

    [Test]
    public void ShouldCreateForeUpDownAtUserMovePosition()
    {
        var xlist = new List<UI_Match_Block>();
        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(0, 0, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(0, 1, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(0, 2, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(0, 3, EBLOCKCOLORTYPE.BLUE)
        };
        var usermoveblock = ylist[2];
        var matchtype = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        var result = _specialblockfactory.CreateRequest(xlist, ylist, matchtype, usermoveblock);

        Assert.IsTrue(result.HasValue, "Expected special block creation for 4-match vertical");
        Assert.AreEqual(EMATCHTYPE.FORE_UPDOWN, result.Value.Type, $"Expected FORE_UPDOWN but got {result.Value.Type}");
        Assert.AreEqual((0, 2), result.Value.Point, $"Expected point (0,2) but got {result.Value.Point}");
        Assert.AreEqual(EBLOCKCOLORTYPE.BLUE, result.Value.Color, $"Expected BLUE but got {result.Value.Color}");
    }

    [Test]
    public void ShouldCreateFiveWithFiveColorType()
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
        var usermoveblock = xlist[2];
        var matchtype = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        var result = _specialblockfactory.CreateRequest(xlist, ylist, matchtype, usermoveblock);

        Assert.IsTrue(result.HasValue, "Expected special block creation for 5-match");
        Assert.AreEqual(EMATCHTYPE.FIVE, result.Value.Type, $"Expected FIVE but got {result.Value.Type}");
        Assert.AreEqual((2, 0), result.Value.Point, $"Expected point (2,0) but got {result.Value.Point}");
        Assert.AreEqual(EBLOCKCOLORTYPE.FIVE, result.Value.Color, $"Expected FIVE color type but got {result.Value.Color}");
    }

    [Test]
    public void ShouldCreateCrossThreeAtUserMovePosition()
    {
        var centerblock = CreateBlock(1, 1, EBLOCKCOLORTYPE.GREEN);
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 1, EBLOCKCOLORTYPE.GREEN),
            centerblock,
            CreateBlock(2, 1, EBLOCKCOLORTYPE.GREEN)
        };
        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(1, 0, EBLOCKCOLORTYPE.GREEN),
            centerblock,
            CreateBlock(1, 2, EBLOCKCOLORTYPE.GREEN)
        };
        var usermoveblock = centerblock;
        var matchtype = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        var result = _specialblockfactory.CreateRequest(xlist, ylist, matchtype, usermoveblock);

        Assert.IsTrue(result.HasValue, "Expected special block creation for 3x3 cross");
        Assert.AreEqual(EMATCHTYPE.CROSS_THREE, result.Value.Type, $"Expected CROSS_THREE but got {result.Value.Type}");
        Assert.AreEqual((1, 1), result.Value.Point, $"Expected point (1,1) but got {result.Value.Point}");
        Assert.AreEqual(EBLOCKCOLORTYPE.GREEN, result.Value.Color, $"Expected GREEN but got {result.Value.Color}");
    }

    [Test]
    public void ShouldCalculateMiddlePointForLineMatchWhenNoUserMove()
    {
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 0, EBLOCKCOLORTYPE.RED),
            CreateBlock(1, 0, EBLOCKCOLORTYPE.RED),
            CreateBlock(2, 0, EBLOCKCOLORTYPE.RED),
            CreateBlock(3, 0, EBLOCKCOLORTYPE.RED)
        };
        var ylist = new List<UI_Match_Block>();
        var matchtype = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        var result = _specialblockfactory.CreateRequest(xlist, ylist, matchtype, null);

        Assert.IsTrue(result.HasValue, "Expected special block creation for 4-match without user move");
        Assert.AreEqual((1, 0), result.Value.Point, $"Expected middle point (1,0) but got {result.Value.Point}");
    }

    [Test]
    public void ShouldCalculateIntersectionPointForCrossWhenNoUserMove()
    {
        var centerblock = CreateBlock(1, 1, EBLOCKCOLORTYPE.PINK);
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 1, EBLOCKCOLORTYPE.PINK),
            centerblock,
            CreateBlock(2, 1, EBLOCKCOLORTYPE.PINK)
        };
        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(1, 0, EBLOCKCOLORTYPE.PINK),
            centerblock,
            CreateBlock(1, 2, EBLOCKCOLORTYPE.PINK)
        };
        var matchtype = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        var result = _specialblockfactory.CreateRequest(xlist, ylist, matchtype, null);

        Assert.IsTrue(result.HasValue, "Expected special block creation for cross match without user move");
        Assert.AreEqual((1, 1), result.Value.Point, $"Expected intersection point (1,1) but got {result.Value.Point}");
    }

    [Test]
    public void ShouldCalculateCorrectPositionForLShape5Match()
    {
        // Arrange: L-shape 5-match pattern (3 horizontal + 3 vertical with 1 overlap)
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
        var matchtype = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Act
        var result = _specialblockfactory.CreateRequest(xlist, ylist, matchtype, null);

        // Assert
        Assert.IsTrue(result.HasValue, "Expected special block creation for L-shape 5-match");
        Assert.AreEqual(EMATCHTYPE.FIVE, result.Value.Type, $"Expected FIVE but got {result.Value.Type}");

        // Expected position should be calculated from ALL 5 unique blocks:
        // X range: 0 to 2, Y range: 0 to 2
        // Middle point: ((0+2)/2, (0+2)/2) = (1, 1)
        Assert.AreEqual((1, 1), result.Value.Point,
            $"Expected middle point (1,1) for L-shape but got {result.Value.Point}. " +
            $"Should calculate from all 5 blocks, not just xlist or ylist alone.");
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
}