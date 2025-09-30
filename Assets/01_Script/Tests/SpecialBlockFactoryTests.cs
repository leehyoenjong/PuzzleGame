using NUnit.Framework;
using System.Collections.Generic;
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

        Assert.IsFalse(result.HasValue);
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

        Assert.IsTrue(result.HasValue);
        Assert.AreEqual(EMATCHTYPE.FORE_LEFTRIGHT, result.Value.Type);
        Assert.AreEqual((1, 0), result.Value.Point);
        Assert.AreEqual(EBLOCKCOLORTYPE.RED, result.Value.Color);
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

        Assert.IsTrue(result.HasValue);
        Assert.AreEqual(EMATCHTYPE.FORE_UPDOWN, result.Value.Type);
        Assert.AreEqual((0, 2), result.Value.Point);
        Assert.AreEqual(EBLOCKCOLORTYPE.BLUE, result.Value.Color);
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

        Assert.IsTrue(result.HasValue);
        Assert.AreEqual(EMATCHTYPE.FIVE, result.Value.Type);
        Assert.AreEqual((2, 0), result.Value.Point);
        Assert.AreEqual(EBLOCKCOLORTYPE.FIVE, result.Value.Color);
    }

    [Test]
    public void ShouldCreateCrossThreeAtUserMovePosition()
    {
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 1, EBLOCKCOLORTYPE.GREEN),
            CreateBlock(1, 1, EBLOCKCOLORTYPE.GREEN),
            CreateBlock(2, 1, EBLOCKCOLORTYPE.GREEN)
        };
        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(1, 0, EBLOCKCOLORTYPE.GREEN),
            CreateBlock(1, 1, EBLOCKCOLORTYPE.GREEN),
            CreateBlock(1, 2, EBLOCKCOLORTYPE.GREEN)
        };
        var usermoveblock = xlist[1];
        var matchtype = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        var result = _specialblockfactory.CreateRequest(xlist, ylist, matchtype, usermoveblock);

        Assert.IsTrue(result.HasValue);
        Assert.AreEqual(EMATCHTYPE.CROSS_THREE, result.Value.Type);
        Assert.AreEqual((1, 1), result.Value.Point);
        Assert.AreEqual(EBLOCKCOLORTYPE.GREEN, result.Value.Color);
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

        Assert.IsTrue(result.HasValue);
        Assert.AreEqual((1, 0), result.Value.Point);
    }

    [Test]
    public void ShouldCalculateIntersectionPointForCrossWhenNoUserMove()
    {
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 1, EBLOCKCOLORTYPE.PINK),
            CreateBlock(1, 1, EBLOCKCOLORTYPE.PINK),
            CreateBlock(2, 1, EBLOCKCOLORTYPE.PINK)
        };
        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(1, 0, EBLOCKCOLORTYPE.PINK),
            CreateBlock(1, 1, EBLOCKCOLORTYPE.PINK),
            CreateBlock(1, 2, EBLOCKCOLORTYPE.PINK)
        };
        var matchtype = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        var result = _specialblockfactory.CreateRequest(xlist, ylist, matchtype, null);

        Assert.IsTrue(result.HasValue);
        Assert.AreEqual((1, 1), result.Value.Point);
    }

    private UI_Match_Block CreateBlock(int x, int y, EBLOCKCOLORTYPE color)
    {
        var gameobject = new GameObject($"Block_{x}_{y}");
        var block = gameobject.AddComponent<UI_Match_Block>();

        var colorfield = typeof(UI_Match_Block).GetField("_colortypes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        colorfield?.SetValue(block, color);

        var pointfield = typeof(UI_Match_Block).GetField("_point",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        pointfield?.SetValue(block, (x, y));

        _testgrid.Add((x, y), block);
        return block;
    }
}