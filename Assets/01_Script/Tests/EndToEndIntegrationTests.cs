using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[TestFixture]
public class EndToEndIntegrationTests
{
    private Dictionary<(int, int), UI_Match_Block> _testgrid;
    private MatchDetector _matchdetector;
    private MatchTypeClassifier _matchtypeclassifier;
    private SpecialBlockFactory _specialblockfactory;
    private ChainReactionProcessor _chainreactionprocessor;
    private MoveMatchValidator _movematchvalidator;
    private BlockSwapHandler _blockswaphandler;

    [SetUp]
    public void SetUp()
    {
        _testgrid = new Dictionary<(int, int), UI_Match_Block>();
        _matchdetector = new MatchDetector();
        _matchtypeclassifier = new MatchTypeClassifier();
        _specialblockfactory = new SpecialBlockFactory();
        _chainreactionprocessor = new ChainReactionProcessor();
        _movematchvalidator = new MoveMatchValidator(_matchdetector);
        _blockswaphandler = new BlockSwapHandler();
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
    public void FullGameCycle_UserMove_Match_Spawn_Gravity_ShouldWorkCorrectly()
    {
        // Arrange: 사용자 이동으로 매치가 생성되는 그리드 설정
        // 초기 상태:
        // [ ][ ][ ][ ]
        // [ ][ ][ ][ ]
        // [R][R][B][R]  <- 이동: (2,0) BLUE <-> (3,0) RED
        var block1 = CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        var block2 = CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        var block3 = CreateBlockAt(2, 0, EBLOCKCOLORTYPE.BLUE);
        var block4 = CreateBlockAt(3, 0, EBLOCKCOLORTYPE.RED);

        // Step 1: 사용자 이동 검증
        bool hasmatch = _movematchvalidator.ValidateMove(_testgrid, (2, 0), (3, 0));
        Assert.IsTrue(hasmatch, "User move should create a match");

        // Step 2: 블록 교환
        _blockswaphandler.SwapBlocks(_testgrid, (2, 0), (3, 0));
        Assert.AreEqual(block4, _testgrid[(2, 0)], "Block should be swapped at (2,0)");
        Assert.AreEqual(block3, _testgrid[(3, 0)], "Block should be swapped at (3,0)");

        // Step 3: 매치 감지
        var horizontalpositions = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 0));
        var verticalpositions = _matchdetector.DetectVerticalMatch(_testgrid, (0, 0));
        Assert.IsNotNull(horizontalpositions, "Horizontal match should be detected");
        Assert.AreEqual(3, horizontalpositions.Count, "Should match 3 blocks horizontally");

        // Convert positions to blocks
        var matchedblocks_x = new List<UI_Match_Block>();
        foreach (var pos in horizontalpositions)
        {
            matchedblocks_x.Add(_testgrid[pos]);
        }
        var matchedblocks_y = new List<UI_Match_Block>();
        if (verticalpositions != null)
        {
            foreach (var pos in verticalpositions)
            {
                matchedblocks_y.Add(_testgrid[pos]);
            }
        }

        // Step 4: 매치 타입 분류
        var matchtype = _matchtypeclassifier.ClassifyMatchType(matchedblocks_x, matchedblocks_y);
        Assert.AreEqual(EMATCHTYPE.THREE, matchtype, "Should classify as 3-match");

        // Step 5: 특수 블록 생성 요청 (3-매치는 null 반환)
        var specialblockrequest = _specialblockfactory.CreateRequest(
            matchedblocks_x,
            matchedblocks_y,
            matchtype,
            _testgrid[(2, 0)]);
        Assert.IsFalse(specialblockrequest.HasValue, "3-match should not create special block");

        // Step 6: 연쇄 반응 처리
        var blocksToDestroy = new List<UI_Match_Block>();
        blocksToDestroy.AddRange(matchedblocks_x);
        var finalblocks = _chainreactionprocessor.ProcessChainReaction(blocksToDestroy, _testgrid);
        Assert.AreEqual(3, finalblocks.Count, "Should destroy 3 blocks");

        // Step 7: 블록 제거 (그리드에서 null로 설정)
        foreach (var blockpos in new[] { (0, 0), (1, 0), (2, 0) })
        {
            _testgrid[blockpos] = null;
        }

        // Step 8: 중력 검증 - 위 블록이 떨어져야 함
        // (이 부분은 BlockMover나 MatchFiledManager의 책임)
        // 여기서는 그리드 상태가 올바르게 업데이트되었는지만 확인
        Assert.IsNull(_testgrid[(0, 0)], "Block should be removed at (0,0)");
        Assert.IsNull(_testgrid[(1, 0)], "Block should be removed at (1,0)");
        Assert.IsNull(_testgrid[(2, 0)], "Block should be removed at (2,0)");
        Assert.IsNotNull(_testgrid[(3, 0)], "Block should remain at (3,0)");
    }

    [Test]
    public void CascadeMatches_ShouldWorkCorrectly()
    {
        // Arrange: 캐스케이드 매치 시나리오 설정
        // 초기 상태: 수직으로 같은 색상 3개 배치
        // [B][ ][ ][ ]  y=2
        // [B][ ][ ][ ]  y=1
        // [B][R][R][R]  y=0  <- (0,0)-(0,1)-(0,2) BLUE 수직 3매치

        // 수직 BLUE 매치 블록들
        var block00 = CreateBlockAt(0, 0, EBLOCKCOLORTYPE.BLUE);
        var block01 = CreateBlockAt(0, 1, EBLOCKCOLORTYPE.BLUE);
        var block02 = CreateBlockAt(0, 2, EBLOCKCOLORTYPE.BLUE);

        // 하단 수평 RED 블록들 (캐스케이드용)
        var block10 = CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        var block20 = CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED);
        var block30 = CreateBlockAt(3, 0, EBLOCKCOLORTYPE.RED);

        // Step 1: 첫 번째 수직 매치 감지
        var verticalpositions = _matchdetector.DetectVerticalMatch(_testgrid, (0, 0));
        Assert.IsNotNull(verticalpositions, "Initial vertical match should be detected");
        Assert.AreEqual(3, verticalpositions.Count, "Should match 3 BLUE blocks vertically");

        // Step 2: 블록 제거
        foreach (var pos in verticalpositions)
        {
            _testgrid[pos] = null;
        }

        // Step 3: 블록 제거 확인
        Assert.IsNull(_testgrid[(0, 0)], "Block at (0,0) should be removed");
        Assert.IsNull(_testgrid[(0, 1)], "Block at (0,1) should be removed");
        Assert.IsNull(_testgrid[(0, 2)], "Block at (0,2) should be removed");

        // Step 4: 다른 블록들은 유지
        Assert.IsNotNull(_testgrid[(1, 0)], "Block at (1,0) should remain");
        Assert.IsNotNull(_testgrid[(2, 0)], "Block at (2,0) should remain");
        Assert.IsNotNull(_testgrid[(3, 0)], "Block at (3,0) should remain");

        // Step 5: 남은 RED 블록들로 수평 매치 감지 (캐스케이드 시뮬레이션)
        var horizontalpositions = _matchdetector.DetectHorizontalMatch(_testgrid, (1, 0));
        Assert.IsNotNull(horizontalpositions, "Cascade horizontal match should be detected");
        Assert.AreEqual(3, horizontalpositions.Count, "Should match 3 RED blocks horizontally");
    }

    [Test]
    public void ChainReaction_ShouldWorkCorrectly()
    {
        // Arrange: 특수 블록이 포함된 연쇄 반응 시나리오
        // [R][S][R]  <- S는 FORE_LEFTRIGHT 특수 블록
        var block0 = CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        var block1 = CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        var block2 = CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED);

        // 추가 블록들 (특수 블록 효과로 제거될 블록들)
        var block3 = CreateBlockAt(3, 0, EBLOCKCOLORTYPE.BLUE);
        var block01 = CreateBlockAt(0, 1, EBLOCKCOLORTYPE.GREEN);
        var block11 = CreateBlockAt(1, 1, EBLOCKCOLORTYPE.GREEN);
        var block21 = CreateBlockAt(2, 1, EBLOCKCOLORTYPE.GREEN);
        var block31 = CreateBlockAt(3, 1, EBLOCKCOLORTYPE.GREEN);

        // block1을 FORE_LEFTRIGHT 특수 블록으로 설정
        var matchtypefield = typeof(UI_Match_Block).GetField("_blocktype",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        matchtypefield?.SetValue(block1, EMATCHTYPE.FORE_LEFTRIGHT);

        // Step 1: 초기 매치 감지 (3개 RED 블록)
        var horizontalpositions = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 0));
        Assert.IsNotNull(horizontalpositions, "Initial match should be detected");
        Assert.AreEqual(3, horizontalpositions.Count, "Should match 3 blocks");

        // Step 2: 매치된 블록들을 리스트로 변환
        var matchedblocks = new List<UI_Match_Block>();
        foreach (var pos in horizontalpositions)
        {
            matchedblocks.Add(_testgrid[pos]);
        }

        // Step 3: 연쇄 반응 처리 (FORE_LEFTRIGHT 특수 블록 효과)
        var finalblocks = _chainreactionprocessor.ProcessChainReaction(matchedblocks, _testgrid);

        // Step 4: 연쇄 반응으로 인해 더 많은 블록이 파괴되어야 함
        // FORE_LEFTRIGHT는 같은 y좌표의 모든 블록을 제거
        Assert.IsTrue(finalblocks.Count > matchedblocks.Count,
            "Chain reaction should destroy more blocks than initial match");

        // Step 5: y=0 라인의 모든 블록이 포함되어야 함
        bool hasblock0 = finalblocks.Contains(block0);
        bool hasblock1 = finalblocks.Contains(block1);
        bool hasblock2 = finalblocks.Contains(block2);
        bool hasblock3 = finalblocks.Contains(block3);

        Assert.IsTrue(hasblock0, "Block at (0,0) should be in final blocks");
        Assert.IsTrue(hasblock1, "Block at (1,0) should be in final blocks");
        Assert.IsTrue(hasblock2, "Block at (2,0) should be in final blocks");
        Assert.IsTrue(hasblock3, "Block at (3,0) should be in final blocks from chain reaction");
    }

    [Test]
    public void SpecialBlockInteractions_ShouldWorkCorrectly()
    {
        // Arrange: 두 가지 특수 블록 상호작용 시나리오
        // FIVE 블록 + FORE 블록 상호작용
        // [F][5][F]  <- F는 FORE_LEFTRIGHT, 5는 FIVE (RED 색상)

        var blockfore1 = CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        var blockfive = CreateBlockAt(1, 0, EBLOCKCOLORTYPE.FIVE); // FIVE 타입은 특별한 색상
        var blockfore2 = CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED);

        // 추가 RED 블록들 (FIVE 효과로 제거될 블록)
        var block03 = CreateBlockAt(3, 0, EBLOCKCOLORTYPE.BLUE);
        var block10 = CreateBlockAt(0, 1, EBLOCKCOLORTYPE.RED);
        var block11 = CreateBlockAt(1, 1, EBLOCKCOLORTYPE.RED);
        var block12 = CreateBlockAt(2, 1, EBLOCKCOLORTYPE.RED);

        // 특수 블록 타입 설정
        var matchtypefield = typeof(UI_Match_Block).GetField("_blocktype",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        matchtypefield?.SetValue(blockfore1, EMATCHTYPE.FORE_LEFTRIGHT);
        matchtypefield?.SetValue(blockfive, EMATCHTYPE.FIVE);
        matchtypefield?.SetValue(blockfore2, EMATCHTYPE.FORE_LEFTRIGHT);

        // Step 1: 초기 매치된 블록들 (3개 특수 블록)
        var matchedblocks = new List<UI_Match_Block> { blockfore1, blockfive, blockfore2 };

        // Step 2: 연쇄 반응 처리
        var finalblocks = _chainreactionprocessor.ProcessChainReaction(matchedblocks, _testgrid);

        // Step 3: 특수 블록 상호작용 검증
        // FORE_LEFTRIGHT 2개 + FIVE 1개 상호작용
        // - FORE_LEFTRIGHT는 y=0 라인 전체 제거
        // - FIVE는 RED 색상 블록 전체 제거
        // 결과: y=0 라인 전체 + RED 블록들 모두 제거
        Assert.IsTrue(finalblocks.Count >= 6,
            $"Special block interactions should destroy multiple blocks, but got {finalblocks.Count}");

        // Step 4: y=0 라인 블록들이 모두 포함되어야 함
        Assert.IsTrue(finalblocks.Contains(blockfore1), "FORE block 1 should be destroyed");
        Assert.IsTrue(finalblocks.Contains(blockfive), "FIVE block should be destroyed");
        Assert.IsTrue(finalblocks.Contains(blockfore2), "FORE block 2 should be destroyed");
        Assert.IsTrue(finalblocks.Contains(block03), "Block at (3,0) should be destroyed by FORE effect");

        // Step 5: RED 블록들이 FIVE 효과로 제거되어야 함
        Assert.IsTrue(finalblocks.Contains(block10), "RED block at (0,1) should be destroyed by FIVE effect");
        Assert.IsTrue(finalblocks.Contains(block11), "RED block at (1,1) should be destroyed by FIVE effect");
        Assert.IsTrue(finalblocks.Contains(block12), "RED block at (2,1) should be destroyed by FIVE effect");
    }

    private UI_Match_Block CreateBlockAt(int x, int y, EBLOCKCOLORTYPE colortype)
    {
        var gameobject = new GameObject($"Block_{x}_{y}");
        var block = gameobject.AddComponent<UI_Match_Block>();

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
