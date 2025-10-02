using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[TestFixture]
public class MatchManagerIntegrationTests
{
    private Dictionary<(int, int), UI_Match_Block> _testgrid;

    [SetUp]
    public void SetUp()
    {
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
    public void UserMoveBlockMatch_ShouldUseMoveMatchValidator()
    {
        // Arrange: 매치가 이미 존재하는 그리드 설정
        var block1 = CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        var block2 = CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        var block3 = CreateBlockAt(2, 0, EBLOCKCOLORTYPE.BLUE);
        var block4 = CreateBlockAt(3, 0, EBLOCKCOLORTYPE.RED);

        var matchdetector = new MatchDetector();
        var validator = new MoveMatchValidator(matchdetector);

        // Act: (2,0) BLUE와 (3,0) RED를 교환하면 (0,0)-(1,0)-(3,0) RED 3매치 생성
        var hasmatch = validator.ValidateMove(_testgrid, (2, 0), (3, 0));

        // Assert: 매치가 감지되어야 함
        Assert.IsTrue(hasmatch);
    }

    [Test]
    public void UserMoveBlockMatch_ShouldUseSpecialBlockFactory()
    {
        // Arrange
        var block1 = CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        var block2 = CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        var block3 = CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED);
        var block4 = CreateBlockAt(3, 0, EBLOCKCOLORTYPE.RED);

        var matchblocks_x = new List<UI_Match_Block> { block1, block2, block3, block4 };
        var matchblocks_y = new List<UI_Match_Block>();

        var classifier = new MatchTypeClassifier();
        var factory = new SpecialBlockFactory();

        // Act
        var matchtype = classifier.ClassifyMatchType(matchblocks_x, matchblocks_y);
        var request = factory.CreateRequest(matchblocks_x, matchblocks_y, matchtype, block2);

        // Assert: 4-매치는 특수 블록 생성 요청을 반환해야 함
        Assert.IsTrue(request.HasValue);
        Assert.AreEqual(EMATCHTYPE.FORE_LEFTRIGHT, request.Value.Type);
    }

    [Test]
    public void UserMoveBlockMatch_ShouldUseBlockSwapHandler()
    {
        // Arrange: 두 블록 생성
        var block1 = CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        var block2 = CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE);

        var swaphandler = new BlockSwapHandler();

        // Act: 블록 교환
        swaphandler.SwapBlocks(_testgrid, (0, 0), (1, 0));

        // Assert: 교환되어야 함
        Assert.AreEqual(block2, _testgrid[(0, 0)]);
        Assert.AreEqual(block1, _testgrid[(1, 0)]);
    }

    [Test]
    public void UserMoveBlockMatch_ShouldUseChainReactionProcessor()
    {
        // Arrange: 특수 블록 포함 그리드
        var block1 = CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        var block2 = CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        var block3 = CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED);

        // block2를 FORE_LEFTRIGHT 특수 블록으로 설정
        var matchtypefield = typeof(UI_Match_Block).GetField("_blocktype",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        matchtypefield?.SetValue(block2, EMATCHTYPE.FORE_LEFTRIGHT);

        var blocksToDestroy = new List<UI_Match_Block> { block1, block2, block3 };
        var processor = new ChainReactionProcessor();

        // Act: 연쇄 반응 처리
        var finalBlocks = processor.ProcessChainReaction(blocksToDestroy, _testgrid);

        // Assert: 특수 블록 효과로 인해 더 많은 블록이 파괴되어야 함
        Assert.IsTrue(finalBlocks.Count >= blocksToDestroy.Count);
    }

    private UI_Match_Block CreateBlockAt(int x, int y, EBLOCKCOLORTYPE colortype)
    {
        var gameobject = new GameObject($"Block_{x}_{y}");
        var block = gameobject.AddComponent<UI_Match_Block>();

        // Use reflection to set private fields for testing
        var colorfield = typeof(UI_Match_Block).GetField("_colortypes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        colorfield?.SetValue(block, colortype);

        var matchtypefield = typeof(UI_Match_Block).GetField("_blocktype",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        matchtypefield?.SetValue(block, EMATCHTYPE.THREE);

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
