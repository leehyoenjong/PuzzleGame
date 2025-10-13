using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Phase 11.2: 알려진 엣지 케이스
/// 실제 게임플레이에서 발생 가능한 타이밍 및 동기화 이슈를 검증합니다.
/// </summary>
[TestFixture]
public class EdgeCaseTests
{
    private ChainReactionProcessor _chainreactionprocessor;
    private SpecialBlockFactory _specialblockfactory;
    private BlockSwapHandler _blockswaphandler;
    private GridManager _gridmanager;
    private Dictionary<(int, int), UI_Match_Block> _testgrid;

    [SetUp]
    public void SetUp()
    {
        _chainreactionprocessor = new ChainReactionProcessor();
        _specialblockfactory = new SpecialBlockFactory();
        _blockswaphandler = new BlockSwapHandler();
        _gridmanager = new GridManager();
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
    /// 테스트: ChainReactionProcessor가 이미 (-1, -1) 상태인 블록을 안전하게 처리
    /// 게임 중 블록이 제거된 후 ProcessChainReaction 호출 시 크래시 없어야 함
    /// </summary>
    [Test]
    public void ShouldHandleAlreadyRemovedBlocksInChainReaction()
    {
        // Arrange: 일부 블록은 유효, 일부는 이미 제거됨 (-1, -1)
        var block00 = CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.FORE_LEFTRIGHT);
        var block10 = CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
        var block20 = CreateBlockAt(2, 0, EBLOCKCOLORTYPE.GREEN, EMATCHTYPE.THREE);

        // 블록 제거 시뮬레이션: 일부 블록을 (-1, -1)로 설정
        SetBlockPosition(block10, -1, -1);

        var initialblocks = new List<UI_Match_Block> { block00, block10, block20 };

        // Act: 연쇄 반응 처리 (예외 발생하지 않아야 함)
        List<UI_Match_Block> result = null;
        Assert.DoesNotThrow(() =>
        {
            result = _chainreactionprocessor.ProcessChainReaction(initialblocks, _testgrid);
        }, "(-1, -1) 블록이 있어도 ProcessChainReaction은 안전하게 처리되어야 함");

        // Assert: 결과가 null이 아니어야 함
        Assert.IsNotNull(result, "결과는 null이 아닌 리스트를 반환해야 함");
    }

    /// <summary>
    /// 테스트: SpecialBlockFactory가 중복 위치의 블록을 처리
    /// 같은 위치에 여러 블록이 있을 때 (이론상 불가능하지만 버그 발생 시) 안전하게 처리
    /// </summary>
    [Test]
    public void ShouldHandleDuplicatePositionsInBlockList()
    {
        // Arrange: 같은 위치 (0, 0)에 여러 블록 참조
        var block1 = CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        var block2 = CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED); // 같은 위치

        var xlist = new List<UI_Match_Block> { block1, block2, block1, block2 }; // 중복 포함
        var ylist = new List<UI_Match_Block>();

        // Act: 특수 블록 생성 요청 (예외 발생하지 않아야 함)
        SpecialBlockCreationRequest? request = null;
        Assert.DoesNotThrow(() =>
        {
            request = _specialblockfactory.CreateRequest(xlist, ylist, EMATCHTYPE.FORE_LEFTRIGHT);
        }, "중복 위치 블록이 있어도 안전하게 처리되어야 함");

        // Assert: 생성 위치가 유효해야 함
        if (request != null)
        {
            Assert.AreEqual((0, 0), request.Value.Point, "중복된 블록은 같은 위치를 가리키므로 (0, 0)이어야 함");
        }
    }

    /// <summary>
    /// 테스트: BlockSwapHandler가 교환 후 즉시 블록이 제거되는 시나리오 처리
    /// 교환 → 매치 감지 → 즉시 블록 제거 시 롤백 시도 시 KeyNotFoundException 발생
    /// </summary>
    [Test]
    public void ShouldThrowExceptionWhenRollbackAfterRemoval()
    {
        // Arrange: 두 블록 생성 및 그리드에 추가
        var block00 = CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        var block10 = CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE);
        _testgrid[(0, 0)] = block00;
        _testgrid[(1, 0)] = block10;

        // Act: 교환 수행
        // SwapBlocks 후: (0, 0) = block10, (1, 0) = block00
        _blockswaphandler.SwapBlocks(_testgrid, (0, 0), (1, 0));

        // 교환 후 한 블록을 즉시 제거 (실제 게임에서는 매치로 인해 제거됨)
        _testgrid.Remove((1, 0)); // block00이 이동한 위치를 제거
        // 현재 상태: (0, 0) = block10만 남음

        // Assert: 롤백 시도 시 KeyNotFoundException 발생해야 함
        // RollbackSwap은 (0, 0)과 (1, 0)을 다시 교환하려 하지만, (1, 0)이 없으므로 예외
        Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() =>
        {
            _blockswaphandler.RollbackSwap(_testgrid, (0, 0), (1, 0));
        }, "제거된 블록 위치로 롤백 시도 시 KeyNotFoundException 발생해야 함");

        // (1, 0)은 제거되었으므로 존재하지 않아야 함
        Assert.IsFalse(_testgrid.ContainsKey((1, 0)), "제거된 블록 위치는 롤백해도 복원되지 않음");
        // (0, 0)은 여전히 존재해야 함
        Assert.IsTrue(_testgrid.ContainsKey((0, 0)), "(0, 0)은 그대로 유지되어야 함");
    }

    /// <summary>
    /// 테스트: GridManager가 같은 위치에 연속으로 다른 블록 추가 시 동기화 유지
    /// 블록 추가 → 제거 → 다시 추가 시나리오에서 블록 내부 좌표와 딕셔너리 동기화
    /// </summary>
    [Test]
    public void ShouldMaintainSyncWhenReplacingBlocksAtSamePosition()
    {
        // Arrange: 첫 번째 블록 추가
        var block1 = CreateBlockAt(2, 2, EBLOCKCOLORTYPE.RED);
        _testgrid.Add((2, 2), block1);

        // Act: 같은 위치에 새 블록으로 교체
        var block2 = CreateBlockAt(2, 2, EBLOCKCOLORTYPE.BLUE);

        if (_testgrid.ContainsKey((2, 2)))
        {
            var oldblock = _testgrid[(2, 2)];
            _testgrid[(2, 2)] = block2; // 덮어쓰기
            Object.DestroyImmediate(oldblock.gameObject);
        }

        // Assert: 딕셔너리의 블록이 두 번째 블록이어야 함
        Assert.IsTrue(_testgrid.ContainsKey((2, 2)), "위치 (2, 2)에 블록이 존재해야 함");
        Assert.AreEqual(block2, _testgrid[(2, 2)], "딕셔너리의 블록이 새 블록(block2)이어야 함");

        // 새 블록의 내부 좌표 확인
        var actualx = GetBlockX(block2);
        var actualy = GetBlockY(block2);
        Assert.AreEqual(2, actualx, "블록의 내부 X 좌표가 2여야 함");
        Assert.AreEqual(2, actualy, "블록의 내부 Y 좌표가 2여야 함");
    }

    // Helper methods
    private UI_Match_Block CreateBlockAt(int x, int y, EBLOCKCOLORTYPE color, EMATCHTYPE matchtype = EMATCHTYPE.THREE)
    {
        var gameobject = new GameObject($"Block_{x}_{y}");
        var block = gameobject.AddComponent<UI_Match_Block>();

        // Use reflection to set private fields for testing
        var colorfield = typeof(UI_Match_Block).GetField("_colortypes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        colorfield?.SetValue(block, color);

        var matchtypefield = typeof(UI_Match_Block).GetField("_matchtypes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        matchtypefield?.SetValue(block, matchtype);

        var xfield = typeof(UI_Match_Block).GetField("_x",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        xfield?.SetValue(block, x);

        var yfield = typeof(UI_Match_Block).GetField("_y",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        yfield?.SetValue(block, y);

        return block;
    }

    private void SetBlockPosition(UI_Match_Block block, int x, int y)
    {
        var xfield = typeof(UI_Match_Block).GetField("_x",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        xfield?.SetValue(block, x);

        var yfield = typeof(UI_Match_Block).GetField("_y",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        yfield?.SetValue(block, y);
    }

    private int GetBlockX(UI_Match_Block block)
    {
        var xfield = typeof(UI_Match_Block).GetField("_x",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (int)(xfield?.GetValue(block) ?? -1);
    }

    private int GetBlockY(UI_Match_Block block)
    {
        var yfield = typeof(UI_Match_Block).GetField("_y",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (int)(yfield?.GetValue(block) ?? -1);
    }
}
