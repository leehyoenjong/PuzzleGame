using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 인터페이스 계약 테스트
/// 각 서비스 인터페이스가 정의한 계약을 구현체가 올바르게 준수하는지 검증합니다.
/// </summary>
[TestFixture]
public class InterfaceContractTests
{
    #region IMatchDetector Tests

    private IMatchDetector _matchdetector;
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

    /// <summary>
    /// 가로 3매치 감지 테스트
    /// IMatchDetector는 가로로 연속된 3개의 같은 색상 블록을 감지할 수 있어야 합니다.
    /// </summary>
    [Test]
    public void ShouldDetectHorizontalThreeMatch()
    {
        // Arrange: 가로로 3개의 빨간색 블록 생성 (0,0), (1,0), (2,0)
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED);

        // Act: (0,0)에서 가로 매치 감지
        var result = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 0));

        // Assert: 3개의 블록이 감지되어야 함
        Assert.IsNotNull(result, "가로 3매치가 감지되지 않았습니다.");
        Assert.AreEqual(3, result.Count, "감지된 블록 수가 3개가 아닙니다.");
        Assert.Contains((0, 0), result, "(0,0) 블록이 결과에 포함되지 않았습니다.");
        Assert.Contains((1, 0), result, "(1,0) 블록이 결과에 포함되지 않았습니다.");
        Assert.Contains((2, 0), result, "(2,0) 블록이 결과에 포함되지 않았습니다.");
    }

    /// <summary>
    /// 세로 3매치 감지 테스트
    /// IMatchDetector는 세로로 연속된 3개의 같은 색상 블록을 감지할 수 있어야 합니다.
    /// </summary>
    [Test]
    public void ShouldDetectVerticalThreeMatch()
    {
        // Arrange: 세로로 3개의 파란색 블록 생성 (0,0), (0,1), (0,2)
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(0, 1, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(0, 2, EBLOCKCOLORTYPE.BLUE);

        // Act: (0,0)에서 세로 매치 감지
        var result = _matchdetector.DetectVerticalMatch(_testgrid, (0, 0));

        // Assert: 3개의 블록이 감지되어야 함
        Assert.IsNotNull(result, "세로 3매치가 감지되지 않았습니다.");
        Assert.AreEqual(3, result.Count, "감지된 블록 수가 3개가 아닙니다.");
        Assert.Contains((0, 0), result, "(0,0) 블록이 결과에 포함되지 않았습니다.");
        Assert.Contains((0, 1), result, "(0,1) 블록이 결과에 포함되지 않았습니다.");
        Assert.Contains((0, 2), result, "(0,2) 블록이 결과에 포함되지 않았습니다.");
    }

    /// <summary>
    /// 가로 4매치 감지 테스트
    /// IMatchDetector는 가로로 연속된 4개의 같은 색상 블록을 감지할 수 있어야 합니다.
    /// </summary>
    [Test]
    public void ShouldDetectHorizontalFourMatch()
    {
        // Arrange: 가로로 4개의 노란색 블록 생성 (0,0), (1,0), (2,0), (3,0)
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(3, 0, EBLOCKCOLORTYPE.YELLOW);

        // Act: (0,0)에서 가로 매치 감지
        var result = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 0));

        // Assert: 4개의 블록이 감지되어야 함
        Assert.IsNotNull(result, "가로 4매치가 감지되지 않았습니다.");
        Assert.AreEqual(4, result.Count, "감지된 블록 수가 4개가 아닙니다.");
        Assert.Contains((0, 0), result, "(0,0) 블록이 결과에 포함되지 않았습니다.");
        Assert.Contains((1, 0), result, "(1,0) 블록이 결과에 포함되지 않았습니다.");
        Assert.Contains((2, 0), result, "(2,0) 블록이 결과에 포함되지 않았습니다.");
        Assert.Contains((3, 0), result, "(3,0) 블록이 결과에 포함되지 않았습니다.");
    }

    /// <summary>
    /// 십자형 매치 감지 테스트
    /// IMatchDetector는 십자 패턴(가로 3개 + 세로 3개)을 감지할 수 있어야 합니다.
    /// </summary>
    [Test]
    public void ShouldDetectCrossMatch()
    {
        // Arrange: 십자형 패턴 생성 (중심: 1,1)
        // 세로: (1,0), (1,1), (1,2)
        // 가로: (0,1), (1,1), (2,1)
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.GREEN);
        CreateBlockAt(0, 1, EBLOCKCOLORTYPE.GREEN);
        CreateBlockAt(1, 1, EBLOCKCOLORTYPE.GREEN); // 중심
        CreateBlockAt(2, 1, EBLOCKCOLORTYPE.GREEN);
        CreateBlockAt(1, 2, EBLOCKCOLORTYPE.GREEN);

        // Act: 중심 (1,1)에서 십자 매치 감지
        var result = _matchdetector.DetectCrossMatch(_testgrid, (1, 1));

        // Assert: 십자형 매치가 감지되어야 함
        Assert.IsTrue(result.HasValue, "십자형 매치가 감지되지 않았습니다.");
        Assert.AreEqual(3, result.Value.horizontalmatches.Count, "가로 매치는 3개여야 합니다.");
        Assert.AreEqual(3, result.Value.verticalmatches.Count, "세로 매치는 3개여야 합니다.");

        // 가로 매치 확인
        Assert.Contains((0, 1), result.Value.horizontalmatches, "가로 매치에 (0,1)이 포함되어야 합니다.");
        Assert.Contains((1, 1), result.Value.horizontalmatches, "가로 매치에 (1,1)이 포함되어야 합니다.");
        Assert.Contains((2, 1), result.Value.horizontalmatches, "가로 매치에 (2,1)이 포함되어야 합니다.");

        // 세로 매치 확인
        Assert.Contains((1, 0), result.Value.verticalmatches, "세로 매치에 (1,0)이 포함되어야 합니다.");
        Assert.Contains((1, 1), result.Value.verticalmatches, "세로 매치에 (1,1)이 포함되어야 합니다.");
        Assert.Contains((1, 2), result.Value.verticalmatches, "세로 매치에 (1,2)이 포함되어야 합니다.");
    }

    /// <summary>
    /// 매치 없을 때 null/빈 결과 반환 테스트
    /// IMatchDetector는 매치가 없을 때 적절하게 null 또는 빈 결과를 반환해야 합니다.
    /// </summary>
    [Test]
    public void ShouldReturnEmptyListWhenNoMatch()
    {
        // Arrange: 매치가 없는 패턴 생성 (2개씩만)
        // [R][R][B][B]
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(3, 0, EBLOCKCOLORTYPE.BLUE);

        // Act: 각 위치에서 매치 감지 시도
        var horizontalResult1 = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 0));
        var horizontalResult2 = _matchdetector.DetectHorizontalMatch(_testgrid, (2, 0));
        var verticalResult = _matchdetector.DetectVerticalMatch(_testgrid, (0, 0));
        var crossResult = _matchdetector.DetectCrossMatch(_testgrid, (0, 0));

        // Assert: 모두 null 또는 false를 반환해야 함
        Assert.IsNull(horizontalResult1, "2개만 있는 경우 가로 매치는 null이어야 합니다.");
        Assert.IsNull(horizontalResult2, "2개만 있는 경우 가로 매치는 null이어야 합니다.");
        Assert.IsNull(verticalResult, "세로 블록이 부족한 경우 세로 매치는 null이어야 합니다.");
        Assert.IsFalse(crossResult.HasValue, "십자 패턴이 없는 경우 false여야 합니다.");
    }

    #endregion

    #region IMatchTypeClassifier Tests

    private IMatchTypeClassifier _matchtypeclassifier;

    /// <summary>
    /// 3-매치 분류 테스트
    /// IMatchTypeClassifier는 3개의 블록 매치를 THREE로 분류해야 합니다.
    /// </summary>
    [Test]
    public void ShouldClassifyThreeMatchAsThree()
    {
        // Arrange
        _matchtypeclassifier = new MatchTypeClassifier();

        // 가로 3개 블록 생성
        var xlist = new List<UI_Match_Block>
        {
            CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED),
            CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED),
            CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED)
        };

        var ylist = new List<UI_Match_Block>(); // 세로 매치 없음

        // Act
        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert
        Assert.AreEqual(EMATCHTYPE.THREE, result, "3개 블록은 THREE로 분류되어야 합니다.");
    }

    /// <summary>
    /// 4-매치 상하 분류 테스트
    /// IMatchTypeClassifier는 세로 4개의 블록 매치를 FORE_UPDOWN으로 분류해야 합니다.
    /// </summary>
    [Test]
    public void ShouldClassifyFourMatchAsForeUpDown()
    {
        // Arrange
        _matchtypeclassifier = new MatchTypeClassifier();

        var xlist = new List<UI_Match_Block>(); // 가로 매치 없음

        // 세로 4개 블록 생성
        var ylist = new List<UI_Match_Block>
        {
            CreateBlockAt(0, 0, EBLOCKCOLORTYPE.BLUE),
            CreateBlockAt(0, 1, EBLOCKCOLORTYPE.BLUE),
            CreateBlockAt(0, 2, EBLOCKCOLORTYPE.BLUE),
            CreateBlockAt(0, 3, EBLOCKCOLORTYPE.BLUE)
        };

        // Act
        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert
        Assert.AreEqual(EMATCHTYPE.FORE_UPDOWN, result, "세로 4개 블록은 FORE_UPDOWN으로 분류되어야 합니다.");
    }

    /// <summary>
    /// 4-매치 좌우 분류 테스트
    /// IMatchTypeClassifier는 가로 4개의 블록 매치를 FORE_LEFTRIGHT로 분류해야 합니다.
    /// </summary>
    [Test]
    public void ShouldClassifyFourMatchAsForeLeftRight()
    {
        // Arrange
        _matchtypeclassifier = new MatchTypeClassifier();

        // 가로 4개 블록 생성
        var xlist = new List<UI_Match_Block>
        {
            CreateBlockAt(0, 0, EBLOCKCOLORTYPE.YELLOW),
            CreateBlockAt(1, 0, EBLOCKCOLORTYPE.YELLOW),
            CreateBlockAt(2, 0, EBLOCKCOLORTYPE.YELLOW),
            CreateBlockAt(3, 0, EBLOCKCOLORTYPE.YELLOW)
        };

        var ylist = new List<UI_Match_Block>(); // 세로 매치 없음

        // Act
        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert
        Assert.AreEqual(EMATCHTYPE.FORE_LEFTRIGHT, result, "가로 4개 블록은 FORE_LEFTRIGHT로 분류되어야 합니다.");
    }

    /// <summary>
    /// 5-매치 분류 테스트
    /// IMatchTypeClassifier는 5개 이상의 블록 매치를 FIVE로 분류해야 합니다.
    /// </summary>
    [Test]
    public void ShouldClassifyFiveMatchAsFive()
    {
        // Arrange
        _matchtypeclassifier = new MatchTypeClassifier();

        // 가로 5개 블록 생성
        var xlist = new List<UI_Match_Block>
        {
            CreateBlockAt(0, 0, EBLOCKCOLORTYPE.GREEN),
            CreateBlockAt(1, 0, EBLOCKCOLORTYPE.GREEN),
            CreateBlockAt(2, 0, EBLOCKCOLORTYPE.GREEN),
            CreateBlockAt(3, 0, EBLOCKCOLORTYPE.GREEN),
            CreateBlockAt(4, 0, EBLOCKCOLORTYPE.GREEN)
        };

        var ylist = new List<UI_Match_Block>(); // 세로 매치 없음

        // Act
        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert
        Assert.AreEqual(EMATCHTYPE.FIVE, result, "5개 블록은 FIVE로 분류되어야 합니다.");
    }

    /// <summary>
    /// 십자형 3-매치 분류 테스트
    /// IMatchTypeClassifier는 십자형 패턴(3x3)을 CROSS_THREE로 분류해야 합니다.
    /// </summary>
    [Test]
    public void ShouldClassifyCrossMatchAsCrossThree()
    {
        // Arrange
        _matchtypeclassifier = new MatchTypeClassifier();

        // 십자형 패턴 생성 (중심: 1,1)
        // 중심 블록을 먼저 생성하고 양쪽 리스트에서 공유
        var centerblock = CreateBlockAt(1, 1, EBLOCKCOLORTYPE.PINK);

        // 가로: (0,1), (1,1), (2,1)
        var xlist = new List<UI_Match_Block>
        {
            CreateBlockAt(0, 1, EBLOCKCOLORTYPE.PINK),
            centerblock, // 중심 (공유)
            CreateBlockAt(2, 1, EBLOCKCOLORTYPE.PINK)
        };

        // 세로: (1,0), (1,1), (1,2)
        var ylist = new List<UI_Match_Block>
        {
            CreateBlockAt(1, 0, EBLOCKCOLORTYPE.PINK),
            centerblock, // 중심 (공유)
            CreateBlockAt(1, 2, EBLOCKCOLORTYPE.PINK)
        };

        // Act
        var result = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert
        Assert.AreEqual(EMATCHTYPE.CROSS_THREE, result, "3x3 십자형은 CROSS_THREE로 분류되어야 합니다.");
    }

    #endregion

    #region ISpecialBlockFactory Tests

    private ISpecialBlockFactory _specialblockfactory;

    /// <summary>
    /// 상하 특수블록 요청 생성 테스트
    /// ISpecialBlockFactory는 FORE_UPDOWN 타입의 특수 블록 생성 요청을 만들어야 합니다.
    /// </summary>
    [Test]
    public void ShouldCreateRequestForForeUpDown()
    {
        // Arrange
        _specialblockfactory = new SpecialBlockFactory();

        var xlist = new List<UI_Match_Block>(); // 가로 매치 없음

        // 세로 4개 블록 생성
        var ylist = new List<UI_Match_Block>
        {
            CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED),
            CreateBlockAt(1, 1, EBLOCKCOLORTYPE.RED),
            CreateBlockAt(1, 2, EBLOCKCOLORTYPE.RED),
            CreateBlockAt(1, 3, EBLOCKCOLORTYPE.RED)
        };

        // Act
        var request = _specialblockfactory.CreateRequest(xlist, ylist, EMATCHTYPE.FORE_UPDOWN);

        // Assert
        Assert.IsNotNull(request, "FORE_UPDOWN 타입은 요청을 생성해야 합니다.");
        Assert.AreEqual(EMATCHTYPE.FORE_UPDOWN, request.Value.Type, "타입이 FORE_UPDOWN이어야 합니다.");
        Assert.AreEqual(EBLOCKCOLORTYPE.RED, request.Value.Color, "색상이 RED여야 합니다.");
        Assert.AreEqual(1, request.Value.Point.x, "X 좌표는 1이어야 합니다.");
        // Y 좌표는 (0+3)/2 = 1 (중간점)
        Assert.AreEqual(1, request.Value.Point.y, "Y 좌표는 중간점 1이어야 합니다.");
    }

    /// <summary>
    /// 좌우 특수블록 요청 생성 테스트
    /// ISpecialBlockFactory는 FORE_LEFTRIGHT 타입의 특수 블록 생성 요청을 만들어야 합니다.
    /// </summary>
    [Test]
    public void ShouldCreateRequestForForeLeftRight()
    {
        // Arrange
        _specialblockfactory = new SpecialBlockFactory();

        // 가로 4개 블록 생성
        var xlist = new List<UI_Match_Block>
        {
            CreateBlockAt(0, 1, EBLOCKCOLORTYPE.BLUE),
            CreateBlockAt(1, 1, EBLOCKCOLORTYPE.BLUE),
            CreateBlockAt(2, 1, EBLOCKCOLORTYPE.BLUE),
            CreateBlockAt(3, 1, EBLOCKCOLORTYPE.BLUE)
        };

        var ylist = new List<UI_Match_Block>(); // 세로 매치 없음

        // Act
        var request = _specialblockfactory.CreateRequest(xlist, ylist, EMATCHTYPE.FORE_LEFTRIGHT);

        // Assert
        Assert.IsNotNull(request, "FORE_LEFTRIGHT 타입은 요청을 생성해야 합니다.");
        Assert.AreEqual(EMATCHTYPE.FORE_LEFTRIGHT, request.Value.Type, "타입이 FORE_LEFTRIGHT여야 합니다.");
        Assert.AreEqual(EBLOCKCOLORTYPE.BLUE, request.Value.Color, "색상이 BLUE여야 합니다.");
        // X 좌표는 (0+3)/2 = 1 (중간점)
        Assert.AreEqual(1, request.Value.Point.x, "X 좌표는 중간점 1이어야 합니다.");
        Assert.AreEqual(1, request.Value.Point.y, "Y 좌표는 1이어야 합니다.");
    }

    /// <summary>
    /// 5매치 특수블록 요청 생성 테스트
    /// ISpecialBlockFactory는 FIVE 타입의 특수 블록 생성 요청을 만들고 색상은 FIVE로 설정해야 합니다.
    /// </summary>
    [Test]
    public void ShouldCreateRequestForFive()
    {
        // Arrange
        _specialblockfactory = new SpecialBlockFactory();

        // 가로 5개 블록 생성
        var xlist = new List<UI_Match_Block>
        {
            CreateBlockAt(0, 2, EBLOCKCOLORTYPE.GREEN),
            CreateBlockAt(1, 2, EBLOCKCOLORTYPE.GREEN),
            CreateBlockAt(2, 2, EBLOCKCOLORTYPE.GREEN),
            CreateBlockAt(3, 2, EBLOCKCOLORTYPE.GREEN),
            CreateBlockAt(4, 2, EBLOCKCOLORTYPE.GREEN)
        };

        var ylist = new List<UI_Match_Block>(); // 세로 매치 없음

        // Act
        var request = _specialblockfactory.CreateRequest(xlist, ylist, EMATCHTYPE.FIVE);

        // Assert
        Assert.IsNotNull(request, "FIVE 타입은 요청을 생성해야 합니다.");
        Assert.AreEqual(EMATCHTYPE.FIVE, request.Value.Type, "타입이 FIVE여야 합니다.");
        Assert.AreEqual(EBLOCKCOLORTYPE.FIVE, request.Value.Color, "색상이 FIVE 특수 타입이어야 합니다.");
        // X 좌표는 (0+4)/2 = 2 (중간점)
        Assert.AreEqual(2, request.Value.Point.x, "X 좌표는 중간점 2여야 합니다.");
        Assert.AreEqual(2, request.Value.Point.y, "Y 좌표는 2여야 합니다.");
    }

    /// <summary>
    /// 십자형 특수블록 요청 생성 테스트
    /// ISpecialBlockFactory는 CROSS_THREE 타입의 특수 블록을 교차점에 생성해야 합니다.
    /// </summary>
    [Test]
    public void ShouldCreateRequestForCrossThree()
    {
        // Arrange
        _specialblockfactory = new SpecialBlockFactory();

        // 십자형 패턴 생성 (중심: 2,2)
        var centerblock = CreateBlockAt(2, 2, EBLOCKCOLORTYPE.YELLOW);

        // 가로: (1,2), (2,2), (3,2)
        var xlist = new List<UI_Match_Block>
        {
            CreateBlockAt(1, 2, EBLOCKCOLORTYPE.YELLOW),
            centerblock, // 중심
            CreateBlockAt(3, 2, EBLOCKCOLORTYPE.YELLOW)
        };

        // 세로: (2,1), (2,2), (2,3)
        var ylist = new List<UI_Match_Block>
        {
            CreateBlockAt(2, 1, EBLOCKCOLORTYPE.YELLOW),
            centerblock, // 중심 (공유)
            CreateBlockAt(2, 3, EBLOCKCOLORTYPE.YELLOW)
        };

        // Act
        var request = _specialblockfactory.CreateRequest(xlist, ylist, EMATCHTYPE.CROSS_THREE);

        // Assert
        Assert.IsNotNull(request, "CROSS_THREE 타입은 요청을 생성해야 합니다.");
        Assert.AreEqual(EMATCHTYPE.CROSS_THREE, request.Value.Type, "타입이 CROSS_THREE여야 합니다.");
        Assert.AreEqual(EBLOCKCOLORTYPE.YELLOW, request.Value.Color, "색상이 YELLOW여야 합니다.");
        // 교차점 (2, 2)에 생성되어야 함
        Assert.AreEqual(2, request.Value.Point.x, "X 좌표는 교차점 2여야 합니다.");
        Assert.AreEqual(2, request.Value.Point.y, "Y 좌표는 교차점 2여야 합니다.");
    }

    /// <summary>
    /// 일반 3매치는 요청 없음 테스트
    /// ISpecialBlockFactory는 THREE 타입의 경우 null을 반환해야 합니다.
    /// </summary>
    [Test]
    public void ShouldNotCreateRequestForThreeMatch()
    {
        // Arrange
        _specialblockfactory = new SpecialBlockFactory();

        // 가로 3개 블록 생성
        var xlist = new List<UI_Match_Block>
        {
            CreateBlockAt(0, 0, EBLOCKCOLORTYPE.PINK),
            CreateBlockAt(1, 0, EBLOCKCOLORTYPE.PINK),
            CreateBlockAt(2, 0, EBLOCKCOLORTYPE.PINK)
        };

        var ylist = new List<UI_Match_Block>(); // 세로 매치 없음

        // Act
        var request = _specialblockfactory.CreateRequest(xlist, ylist, EMATCHTYPE.THREE);

        // Assert
        Assert.IsNull(request, "THREE 타입은 특수 블록을 생성하지 않으므로 null을 반환해야 합니다.");
    }

    #endregion

    #region IChainReactionProcessor Tests

    private IChainReactionProcessor _chainreactionprocessor;

    /// <summary>
    /// 상하 블록 연쇄반응 처리 테스트
    /// IChainReactionProcessor는 FORE_UPDOWN 블록의 세로 라인 파괴 효과를 처리해야 합니다.
    /// </summary>
    [Test]
    public void ShouldProcessForeUpDownChainReaction()
    {
        // Arrange
        _chainreactionprocessor = new ChainReactionProcessor();

        // 5x5 그리드 생성
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                CreateBlockAt(x, y, EBLOCKCOLORTYPE.RED);
            }
        }

        // (2,2)에 FORE_UPDOWN 특수 블록 생성
        var specialblock = CreateBlockAt(2, 2, EBLOCKCOLORTYPE.BLUE);
        var matchtypefield = typeof(UI_Match_Block).GetField("_blocktype",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        matchtypefield?.SetValue(specialblock, EMATCHTYPE.FORE_UPDOWN);

        var initialblocks = new List<UI_Match_Block> { specialblock };

        // Act
        var result = _chainreactionprocessor.ProcessChainReaction(initialblocks, _testgrid);

        // Assert
        Assert.IsNotNull(result, "결과 리스트는 null이 아니어야 합니다.");
        Assert.Greater(result.Count, 0, "FORE_UPDOWN은 최소 1개 이상의 블록을 파괴해야 합니다.");

        // x=2 열의 모든 블록(5개)이 파괴되어야 함
        var destroyedcolumn = result.Where(b => b.GetPoint().x == 2).ToList();
        Assert.AreEqual(5, destroyedcolumn.Count, "x=2 열의 모든 블록(5개)이 파괴되어야 합니다.");
    }

    /// <summary>
    /// 좌우 블록 연쇄반응 처리 테스트
    /// IChainReactionProcessor는 FORE_LEFTRIGHT 블록의 가로 라인 파괴 효과를 처리해야 합니다.
    /// </summary>
    [Test]
    public void ShouldProcessForeLeftRightChainReaction()
    {
        // Arrange
        _chainreactionprocessor = new ChainReactionProcessor();

        // 5x5 그리드 생성
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                CreateBlockAt(x, y, EBLOCKCOLORTYPE.GREEN);
            }
        }

        // (3,1)에 FORE_LEFTRIGHT 특수 블록 생성
        var specialblock = CreateBlockAt(3, 1, EBLOCKCOLORTYPE.YELLOW);
        var matchtypefield = typeof(UI_Match_Block).GetField("_blocktype",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        matchtypefield?.SetValue(specialblock, EMATCHTYPE.FORE_LEFTRIGHT);

        var initialblocks = new List<UI_Match_Block> { specialblock };

        // Act
        var result = _chainreactionprocessor.ProcessChainReaction(initialblocks, _testgrid);

        // Assert
        Assert.IsNotNull(result, "결과 리스트는 null이 아니어야 합니다.");
        Assert.Greater(result.Count, 0, "FORE_LEFTRIGHT은 최소 1개 이상의 블록을 파괴해야 합니다.");

        // y=1 행의 모든 블록(5개)이 파괴되어야 함
        var destroyedrow = result.Where(b => b.GetPoint().y == 1).ToList();
        Assert.AreEqual(5, destroyedrow.Count, "y=1 행의 모든 블록(5개)이 파괴되어야 합니다.");
    }

    /// <summary>
    /// 5매치 블록 색상 연쇄반응 테스트
    /// IChainReactionProcessor는 FIVE 블록이 지정된 색상의 모든 블록을 파괴해야 합니다.
    /// </summary>
    [Test]
    public void ShouldProcessFiveBlockWithColor()
    {
        // Arrange
        _chainreactionprocessor = new ChainReactionProcessor();

        // RED 블록 10개 생성 (0,0)~(4,1)
        for (int i = 0; i < 10; i++)
        {
            CreateBlockAt(i % 5, i / 5, EBLOCKCOLORTYPE.RED);
        }
        // BLUE 블록 5개 생성 (0,2)~(4,2)
        for (int i = 0; i < 5; i++)
        {
            CreateBlockAt(i, 2, EBLOCKCOLORTYPE.BLUE);
        }

        // (0,3)에 FIVE 특수 블록 생성 (RED 블록과 겹치지 않는 위치)
        var fiveblock = CreateBlockAt(0, 3, EBLOCKCOLORTYPE.FIVE);
        var matchtypefield = typeof(UI_Match_Block).GetField("_blocktype",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        matchtypefield?.SetValue(fiveblock, EMATCHTYPE.FIVE);

        // RED 블록 하나를 initial blocks에 포함 (FIVE가 RED 색상을 상속받도록)
        var redblock = _testgrid.Values.First(b => b != null && b.GetBlockColorTypes() == EBLOCKCOLORTYPE.RED && b != fiveblock);
        var initialblocks = new List<UI_Match_Block> { fiveblock, redblock };

        // Act
        var result = _chainreactionprocessor.ProcessChainReaction(initialblocks, _testgrid);

        // Assert
        Assert.IsNotNull(result, "결과 리스트는 null이 아니어야 합니다.");

        // FIVE는 RED 색상의 모든 블록을 파괴해야 함 (10개)
        var redblocksdestroyed = result.Where(b => b.GetBlockColorTypes() == EBLOCKCOLORTYPE.RED).ToList();
        Assert.GreaterOrEqual(redblocksdestroyed.Count, 10, "RED 블록이 최소 10개 파괴되어야 합니다.");
    }

    /// <summary>
    /// 특수블록 2개 조합 테스트
    /// IChainReactionProcessor는 여러 특수 블록이 동시에 매치될 때 모든 효과를 처리해야 합니다.
    /// </summary>
    [Test]
    public void ShouldCombineTwoSpecialBlocks()
    {
        // Arrange
        _chainreactionprocessor = new ChainReactionProcessor();

        // 5x5 그리드 생성
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                CreateBlockAt(x, y, EBLOCKCOLORTYPE.PINK);
            }
        }

        // (1,1)에 FORE_UPDOWN, (2,1)에 FORE_LEFTRIGHT 생성
        var foreupdown = CreateBlockAt(1, 1, EBLOCKCOLORTYPE.BLUE);
        var foreleftright = CreateBlockAt(2, 1, EBLOCKCOLORTYPE.YELLOW);

        var matchtypefield = typeof(UI_Match_Block).GetField("_blocktype",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        matchtypefield?.SetValue(foreupdown, EMATCHTYPE.FORE_UPDOWN);
        matchtypefield?.SetValue(foreleftright, EMATCHTYPE.FORE_LEFTRIGHT);

        var initialblocks = new List<UI_Match_Block> { foreupdown, foreleftright };

        // Act
        var result = _chainreactionprocessor.ProcessChainReaction(initialblocks, _testgrid);

        // Assert
        Assert.IsNotNull(result, "결과 리스트는 null이 아니어야 합니다.");
        Assert.Greater(result.Count, 5, "두 특수 블록의 조합은 5개 이상의 블록을 파괴해야 합니다.");

        // x=1 열 전체(FORE_UPDOWN) + y=1 행 전체(FORE_LEFTRIGHT)가 파괴되어야 함
        var column1 = result.Where(b => b.GetPoint().x == 1).ToList();
        var row1 = result.Where(b => b.GetPoint().y == 1).ToList();
        Assert.AreEqual(5, column1.Count, "x=1 열의 모든 블록이 파괴되어야 합니다.");
        Assert.AreEqual(5, row1.Count, "y=1 행의 모든 블록이 파괴되어야 합니다.");
    }

    /// <summary>
    /// 일반 블록은 빈 리스트 반환 테스트
    /// IChainReactionProcessor는 일반 블록(THREE)만 있을 때 초기 블록만 반환해야 합니다.
    /// </summary>
    [Test]
    public void ShouldReturnEmptyListForNormalBlocks()
    {
        // Arrange
        _chainreactionprocessor = new ChainReactionProcessor();

        // 일반 블록(THREE) 3개 생성
        var block1 = CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        var block2 = CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        var block3 = CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED);

        var initialblocks = new List<UI_Match_Block> { block1, block2, block3 };

        // Act
        var result = _chainreactionprocessor.ProcessChainReaction(initialblocks, _testgrid);

        // Assert
        Assert.IsNotNull(result, "결과 리스트는 null이 아니어야 합니다.");
        // 일반 블록은 연쇄반응이 없으므로 초기 블록만 반환 (3개)
        Assert.AreEqual(3, result.Count, "일반 블록은 연쇄반응이 없으므로 초기 블록 3개만 반환되어야 합니다.");
        Assert.Contains(block1, result, "초기 블록1이 결과에 포함되어야 합니다.");
        Assert.Contains(block2, result, "초기 블록2가 결과에 포함되어야 합니다.");
        Assert.Contains(block3, result, "초기 블록3이 결과에 포함되어야 합니다.");
    }

    #endregion

    #region IGridManager Tests

    private IGridManager _gridmanager;

    /// <summary>
    /// 위치로 블록 가져오기 테스트
    /// IGridManager는 지정된 위치의 블록을 정확히 반환해야 합니다.
    /// </summary>
    [Test]
    public void ShouldGetBlockAtPosition()
    {
        // Arrange
        _gridmanager = new GridManager();
        _gridmanager.Initialize(5, 5);

        var testblock = CreateBlockAt(2, 3, EBLOCKCOLORTYPE.RED);
        _gridmanager.SetBlock((2, 3), testblock);

        // Act
        var result = _gridmanager.GetBlock((2, 3));

        // Assert
        Assert.IsNotNull(result, "블록이 존재하는 위치에서는 null이 아닌 블록을 반환해야 합니다.");
        Assert.AreEqual(testblock, result, "설정한 블록과 동일한 블록을 반환해야 합니다.");
        Assert.AreEqual((2, 3), result.GetPoint(), "블록의 위치가 설정한 위치와 일치해야 합니다.");
    }

    /// <summary>
    /// 위치에 블록 설정 테스트
    /// IGridManager는 지정된 위치에 블록을 정확히 설정해야 합니다.
    /// </summary>
    [Test]
    public void ShouldSetBlockAtPosition()
    {
        // Arrange
        _gridmanager = new GridManager();
        _gridmanager.Initialize(5, 5);

        var block1 = CreateBlockAt(1, 1, EBLOCKCOLORTYPE.BLUE);
        var block2 = CreateBlockAt(3, 4, EBLOCKCOLORTYPE.GREEN);

        // Act
        _gridmanager.SetBlock((1, 1), block1);
        _gridmanager.SetBlock((3, 4), block2);

        // Assert
        var retrievedblock1 = _gridmanager.GetBlock((1, 1));
        var retrievedblock2 = _gridmanager.GetBlock((3, 4));

        Assert.AreEqual(block1, retrievedblock1, "(1,1) 위치에 block1이 설정되어야 합니다.");
        Assert.AreEqual(block2, retrievedblock2, "(3,4) 위치에 block2가 설정되어야 합니다.");
        Assert.IsTrue(_gridmanager.HasBlock((1, 1)), "(1,1) 위치에 블록이 존재해야 합니다.");
        Assert.IsTrue(_gridmanager.HasBlock((3, 4)), "(3,4) 위치에 블록이 존재해야 합니다.");
    }

    /// <summary>
    /// 위치의 블록 제거 테스트
    /// IGridManager는 지정된 위치의 블록을 제거한 후 null을 반환해야 합니다.
    /// </summary>
    [Test]
    public void ShouldRemoveBlockAtPosition()
    {
        // Arrange
        _gridmanager = new GridManager();
        _gridmanager.Initialize(5, 5);

        var testblock = CreateBlockAt(2, 2, EBLOCKCOLORTYPE.YELLOW);
        _gridmanager.SetBlock((2, 2), testblock);

        // 제거 전 확인
        Assert.IsNotNull(_gridmanager.GetBlock((2, 2)), "제거 전에는 블록이 존재해야 합니다.");
        Assert.IsTrue(_gridmanager.HasBlock((2, 2)), "제거 전에는 HasBlock이 true를 반환해야 합니다.");

        // Act
        _gridmanager.RemoveBlock((2, 2));

        // Assert
        var result = _gridmanager.GetBlock((2, 2));
        Assert.IsNull(result, "제거 후에는 null을 반환해야 합니다.");
        Assert.IsFalse(_gridmanager.HasBlock((2, 2)), "제거 후에는 HasBlock이 false를 반환해야 합니다.");
    }

    /// <summary>
    /// 유효하지 않은 위치에서 null 반환 테스트
    /// IGridManager는 존재하지 않는 위치나 블록이 없는 위치에서 null을 반환해야 합니다.
    /// </summary>
    [Test]
    public void ShouldReturnNullForInvalidPosition()
    {
        // Arrange
        _gridmanager = new GridManager();
        _gridmanager.Initialize(5, 5);

        // Act & Assert
        // 1. 블록이 설정되지 않은 유효한 위치
        var emptyposition = _gridmanager.GetBlock((1, 1));
        Assert.IsNull(emptyposition, "블록이 없는 위치에서는 null을 반환해야 합니다.");
        Assert.IsFalse(_gridmanager.HasBlock((1, 1)), "블록이 없는 위치에서는 HasBlock이 false를 반환해야 합니다.");

        // 2. 범위를 벗어난 위치 (음수)
        var negativeposition = _gridmanager.GetBlock((-1, 2));
        Assert.IsNull(negativeposition, "범위를 벗어난 위치(-1, 2)에서는 null을 반환해야 합니다.");

        // 3. 범위를 벗어난 위치 (초과)
        var outofboundsposition = _gridmanager.GetBlock((10, 10));
        Assert.IsNull(outofboundsposition, "범위를 벗어난 위치(10, 10)에서는 null을 반환해야 합니다.");
    }

    /// <summary>
    /// 위치 유효성 확인 테스트
    /// IGridManager는 그리드 범위 내의 위치인지 정확히 판단해야 합니다.
    /// </summary>
    [Test]
    public void ShouldCheckIfPositionIsValid()
    {
        // Arrange
        _gridmanager = new GridManager();
        _gridmanager.Initialize(5, 5); // 0~4 범위

        // Act & Assert
        // 1. 유효한 위치들
        Assert.IsTrue(_gridmanager.IsValidPosition((0, 0)), "(0, 0)은 유효한 위치입니다.");
        Assert.IsTrue(_gridmanager.IsValidPosition((4, 4)), "(4, 4)는 유효한 위치입니다.");
        Assert.IsTrue(_gridmanager.IsValidPosition((2, 3)), "(2, 3)은 유효한 위치입니다.");

        // 2. 유효하지 않은 위치들 (음수)
        Assert.IsFalse(_gridmanager.IsValidPosition((-1, 0)), "(-1, 0)은 유효하지 않은 위치입니다.");
        Assert.IsFalse(_gridmanager.IsValidPosition((0, -1)), "(0, -1)은 유효하지 않은 위치입니다.");

        // 3. 유효하지 않은 위치들 (범위 초과)
        Assert.IsFalse(_gridmanager.IsValidPosition((5, 0)), "(5, 0)은 유효하지 않은 위치입니다.");
        Assert.IsFalse(_gridmanager.IsValidPosition((0, 5)), "(0, 5)는 유효하지 않은 위치입니다.");
        Assert.IsFalse(_gridmanager.IsValidPosition((10, 10)), "(10, 10)은 유효하지 않은 위치입니다.");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// 지정된 위치에 테스트용 블록을 생성합니다.
    /// </summary>
    private UI_Match_Block CreateBlockAt(int x, int y, EBLOCKCOLORTYPE color)
    {
        // 이미 블록이 존재하면 먼저 제거
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

        // 리플렉션을 사용하여 private 필드 설정 (테스트 목적)
        var colorfield = typeof(UI_Match_Block).GetField("_colortypes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        colorfield?.SetValue(block, color);

        // 위치 설정 (x, y 필드)
        var xfield = typeof(UI_Match_Block).GetField("_x",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        xfield?.SetValue(block, x);

        var yfield = typeof(UI_Match_Block).GetField("_y",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        yfield?.SetValue(block, y);

        _testgrid.Add((x, y), block);

        return block;
    }

    #endregion
}
