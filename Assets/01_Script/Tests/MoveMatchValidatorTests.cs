using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[TestFixture]
public class MoveMatchValidatorTests
{
    private MoveMatchValidator _movematchvalidator;
    private MatchDetector _matchdetector;
    private Dictionary<(int, int), UI_Match_Block> _testgrid;

    [SetUp]
    public void SetUp()
    {
        _matchdetector = new MatchDetector();
        _movematchvalidator = new MoveMatchValidator(_matchdetector);
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
    public void ShouldValidateMoveCreatesMatch()
    {
        // Arrange: Create a grid where swapping (0,0) and (1,0) creates a match
        // (0,0)=RED, (1,0)=BLUE, (2,0)=RED, (3,0)=RED
        // After swap: (0,0)=BLUE, (1,0)=RED, (2,0)=RED, (3,0)=RED -> 3-match at (1,0)
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        CreateBlockAt(3, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);

        // Act: Validate move between (0,0) and (1,0)
        var result = _movematchvalidator.ValidateMove(_testgrid, (0, 0), (1, 0));

        // Assert: Should return true (move creates match)
        Assert.IsTrue(result);
    }

    [Test]
    public void ShouldHandleFiveBlockSpecialCase()
    {
        // Arrange: FIVE 블록은 어떤 블록과도 교환 가능 (항상 매치 생성)
        // (0,0)=FIVE, (1,0)=RED (일반 블록)
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.FIVE);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);

        // Act: FIVE 블록과 일반 블록 교환
        var result = _movematchvalidator.ValidateMove(_testgrid, (0, 0), (1, 0));

        // Assert: FIVE 블록은 항상 매치를 생성해야 함
        Assert.IsTrue(result);
    }

    [Test]
    public void ShouldReturnMatchResultsForBothBlocks()
    {
        // Arrange: 두 블록 모두 매치를 생성하는 경우
        // (0,0)=RED, (1,0)=BLUE, (2,0)=RED, (3,0)=RED
        // (0,1)=BLUE, (1,1)=RED, (2,1)=BLUE
        // (0,2)=BLUE
        // 교환 후: (0,0)에서 세로 매치, (1,0)에서 가로 매치
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        CreateBlockAt(3, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        CreateBlockAt(0, 1, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
        CreateBlockAt(1, 1, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        CreateBlockAt(2, 1, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
        CreateBlockAt(0, 2, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);

        // Act: (0,0) RED와 (1,0) BLUE 교환
        // 결과: (0,0)=BLUE는 세로 매치, (1,0)=RED는 가로 매치
        var result = _movematchvalidator.ValidateMove(_testgrid, (0, 0), (1, 0));

        // Assert: 매치가 생성되어야 함
        Assert.IsTrue(result);
    }

    private void CreateBlockAt(int x, int y, EBLOCKCOLORTYPE colortype, EMATCHTYPE matchtype)
    {
        var gameobject = new GameObject($"Block_{x}_{y}");
        var block = gameobject.AddComponent<UI_Match_Block>();

        // Use reflection to set private fields for testing
        var colorfield = typeof(UI_Match_Block).GetField("_colortypes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        colorfield?.SetValue(block, colortype);

        var matchtypefield = typeof(UI_Match_Block).GetField("_blocktype",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        matchtypefield?.SetValue(block, matchtype);

        _testgrid[(x, y)] = block;
    }
}
