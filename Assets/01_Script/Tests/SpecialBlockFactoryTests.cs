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

    // Test 3.1: 중간점 계산 엣지 케이스

    [Test]
    public void ShouldReturnBlockPositionWhenOnlyOneBlock()
    {
        // Arrange: 블록이 1개만 있을 때
        // 4-match 패턴이지만 실제로는 1개의 블록만 전달된 경우
        // (실제로는 발생하지 않지만 엣지 케이스 테스트)
        var block = CreateBlock(3, 5, EBLOCKCOLORTYPE.RED);
        var xlist = new List<UI_Match_Block> { block };
        var ylist = new List<UI_Match_Block>();

        // Act
        var result = _specialblockfactory.CreateRequest(xlist, ylist, EMATCHTYPE.FORE_LEFTRIGHT, null);

        // Assert: 블록이 1개일 때는 그 블록의 위치를 반환해야 함
        // CalculateMiddlePoint: ((3+3)/2, (5+5)/2) = (3, 5)
        Assert.IsTrue(result.HasValue, "Expected special block creation for single block");
        Assert.AreEqual((3, 5), result.Value.Point,
            $"Expected single block position (3,5) but got {result.Value.Point}");
    }

    [Test]
    public void ShouldReturnLeftBottomPositionWhenTwoBlocksHorizontal()
    {
        // Arrange: 블록이 2개 (가로로 인접)
        // (0, 0)과 (1, 0) → 중간점 = ((0+1)/2, (0+0)/2) = (0, 0)
        // 정수 나눗셈이므로 왼쪽 블록 위치가 반환됨
        var block1 = CreateBlock(0, 0, EBLOCKCOLORTYPE.BLUE);
        var block2 = CreateBlock(1, 0, EBLOCKCOLORTYPE.BLUE);
        var xlist = new List<UI_Match_Block> { block1, block2 };
        var ylist = new List<UI_Match_Block>();

        // Act
        var result = _specialblockfactory.CreateRequest(xlist, ylist, EMATCHTYPE.FORE_LEFTRIGHT, null);

        // Assert: 블록이 2개일 때는 왼쪽 블록 위치 반환
        Assert.IsTrue(result.HasValue, "Expected special block creation for two blocks");
        Assert.AreEqual((0, 0), result.Value.Point,
            $"Expected left block position (0,0) but got {result.Value.Point}");
    }

    [Test]
    public void ShouldReturnLeftBottomPositionWhenTwoBlocksVertical()
    {
        // Arrange: 블록이 2개 (세로로 인접)
        // (0, 0)과 (0, 1) → 중간점 = ((0+0)/2, (0+1)/2) = (0, 0)
        // 정수 나눗셈이므로 아래쪽 블록 위치가 반환됨
        var block1 = CreateBlock(0, 0, EBLOCKCOLORTYPE.GREEN);
        var block2 = CreateBlock(0, 1, EBLOCKCOLORTYPE.GREEN);
        var xlist = new List<UI_Match_Block>();
        var ylist = new List<UI_Match_Block> { block1, block2 };

        // Act
        var result = _specialblockfactory.CreateRequest(xlist, ylist, EMATCHTYPE.FORE_UPDOWN, null);

        // Assert: 블록이 2개일 때는 아래쪽 블록 위치 반환
        Assert.IsTrue(result.HasValue, "Expected special block creation for two blocks");
        Assert.AreEqual((0, 0), result.Value.Point,
            $"Expected bottom block position (0,0) but got {result.Value.Point}");
    }

    [Test]
    public void ShouldCalculateMiddlePointForNonConsecutiveBlocks()
    {
        // Arrange: 블록 위치가 연속되지 않을 때
        // (0,0)과 (5,0) 사이에 빈 공간 존재
        // 중간점 = ((0+5)/2, (0+0)/2) = (2, 0)
        var block1 = CreateBlock(0, 0, EBLOCKCOLORTYPE.YELLOW);
        var block2 = CreateBlock(5, 0, EBLOCKCOLORTYPE.YELLOW);
        var xlist = new List<UI_Match_Block> { block1, block2 };
        var ylist = new List<UI_Match_Block>();

        // Act
        var result = _specialblockfactory.CreateRequest(xlist, ylist, EMATCHTYPE.FORE_LEFTRIGHT, null);

        // Assert: 연속되지 않은 블록도 범위의 중간점 계산
        Assert.IsTrue(result.HasValue, "Expected special block creation for non-consecutive blocks");
        Assert.AreEqual((2, 0), result.Value.Point,
            $"Expected middle point (2,0) for blocks at (0,0) and (5,0) but got {result.Value.Point}");
    }

    [Test]
    public void ShouldCalculateMiddlePointForVerticalNonConsecutiveBlocks()
    {
        // Arrange: 세로로 떨어진 블록들
        // (3,1)과 (3,7) 사이에 빈 공간 존재
        // 중간점 = ((3+3)/2, (1+7)/2) = (3, 4)
        var block1 = CreateBlock(3, 1, EBLOCKCOLORTYPE.PINK);
        var block2 = CreateBlock(3, 7, EBLOCKCOLORTYPE.PINK);
        var xlist = new List<UI_Match_Block>();
        var ylist = new List<UI_Match_Block> { block1, block2 };

        // Act
        var result = _specialblockfactory.CreateRequest(xlist, ylist, EMATCHTYPE.FORE_UPDOWN, null);

        // Assert: 세로로 연속되지 않은 블록도 범위의 중간점 계산
        Assert.IsTrue(result.HasValue, "Expected special block creation for vertical non-consecutive blocks");
        Assert.AreEqual((3, 4), result.Value.Point,
            $"Expected middle point (3,4) for blocks at (3,1) and (3,7) but got {result.Value.Point}");
    }

    [Test]
    public void ShouldCalculateMiddlePointForDiagonalBlocks()
    {
        // Arrange: 대각선 블록들 (L자형 5-match의 변형)
        // (0,0), (1,1), (2,2) - 대각선 패턴
        // X 범위: 0~2, Y 범위: 0~2
        // 중간점 = ((0+2)/2, (0+2)/2) = (1, 1)
        var block1 = CreateBlock(0, 0, EBLOCKCOLORTYPE.RED);
        var block2 = CreateBlock(1, 1, EBLOCKCOLORTYPE.RED);
        var block3 = CreateBlock(2, 2, EBLOCKCOLORTYPE.RED);

        // FIVE 타입으로 테스트 (모든 고유 블록 사용)
        var xlist = new List<UI_Match_Block> { block1, block2, block3 };
        var ylist = new List<UI_Match_Block>();

        // Act
        var result = _specialblockfactory.CreateRequest(xlist, ylist, EMATCHTYPE.FIVE, null);

        // Assert: 대각선 블록들의 중간점은 범위 중심
        Assert.IsTrue(result.HasValue, "Expected special block creation for diagonal blocks");
        Assert.AreEqual((1, 1), result.Value.Point,
            $"Expected middle point (1,1) for diagonal blocks at (0,0), (1,1), (2,2) but got {result.Value.Point}");
    }

    [Test]
    public void ShouldReturnNullWhenAllBlocksAreInvalid()
    {
        // Arrange: 모든 블록이 (-1, -1) 위치 (이미 제거된 블록들)
        // CreateBlock으로는 (-1, -1) 블록을 만들 수 없으므로
        // 리플렉션을 사용하여 위치를 변경
        var block1 = CreateBlock(0, 0, EBLOCKCOLORTYPE.RED);
        var block2 = CreateBlock(1, 0, EBLOCKCOLORTYPE.RED);

        // 리플렉션으로 _x, _y를 -1로 변경
        var xfield = typeof(UI_Match_Block).GetField("_x",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var yfield = typeof(UI_Match_Block).GetField("_y",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        xfield?.SetValue(block1, -1);
        yfield?.SetValue(block1, -1);
        xfield?.SetValue(block2, -1);
        yfield?.SetValue(block2, -1);

        var xlist = new List<UI_Match_Block> { block1, block2 };
        var ylist = new List<UI_Match_Block>();

        // Act
        // Debug.LogWarning이 호출될 것으로 예상
        var result = _specialblockfactory.CreateRequest(xlist, ylist, EMATCHTYPE.FORE_LEFTRIGHT, null);

        // Assert: 모든 블록이 (-1, -1)이면 null 반환
        Assert.IsFalse(result.HasValue,
            "Expected null when all blocks are at invalid position (-1, -1)");
    }

    // Test 3.2: 사용자 이동 위치 우선순위

    [Test]
    public void ShouldIgnoreUserMoveBlockNotInMatchList()
    {
        // Arrange: usermoveblock이 매치 리스트에 없는 경우
        // 매치된 블록: (0,0), (1,0), (2,0), (3,0)
        // 사용자 이동 블록: (5,5) - 매치 리스트에 없음
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 0, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(1, 0, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(2, 0, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(3, 0, EBLOCKCOLORTYPE.BLUE)
        };
        var ylist = new List<UI_Match_Block>();

        // 매치 리스트에 없는 블록을 usermoveblock으로 전달
        var usermoveblock = CreateBlock(5, 5, EBLOCKCOLORTYPE.BLUE);

        // Act
        var result = _specialblockfactory.CreateRequest(xlist, ylist, EMATCHTYPE.FORE_LEFTRIGHT, usermoveblock);

        // Assert: 리스트에 없는 usermoveblock은 무시하고 중간점 사용해야 함
        // 기대: 중간점 ((0+3)/2, 0) = (1, 0)
        Assert.IsTrue(result.HasValue, "Expected special block creation");
        Assert.AreEqual((1, 0), result.Value.Point,
            $"Expected middle point (1,0) when usermoveblock not in list, but got {result.Value.Point}");
    }

    [Test]
    public void ShouldUseMiddlePointWhenUserMoveBlockIsNull()
    {
        // Arrange: usermoveblock이 null인 경우 (가장 일반적인 케이스)
        // 매치된 블록: (2,3), (3,3), (4,3), (5,3)
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(2, 3, EBLOCKCOLORTYPE.GREEN),
            CreateBlock(3, 3, EBLOCKCOLORTYPE.GREEN),
            CreateBlock(4, 3, EBLOCKCOLORTYPE.GREEN),
            CreateBlock(5, 3, EBLOCKCOLORTYPE.GREEN)
        };
        var ylist = new List<UI_Match_Block>();

        // Act: usermoveblock = null
        var result = _specialblockfactory.CreateRequest(xlist, ylist, EMATCHTYPE.FORE_LEFTRIGHT, null);

        // Assert: null일 때는 중간점 사용
        // 중간점 = ((2+5)/2, 3) = (3, 3)
        Assert.IsTrue(result.HasValue, "Expected special block creation when usermoveblock is null");
        Assert.AreEqual((3, 3), result.Value.Point,
            $"Expected middle point (3,3) when usermoveblock is null, but got {result.Value.Point}");
    }

    [Test]
    public void ShouldUseMiddlePointWhenUserMoveBlockIsInvalid()
    {
        // Arrange: usermoveblock이 (-1, -1) 위치 (이미 제거된 블록)
        // 매치된 블록: (1,2), (2,2), (3,2)
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(1, 2, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(2, 2, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(3, 2, EBLOCKCOLORTYPE.YELLOW)
        };
        var ylist = new List<UI_Match_Block>();

        // usermoveblock을 만들고 위치를 (-1, -1)로 변경
        var usermoveblock = CreateBlock(2, 2, EBLOCKCOLORTYPE.YELLOW);
        var xfield = typeof(UI_Match_Block).GetField("_x",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var yfield = typeof(UI_Match_Block).GetField("_y",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        xfield?.SetValue(usermoveblock, -1);
        yfield?.SetValue(usermoveblock, -1);

        // Act
        var result = _specialblockfactory.CreateRequest(xlist, ylist, EMATCHTYPE.FORE_LEFTRIGHT, usermoveblock);

        // Assert: (-1, -1) 위치는 무효하므로 중간점 사용
        // 중간점 = ((1+3)/2, 2) = (2, 2)
        Assert.IsTrue(result.HasValue, "Expected special block creation when usermoveblock is at (-1,-1)");
        Assert.AreEqual((2, 2), result.Value.Point,
            $"Expected middle point (2,2) when usermoveblock is at invalid position (-1,-1), but got {result.Value.Point}");
    }

    [Test]
    public void ShouldAlwaysUseUserMoveBlockWhenValidAndInList()
    {
        // Arrange: usermoveblock이 유효하고 매치 리스트에 포함된 경우
        // 매치된 블록: (0,5), (1,5), (2,5), (3,5), (4,5) - 5-match
        // usermoveblock: (1,5) - 리스트의 두 번째 블록
        // 중간점은 (2,5)이지만 usermoveblock 위치인 (1,5)를 우선 사용해야 함
        var block1 = CreateBlock(0, 5, EBLOCKCOLORTYPE.PINK);
        var usermoveblock = CreateBlock(1, 5, EBLOCKCOLORTYPE.PINK);
        var block3 = CreateBlock(2, 5, EBLOCKCOLORTYPE.PINK);
        var block4 = CreateBlock(3, 5, EBLOCKCOLORTYPE.PINK);
        var block5 = CreateBlock(4, 5, EBLOCKCOLORTYPE.PINK);

        var xlist = new List<UI_Match_Block> { block1, usermoveblock, block3, block4, block5 };
        var ylist = new List<UI_Match_Block>();

        // Act
        var result = _specialblockfactory.CreateRequest(xlist, ylist, EMATCHTYPE.FIVE, usermoveblock);

        // Assert: usermoveblock이 유효하고 리스트에 있으면 그 위치를 우선 사용
        // 기대: (1, 5) (usermoveblock 위치)
        // 중간점 (2, 5)가 아님
        Assert.IsTrue(result.HasValue, "Expected special block creation with valid usermoveblock");
        Assert.AreEqual((1, 5), result.Value.Point,
            $"Expected usermoveblock position (1,5) to be used instead of middle point (2,5), but got {result.Value.Point}");
    }

    // Test 3.3: 특수 케이스 위치 계산

    [Test]
    public void ShouldUseFirstIntersectionWhenMultipleIntersections()
    {
        // Arrange: 교차점이 여러 개인 경우 (비정상적인 격자 패턴)
        // xlist: (1,1), (2,1), (3,1) - 가로 3개
        // ylist: (2,0), (2,1), (2,2), (3,0), (3,1), (3,2) - 두 개의 세로 라인
        // 교차점: (2,1), (3,1) - 2개
        // FirstOrDefault()로 첫 번째 교차점 (2,1) 사용
        var intersection1 = CreateBlock(2, 1, EBLOCKCOLORTYPE.RED);
        var intersection2 = CreateBlock(3, 1, EBLOCKCOLORTYPE.RED);

        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(1, 1, EBLOCKCOLORTYPE.RED),
            intersection1,
            intersection2
        };

        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(2, 0, EBLOCKCOLORTYPE.RED),
            intersection1,
            CreateBlock(2, 2, EBLOCKCOLORTYPE.RED),
            CreateBlock(3, 0, EBLOCKCOLORTYPE.RED),
            intersection2,
            CreateBlock(3, 2, EBLOCKCOLORTYPE.RED)
        };

        // Act
        var result = _specialblockfactory.CreateRequest(xlist, ylist, EMATCHTYPE.CROSS_THREE, null);

        // Assert: 교차점이 여러 개일 때는 첫 번째 교차점 사용
        // xlist.Intersect(ylist)의 순서는 xlist 기준이므로 (2,1)이 먼저
        Assert.IsTrue(result.HasValue, "Expected special block creation for cross with multiple intersections");
        Assert.AreEqual((2, 1), result.Value.Point,
            $"Expected first intersection point (2,1) but got {result.Value.Point}");
    }

    [Test]
    public void ShouldFallbackToMiddlePointWhenNoIntersection()
    {
        // Arrange: 교차점이 없는 경우 (평행선)
        // xlist: (0,0), (1,0), (2,0) - 가로 y=0
        // ylist: (0,2), (1,2), (2,2) - 가로 y=2 (평행선, 교차점 없음)
        // CROSS_THREE 타입이지만 교차점이 없으므로 fallback으로 중간점 계산
        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 0, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(1, 0, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(2, 0, EBLOCKCOLORTYPE.BLUE)
        };

        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(0, 2, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(1, 2, EBLOCKCOLORTYPE.BLUE),
            CreateBlock(2, 2, EBLOCKCOLORTYPE.BLUE)
        };

        // Act
        // Debug.LogWarning이 호출될 것으로 예상
        var result = _specialblockfactory.CreateRequest(xlist, ylist, EMATCHTYPE.CROSS_THREE, null);

        // Assert: 교차점이 없으면 fallback으로 전체 블록의 중간점 사용
        // 모든 고유 블록: 6개, X범위(0~2), Y범위(0~2)
        // 중간점 = ((0+2)/2, (0+2)/2) = (1, 1)
        Assert.IsTrue(result.HasValue, "Expected special block creation with fallback to middle point");
        Assert.AreEqual((1, 1), result.Value.Point,
            $"Expected fallback middle point (1,1) when no intersection found, but got {result.Value.Point}");
    }

    [Test]
    public void ShouldUseAllBlocksForMiddlePointInLShape5Match()
    {
        // Arrange: L자형 5-매치
        // xlist: (0,0), (1,0), (2,0) - 가로 3개
        // ylist: (0,0), (0,1), (0,2) - 세로 3개
        // 교차점: (0,0) - 모서리 (L자형)
        // 고유 블록 5개: (0,0), (1,0), (2,0), (0,1), (0,2)
        // X범위: 0~2, Y범위: 0~2
        // 중간점: ((0+2)/2, (0+2)/2) = (1, 1)
        var corner = CreateBlock(0, 0, EBLOCKCOLORTYPE.GREEN);

        var xlist = new List<UI_Match_Block>
        {
            corner,
            CreateBlock(1, 0, EBLOCKCOLORTYPE.GREEN),
            CreateBlock(2, 0, EBLOCKCOLORTYPE.GREEN)
        };

        var ylist = new List<UI_Match_Block>
        {
            corner,
            CreateBlock(0, 1, EBLOCKCOLORTYPE.GREEN),
            CreateBlock(0, 2, EBLOCKCOLORTYPE.GREEN)
        };

        // Act: FIVE 타입 (L자형)
        var result = _specialblockfactory.CreateRequest(xlist, ylist, EMATCHTYPE.FIVE, null);

        // Assert: L자형 5-매치는 5개 블록 모두 사용하여 중간점 계산
        // 고유 블록 5개의 범위를 모두 고려해야 함
        Assert.IsTrue(result.HasValue, "Expected special block creation for L-shape 5-match");
        Assert.AreEqual((1, 1), result.Value.Point,
            $"Expected middle point (1,1) using all 5 unique blocks, but got {result.Value.Point}");
        Assert.AreEqual(EMATCHTYPE.FIVE, result.Value.Type);
        Assert.AreEqual(EBLOCKCOLORTYPE.FIVE, result.Value.Color, "FIVE match should have FIVE color");
    }

    [Test]
    public void ShouldCalculateCorrectMiddlePointWith10PlusBlocks()
    {
        // Arrange: 10개 이상의 블록으로 구성된 FIVE 매치 (대형 패턴)
        // xlist: (0,3), (1,3), (2,3), (3,3), (4,3), (5,3), (6,3) - 가로 7개
        // ylist: (3,0), (3,1), (3,2), (3,3), (3,4), (3,5), (3,6) - 세로 7개
        // 교차점: (3,3) - 1개
        // 고유 블록: 13개 (7 + 7 - 1)
        // X범위: 0~6, Y범위: 0~6
        // 중간점: ((0+6)/2, (0+6)/2) = (3, 3)
        var center = CreateBlock(3, 3, EBLOCKCOLORTYPE.YELLOW);

        var xlist = new List<UI_Match_Block>
        {
            CreateBlock(0, 3, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(1, 3, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(2, 3, EBLOCKCOLORTYPE.YELLOW),
            center,
            CreateBlock(4, 3, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(5, 3, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(6, 3, EBLOCKCOLORTYPE.YELLOW)
        };

        var ylist = new List<UI_Match_Block>
        {
            CreateBlock(3, 0, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(3, 1, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(3, 2, EBLOCKCOLORTYPE.YELLOW),
            center,
            CreateBlock(3, 4, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(3, 5, EBLOCKCOLORTYPE.YELLOW),
            CreateBlock(3, 6, EBLOCKCOLORTYPE.YELLOW)
        };

        // Act: FIVE 타입 (13개 블록)
        var result = _specialblockfactory.CreateRequest(xlist, ylist, EMATCHTYPE.FIVE, null);

        // Assert: 10개 이상의 블록에서도 올바른 중간점 계산
        // 모든 블록의 범위를 고려하여 중간점 계산
        Assert.IsTrue(result.HasValue, "Expected special block creation for large FIVE match (10+ blocks)");
        Assert.AreEqual((3, 3), result.Value.Point,
            $"Expected middle point (3,3) for 13-block pattern, but got {result.Value.Point}");
        Assert.AreEqual(EMATCHTYPE.FIVE, result.Value.Type);
        Assert.AreEqual(EBLOCKCOLORTYPE.FIVE, result.Value.Color, "FIVE match should have FIVE color");
    }
}