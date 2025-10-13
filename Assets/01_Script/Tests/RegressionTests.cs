using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Phase 11.1: bugfix-plan.md에서 수정한 버그들의 회귀 테스트
/// 과거에 발생한 버그가 재발하지 않는지 검증합니다.
/// </summary>
[TestFixture]
public class RegressionTests
{
    private MatchDetector _matchdetector;
    private MatchTypeClassifier _matchtypeclassifier;
    private SpecialBlockFactory _specialblockfactory;
    private Dictionary<(int, int), UI_Match_Block> _testgrid;

    [SetUp]
    public void SetUp()
    {
        _matchdetector = new MatchDetector();
        _matchtypeclassifier = new MatchTypeClassifier();
        _specialblockfactory = new SpecialBlockFactory();
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
    /// 회귀 테스트 #1: L자형 5-매치가 CROSS로 분류되지 않는지 검증
    /// Bug #1 from bugfix-plan.md (커밋: ea79a1b)
    /// L자형 패턴 (3H + 3V, 5개 고유 블록)이 CROSS_THREE로 잘못 분류되던 버그
    /// </summary>
    [Test]
    public void ShouldNotClassifyLShapeFiveMatchAsCross()
    {
        // Arrange: L자형 패턴 생성 (모서리 교차)
        // [R][R][R]
        // [R]
        // [R]
        var block00 = CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        var block01 = CreateBlockAt(0, 1, EBLOCKCOLORTYPE.RED);
        var block02 = CreateBlockAt(0, 2, EBLOCKCOLORTYPE.RED);
        var block10 = CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        var block20 = CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED);

        var xlist = new List<UI_Match_Block> { block00, block10, block20 }; // 가로 3개
        var ylist = new List<UI_Match_Block> { block00, block01, block02 }; // 세로 3개

        // Act: 매치 타입 분류
        var matchtype = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);

        // Assert: FIVE로 분류되어야 함 (CROSS가 아님)
        Assert.AreEqual(EMATCHTYPE.FIVE, matchtype,
            "L자형 패턴은 모서리 교차이므로 CROSS가 아닌 FIVE로 분류되어야 함");
    }

    /// <summary>
    /// 회귀 테스트 #2: 블록 제거 후 (-1, -1) 위치 에러 발생하지 않는지 검증
    /// Bug #2 from bugfix-plan.md (커밋: 26dadee)
    /// 블록이 제거되어 (-1, -1) 상태일 때 SpecialBlockFactory가 크래시하던 버그
    /// </summary>
    [Test]
    public void ShouldHandleInvalidBlockPositionsSafely()
    {
        // Arrange: 유효한 블록과 무효한 블록 (-1, -1) 혼합
        var block00 = CreateBlockAt(0, 0, EBLOCKCOLORTYPE.BLUE);
        var block10 = CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE);
        var block20 = CreateBlockAt(2, 0, EBLOCKCOLORTYPE.BLUE);
        var block30 = CreateBlockAt(3, 0, EBLOCKCOLORTYPE.BLUE);
        var block40 = CreateBlockAt(4, 0, EBLOCKCOLORTYPE.BLUE);

        // 블록 제거 시뮬레이션: 일부 블록의 위치를 (-1, -1)로 설정
        SetBlockPosition(block10, -1, -1); // 제거된 블록
        SetBlockPosition(block30, -1, -1); // 제거된 블록

        var xlist = new List<UI_Match_Block> { block00, block10, block20, block30, block40 };
        var ylist = new List<UI_Match_Block>();

        // Act: 특수 블록 생성 요청 (예외 발생하지 않아야 함)
        SpecialBlockCreationRequest? request = null;
        Assert.DoesNotThrow(() =>
        {
            request = _specialblockfactory.CreateRequest(xlist, ylist, EMATCHTYPE.FIVE);
        }, "(-1, -1) 블록이 있어도 예외 없이 안전하게 처리되어야 함");

        // Assert: 유효한 블록만 사용하여 생성되거나 null 반환
        // 3개의 유효한 블록 (block00, block20, block40)으로 생성 가능
        if (request != null)
        {
            Assert.AreNotEqual((-1, -1), request.Value.Point,
                "생성 위치가 (-1, -1)이면 안 됨");
        }
        // request == null도 허용 (유효한 블록이 충분하지 않을 경우)
    }

    /// <summary>
    /// 회귀 테스트 #3: 빈 블록 리스트로 CalculateMiddlePoint 호출 시 크래시 없음
    /// Bug #3 from bugfix-plan.md (커밋: 349f09d)
    /// 빈 리스트로 Max/Min 연산 시 예외가 발생하던 버그
    /// </summary>
    [Test]
    public void ShouldHandleEmptyBlockListWithoutCrash()
    {
        // Arrange: 빈 블록 리스트
        var xlist = new List<UI_Match_Block>();
        var ylist = new List<UI_Match_Block>();

        // Act: 특수 블록 생성 요청 (예외 발생하지 않아야 함)
        SpecialBlockCreationRequest? request = null;
        Assert.DoesNotThrow(() =>
        {
            request = _specialblockfactory.CreateRequest(xlist, ylist, EMATCHTYPE.FIVE);
        }, "빈 블록 리스트로 호출 시 예외 없이 안전하게 처리되어야 함");

        // Assert: null 반환 (생성할 블록이 없음)
        Assert.IsNull(request, "빈 블록 리스트는 null을 반환해야 함");
    }

    /// <summary>
    /// 회귀 테스트 #4: 양방향 스캔이 모든 위치에서 동작하는지 검증
    /// Bug #1 from bugfix-plan.md (커밋: d905c22)
    /// MatchDetector가 단방향으로만 스캔하여 중간/끝 위치에서 감지 실패하던 버그
    /// </summary>
    [Test]
    public void ShouldDetectMatchFromAnyPositionWithBidirectionalScan()
    {
        // Arrange: 5-매치 생성
        // [R][R][R][R][R]
        //  0  1  2  3  4
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(3, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(4, 0, EBLOCKCOLORTYPE.RED);

        // Act & Assert: 모든 위치에서 전체 5-매치를 감지해야 함
        for (int x = 0; x < 5; x++)
        {
            var result = _matchdetector.DetectHorizontalMatch(_testgrid, (x, 0));

            Assert.IsNotNull(result, $"위치 ({x}, 0)에서 매치를 감지해야 함");
            Assert.AreEqual(5, result.Count, $"위치 ({x}, 0)에서 전체 5-매치를 감지해야 함");

            // 모든 5개 블록이 포함되어야 함
            Assert.Contains((0, 0), result, $"위치 ({x}, 0)에서 시작해도 (0, 0)을 포함해야 함");
            Assert.Contains((1, 0), result, $"위치 ({x}, 0)에서 시작해도 (1, 0)을 포함해야 함");
            Assert.Contains((2, 0), result, $"위치 ({x}, 0)에서 시작해도 (2, 0)을 포함해야 함");
            Assert.Contains((3, 0), result, $"위치 ({x}, 0)에서 시작해도 (3, 0)을 포함해야 함");
            Assert.Contains((4, 0), result, $"위치 ({x}, 0)에서 시작해도 (4, 0)을 포함해야 함");
        }
    }

    /// <summary>
    /// 회귀 테스트 #5: AllBlockMatch 중복 방지가 동작하는지 검증
    /// Bug from bugfix-plan.md Phase 1.2 (커밋: 3d03b2c, b180e1a)
    /// AllBlockMatch가 같은 매치를 여러 번 처리하던 버그
    ///
    /// 참고: 이 테스트는 MatchManager.AllBlockMatch()를 직접 테스트해야 하지만,
    /// 현재 테스트 환경에서는 MatchManager가 MonoBehaviour이므로 간접 검증
    /// </summary>
    [Test]
    public void ShouldPreventDuplicateMatchProcessing()
    {
        // Arrange: 3-매치 생성
        // [G][G][G]
        //  0  1  2
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.GREEN);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.GREEN);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.GREEN);

        // Act: 서로 다른 위치에서 감지한 매치 결과
        var result0 = _matchdetector.DetectHorizontalMatch(_testgrid, (0, 0));
        var result1 = _matchdetector.DetectHorizontalMatch(_testgrid, (1, 0));
        var result2 = _matchdetector.DetectHorizontalMatch(_testgrid, (2, 0));

        // Assert: 모든 결과가 동일한 3개 블록을 포함해야 함
        Assert.IsNotNull(result0);
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);

        Assert.AreEqual(3, result0.Count);
        Assert.AreEqual(3, result1.Count);
        Assert.AreEqual(3, result2.Count);

        // 모든 결과에 같은 블록들이 포함됨 (순서는 다를 수 있음)
        var set0 = new HashSet<(int, int)>(result0);
        var set1 = new HashSet<(int, int)>(result1);
        var set2 = new HashSet<(int, int)>(result2);

        Assert.IsTrue(set0.SetEquals(set1), "위치 0과 1에서 감지한 매치가 동일해야 함");
        Assert.IsTrue(set1.SetEquals(set2), "위치 1과 2에서 감지한 매치가 동일해야 함");
        Assert.IsTrue(set0.SetEquals(set2), "위치 0과 2에서 감지한 매치가 동일해야 함");

        // AllBlockMatch 사용 시 이 3개 위치를 스캔해도 하나의 매치만 처리되어야 함
        // (실제 검증은 AllBlockMatchTests.cs에서 수행됨)
    }

    // Helper methods
    private UI_Match_Block CreateBlockAt(int x, int y, EBLOCKCOLORTYPE color)
    {
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

        // Use reflection to set private fields for testing
        var colorfield = typeof(UI_Match_Block).GetField("_colortypes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        colorfield?.SetValue(block, color);

        _testgrid.Add((x, y), block);
        return block;
    }

    private void SetBlockPosition(UI_Match_Block block, int x, int y)
    {
        // Use reflection to set position via ResetPoint or similar
        var type = typeof(UI_Match_Block);
        var xfield = type.GetField("_x", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var yfield = type.GetField("_y", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        xfield?.SetValue(block, x);
        yfield?.SetValue(block, y);
    }
}
