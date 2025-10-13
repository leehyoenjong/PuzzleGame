using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AllBlockMatch에서 특수 블록 생성이 안 되는 버그 재현 테스트
/// 사용자 보고: 게임 시작 시 4x4 매칭이 되었지만 특수 블록이 생성되지 않음
/// </summary>
[TestFixture]
public class AllBlockMatchBugTests
{
    private MatchTypeClassifier _matchtypeclassifier;
    private SpecialBlockFactory _specialblockfactory;
    private Dictionary<(int, int), UI_Match_Block> _testgrid;

    [SetUp]
    public void SetUp()
    {
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
    /// 테스트: AllBlockMatch 시나리오에서 4-매치 발생 시 특수 블록 생성 요청 반환
    /// 버그 재현: usermoveblock = null일 때도 특수 블록 생성되어야 함
    /// </summary>
    [Test]
    public void ShouldCreateSpecialBlockWhenFourMatchInAllBlockMatch()
    {
        // Arrange: 4-매치 가로 패턴 (게임 시작 시 발생 가능)
        // [R][R][R][R]
        //  0  1  2  3
        var block0 = CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        var block1 = CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        var block2 = CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED);
        var block3 = CreateBlockAt(3, 0, EBLOCKCOLORTYPE.RED);

        var xlist = new List<UI_Match_Block> { block0, block1, block2, block3 };
        var ylist = new List<UI_Match_Block>();

        // Act: AllBlockMatch 시나리오 - usermoveblock = null
        var matchtype = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);
        var creationrequest = _specialblockfactory.CreateRequest(xlist, ylist, matchtype, usermoveblock: null);

        // Assert: 특수 블록 생성 요청이 반환되어야 함
        Assert.IsNotNull(creationrequest, "4-매치일 때 usermoveblock이 null이어도 특수 블록 생성 요청이 반환되어야 함");
        Assert.AreEqual(EMATCHTYPE.FORE_LEFTRIGHT, creationrequest.Value.Type, "4-매치 가로는 FORE_LEFTRIGHT 타입이어야 함");

        // 중간점 계산 검증: (0+3)/2 = 1
        Assert.AreEqual((1, 0), creationrequest.Value.Point, "usermoveblock이 없으면 중간점으로 생성되어야 함");
        Assert.AreEqual(EBLOCKCOLORTYPE.RED, creationrequest.Value.Color, "색상은 RED여야 함");
    }

    /// <summary>
    /// 테스트: AllBlockMatch 시나리오에서 5-매치 발생 시 특수 블록 생성 요청 반환
    /// </summary>
    [Test]
    public void ShouldCreateSpecialBlockWhenFiveMatchInAllBlockMatch()
    {
        // Arrange: 5-매치 가로 패턴
        // [B][B][B][B][B]
        //  0  1  2  3  4
        var block0 = CreateBlockAt(0, 0, EBLOCKCOLORTYPE.BLUE);
        var block1 = CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE);
        var block2 = CreateBlockAt(2, 0, EBLOCKCOLORTYPE.BLUE);
        var block3 = CreateBlockAt(3, 0, EBLOCKCOLORTYPE.BLUE);
        var block4 = CreateBlockAt(4, 0, EBLOCKCOLORTYPE.BLUE);

        var xlist = new List<UI_Match_Block> { block0, block1, block2, block3, block4 };
        var ylist = new List<UI_Match_Block>();

        // Act: AllBlockMatch 시나리오 - usermoveblock = null
        var matchtype = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);
        var creationrequest = _specialblockfactory.CreateRequest(xlist, ylist, matchtype, usermoveblock: null);

        // Assert: 특수 블록 생성 요청이 반환되어야 함
        Assert.IsNotNull(creationrequest, "5-매치일 때 usermoveblock이 null이어도 특수 블록 생성 요청이 반환되어야 함");
        Assert.AreEqual(EMATCHTYPE.FIVE, creationrequest.Value.Type, "5-매치는 FIVE 타입이어야 함");

        // 중간점 계산 검증: (0+4)/2 = 2
        Assert.AreEqual((2, 0), creationrequest.Value.Point, "usermoveblock이 없으면 중간점으로 생성되어야 함");
        Assert.AreEqual(EBLOCKCOLORTYPE.FIVE, creationrequest.Value.Color, "FIVE 블록 색상은 FIVE여야 함");
    }

    /// <summary>
    /// 테스트: AllBlockMatch 시나리오에서 CROSS 패턴 발생 시 특수 블록 생성 요청 반환
    /// </summary>
    [Test]
    public void ShouldCreateSpecialBlockWhenCrossMatchInAllBlockMatch()
    {
        // Arrange: CROSS_THREE 패턴 (T자형)
        //     [G]
        // [G][G][G]
        //     [G]
        //  0  1  2
        var block10 = CreateBlockAt(1, 0, EBLOCKCOLORTYPE.GREEN); // 세로 하단
        var block11 = CreateBlockAt(1, 1, EBLOCKCOLORTYPE.GREEN); // 교차점 (중앙)
        var block12 = CreateBlockAt(1, 2, EBLOCKCOLORTYPE.GREEN); // 세로 상단
        var block01 = CreateBlockAt(0, 1, EBLOCKCOLORTYPE.GREEN); // 가로 좌
        var block21 = CreateBlockAt(2, 1, EBLOCKCOLORTYPE.GREEN); // 가로 우

        var xlist = new List<UI_Match_Block> { block01, block11, block21 }; // 가로 3개
        var ylist = new List<UI_Match_Block> { block10, block11, block12 }; // 세로 3개

        // Act: AllBlockMatch 시나리오 - usermoveblock = null
        var matchtype = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);
        var creationrequest = _specialblockfactory.CreateRequest(xlist, ylist, matchtype, usermoveblock: null);

        // Assert: 특수 블록 생성 요청이 반환되어야 함
        Assert.IsNotNull(creationrequest, "CROSS 패턴일 때 usermoveblock이 null이어도 특수 블록 생성 요청이 반환되어야 함");
        Assert.AreEqual(EMATCHTYPE.CROSS_THREE, creationrequest.Value.Type, "T자형 패턴은 CROSS_THREE 타입이어야 함");

        // 교차점 검증: (1, 1)
        Assert.AreEqual((1, 1), creationrequest.Value.Point, "CROSS 패턴은 교차점에 생성되어야 함");
        Assert.AreEqual(EBLOCKCOLORTYPE.GREEN, creationrequest.Value.Color, "색상은 GREEN이어야 함");
    }

    /// <summary>
    /// 테스트: 3-매치는 특수 블록을 생성하지 않아야 함
    /// </summary>
    [Test]
    public void ShouldNotCreateSpecialBlockWhenThreeMatch()
    {
        // Arrange: 3-매치 패턴
        // [Y][Y][Y]
        //  0  1  2
        var block0 = CreateBlockAt(0, 0, EBLOCKCOLORTYPE.YELLOW);
        var block1 = CreateBlockAt(1, 0, EBLOCKCOLORTYPE.YELLOW);
        var block2 = CreateBlockAt(2, 0, EBLOCKCOLORTYPE.YELLOW);

        var xlist = new List<UI_Match_Block> { block0, block1, block2 };
        var ylist = new List<UI_Match_Block>();

        // Act: AllBlockMatch 시나리오 - usermoveblock = null
        var matchtype = _matchtypeclassifier.ClassifyMatchType(xlist, ylist);
        var creationrequest = _specialblockfactory.CreateRequest(xlist, ylist, matchtype, usermoveblock: null);

        // Assert: 특수 블록 생성 요청이 반환되지 않아야 함
        Assert.IsNull(creationrequest, "3-매치는 특수 블록을 생성하지 않아야 함");
    }

    // Helper method
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
