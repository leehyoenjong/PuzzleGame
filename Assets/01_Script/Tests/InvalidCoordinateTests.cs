using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Phase 10.2: 잘못된 좌표 처리
/// 음수 좌표, 매우 큰 좌표, (0, 0) 경계 케이스 등을 검증합니다.
/// </summary>
[TestFixture]
public class InvalidCoordinateTests
{
    private MatchDetector _matchdetector;
    private BlockMover _blockmover;
    private GridManager _gridmanager;

    [SetUp]
    public void SetUp()
    {
        _matchdetector = new MatchDetector();
        _blockmover = new BlockMover();
        _gridmanager = new GridManager();
    }

    [TearDown]
    public void TearDown()
    {
        _matchdetector = null;
        _blockmover = null;
        _gridmanager = null;
    }

    /// <summary>
    /// 테스트: 음수 좌표 전달 시 안전하게 처리
    /// MatchDetector, GridManager 등이 음수 좌표를 받을 때 예외 없이 처리해야 함
    /// </summary>
    [Test]
    public void ShouldHandleNegativeCoordinatesSafely()
    {
        var grid = new Dictionary<(int, int), UI_Match_Block>();

        // 음수 좌표에서 매치 감지 시도
        Assert.DoesNotThrow(() =>
        {
            var result = _matchdetector.DetectHorizontalMatch(grid, (-1, 0));
            Assert.IsNull(result, "음수 좌표는 null 반환해야 함");
        });

        Assert.DoesNotThrow(() =>
        {
            var result = _matchdetector.DetectVerticalMatch(grid, (0, -1));
            Assert.IsNull(result, "음수 좌표는 null 반환해야 함");
        });

        Assert.DoesNotThrow(() =>
        {
            var result = _matchdetector.DetectHorizontalMatch(grid, (-5, -10));
            Assert.IsNull(result, "음수 좌표는 null 반환해야 함");
        });
    }

    /// <summary>
    /// 테스트: 매우 큰 좌표 (int.MaxValue) 전달 시 안전하게 처리
    /// 오버플로우나 OutOfMemoryException 없이 처리되어야 함
    /// </summary>
    [Test]
    public void ShouldHandleVeryLargeCoordinatesSafely()
    {
        var grid = new Dictionary<(int, int), UI_Match_Block>();

        // int.MaxValue 좌표에서 매치 감지 시도
        Assert.DoesNotThrow(() =>
        {
            var result = _matchdetector.DetectHorizontalMatch(grid, (int.MaxValue, 0));
            Assert.IsNull(result, "존재하지 않는 좌표는 null 반환해야 함");
        });

        Assert.DoesNotThrow(() =>
        {
            var result = _matchdetector.DetectVerticalMatch(grid, (0, int.MaxValue));
            Assert.IsNull(result, "존재하지 않는 좌표는 null 반환해야 함");
        });

        Assert.DoesNotThrow(() =>
        {
            var result = _matchdetector.DetectHorizontalMatch(grid, (int.MaxValue, int.MaxValue));
            Assert.IsNull(result, "존재하지 않는 좌표는 null 반환해야 함");
        });
    }

    /// <summary>
    /// 테스트: (0, 0) 좌표 정상 처리 (경계 케이스)
    /// (0, 0)은 유효한 좌표이므로 정상적으로 처리되어야 함
    /// </summary>
    [Test]
    public void ShouldHandleZeroZeroCoordinateNormally()
    {
        var grid = new Dictionary<(int, int), UI_Match_Block>();

        // (0, 0)에 블록 생성
        var block00 = CreateBlockAt(grid, 0, 0, EBLOCKCOLORTYPE.RED);
        var block10 = CreateBlockAt(grid, 1, 0, EBLOCKCOLORTYPE.RED);
        var block20 = CreateBlockAt(grid, 2, 0, EBLOCKCOLORTYPE.RED);

        // (0, 0)에서 수평 매치 감지
        var horizontalmatch = _matchdetector.DetectHorizontalMatch(grid, (0, 0));
        Assert.IsNotNull(horizontalmatch, "(0, 0) 좌표에서 매치를 정상적으로 감지해야 함");
        Assert.AreEqual(3, horizontalmatch.Count, "3-매치가 감지되어야 함");
        Assert.Contains((0, 0), horizontalmatch);

        // (0, 0)에 세로 블록 추가 (기존 그리드 클리어 후 새로 생성)
        CleanupGrid(grid);
        grid = new Dictionary<(int, int), UI_Match_Block>();

        var block00b = CreateBlockAt(grid, 0, 0, EBLOCKCOLORTYPE.BLUE);
        var block01 = CreateBlockAt(grid, 0, 1, EBLOCKCOLORTYPE.BLUE);
        var block02 = CreateBlockAt(grid, 0, 2, EBLOCKCOLORTYPE.BLUE);

        // (0, 0)에서 수직 매치 감지
        var verticalmatch = _matchdetector.DetectVerticalMatch(grid, (0, 0));
        Assert.IsNotNull(verticalmatch, "(0, 0) 좌표에서 수직 매치를 정상적으로 감지해야 함");
        Assert.AreEqual(3, verticalmatch.Count, "3-매치가 감지되어야 함");

        CleanupGrid(grid);
    }

    /// <summary>
    /// 테스트: int.MinValue 좌표 처리
    /// 극단적인 음수 값도 안전하게 처리되어야 함
    /// </summary>
    [Test]
    public void ShouldHandleIntMinValueCoordinateSafely()
    {
        var grid = new Dictionary<(int, int), UI_Match_Block>();

        Assert.DoesNotThrow(() =>
        {
            var result = _matchdetector.DetectHorizontalMatch(grid, (int.MinValue, 0));
            Assert.IsNull(result, "int.MinValue 좌표는 null 반환해야 함");
        });

        Assert.DoesNotThrow(() =>
        {
            var result = _matchdetector.DetectVerticalMatch(grid, (0, int.MinValue));
            Assert.IsNull(result, "int.MinValue 좌표는 null 반환해야 함");
        });

        Assert.DoesNotThrow(() =>
        {
            var result = _matchdetector.DetectHorizontalMatch(grid, (int.MinValue, int.MinValue));
            Assert.IsNull(result, "int.MinValue 좌표는 null 반환해야 함");
        });
    }

    // Helper methods
    private UI_Match_Block CreateBlockAt(Dictionary<(int, int), UI_Match_Block> grid, int x, int y, EBLOCKCOLORTYPE color)
    {
        var blockobj = new GameObject($"Block_{x}_{y}");
        var block = blockobj.AddComponent<UI_Match_Block>();

        // Use reflection to set private fields for testing
        var colorfield = typeof(UI_Match_Block).GetField("_colortypes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        colorfield?.SetValue(block, color);

        grid[(x, y)] = block;
        return block;
    }

    private void CleanupGrid(Dictionary<(int, int), UI_Match_Block> grid)
    {
        foreach (var block in grid.Values)
        {
            if (block != null && block.gameObject != null)
            {
                Object.DestroyImmediate(block.gameObject);
            }
        }
        grid.Clear();
    }
}
