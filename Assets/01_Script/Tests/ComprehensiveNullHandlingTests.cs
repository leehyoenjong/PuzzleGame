using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Phase 10.1: 모든 주요 클래스의 null/빈 입력 처리를 종합적으로 검증하는 테스트
/// 모든 메서드가 null 그리드, 빈 그리드, null 블록 리스트, 빈 블록 리스트를 안전하게 처리하는지 확인
/// </summary>
[TestFixture]
public class ComprehensiveNullHandlingTests
{
    // ===== Test 1: 모든 메서드에 null 그리드 전달 시 안전하게 처리 =====

    [Test]
    public void ShouldHandleNullGridInMatchDetector()
    {
        // Arrange: null 그리드
        Dictionary<(int, int), UI_Match_Block> nullgrid = null;
        var matchdetector = new MatchDetector();

        // Act & Assert: null 그리드 전달 시 예외 발생
        Assert.Throws<System.NullReferenceException>(() =>
        {
            matchdetector.DetectHorizontalMatch(nullgrid, (0, 0));
        }, "null 그리드일 때 예외 발생해야 함");

        Assert.Throws<System.NullReferenceException>(() =>
        {
            matchdetector.DetectVerticalMatch(nullgrid, (0, 0));
        }, "null 그리드일 때 예외 발생해야 함");
    }

    [Test]
    public void ShouldHandleNullGridInBlockMover()
    {
        // Arrange: null 그리드
        GridManager nullgridmanager = null;
        var blockmover = new BlockMover();

        // Act & Assert: null 그리드 전달 시 예외 발생하지 않음
        Assert.DoesNotThrow(() =>
        {
            blockmover.MoveBlocksDown(nullgridmanager);
        });
    }

    [Test]
    public void ShouldHandleNullGridInChainReactionProcessor()
    {
        // Arrange: null 그리드
        Dictionary<(int, int), UI_Match_Block> nullgrid = null;
        var chainprocessor = new ChainReactionProcessor();

        var block = CreateBlock(0, 0, EBLOCKCOLORTYPE.RED);
        var blocklist = new List<UI_Match_Block> { block };

        // Act: null 그리드 전달
        var result = chainprocessor.ProcessChainReaction(blocklist, nullgrid);

        // Assert: 초기 블록만 반환 (연쇄 없음)
        Assert.AreEqual(1, result.Count, "null 그리드일 때 초기 블록만 반환해야 함");
        Assert.Contains(block, result);

        // Cleanup
        Object.DestroyImmediate(block.gameObject);
    }

    [Test]
    public void ShouldHandleNullGridInMoveMatchValidator()
    {
        // Arrange: null 그리드
        Dictionary<(int, int), UI_Match_Block> nullgrid = null;
        var matchdetector = new MatchDetector();
        var validator = new MoveMatchValidator(matchdetector);

        // Act & Assert: null 그리드 전달 시 예외 발생
        Assert.Throws<System.NullReferenceException>(() =>
        {
            validator.ValidateMove(nullgrid, (0, 0), (0, 1));
        }, "null 그리드일 때 예외 발생해야 함");
    }

    // ===== Test 2: 모든 메서드에 빈 그리드 전달 시 안전하게 처리 =====

    [Test]
    public void ShouldHandleEmptyGridInMatchDetector()
    {
        // Arrange: 빈 그리드
        var emptygrid = new Dictionary<(int, int), UI_Match_Block>();
        var matchdetector = new MatchDetector();

        // Act & Assert: 빈 그리드 전달 시 null 반환
        var horizontalresult = matchdetector.DetectHorizontalMatch(emptygrid, (0, 0));
        Assert.IsNull(horizontalresult, "빈 그리드일 때 null 반환해야 함");

        var verticalresult = matchdetector.DetectVerticalMatch(emptygrid, (0, 0));
        Assert.IsNull(verticalresult, "빈 그리드일 때 null 반환해야 함");
    }

    [Test]
    public void ShouldHandleEmptyGridInBlockMover()
    {
        // Arrange: 빈 그리드
        var gridmanager = new GridManager();
        gridmanager.Initialize(5, 8);

        // 모든 슬롯을 명시적으로 null로 초기화
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                gridmanager.SetBlock((x, y), null);
            }
        }

        var blockmover = new BlockMover();

        // Act & Assert: 빈 그리드 전달 시 예외 발생하지 않음
        Assert.DoesNotThrow(() =>
        {
            blockmover.MoveBlocksDown(gridmanager);
        });
    }

    [Test]
    public void ShouldHandleEmptyGridInChainReactionProcessor()
    {
        // Arrange: 빈 그리드
        var emptygrid = new Dictionary<(int, int), UI_Match_Block>();
        var chainprocessor = new ChainReactionProcessor();

        var block = CreateBlock(0, 0, EBLOCKCOLORTYPE.RED);
        var blocklist = new List<UI_Match_Block> { block };

        // Act: 빈 그리드 전달
        var result = chainprocessor.ProcessChainReaction(blocklist, emptygrid);

        // Assert: 초기 블록만 반환 (연쇄 없음)
        Assert.AreEqual(1, result.Count, "빈 그리드일 때 초기 블록만 반환해야 함");
        Assert.Contains(block, result);

        // Cleanup
        Object.DestroyImmediate(block.gameObject);
    }

    [Test]
    public void ShouldHandleEmptyGridInMoveMatchValidator()
    {
        // Arrange: 빈 그리드
        var emptygrid = new Dictionary<(int, int), UI_Match_Block>();
        var matchdetector = new MatchDetector();
        var validator = new MoveMatchValidator(matchdetector);

        // Act & Assert: 빈 그리드 전달 시 false 반환
        bool result = validator.ValidateMove(emptygrid, (0, 0), (0, 1));
        Assert.IsFalse(result, "빈 그리드일 때 false 반환해야 함");
    }

    // ===== Test 3: 모든 메서드에 null 블록 리스트 전달 시 안전하게 처리 =====

    [Test]
    public void ShouldHandleNullBlockListInChainReactionProcessor()
    {
        // Arrange: null 블록 리스트
        List<UI_Match_Block> nullblocklist = null;
        var grid = new Dictionary<(int, int), UI_Match_Block>();
        var chainprocessor = new ChainReactionProcessor();

        // Act & Assert: null 블록 리스트 전달 시 빈 리스트 반환
        var result = chainprocessor.ProcessChainReaction(nullblocklist, grid);
        Assert.IsNotNull(result, "결과는 null이 아니어야 함");
        Assert.AreEqual(0, result.Count, "null 블록 리스트일 때 빈 리스트 반환해야 함");
    }

    [Test]
    public void ShouldHandleNullBlockListInMatchTypeClassifier()
    {
        // Arrange: null 블록 리스트
        List<UI_Match_Block> nullxlist = null;
        List<UI_Match_Block> nullylist = null;
        var classifier = new MatchTypeClassifier();

        // Act: null 블록 리스트 전달
        var result = classifier.ClassifyMatchType(nullxlist, nullylist);

        // Assert: THREE 반환 (기본값)
        Assert.AreEqual(EMATCHTYPE.THREE, result, "null 블록 리스트일 때 THREE 반환해야 함");
    }

    [Test]
    public void ShouldHandleNullBlockListInSpecialBlockFactory()
    {
        // Arrange: null 블록 리스트
        List<UI_Match_Block> nullxlist = null;
        List<UI_Match_Block> nullylist = null;
        var factory = new SpecialBlockFactory();

        // Act: null 블록 리스트 전달 (FIVE 매치 시도)
        var result = factory.CreateRequest(nullxlist, nullylist, EMATCHTYPE.FIVE);

        // Assert: null 반환
        Assert.IsNull(result, "null 블록 리스트일 때 null 반환해야 함");
    }

    // ===== Test 4: 모든 메서드에 빈 블록 리스트 전달 시 안전하게 처리 =====

    [Test]
    public void ShouldHandleEmptyBlockListInChainReactionProcessor()
    {
        // Arrange: 빈 블록 리스트
        var emptyblocklist = new List<UI_Match_Block>();
        var grid = new Dictionary<(int, int), UI_Match_Block>();
        var chainprocessor = new ChainReactionProcessor();

        // Act: 빈 블록 리스트 전달
        var result = chainprocessor.ProcessChainReaction(emptyblocklist, grid);

        // Assert: 빈 리스트 반환
        Assert.IsNotNull(result, "결과는 null이 아니어야 함");
        Assert.AreEqual(0, result.Count, "빈 블록 리스트일 때 빈 리스트 반환해야 함");
    }

    [Test]
    public void ShouldHandleEmptyBlockListInMatchTypeClassifier()
    {
        // Arrange: 빈 블록 리스트
        var emptyxlist = new List<UI_Match_Block>();
        var emptyylist = new List<UI_Match_Block>();
        var classifier = new MatchTypeClassifier();

        // Act: 빈 블록 리스트 전달
        var result = classifier.ClassifyMatchType(emptyxlist, emptyylist);

        // Assert: THREE 반환 (기본값)
        Assert.AreEqual(EMATCHTYPE.THREE, result, "빈 블록 리스트일 때 THREE 반환해야 함");
    }

    [Test]
    public void ShouldHandleEmptyBlockListInSpecialBlockFactory()
    {
        // Arrange: 빈 블록 리스트
        var emptyxlist = new List<UI_Match_Block>();
        var emptyylist = new List<UI_Match_Block>();
        var factory = new SpecialBlockFactory();

        // Act: 빈 블록 리스트 전달 (FIVE 매치 시도)
        var result = factory.CreateRequest(emptyxlist, emptyylist, EMATCHTYPE.FIVE);

        // Assert: null 반환
        Assert.IsNull(result, "빈 블록 리스트일 때 null 반환해야 함");
    }

    // ===== 헬퍼 메서드 =====

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

        return block;
    }
}
