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

    private UI_Match_Block CreateBlock(int x, int y, EBLOCKCOLORTYPE color)
    {
        var gameobject = new GameObject($"Block_{x}_{y}");
        var block = gameobject.AddComponent<UI_Match_Block>();

        var colorfield = typeof(UI_Match_Block).GetField("_colortypes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        colorfield?.SetValue(block, color);

        _testgrid.Add((x, y), block);
        return block;
    }
}