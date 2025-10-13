using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class ChainReactionProcessorTests
{
    private Dictionary<(int, int), UI_Match_Block> _testgrid;
    private ChainReactionProcessor _processor;

    [SetUp]
    public void SetUp()
    {
        _testgrid = new Dictionary<(int, int), UI_Match_Block>();
        _processor = new ChainReactionProcessor();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var block in _testgrid.Values)
        {
            if (block != null && block.gameObject != null)
                Object.DestroyImmediate(block.gameObject);
        }
        _testgrid.Clear();
    }

    private UI_Match_Block CreateTestBlock(int x, int y, EBLOCKCOLORTYPE color, EMATCHTYPE matchtype)
    {
        var gameobject = new GameObject($"TestBlock_{x}_{y}");
        var block = gameobject.AddComponent<UI_Match_Block>();
        
        // RectTransform 컴포넌트 추가 (UI_Match_Block이 필요로 함)
        if (block.GetComponent<RectTransform>() == null)
        {
            gameobject.AddComponent<RectTransform>();
        }
        
        // 리플렉션을 사용하여 private 필드들 설정
        var blocktype = typeof(UI_Match_Block).GetField("_blocktype", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        blocktype?.SetValue(block, matchtype);
        
        var colortypes = typeof(UI_Match_Block).GetField("_colortypes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        colortypes?.SetValue(block, color);
        
        var xfield = typeof(UI_Match_Block).GetField("_x", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        xfield?.SetValue(block, x);
        
        var yfield = typeof(UI_Match_Block).GetField("_y", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        yfield?.SetValue(block, y);
        
        return block;
    }

    [Test]
    public void ShouldProcessForeLeftRightLineClearEffect()
    {
        // Arrange: 3x3 그리드에서 가운데 행의 FORE_LEFTRIGHT 블록
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                var color = (y == 1) ? EBLOCKCOLORTYPE.RED : EBLOCKCOLORTYPE.BLUE;
                var matchtype = (x == 1 && y == 1) ? EMATCHTYPE.FORE_LEFTRIGHT : EMATCHTYPE.THREE;
                var block = CreateTestBlock(x, y, color, matchtype);
                _testgrid[(x, y)] = block;
            }
        }

        var specialblock = _testgrid[(1, 1)]; // 가운데 FORE_LEFTRIGHT 블록
        
        // Act
        var affectedblocks = _processor.ProcessEffect(specialblock, _testgrid);
        
        // Assert: 같은 y좌표(y=1)의 모든 블록이 영향을 받아야 함
        Assert.AreEqual(3, affectedblocks.Count);
        Assert.Contains(_testgrid[(0, 1)], affectedblocks);
        Assert.Contains(_testgrid[(1, 1)], affectedblocks);
        Assert.Contains(_testgrid[(2, 1)], affectedblocks);
        
        // 다른 행의 블록들은 영향을 받지 않아야 함
        Assert.IsFalse(affectedblocks.Contains(_testgrid[(0, 0)]));
        Assert.IsFalse(affectedblocks.Contains(_testgrid[(0, 2)]));
    }

    [Test]
    public void ShouldProcessForeUpDownLineClearEffect()
    {
        // Arrange: 3x3 그리드에서 가운데 열의 FORE_UPDOWN 블록
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                var color = (x == 1) ? EBLOCKCOLORTYPE.RED : EBLOCKCOLORTYPE.BLUE;
                var matchtype = (x == 1 && y == 1) ? EMATCHTYPE.FORE_UPDOWN : EMATCHTYPE.THREE;
                var block = CreateTestBlock(x, y, color, matchtype);
                _testgrid[(x, y)] = block;
            }
        }

        var specialblock = _testgrid[(1, 1)]; // 가운데 FORE_UPDOWN 블록
        
        // Act
        var affectedblocks = _processor.ProcessEffect(specialblock, _testgrid);
        
        // Assert: 같은 x좌표(x=1)의 모든 블록이 영향을 받아야 함
        Assert.AreEqual(3, affectedblocks.Count);
        Assert.Contains(_testgrid[(1, 0)], affectedblocks);
        Assert.Contains(_testgrid[(1, 1)], affectedblocks);
        Assert.Contains(_testgrid[(1, 2)], affectedblocks);
        
        // 다른 열의 블록들은 영향을 받지 않아야 함
        Assert.IsFalse(affectedblocks.Contains(_testgrid[(0, 0)]));
        Assert.IsFalse(affectedblocks.Contains(_testgrid[(2, 0)]));
    }

    [Test]
    public void ShouldProcessFiveColorMatchEffect()
    {
        // Arrange: 다양한 색상의 블록들이 있는 그리드에서 FIVE 블록
        var redblocks = new List<(int, int)> { (0, 0), (1, 0), (2, 0) };
        var blueblocks = new List<(int, int)> { (0, 1), (1, 1) };
        var greenblocks = new List<(int, int)> { (0, 2), (1, 2) };
        
        // RED 블록들 생성
        foreach (var pos in redblocks)
        {
            var block = CreateTestBlock(pos.Item1, pos.Item2, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
            _testgrid[pos] = block;
        }
        
        // BLUE 블록들 생성
        foreach (var pos in blueblocks)
        {
            var block = CreateTestBlock(pos.Item1, pos.Item2, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
            _testgrid[pos] = block;
        }
        
        // GREEN 블록들 생성
        foreach (var pos in greenblocks)
        {
            var block = CreateTestBlock(pos.Item1, pos.Item2, EBLOCKCOLORTYPE.GREEN, EMATCHTYPE.THREE);
            _testgrid[pos] = block;
        }
        
        // FIVE 블록 생성 (RED 색상을 타겟으로)
        var fiveblock = CreateTestBlock(2, 1, EBLOCKCOLORTYPE.RED, EMATCHTYPE.FIVE);
        _testgrid[(2, 1)] = fiveblock;
        
        // Act: RED 색상으로 FIVE 블록 활성화
        var affectedblocks = _processor.ProcessEffect(fiveblock, _testgrid, EBLOCKCOLORTYPE.RED);
        
        // Assert: 모든 RED 블록과 FIVE 블록 자체가 영향을 받아야 함
        Assert.AreEqual(4, affectedblocks.Count); // 3개 RED + 1개 FIVE
        foreach (var pos in redblocks)
        {
            Assert.Contains(_testgrid[pos], affectedblocks);
        }
        Assert.Contains(fiveblock, affectedblocks);
        
        // 다른 색상 블록들은 영향을 받지 않아야 함
        foreach (var pos in blueblocks.Concat(greenblocks))
        {
            Assert.IsFalse(affectedblocks.Contains(_testgrid[pos]));
        }
    }

    [Test]
    public void ShouldProcessCrossThreeAreaClearEffect()
    {
        // Arrange: 5x5 그리드에서 가운데에 CROSS_THREE 블록
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                var matchtype = (x == 2 && y == 2) ? EMATCHTYPE.CROSS_THREE : EMATCHTYPE.THREE;
                var block = CreateTestBlock(x, y, EBLOCKCOLORTYPE.RED, matchtype);
                _testgrid[(x, y)] = block;
            }
        }

        var crossblock = _testgrid[(2, 2)]; // 가운데 CROSS_THREE 블록
        
        // Act
        var affectedblocks = _processor.ProcessEffect(crossblock, _testgrid);
        
        // Assert: 3x3 영역의 블록들이 영향을 받아야 함 (startindex: -1, endindex: 2)
        var expectedpositions = new List<(int, int)>();
        for (int y = -1; y < 2; y++) // startindex: -1, endindex: 2
        {
            for (int x = -1; x < 2; x++)
            {
                var keyx = 2 + x; // 중심 (2,2)에서 offset
                var keyy = 2 + y;
                if (keyx >= 0 && keyx < 5 && keyy >= 0 && keyy < 5)
                {
                    expectedpositions.Add((keyx, keyy));
                }
            }
        }
        
        Assert.AreEqual(9, affectedblocks.Count); // 3x3 = 9개
        foreach (var pos in expectedpositions)
        {
            Assert.Contains(_testgrid[pos], affectedblocks);
        }
        
        // 3x3 영역 밖의 블록들은 영향을 받지 않아야 함
        Assert.IsFalse(affectedblocks.Contains(_testgrid[(0, 0)])); // 영역 밖
        Assert.IsFalse(affectedblocks.Contains(_testgrid[(4, 4)])); // 영역 밖
    }

    [Test]
    public void ShouldProcessCrossFourAreaClearEffect()
    {
        // Arrange: 7x7 그리드에서 가운데에 CROSS_FOUR 블록
        for (int x = 0; x < 7; x++)
        {
            for (int y = 0; y < 7; y++)
            {
                var matchtype = (x == 3 && y == 3) ? EMATCHTYPE.CROSS_FOUR : EMATCHTYPE.THREE;
                var block = CreateTestBlock(x, y, EBLOCKCOLORTYPE.RED, matchtype);
                _testgrid[(x, y)] = block;
            }
        }

        var crossblock = _testgrid[(3, 3)]; // 가운데 CROSS_FOUR 블록
        
        // Act
        var affectedblocks = _processor.ProcessEffect(crossblock, _testgrid);
        
        // Assert: 4x4 영역의 블록들이 영향을 받아야 함 (startindex: -3, endindex: 4)
        var expectedpositions = new List<(int, int)>();
        for (int y = -3; y < 4; y++) // startindex: -3, endindex: 4
        {
            for (int x = -3; x < 4; x++)
            {
                var keyx = 3 + x; // 중심 (3,3)에서 offset
                var keyy = 3 + y;
                if (keyx >= 0 && keyx < 7 && keyy >= 0 && keyy < 7)
                {
                    expectedpositions.Add((keyx, keyy));
                }
            }
        }
        
        Assert.AreEqual(49, affectedblocks.Count); // 7x7 = 49개 (전체 그리드)
        foreach (var pos in expectedpositions)
        {
            Assert.Contains(_testgrid[pos], affectedblocks);
        }
    }

    [Test]
    public void ShouldDetectAndProcessChainedSpecialBlocks()
    {
        // Arrange: FORE_LEFTRIGHT 블록이 같은 라인에 있는 다른 특수 블록들을 체인시키는 상황
        // 3x3 그리드에서 가운데 행에 여러 특수 블록들 배치
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                EMATCHTYPE matchtype = EMATCHTYPE.THREE;
                if (y == 1) // 가운데 행
                {
                    if (x == 0) matchtype = EMATCHTYPE.FORE_LEFTRIGHT; // 시작 블록
                    else if (x == 1) matchtype = EMATCHTYPE.FORE_UPDOWN; // 체인될 블록
                    else if (x == 2) matchtype = EMATCHTYPE.CROSS_THREE; // 또 다른 체인될 블록
                }
                
                var block = CreateTestBlock(x, y, EBLOCKCOLORTYPE.RED, matchtype);
                _testgrid[(x, y)] = block;
            }
        }

        var initialblocks = new List<UI_Match_Block> { _testgrid[(0, 1)] }; // FORE_LEFTRIGHT 블록만 시작
        
        // Act
        var finaldestroylist = _processor.ProcessChainReaction(initialblocks, _testgrid);
        
        // Assert: 체인 반응으로 모든 특수 블록들이 포함되어야 함
        Assert.IsTrue(finaldestroylist.Count > 3); // 최소한 초기 3개 블록보다 많아야 함
        Assert.Contains(_testgrid[(0, 1)], finaldestroylist); // 시작 FORE_LEFTRIGHT
        Assert.Contains(_testgrid[(1, 1)], finaldestroylist); // 체인된 FORE_UPDOWN  
        Assert.Contains(_testgrid[(2, 1)], finaldestroylist); // 체인된 CROSS_THREE
        
        // FORE_UPDOWN의 효과로 세로 라인도 포함되어야 함
        Assert.Contains(_testgrid[(1, 0)], finaldestroylist);
        Assert.Contains(_testgrid[(1, 2)], finaldestroylist);
    }

    [Test]
    public void ShouldInheritColorForFiveBlockChains()
    {
        // Arrange: FORE 블록이 FIVE 블록을 체인시키고, FIVE 블록이 색상을 상속받아야 하는 상황
        // 5x3 그리드 설정
        
        // RED 블록들 (FIVE 블록이 타겟할 색상)
        var redpositions = new List<(int, int)> { (0, 0), (0, 1), (0, 2), (4, 0), (4, 2) };
        foreach (var pos in redpositions)
        {
            var block = CreateTestBlock(pos.Item1, pos.Item2, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
            _testgrid[pos] = block;
        }
        
        // BLUE 블록들 (영향받지 않아야 함)
        var bluepositions = new List<(int, int)> { (1, 0), (1, 2), (3, 0), (3, 2) };
        foreach (var pos in bluepositions)
        {
            var block = CreateTestBlock(pos.Item1, pos.Item2, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
            _testgrid[pos] = block;
        }
        
        // 가운데 행에 FORE_LEFTRIGHT와 FIVE 블록 배치
        var foreblock = CreateTestBlock(2, 1, EBLOCKCOLORTYPE.RED, EMATCHTYPE.FORE_LEFTRIGHT);
        _testgrid[(2, 1)] = foreblock;
        
        var fiveblock = CreateTestBlock(4, 1, EBLOCKCOLORTYPE.GREEN, EMATCHTYPE.FIVE); // GREEN이지만 RED 색상을 상속받아야 함
        _testgrid[(4, 1)] = fiveblock;
        
        var initialblocks = new List<UI_Match_Block> { foreblock }; // FORE_LEFTRIGHT로 시작
        
        // Act
        var finaldestroylist = _processor.ProcessChainReaction(initialblocks, _testgrid);
        
        // Assert: FIVE 블록이 FORE 블록으로부터 RED 색상을 상속받아 모든 RED 블록들을 파괴해야 함
        Assert.Contains(fiveblock, finaldestroylist); // FIVE 블록 자체도 포함
        
        // 모든 RED 블록들이 포함되어야 함 (색상 상속으로 인해)
        foreach (var pos in redpositions)
        {
            Assert.Contains(_testgrid[pos], finaldestroylist);
        }
        
        // BLUE 블록들은 영향받지 않아야 함 (상속된 색상이 RED이므로)
        foreach (var pos in bluepositions)
        {
            Assert.IsFalse(finaldestroylist.Contains(_testgrid[pos]));
        }
    }

    [Test]
    public void ShouldNotProcessSameBlockTwice()
    {
        // Arrange: 겹치는 영향을 받는 블록이 있는 상황 - 두 FORE 블록이 교차하는 지점
        // 3x3 그리드에서 십자 형태로 FORE 블록들 배치
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                EMATCHTYPE matchtype = EMATCHTYPE.THREE;
                if (x == 1 && y == 0) matchtype = EMATCHTYPE.FORE_UPDOWN; // 세로 FORE
                else if (x == 0 && y == 1) matchtype = EMATCHTYPE.FORE_LEFTRIGHT; // 가로 FORE

                var block = CreateTestBlock(x, y, EBLOCKCOLORTYPE.RED, matchtype);
                _testgrid[(x, y)] = block;
            }
        }

        var initialblocks = new List<UI_Match_Block>
        {
            _testgrid[(1, 0)], // FORE_UPDOWN
            _testgrid[(0, 1)]  // FORE_LEFTRIGHT
        };

        Debug.Log($"[ShouldNotProcessSameBlockTwice] Initial blocks count: {initialblocks.Count}");
        Debug.Log($"[ShouldNotProcessSameBlockTwice] FORE_UPDOWN at (1,0): {_testgrid[(1, 0)].GetBlockMatchTypes()}");
        Debug.Log($"[ShouldNotProcessSameBlockTwice] FORE_LEFTRIGHT at (0,1): {_testgrid[(0, 1)].GetBlockMatchTypes()}");
        Debug.Log($"[ShouldNotProcessSameBlockTwice] Intersection block at (1,1): {_testgrid[(1, 1)].GetBlockMatchTypes()}");

        // Act
        var finaldestroylist = _processor.ProcessChainReaction(initialblocks, _testgrid);

        Debug.Log($"[ShouldNotProcessSameBlockTwice] Final destroy list count: {finaldestroylist.Count}");
        Debug.Log($"[ShouldNotProcessSameBlockTwice] Final destroy list blocks:");
        foreach (var block in finaldestroylist)
        {
            Debug.Log($"  - Block at ({block.GetPoint().x}, {block.GetPoint().y}): {block.GetBlockMatchTypes()}");
        }

        // Assert: (1,1) 블록은 두 FORE 블록 모두의 영향을 받지만 한 번만 포함되어야 함
        var intersectionblock = _testgrid[(1, 1)];
        var intersectioncount = finaldestroylist.Count(block => block == intersectionblock);

        Debug.Log($"[ShouldNotProcessSameBlockTwice] Intersection block count: {intersectioncount}");
        Assert.AreEqual(1, intersectioncount, "Intersection block should appear only once in final destroy list");

        // 두 FORE 블록의 영향을 받는 블록들:
        // FORE_UPDOWN(1,0): (1,0), (1,1), (1,2) - x=1 라인
        // FORE_LEFTRIGHT(0,1): (0,1), (1,1), (2,1) - y=1 라인
        // 합집합: (1,0), (1,1), (1,2), (0,1), (2,1) = 5개 (초기 2개 + 추가 3개)
        Assert.AreEqual(5, finaldestroylist.Count, $"Expected 5 blocks, but got {finaldestroylist.Count}");

        // 중복이 없어야 함
        var uniqueblocks = finaldestroylist.Distinct().ToList();
        Assert.AreEqual(finaldestroylist.Count, uniqueblocks.Count, "No duplicate blocks should exist in final destroy list");
    }

    [Test]
    public void ShouldHandleCircularChainReferences()
    {
        // Arrange: 순환 참조가 발생할 수 있는 상황
        // 두 개의 FORE 블록이 서로를 활성화시킬 수 있는 배치
        // 3x3 그리드에서 FORE_LEFTRIGHT와 FORE_UPDOWN가 서로의 영향 범위에 있도록 배치
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                EMATCHTYPE matchtype = EMATCHTYPE.THREE;
                if (x == 1 && y == 1) matchtype = EMATCHTYPE.FORE_LEFTRIGHT; // 가운데 가로 FORE
                else if (x == 2 && y == 1) matchtype = EMATCHTYPE.FORE_UPDOWN; // 오른쪽 세로 FORE (같은 y=1 라인)

                var block = CreateTestBlock(x, y, EBLOCKCOLORTYPE.RED, matchtype);
                _testgrid[(x, y)] = block;
            }
        }

        // 첫 번째 FORE 블록만으로 시작
        var initialblocks = new List<UI_Match_Block> { _testgrid[(1, 1)] }; // FORE_LEFTRIGHT

        Debug.Log($"[ShouldHandleCircularChainReferences] Initial block: FORE_LEFTRIGHT at (1,1)");
        Debug.Log($"[ShouldHandleCircularChainReferences] Chain target: FORE_UPDOWN at (2,1)");

        // Act - 무한 루프가 발생하지 않아야 함
        var finaldestroylist = _processor.ProcessChainReaction(initialblocks, _testgrid);

        Debug.Log($"[ShouldHandleCircularChainReferences] Final destroy list count: {finaldestroylist.Count}");
        Debug.Log($"[ShouldHandleCircularChainReferences] Final destroy list blocks:");
        foreach (var block in finaldestroylist)
        {
            Debug.Log($"  - Block at ({block.GetPoint().x}, {block.GetPoint().y}): {block.GetBlockMatchTypes()}");
        }

        // Assert: 체인 반응으로 모든 블록이 포함되어야 함
        // FORE_LEFTRIGHT(1,1): y=1 라인 파괴 → (0,1), (1,1), (2,1) - FORE_UPDOWN(2,1) 포함!
        // FORE_UPDOWN(2,1): x=2 라인 파괴 → (2,0), (2,1), (2,2)
        // 합집합: (0,1), (1,1), (2,1), (2,0), (2,2) = 5개
        Assert.AreEqual(5, finaldestroylist.Count, $"Expected 5 blocks, but got {finaldestroylist.Count}");

        // 두 FORE 블록 모두 포함되어야 함
        Assert.Contains(_testgrid[(1, 1)], finaldestroylist); // FORE_LEFTRIGHT
        Assert.Contains(_testgrid[(2, 1)], finaldestroylist); // FORE_UPDOWN

        // 중복이 없어야 함 (순환 참조로 인한 중복 처리 방지)
        var uniqueblocks = finaldestroylist.Distinct().ToList();
        Assert.AreEqual(finaldestroylist.Count, uniqueblocks.Count, "Circular references should not cause duplicate processing");
    }

    // Phase 4.1: 무한 루프 방지 테스트

    [Test]
    public void ShouldPreventInfiniteLoopWithAToBACircularReference()
    {
        // Arrange: A → B → A 순환 참조 시나리오
        // 블록 A(1,1)가 블록 B(2,1)를 트리거하고, B가 다시 A를 트리거하는 상황
        // A: FORE_LEFTRIGHT at (1,1) → y=1 라인 전체를 파괴 (B 포함)
        // B: FORE_UPDOWN at (2,1) → x=2 라인 전체를 파괴 (다시 y=1에 영향 없음, 하지만 이미 A는 처리됨)

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                EMATCHTYPE matchtype = EMATCHTYPE.THREE;
                if (x == 1 && y == 1) matchtype = EMATCHTYPE.FORE_LEFTRIGHT; // A: 가로 FORE
                else if (x == 2 && y == 1) matchtype = EMATCHTYPE.FORE_UPDOWN; // B: 세로 FORE

                var block = CreateTestBlock(x, y, EBLOCKCOLORTYPE.RED, matchtype);
                _testgrid[(x, y)] = block;
            }
        }

        var blockA = _testgrid[(1, 1)]; // FORE_LEFTRIGHT
        var blockB = _testgrid[(2, 1)]; // FORE_UPDOWN

        var initialblocks = new List<UI_Match_Block> { blockA };

        // Act: 순환 참조가 있어도 무한 루프 없이 정상 완료되어야 함
        var finaldestroylist = _processor.ProcessChainReaction(initialblocks, _testgrid);

        // Assert:
        // 1. 무한 루프 없이 완료됨 (테스트가 종료되면 성공)
        // 2. A와 B 모두 최종 리스트에 포함
        Assert.Contains(blockA, finaldestroylist, "Block A should be in final destroy list");
        Assert.Contains(blockB, finaldestroylist, "Block B should be in final destroy list");

        // 3. A와 B는 각각 한 번씩만 처리되어야 함
        Assert.AreEqual(1, finaldestroylist.Count(b => b == blockA), "Block A should appear exactly once");
        Assert.AreEqual(1, finaldestroylist.Count(b => b == blockB), "Block B should appear exactly once");

        // 4. 전체 결과는 합리적인 범위 (최대 9개 블록, 3x3 그리드)
        Assert.LessOrEqual(finaldestroylist.Count, 9, "Should not exceed grid size");
    }

    [Test]
    public void ShouldPreventInfiniteLoopWithABCACircularReference()
    {
        // Arrange: A → B → C → A 순환 참조 시나리오 (더 복잡한 순환)
        // 4x4 그리드에서 3개의 FORE 블록이 순환 참조를 형성
        // A: FORE_LEFTRIGHT at (1,1) → y=1 라인 파괴 (B 포함)
        // B: FORE_UPDOWN at (2,1) → x=2 라인 파괴 (C 포함)
        // C: FORE_LEFTRIGHT at (2,2) → y=2 라인 파괴 (D 포함)
        // D: FORE_UPDOWN at (1,2) → x=1 라인 파괴 (A 포함 - 순환 완성!)

        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                EMATCHTYPE matchtype = EMATCHTYPE.THREE;
                if (x == 1 && y == 1) matchtype = EMATCHTYPE.FORE_LEFTRIGHT; // A
                else if (x == 2 && y == 1) matchtype = EMATCHTYPE.FORE_UPDOWN; // B
                else if (x == 2 && y == 2) matchtype = EMATCHTYPE.FORE_LEFTRIGHT; // C
                else if (x == 1 && y == 2) matchtype = EMATCHTYPE.FORE_UPDOWN; // D (순환 완성)

                var block = CreateTestBlock(x, y, EBLOCKCOLORTYPE.RED, matchtype);
                _testgrid[(x, y)] = block;
            }
        }

        var blockA = _testgrid[(1, 1)]; // FORE_LEFTRIGHT
        var blockB = _testgrid[(2, 1)]; // FORE_UPDOWN
        var blockC = _testgrid[(2, 2)]; // FORE_LEFTRIGHT
        var blockD = _testgrid[(1, 2)]; // FORE_UPDOWN

        var initialblocks = new List<UI_Match_Block> { blockA };

        // Act: 복잡한 순환 참조가 있어도 무한 루프 없이 정상 완료되어야 함
        var finaldestroylist = _processor.ProcessChainReaction(initialblocks, _testgrid);

        // Assert:
        // 1. 무한 루프 없이 완료됨 (테스트가 종료되면 성공)
        // 2. A, B, C, D 모두 최종 리스트에 포함
        Assert.Contains(blockA, finaldestroylist, "Block A should be in final destroy list");
        Assert.Contains(blockB, finaldestroylist, "Block B should be in final destroy list");
        Assert.Contains(blockC, finaldestroylist, "Block C should be in final destroy list");
        Assert.Contains(blockD, finaldestroylist, "Block D should be in final destroy list");

        // 3. 각 블록은 정확히 한 번씩만 처리되어야 함
        Assert.AreEqual(1, finaldestroylist.Count(b => b == blockA), "Block A should appear exactly once");
        Assert.AreEqual(1, finaldestroylist.Count(b => b == blockB), "Block B should appear exactly once");
        Assert.AreEqual(1, finaldestroylist.Count(b => b == blockC), "Block C should appear exactly once");
        Assert.AreEqual(1, finaldestroylist.Count(b => b == blockD), "Block D should appear exactly once");

        // 4. 전체 결과는 합리적인 범위 (최대 16개 블록, 4x4 그리드)
        Assert.LessOrEqual(finaldestroylist.Count, 16, "Should not exceed grid size");

        // 5. 중복 없음 확인
        var uniqueblocks = finaldestroylist.Distinct().ToList();
        Assert.AreEqual(finaldestroylist.Count, uniqueblocks.Count, "No duplicate blocks in final list");
    }

    [Test]
    public void ShouldNotProcessAlreadyProcessedBlockAgain()
    {
        // Arrange: 여러 경로로 같은 블록에 도달할 수 있는 시나리오
        // 5x5 그리드에서 중앙에 일반 블록, 주변에 여러 FORE 블록 배치
        // 모든 FORE 블록이 중앙 블록을 포함하지만, 중앙 블록은 한 번만 처리되어야 함

        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                EMATCHTYPE matchtype = EMATCHTYPE.THREE;

                // 중앙(2,2)을 제외한 + 형태로 FORE 블록 배치
                if (x == 2 && y == 0) matchtype = EMATCHTYPE.FORE_UPDOWN; // 아래쪽
                else if (x == 2 && y == 4) matchtype = EMATCHTYPE.FORE_UPDOWN; // 위쪽
                else if (x == 0 && y == 2) matchtype = EMATCHTYPE.FORE_LEFTRIGHT; // 왼쪽
                else if (x == 4 && y == 2) matchtype = EMATCHTYPE.FORE_LEFTRIGHT; // 오른쪽

                var block = CreateTestBlock(x, y, EBLOCKCOLORTYPE.RED, matchtype);
                _testgrid[(x, y)] = block;
            }
        }

        var centerblock = _testgrid[(2, 2)]; // 중앙 블록 (모든 FORE의 타겟)

        var initialblocks = new List<UI_Match_Block>
        {
            _testgrid[(2, 0)], // 아래쪽 FORE_UPDOWN
            _testgrid[(2, 4)], // 위쪽 FORE_UPDOWN
            _testgrid[(0, 2)], // 왼쪽 FORE_LEFTRIGHT
            _testgrid[(4, 2)]  // 오른쪽 FORE_LEFTRIGHT
        };

        // Act: 4개의 FORE 블록이 모두 중앙을 포함하지만, 중앙은 한 번만 처리되어야 함
        var finaldestroylist = _processor.ProcessChainReaction(initialblocks, _testgrid);

        // Assert:
        // 1. 중앙 블록이 최종 리스트에 포함됨
        Assert.Contains(centerblock, finaldestroylist, "Center block should be in final destroy list");

        // 2. 중앙 블록은 정확히 한 번만 나타나야 함 (4개 FORE가 모두 포함하지만 중복 없음)
        var centercount = finaldestroylist.Count(b => b == centerblock);
        Assert.AreEqual(1, centercount, $"Center block should appear exactly once, but appeared {centercount} times");

        // 3. 모든 초기 FORE 블록들이 포함됨
        foreach (var initialblock in initialblocks)
        {
            Assert.Contains(initialblock, finaldestroylist, $"Initial block at {initialblock.GetPoint()} should be included");
        }

        // 4. 중복 없음 확인
        var uniqueblocks = finaldestroylist.Distinct().ToList();
        Assert.AreEqual(finaldestroylist.Count, uniqueblocks.Count,
            $"No duplicate blocks should exist. Total: {finaldestroylist.Count}, Unique: {uniqueblocks.Count}");

        // 5. 결과가 예상 범위 내 (십자 형태 + 중앙 = 최대 13개)
        // 세로: (2,0), (2,1), (2,2), (2,3), (2,4) = 5개
        // 가로: (0,2), (1,2), (2,2 중복), (3,2), (4,2) = 4개 추가 (중앙 제외)
        // 합: 5 + 4 = 9개 고유 블록 (4 FORE + 중앙 + 추가 4개)
        Assert.LessOrEqual(finaldestroylist.Count, 13, "Should not exceed maximum expected blocks");
    }

    [Test]
    public void ShouldHandleVeryLongChainReactionWithoutStackOverflow()
    {
        // Arrange: 매우 긴 연쇄 반응 시나리오 (100단계 이상)
        // 100x1 그리드에서 모든 블록을 FORE_UPDOWN으로 배치
        // 각 블록이 다음 블록을 체인시키는 긴 연쇄 생성
        // 주의: 실제로는 processedblocks HashSet이 중복을 막지만,
        // 이 테스트는 큐가 매우 커져도 스택 오버플로우 없이 처리됨을 검증

        int gridsize = 50; // 50개 블록 (충분히 긴 체인)

        for (int x = 0; x < gridsize; x++)
        {
            // x축으로 긴 라인, FORE_LEFTRIGHT 블록들
            // 각 블록이 같은 y=0 라인에 있어서 서로 체인됨
            var matchtype = (x % 2 == 0) ? EMATCHTYPE.FORE_LEFTRIGHT : EMATCHTYPE.FORE_UPDOWN;
            var block = CreateTestBlock(x, 0, EBLOCKCOLORTYPE.RED, matchtype);
            _testgrid[(x, 0)] = block;
        }

        // 추가 행 생성 (FORE_UPDOWN이 영향을 미칠 수 있도록)
        for (int x = 0; x < gridsize; x++)
        {
            for (int y = 1; y < 3; y++)
            {
                var block = CreateTestBlock(x, y, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
                _testgrid[(x, y)] = block;
            }
        }

        var startblock = _testgrid[(0, 0)];
        var initialblocks = new List<UI_Match_Block> { startblock };

        var starttime = System.DateTime.Now;

        // Act: 긴 체인 반응이 합리적인 시간 내에 완료되어야 함
        var finaldestroylist = _processor.ProcessChainReaction(initialblocks, _testgrid);

        var elapsed = System.DateTime.Now - starttime;

        // Assert:
        // 1. 스택 오버플로우 없이 완료됨 (테스트가 완료되면 성공)
        Assert.IsNotNull(finaldestroylist, "Chain reaction should complete without crash");

        // 2. 결과 리스트가 비어있지 않음
        Assert.Greater(finaldestroylist.Count, 0, "Final destroy list should not be empty");

        // 3. 합리적인 시간 내에 완료 (10초 이내)
        Assert.Less(elapsed.TotalSeconds, 10.0,
            $"Chain reaction should complete within 10 seconds, but took {elapsed.TotalSeconds:F2}s");

        // 4. 결과가 그리드 크기를 초과하지 않음
        int maxblocks = gridsize * 3; // 50x3 = 150개
        Assert.LessOrEqual(finaldestroylist.Count, maxblocks,
            $"Result should not exceed grid size ({maxblocks}), but got {finaldestroylist.Count}");

        // 5. 중복 없음
        var uniqueblocks = finaldestroylist.Distinct().ToList();
        Assert.AreEqual(finaldestroylist.Count, uniqueblocks.Count,
            "No duplicate blocks should exist in final list");

        Debug.Log($"[ShouldHandleVeryLongChainReaction] Processed {finaldestroylist.Count} blocks in {elapsed.TotalMilliseconds:F2}ms");
    }

    // Phase 4.2: 복잡한 특수 블록 조합 테스트

    [Test]
    public void ShouldDestroyEntireGridWhenTwoFiveBlocksOfSameColorCombine()
    {
        // Arrange: FIVE + FIVE (같은 색상 RED) → 전체 그리드의 RED 블록 제거
        // 5x5 그리드에서 모든 블록을 RED로 생성
        // 두 개의 FIVE 블록을 배치하고 초기 블록으로 설정

        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                EMATCHTYPE matchtype = EMATCHTYPE.THREE;

                // 두 개의 FIVE 블록 배치
                if (x == 1 && y == 1) matchtype = EMATCHTYPE.FIVE; // 첫 번째 FIVE
                else if (x == 3 && y == 3) matchtype = EMATCHTYPE.FIVE; // 두 번째 FIVE

                var block = CreateTestBlock(x, y, EBLOCKCOLORTYPE.RED, matchtype);
                _testgrid[(x, y)] = block;
            }
        }

        var five1 = _testgrid[(1, 1)];
        var five2 = _testgrid[(3, 3)];

        // 두 FIVE 블록을 초기 블록으로 설정 (동시에 폭발)
        var initialblocks = new List<UI_Match_Block> { five1, five2 };

        // Act: 두 FIVE 블록이 같은 색상(RED)을 타겟으로 하므로 전체 그리드 제거
        var finaldestroylist = _processor.ProcessChainReaction(initialblocks, _testgrid);

        // Assert:
        // 1. 모든 블록이 제거됨 (5x5 = 25개)
        Assert.AreEqual(25, finaldestroylist.Count,
            $"All blocks should be destroyed when two FIVE blocks of same color combine, but got {finaldestroylist.Count}/25");

        // 2. 모든 그리드 위치의 블록이 최종 리스트에 포함됨
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                Assert.Contains(_testgrid[(x, y)], finaldestroylist,
                    $"Block at ({x}, {y}) should be in final destroy list");
            }
        }

        // 3. 중복 없음
        var uniqueblocks = finaldestroylist.Distinct().ToList();
        Assert.AreEqual(finaldestroylist.Count, uniqueblocks.Count, "No duplicate blocks");
    }

    [Test]
    public void ShouldDestroyBothColorsWhenTwoFiveBlocksOfDifferentColorsCombine()
    {
        // Arrange: FIVE + FIVE (다른 색상) → 두 색상 모두 제거
        // 5x5 그리드에서 RED와 BLUE 블록을 섞어서 배치
        // RED FIVE 블록과 BLUE FIVE 블록을 초기 블록으로 설정

        int redcount = 0;
        int bluecount = 0;

        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                EMATCHTYPE matchtype = EMATCHTYPE.THREE;
                EBLOCKCOLORTYPE color;

                // 첫 번째 FIVE 블록 (RED)
                if (x == 1 && y == 1)
                {
                    matchtype = EMATCHTYPE.FIVE;
                    color = EBLOCKCOLORTYPE.RED;
                }
                // 두 번째 FIVE 블록 (BLUE)
                else if (x == 3 && y == 3)
                {
                    matchtype = EMATCHTYPE.FIVE;
                    color = EBLOCKCOLORTYPE.BLUE;
                }
                // 체스판 패턴으로 RED와 BLUE 배치
                else if ((x + y) % 2 == 0)
                {
                    color = EBLOCKCOLORTYPE.RED;
                    redcount++;
                }
                else
                {
                    color = EBLOCKCOLORTYPE.BLUE;
                    bluecount++;
                }

                var block = CreateTestBlock(x, y, color, matchtype);
                _testgrid[(x, y)] = block;

                if (matchtype == EMATCHTYPE.FIVE && color == EBLOCKCOLORTYPE.RED) redcount++;
                if (matchtype == EMATCHTYPE.FIVE && color == EBLOCKCOLORTYPE.BLUE) bluecount++;
            }
        }

        var fivered = _testgrid[(1, 1)];
        var fiveblue = _testgrid[(3, 3)];

        // 두 FIVE 블록을 초기 블록으로 설정
        var initialblocks = new List<UI_Match_Block> { fivered, fiveblue };

        Debug.Log($"[ShouldDestroyBothColors] Grid composition: {redcount} RED, {bluecount} BLUE");

        // Act: 두 FIVE 블록이 각각 RED와 BLUE를 타겟으로 하므로 전체 그리드 제거
        var finaldestroylist = _processor.ProcessChainReaction(initialblocks, _testgrid);

        // Assert:
        // 1. 모든 블록이 제거됨 (5x5 = 25개)
        Assert.AreEqual(25, finaldestroylist.Count,
            $"All blocks should be destroyed when two FIVE blocks of different colors combine, but got {finaldestroylist.Count}/25");

        // 2. RED FIVE 블록과 BLUE FIVE 블록 모두 포함
        Assert.Contains(fivered, finaldestroylist, "RED FIVE block should be destroyed");
        Assert.Contains(fiveblue, finaldestroylist, "BLUE FIVE block should be destroyed");

        // 3. 모든 RED 블록과 BLUE 블록이 포함됨
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                Assert.Contains(_testgrid[(x, y)], finaldestroylist,
                    $"Block at ({x}, {y}) with color {_testgrid[(x, y)].GetBlockColorTypes()} should be destroyed");
            }
        }

        // 4. 중복 없음
        var uniqueblocks = finaldestroylist.Distinct().ToList();
        Assert.AreEqual(finaldestroylist.Count, uniqueblocks.Count, "No duplicate blocks");
    }

    [Test]
    public void ShouldCombineFiveAndForeLeftRightEffects()
    {
        // Arrange: FIVE + FORE_LEFTRIGHT → 라인 제거 + 색상 제거
        // 7x7 그리드에서 FIVE 블록과 FORE_LEFTRIGHT 블록을 같은 라인에 배치
        // FORE_LEFTRIGHT가 먼저 라인을 제거하고, 그 라인에 있는 FIVE 블록이 색상 제거 효과 발동

        int redcount = 0;
        int bluecount = 0;

        for (int x = 0; x < 7; x++)
        {
            for (int y = 0; y < 7; y++)
            {
                EMATCHTYPE matchtype = EMATCHTYPE.THREE;
                EBLOCKCOLORTYPE color;

                // FORE_LEFTRIGHT at (2, 3) - 가로 라인 제거
                if (x == 2 && y == 3)
                {
                    matchtype = EMATCHTYPE.FORE_LEFTRIGHT;
                    color = EBLOCKCOLORTYPE.RED;
                }
                // FIVE at (5, 3) - 같은 y=3 라인에 있어서 FORE에 의해 체인됨
                else if (x == 5 && y == 3)
                {
                    matchtype = EMATCHTYPE.FIVE;
                    color = EBLOCKCOLORTYPE.GREEN; // GREEN이지만 RED 색상을 상속받아야 함
                }
                // 나머지는 RED와 BLUE를 섞어서 배치
                else if (y < 3)
                {
                    color = EBLOCKCOLORTYPE.RED;
                    redcount++;
                }
                else
                {
                    color = EBLOCKCOLORTYPE.BLUE;
                    bluecount++;
                }

                var block = CreateTestBlock(x, y, color, matchtype);
                _testgrid[(x, y)] = block;

                if (matchtype == EMATCHTYPE.FORE_LEFTRIGHT && color == EBLOCKCOLORTYPE.RED) redcount++;
            }
        }

        var foreleftright = _testgrid[(2, 3)];
        var fiveblock = _testgrid[(5, 3)];

        // FORE_LEFTRIGHT로 시작
        var initialblocks = new List<UI_Match_Block> { foreleftright };

        Debug.Log($"[ShouldCombineFiveAndFore] Grid composition: {redcount} RED, {bluecount} BLUE");

        // Act: FORE_LEFTRIGHT가 y=3 라인을 제거 → FIVE 블록 체인 → FIVE가 RED 색상 상속 → 모든 RED 블록 제거
        var finaldestroylist = _processor.ProcessChainReaction(initialblocks, _testgrid);

        // Assert:
        // 1. FORE_LEFTRIGHT와 FIVE 블록 모두 포함
        Assert.Contains(foreleftright, finaldestroylist, "FORE_LEFTRIGHT block should be destroyed");
        Assert.Contains(fiveblock, finaldestroylist, "FIVE block should be destroyed");

        // 2. y=3 라인의 모든 블록이 포함됨 (FORE_LEFTRIGHT 효과)
        for (int x = 0; x < 7; x++)
        {
            Assert.Contains(_testgrid[(x, 3)], finaldestroylist,
                $"Block at ({x}, 3) should be destroyed by FORE_LEFTRIGHT");
        }

        // 3. 모든 RED 블록이 포함됨 (FIVE 블록이 RED 색상을 상속받아 색상 제거)
        for (int x = 0; x < 7; x++)
        {
            for (int y = 0; y < 3; y++) // y < 3은 모두 RED
            {
                Assert.Contains(_testgrid[(x, y)], finaldestroylist,
                    $"RED block at ({x}, {y}) should be destroyed by FIVE color effect");
            }
        }

        // 4. 최종 리스트 크기 확인: y=3 라인(7개) + RED 블록들(21개, y<3의 7x3) = 28개
        // 하지만 (2,3) FORE는 RED이므로 중복 없이 계산
        int expectedcount = 7 + 21; // y=3 라인 + y<3 RED 블록들
        Assert.AreEqual(expectedcount, finaldestroylist.Count,
            $"Expected {expectedcount} blocks (7 from line + 21 RED blocks), but got {finaldestroylist.Count}");

        // 5. BLUE 블록들은 영향받지 않아야 함 (y >= 4, y=3 라인 제외)
        for (int x = 0; x < 7; x++)
        {
            for (int y = 4; y < 7; y++) // y > 3은 모두 BLUE
            {
                Assert.IsFalse(finaldestroylist.Contains(_testgrid[(x, y)]),
                    $"BLUE block at ({x}, {y}) should NOT be destroyed");
            }
        }

        // 6. 중복 없음
        var uniqueblocks = finaldestroylist.Distinct().ToList();
        Assert.AreEqual(finaldestroylist.Count, uniqueblocks.Count, "No duplicate blocks");
    }

    [Test]
    public void ShouldCombineFiveAndCrossThreeEffects()
    {
        // Arrange: FIVE + CROSS_THREE → 영역 제거 + 색상 제거
        // 9x9 그리드에서 CROSS_THREE 블록과 FIVE 블록을 3x3 영역 내에 배치
        // CROSS_THREE가 먼저 3x3 영역을 제거하고, 그 영역에 있는 FIVE 블록이 색상 제거 효과 발동

        int redcount = 0;
        int bluecount = 0;

        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                EMATCHTYPE matchtype = EMATCHTYPE.THREE;
                EBLOCKCOLORTYPE color;

                // CROSS_THREE at (4, 4) - 중앙에 배치, 3x3 영역 제거
                if (x == 4 && y == 4)
                {
                    matchtype = EMATCHTYPE.CROSS_THREE;
                    color = EBLOCKCOLORTYPE.RED;
                }
                // FIVE at (5, 5) - CROSS_THREE 영역 내에 있어서 체인됨
                else if (x == 5 && y == 5)
                {
                    matchtype = EMATCHTYPE.FIVE;
                    color = EBLOCKCOLORTYPE.GREEN; // GREEN이지만 RED 색상을 상속받아야 함
                }
                // 상단 영역: RED 블록들 (y < 4)
                else if (y < 4)
                {
                    color = EBLOCKCOLORTYPE.RED;
                    redcount++;
                }
                // 하단 영역: BLUE 블록들 (y > 5)
                else if (y > 5)
                {
                    color = EBLOCKCOLORTYPE.BLUE;
                    bluecount++;
                }
                // 중간 영역 (y == 4, 5): 혼합
                else
                {
                    if (x < 4)
                    {
                        color = EBLOCKCOLORTYPE.RED;
                        redcount++;
                    }
                    else if (x > 5)
                    {
                        color = EBLOCKCOLORTYPE.BLUE;
                        bluecount++;
                    }
                    else
                    {
                        color = EBLOCKCOLORTYPE.RED;
                        redcount++;
                    }
                }

                var block = CreateTestBlock(x, y, color, matchtype);
                _testgrid[(x, y)] = block;

                if (matchtype == EMATCHTYPE.CROSS_THREE && color == EBLOCKCOLORTYPE.RED) redcount++;
            }
        }

        var crossthree = _testgrid[(4, 4)];
        var fiveblock = _testgrid[(5, 5)];

        // CROSS_THREE로 시작
        var initialblocks = new List<UI_Match_Block> { crossthree };

        Debug.Log($"[ShouldCombineFiveAndCrossThree] Grid composition: {redcount} RED, {bluecount} BLUE");

        // Act: CROSS_THREE가 3x3 영역 제거 → FIVE 블록 체인 → FIVE가 RED 색상 상속 → 모든 RED 블록 제거
        var finaldestroylist = _processor.ProcessChainReaction(initialblocks, _testgrid);

        // Assert:
        // 1. CROSS_THREE와 FIVE 블록 모두 포함
        Assert.Contains(crossthree, finaldestroylist, "CROSS_THREE block should be destroyed");
        Assert.Contains(fiveblock, finaldestroylist, "FIVE block should be destroyed");

        // 2. 3x3 영역의 모든 블록이 포함됨 (CROSS_THREE 효과)
        // CROSS_THREE at (4,4): 영역 범위 (3,3) ~ (5,5)
        for (int x = 3; x <= 5; x++)
        {
            for (int y = 3; y <= 5; y++)
            {
                Assert.Contains(_testgrid[(x, y)], finaldestroylist,
                    $"Block at ({x}, {y}) should be destroyed by CROSS_THREE area effect");
            }
        }

        // 3. 모든 RED 블록이 포함됨 (FIVE 블록이 RED 색상을 상속받아 색상 제거)
        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                if (_testgrid[(x, y)].GetBlockColorTypes() == EBLOCKCOLORTYPE.RED)
                {
                    Assert.Contains(_testgrid[(x, y)], finaldestroylist,
                        $"RED block at ({x}, {y}) should be destroyed by FIVE color effect");
                }
            }
        }

        // 4. BLUE 블록들은 영향받지 않아야 함
        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                if (_testgrid[(x, y)].GetBlockColorTypes() == EBLOCKCOLORTYPE.BLUE)
                {
                    Assert.IsFalse(finaldestroylist.Contains(_testgrid[(x, y)]),
                        $"BLUE block at ({x}, {y}) should NOT be destroyed");
                }
            }
        }

        // 5. 최종 리스트 크기 확인: RED 블록들 + FIVE 블록(GREEN)
        // FIVE 블록은 GREEN 색상이므로 RED 카운트에 포함되지 않음
        // 하지만 CROSS_THREE 효과로 파괴되어 최종 리스트에는 포함됨
        int totalredblocks = 0;
        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                if (_testgrid[(x, y)].GetBlockColorTypes() == EBLOCKCOLORTYPE.RED)
                {
                    totalredblocks++;
                }
            }
        }

        // 최종 = 모든 RED 블록 + FIVE 블록(GREEN, CROSS_THREE 영역에 포함)
        int expectedcount = totalredblocks + 1; // +1 for FIVE block (GREEN)
        Assert.AreEqual(expectedcount, finaldestroylist.Count,
            $"Expected {expectedcount} blocks ({totalredblocks} RED + 1 FIVE), but got {finaldestroylist.Count}");

        // 6. 중복 없음
        var uniqueblocks = finaldestroylist.Distinct().ToList();
        Assert.AreEqual(finaldestroylist.Count, uniqueblocks.Count, "No duplicate blocks");
    }

    [Test]
    public void ShouldCombineForeLeftRightAndForeUpDownEffects()
    {
        // Arrange: FORE_LEFTRIGHT + FORE_UPDOWN → 라인 교차 지점 전체 제거
        // 5x5 그리드에서 FORE_LEFTRIGHT와 FORE_UPDOWN이 교차하는 패턴
        // 두 블록이 동시에 파괴되면 십자 형태로 블록들이 제거됨

        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                EMATCHTYPE matchtype = EMATCHTYPE.THREE;

                // FORE_LEFTRIGHT at (2, 2) - 가로 라인 제거
                if (x == 2 && y == 2)
                {
                    matchtype = EMATCHTYPE.FORE_LEFTRIGHT;
                }
                // FORE_UPDOWN at (2, 3) - 세로 라인 제거, FORE_LEFTRIGHT와 인접
                else if (x == 2 && y == 3)
                {
                    matchtype = EMATCHTYPE.FORE_UPDOWN;
                }

                var block = CreateTestBlock(x, y, EBLOCKCOLORTYPE.RED, matchtype);
                _testgrid[(x, y)] = block;
            }
        }

        var foreleftright = _testgrid[(2, 2)];
        var foreupdown = _testgrid[(2, 3)];

        // 두 FORE 블록을 동시에 시작
        var initialblocks = new List<UI_Match_Block> { foreleftright, foreupdown };

        // Act: FORE_LEFTRIGHT가 y=2 라인 제거, FORE_UPDOWN이 x=2 라인 제거
        var finaldestroylist = _processor.ProcessChainReaction(initialblocks, _testgrid);

        // Assert:
        // 1. 두 FORE 블록 모두 포함
        Assert.Contains(foreleftright, finaldestroylist, "FORE_LEFTRIGHT block should be destroyed");
        Assert.Contains(foreupdown, finaldestroylist, "FORE_UPDOWN block should be destroyed");

        // 2. y=2 라인의 모든 블록이 포함됨 (FORE_LEFTRIGHT 효과)
        for (int x = 0; x < 5; x++)
        {
            Assert.Contains(_testgrid[(x, 2)], finaldestroylist,
                $"Block at ({x}, 2) should be destroyed by FORE_LEFTRIGHT");
        }

        // 3. x=2 라인의 모든 블록이 포함됨 (FORE_UPDOWN 효과)
        for (int y = 0; y < 5; y++)
        {
            Assert.Contains(_testgrid[(2, y)], finaldestroylist,
                $"Block at (2, {y}) should be destroyed by FORE_UPDOWN");
        }

        // 4. 최종 리스트 크기 확인: 십자 형태 (중복 제거)
        // y=2 라인: 5개 블록
        // x=2 라인: 5개 블록
        // 교차점 (2,2): 중복 1개
        // 합계: 5 + 5 - 1 = 9개
        Assert.AreEqual(9, finaldestroylist.Count,
            $"Expected 9 blocks (5 horizontal + 5 vertical - 1 overlap), but got {finaldestroylist.Count}");

        // 5. 교차점이 중복되지 않아야 함
        var intersectionblock = _testgrid[(2, 2)];
        var intersectioncount = finaldestroylist.Count(b => b == intersectionblock);
        Assert.AreEqual(1, intersectioncount, "Intersection block should appear exactly once");

        // 6. 십자 형태 외의 블록은 영향받지 않아야 함
        Assert.IsFalse(finaldestroylist.Contains(_testgrid[(0, 0)]), "Corner block should NOT be destroyed");
        Assert.IsFalse(finaldestroylist.Contains(_testgrid[(4, 4)]), "Opposite corner should NOT be destroyed");
        Assert.IsFalse(finaldestroylist.Contains(_testgrid[(1, 1)]), "Non-cross block should NOT be destroyed");

        // 7. 중복 없음
        var uniqueblocks = finaldestroylist.Distinct().ToList();
        Assert.AreEqual(finaldestroylist.Count, uniqueblocks.Count, "No duplicate blocks");
    }

    [Test]
    public void ShouldHandleOverlappingCrossThreeAreas()
    {
        // Arrange: CROSS_THREE + CROSS_THREE (인접) → 중복 영역 한 번만 제거
        // 7x7 그리드에서 두 개의 CROSS_THREE 블록을 인접하게 배치
        // 두 블록의 3x3 영역이 겹치는 부분은 한 번만 카운트되어야 함

        for (int x = 0; x < 7; x++)
        {
            for (int y = 0; y < 7; y++)
            {
                EMATCHTYPE matchtype = EMATCHTYPE.THREE;

                // 첫 번째 CROSS_THREE at (2, 3) - 영역: (1,2) ~ (3,4)
                if (x == 2 && y == 3)
                {
                    matchtype = EMATCHTYPE.CROSS_THREE;
                }
                // 두 번째 CROSS_THREE at (4, 3) - 영역: (3,2) ~ (5,4)
                // 첫 번째와 x=3 열에서 겹침
                else if (x == 4 && y == 3)
                {
                    matchtype = EMATCHTYPE.CROSS_THREE;
                }

                var block = CreateTestBlock(x, y, EBLOCKCOLORTYPE.RED, matchtype);
                _testgrid[(x, y)] = block;
            }
        }

        var crossthree1 = _testgrid[(2, 3)];
        var crossthree2 = _testgrid[(4, 3)];

        // 두 CROSS_THREE 블록을 동시에 시작
        var initialblocks = new List<UI_Match_Block> { crossthree1, crossthree2 };

        Debug.Log("[ShouldHandleOverlappingCrossThree] Testing overlapping CROSS_THREE areas");

        // Act: 두 CROSS_THREE 블록이 각각 3x3 영역을 제거하되, 겹치는 부분은 중복 제거
        var finaldestroylist = _processor.ProcessChainReaction(initialblocks, _testgrid);

        // Assert:
        // 1. 두 CROSS_THREE 블록 모두 포함
        Assert.Contains(crossthree1, finaldestroylist, "First CROSS_THREE block should be destroyed");
        Assert.Contains(crossthree2, finaldestroylist, "Second CROSS_THREE block should be destroyed");

        // 2. 첫 번째 CROSS_THREE 영역: (1,2) ~ (3,4) = 9개 블록
        for (int x = 1; x <= 3; x++)
        {
            for (int y = 2; y <= 4; y++)
            {
                Assert.Contains(_testgrid[(x, y)], finaldestroylist,
                    $"Block at ({x}, {y}) should be destroyed by first CROSS_THREE");
            }
        }

        // 3. 두 번째 CROSS_THREE 영역: (3,2) ~ (5,4) = 9개 블록
        for (int x = 3; x <= 5; x++)
        {
            for (int y = 2; y <= 4; y++)
            {
                Assert.Contains(_testgrid[(x, y)], finaldestroylist,
                    $"Block at ({x}, {y}) should be destroyed by second CROSS_THREE");
            }
        }

        // 4. 겹치는 영역: x=3, y=(2,3,4) = 3개 블록
        // 이 블록들은 한 번만 카운트되어야 함
        var overlapblocks = new List<UI_Match_Block>
        {
            _testgrid[(3, 2)],
            _testgrid[(3, 3)],
            _testgrid[(3, 4)]
        };

        foreach (var block in overlapblocks)
        {
            var count = finaldestroylist.Count(b => b == block);
            Assert.AreEqual(1, count, $"Overlap block at ({block.GetPoint().x}, {block.GetPoint().y}) should appear exactly once");
        }

        // 5. 최종 리스트 크기 확인
        // 첫 번째 영역: 9개
        // 두 번째 영역: 9개
        // 겹치는 영역: 3개 (중복)
        // 합계: 9 + 9 - 3 = 15개
        Assert.AreEqual(15, finaldestroylist.Count,
            $"Expected 15 blocks (9 + 9 - 3 overlap), but got {finaldestroylist.Count}");

        // 6. 영역 외의 블록은 영향받지 않아야 함
        Assert.IsFalse(finaldestroylist.Contains(_testgrid[(0, 0)]), "Corner block should NOT be destroyed");
        Assert.IsFalse(finaldestroylist.Contains(_testgrid[(6, 6)]), "Opposite corner should NOT be destroyed");
        Assert.IsFalse(finaldestroylist.Contains(_testgrid[(0, 3)]), "Left edge block should NOT be destroyed");
        Assert.IsFalse(finaldestroylist.Contains(_testgrid[(6, 3)]), "Right edge block should NOT be destroyed");

        // 7. 중복 없음
        var uniqueblocks = finaldestroylist.Distinct().ToList();
        Assert.AreEqual(finaldestroylist.Count, uniqueblocks.Count,
            $"No duplicate blocks should exist. Total: {finaldestroylist.Count}, Unique: {uniqueblocks.Count}");
    }

    // Phase 4.3: 색상 상속 검증

    [Test]
    public void ShouldInheritColorFromForeBlockWhenFiveBlockIsChained()
    {
        // Arrange: FIVE 블록이 FORE 블록과 연쇄될 때 FORE 블록 색상을 상속해야 함
        // 5x5 그리드에서 FORE_LEFTRIGHT가 FIVE 블록을 체인시키고,
        // FIVE 블록이 FORE 블록의 색상을 상속받아 같은 색상의 블록들을 제거해야 함

        // RED 블록들 (FIVE가 타겟으로 삼을 색상)
        var redpositions = new List<(int, int)> { (0, 0), (0, 1), (0, 2), (1, 0), (1, 1) };
        foreach (var pos in redpositions)
        {
            var block = CreateTestBlock(pos.Item1, pos.Item2, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
            _testgrid[pos] = block;
        }

        // BLUE 블록들 (영향받지 않아야 함)
        var bluepositions = new List<(int, int)> { (3, 0), (3, 1), (3, 2), (4, 0), (4, 1) };
        foreach (var pos in bluepositions)
        {
            var block = CreateTestBlock(pos.Item1, pos.Item2, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
            _testgrid[pos] = block;
        }

        // 중앙에 FORE_LEFTRIGHT (RED 색상)
        var foreblock = CreateTestBlock(2, 2, EBLOCKCOLORTYPE.RED, EMATCHTYPE.FORE_LEFTRIGHT);
        _testgrid[(2, 2)] = foreblock;

        // FORE 블록과 같은 라인에 FIVE 블록 배치 (GREEN 색상이지만 RED를 상속받아야 함)
        var fiveblock = CreateTestBlock(4, 2, EBLOCKCOLORTYPE.GREEN, EMATCHTYPE.FIVE);
        _testgrid[(4, 2)] = fiveblock;

        // 나머지 블록들 채우기
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                if (!_testgrid.ContainsKey((x, y)))
                {
                    var block = CreateTestBlock(x, y, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
                    _testgrid[(x, y)] = block;
                }
            }
        }

        var initialblocks = new List<UI_Match_Block> { foreblock };

        Debug.Log("[ShouldInheritColorFromFore] Testing FIVE color inheritance from FORE block");

        // Act: FORE_LEFTRIGHT가 y=2 라인 제거 → FIVE 블록 체인 → FIVE가 RED 색상 상속 → 모든 RED 블록 제거
        var finaldestroylist = _processor.ProcessChainReaction(initialblocks, _testgrid);

        // Assert:
        // 1. FORE와 FIVE 블록 모두 포함
        Assert.Contains(foreblock, finaldestroylist, "FORE_LEFTRIGHT block should be destroyed");
        Assert.Contains(fiveblock, finaldestroylist, "FIVE block should be destroyed");

        // 2. y=2 라인의 모든 블록이 포함됨 (FORE_LEFTRIGHT 효과)
        for (int x = 0; x < 5; x++)
        {
            Assert.Contains(_testgrid[(x, 2)], finaldestroylist,
                $"Block at ({x}, 2) should be destroyed by FORE_LEFTRIGHT");
        }

        // 3. 모든 RED 블록이 포함됨 (FIVE 블록이 RED 색상을 상속받아 색상 제거)
        foreach (var pos in redpositions)
        {
            Assert.Contains(_testgrid[pos], finaldestroylist,
                $"RED block at ({pos.Item1}, {pos.Item2}) should be destroyed by FIVE color effect");
        }

        // 4. FIVE 블록이 GREEN 색상임에도 불구하고 RED 색상을 상속받았는지 확인
        // GREEN 색상 블록(FIVE 자신 외)은 영향받지 않아야 함
        Assert.AreEqual(EBLOCKCOLORTYPE.GREEN, fiveblock.GetBlockColorTypes(),
            "FIVE block should still have GREEN color");

        // 5. y=2 라인 외의 BLUE 블록들 중 일부는 영향받지 않아야 함
        // (y=2 라인의 BLUE 블록들은 FORE 효과로 제거됨)
        var untouchedblueblocks = bluepositions.Where(pos => pos.Item2 != 2).ToList();
        foreach (var pos in untouchedblueblocks)
        {
            Assert.IsFalse(finaldestroylist.Contains(_testgrid[pos]),
                $"BLUE block at ({pos.Item1}, {pos.Item2}) should NOT be destroyed");
        }

        // 6. 중복 없음
        var uniqueblocks = finaldestroylist.Distinct().ToList();
        Assert.AreEqual(finaldestroylist.Count, uniqueblocks.Count, "No duplicate blocks");
    }

    [Test]
    public void ShouldUseRegularBlockColorWhenFiveBlockMatchesWithRegularBlocks()
    {
        // Arrange: FIVE 블록이 일반 블록과 매치될 때 일반 블록 색상 사용
        // FIVE 블록(GREEN)이 일반 블록들(RED)과 함께 초기 블록 리스트에 포함되면,
        // FIVE 블록이 일반 블록의 색상(RED)을 타겟으로 삼아야 함

        // RED 블록들 (FIVE가 타겟으로 삼을 색상)
        var redpositions = new List<(int, int)> { (0, 0), (0, 1), (0, 2), (1, 0), (1, 1) };
        foreach (var pos in redpositions)
        {
            var block = CreateTestBlock(pos.Item1, pos.Item2, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
            _testgrid[pos] = block;
        }

        // BLUE 블록들 (영향받지 않아야 함)
        var bluepositions = new List<(int, int)> { (3, 0), (3, 1), (3, 2), (4, 0), (4, 1) };
        foreach (var pos in bluepositions)
        {
            var block = CreateTestBlock(pos.Item1, pos.Item2, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
            _testgrid[pos] = block;
        }

        // FIVE 블록 (GREEN 색상)
        var fiveblock = CreateTestBlock(2, 2, EBLOCKCOLORTYPE.GREEN, EMATCHTYPE.FIVE);
        _testgrid[(2, 2)] = fiveblock;

        // 일반 RED 블록 (FIVE와 함께 매치)
        var regularredblock1 = CreateTestBlock(2, 1, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        _testgrid[(2, 1)] = regularredblock1;

        var regularredblock2 = CreateTestBlock(2, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        _testgrid[(2, 0)] = regularredblock2;

        // 나머지 블록들 채우기
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                if (!_testgrid.ContainsKey((x, y)))
                {
                    var block = CreateTestBlock(x, y, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
                    _testgrid[(x, y)] = block;
                }
            }
        }

        // FIVE 블록과 일반 RED 블록들을 함께 초기 블록으로 설정
        var initialblocks = new List<UI_Match_Block> { fiveblock, regularredblock1, regularredblock2 };

        Debug.Log("[ShouldUseRegularBlockColor] Testing FIVE block using regular block color");

        // Act: FIVE 블록이 일반 RED 블록들과 함께 매치되어 RED 색상을 타겟으로 삼아야 함
        var finaldestroylist = _processor.ProcessChainReaction(initialblocks, _testgrid);

        // Assert:
        // 1. FIVE 블록과 일반 블록들 모두 포함
        Assert.Contains(fiveblock, finaldestroylist, "FIVE block should be destroyed");
        Assert.Contains(regularredblock1, finaldestroylist, "Regular RED block 1 should be destroyed");
        Assert.Contains(regularredblock2, finaldestroylist, "Regular RED block 2 should be destroyed");

        // 2. 모든 RED 블록이 포함됨 (FIVE 블록이 일반 블록의 RED 색상을 사용)
        foreach (var pos in redpositions)
        {
            Assert.Contains(_testgrid[pos], finaldestroylist,
                $"RED block at ({pos.Item1}, {pos.Item2}) should be destroyed by FIVE color effect");
        }

        // 3. BLUE 블록들은 영향받지 않아야 함
        foreach (var pos in bluepositions)
        {
            Assert.IsFalse(finaldestroylist.Contains(_testgrid[pos]),
                $"BLUE block at ({pos.Item1}, {pos.Item2}) should NOT be destroyed");
        }

        // 4. 최종 리스트 크기: 모든 RED 블록 (5개 + FIVE 블록 1개 + 일반 2개) = 8개
        int totalredcount = redpositions.Count + 2; // redpositions + 2 regular RED blocks
        int expectedcount = totalredcount + 1; // +1 for FIVE block (GREEN but in destroy list)
        Assert.AreEqual(expectedcount, finaldestroylist.Count,
            $"Expected {expectedcount} blocks ({totalredcount} RED + 1 FIVE), but got {finaldestroylist.Count}");

        // 5. 중복 없음
        var uniqueblocks = finaldestroylist.Distinct().ToList();
        Assert.AreEqual(finaldestroylist.Count, uniqueblocks.Count, "No duplicate blocks");
    }

    [Test]
    public void ShouldUseOwnColorWhenTwoFiveBlocksOfDifferentColorsMatch()
    {
        // Arrange: FIVE 블록이 다른 FIVE 블록과 매치될 때 각자 자신의 색상 사용
        // 두 FIVE 블록이 초기 블록 리스트에 함께 있을 때,
        // 각 FIVE 블록이 자신의 색상을 사용하여 해당 색상 블록들을 제거

        // RED 블록들 (첫 번째 FIVE(RED)가 타겟으로 삼을 색상)
        var redpositions = new List<(int, int)> { (0, 0), (0, 1), (0, 2) };
        foreach (var pos in redpositions)
        {
            var block = CreateTestBlock(pos.Item1, pos.Item2, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
            _testgrid[pos] = block;
        }

        // BLUE 블록들 (두 번째 FIVE(BLUE)가 타겟으로 삼을 색상)
        var bluepositions = new List<(int, int)> { (4, 0), (4, 1), (4, 2) };
        foreach (var pos in bluepositions)
        {
            var block = CreateTestBlock(pos.Item1, pos.Item2, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
            _testgrid[pos] = block;
        }

        // 첫 번째 FIVE 블록 (RED 색상) - RED 블록들을 제거할 것
        var fiveblock1 = CreateTestBlock(2, 2, EBLOCKCOLORTYPE.RED, EMATCHTYPE.FIVE);
        _testgrid[(2, 2)] = fiveblock1;

        // 두 번째 FIVE 블록 (BLUE 색상) - BLUE 블록들을 제거할 것
        var fiveblock2 = CreateTestBlock(3, 2, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.FIVE);
        _testgrid[(3, 2)] = fiveblock2;

        // 나머지 블록들 채우기 (GREEN으로)
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                if (!_testgrid.ContainsKey((x, y)))
                {
                    var block = CreateTestBlock(x, y, EBLOCKCOLORTYPE.GREEN, EMATCHTYPE.THREE);
                    _testgrid[(x, y)] = block;
                }
            }
        }

        // 두 FIVE 블록을 초기 블록으로 설정
        var initialblocks = new List<UI_Match_Block> { fiveblock1, fiveblock2 };

        Debug.Log("[ShouldUseOwnColor] Testing two FIVE blocks with different colors");

        // Act: 두 FIVE 블록이 각자 자신의 색상을 사용
        var finaldestroylist = _processor.ProcessChainReaction(initialblocks, _testgrid);

        // Assert:
        // 1. 두 FIVE 블록 모두 포함
        Assert.Contains(fiveblock1, finaldestroylist, "First FIVE block (RED) should be destroyed");
        Assert.Contains(fiveblock2, finaldestroylist, "Second FIVE block (BLUE) should be destroyed");

        // 2. 모든 RED 블록이 제거됨 (첫 번째 FIVE의 효과)
        foreach (var pos in redpositions)
        {
            Assert.Contains(_testgrid[pos], finaldestroylist,
                $"RED block at ({pos.Item1}, {pos.Item2}) should be destroyed by first FIVE");
        }

        // 3. 모든 BLUE 블록이 제거됨 (두 번째 FIVE의 효과)
        foreach (var pos in bluepositions)
        {
            Assert.Contains(_testgrid[pos], finaldestroylist,
                $"BLUE block at ({pos.Item1}, {pos.Item2}) should be destroyed by second FIVE");
        }

        // 4. GREEN 블록들은 영향받지 않아야 함
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                if (_testgrid.ContainsKey((x, y)) &&
                    _testgrid[(x, y)].GetBlockColorTypes() == EBLOCKCOLORTYPE.GREEN)
                {
                    Assert.IsFalse(finaldestroylist.Contains(_testgrid[(x, y)]),
                        $"GREEN block at ({x}, {y}) should NOT be destroyed");
                }
            }
        }

        // 5. 최종 리스트 크기: 2 FIVE + 3 RED + 3 BLUE = 8개
        int expectedcount = 2 + redpositions.Count + bluepositions.Count;
        Assert.AreEqual(expectedcount, finaldestroylist.Count,
            $"Expected {expectedcount} blocks (2 FIVE + {redpositions.Count} RED + {bluepositions.Count} BLUE), but got {finaldestroylist.Count}");

        // 6. 중복 없음
        var uniqueblocks = finaldestroylist.Distinct().ToList();
        Assert.AreEqual(finaldestroylist.Count, uniqueblocks.Count, "No duplicate blocks");
    }

    [Test]
    public void ShouldUseOwnColorWhenFiveBlockHasNoRegularBlocksToInheritFrom()
    {
        // Arrange: FIVE 블록이 단독으로 있을 때 자체 색상 사용
        // FIVE 블록만 초기 블록 리스트에 있고, 일반 블록(THREE)이 없을 때
        // FIVE 블록이 자신의 색상을 사용하여 해당 색상 블록들을 제거

        // RED 블록들 (FIVE(RED)가 타겟으로 삼을 색상)
        var redpositions = new List<(int, int)> { (0, 0), (0, 1), (0, 2), (1, 0), (1, 1) };
        foreach (var pos in redpositions)
        {
            var block = CreateTestBlock(pos.Item1, pos.Item2, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
            _testgrid[pos] = block;
        }

        // BLUE 블록들 (영향받지 않아야 함)
        var bluepositions = new List<(int, int)> { (3, 0), (3, 1), (3, 2), (4, 0), (4, 1) };
        foreach (var pos in bluepositions)
        {
            var block = CreateTestBlock(pos.Item1, pos.Item2, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
            _testgrid[pos] = block;
        }

        // FIVE 블록 (RED 색상) - 단독으로 초기 블록
        var fiveblock = CreateTestBlock(2, 2, EBLOCKCOLORTYPE.RED, EMATCHTYPE.FIVE);
        _testgrid[(2, 2)] = fiveblock;

        // 나머지 블록들 채우기
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                if (!_testgrid.ContainsKey((x, y)))
                {
                    var block = CreateTestBlock(x, y, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
                    _testgrid[(x, y)] = block;
                }
            }
        }

        // FIVE 블록만 초기 블록으로 설정 (일반 블록 없음)
        var initialblocks = new List<UI_Match_Block> { fiveblock };

        Debug.Log("[ShouldUseOwnColor] Testing FIVE block using own color when no regular blocks");

        // Act: FIVE 블록이 일반 블록 없이 단독으로 있으므로 자신의 색상(RED)을 사용
        var finaldestroylist = _processor.ProcessChainReaction(initialblocks, _testgrid);

        // Assert:
        // 1. FIVE 블록 포함
        Assert.Contains(fiveblock, finaldestroylist, "FIVE block should be destroyed");

        // 2. 모든 RED 블록이 포함됨 (FIVE 블록이 자신의 RED 색상을 사용)
        foreach (var pos in redpositions)
        {
            Assert.Contains(_testgrid[pos], finaldestroylist,
                $"RED block at ({pos.Item1}, {pos.Item2}) should be destroyed by FIVE color effect");
        }

        // 3. BLUE 블록들은 영향받지 않아야 함
        foreach (var pos in bluepositions)
        {
            Assert.IsFalse(finaldestroylist.Contains(_testgrid[pos]),
                $"BLUE block at ({pos.Item1}, {pos.Item2}) should NOT be destroyed");
        }

        // 4. 최종 리스트 크기: RED 블록들 (5개) + FIVE 블록 (1개, RED 색상) = 6개
        int expectedcount = redpositions.Count + 1; // +1 for FIVE block itself
        Assert.AreEqual(expectedcount, finaldestroylist.Count,
            $"Expected {expectedcount} blocks ({redpositions.Count} RED + 1 FIVE), but got {finaldestroylist.Count}");

        // 5. 중복 없음
        var uniqueblocks = finaldestroylist.Distinct().ToList();
        Assert.AreEqual(finaldestroylist.Count, uniqueblocks.Count, "No duplicate blocks");
    }

    // Phase 4.4: 빈 그리드 및 null 처리

    [Test]
    public void ShouldReturnEmptyListWhenEmptyBlockListProvided()
    {
        // Arrange: 빈 블록 리스트를 초기 블록으로 전달
        // 3x3 그리드는 생성하지만 빈 초기 블록 리스트 사용
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                var block = CreateTestBlock(x, y, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
                _testgrid[(x, y)] = block;
            }
        }

        var emptylist = new List<UI_Match_Block>(); // 빈 리스트

        // Act: 빈 블록 리스트로 체인 반응 처리
        var finaldestroylist = _processor.ProcessChainReaction(emptylist, _testgrid);

        // Assert: 빈 리스트를 반환해야 함
        Assert.IsNotNull(finaldestroylist, "Result should not be null");
        Assert.AreEqual(0, finaldestroylist.Count, "Empty input should return empty list");
    }

    [Test]
    public void ShouldReturnEmptyListWhenOnlyNullBlocksProvided()
    {
        // Arrange: null 블록만 포함된 리스트를 초기 블록으로 전달
        // 3x3 그리드 생성
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                var block = CreateTestBlock(x, y, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
                _testgrid[(x, y)] = block;
            }
        }

        // null 블록만 포함된 리스트 생성
        var nullblocklist = new List<UI_Match_Block> { null, null, null };

        // Act: null 블록 리스트로 체인 반응 처리
        var finaldestroylist = _processor.ProcessChainReaction(nullblocklist, _testgrid);

        // Assert: 빈 리스트를 반환해야 함 (null 블록은 무시됨)
        Assert.IsNotNull(finaldestroylist, "Result should not be null");
        Assert.AreEqual(0, finaldestroylist.Count, "Null blocks should be ignored and return empty list");
    }

    [Test]
    public void ShouldReturnInitialBlocksWhenGridIsNull()
    {
        // Arrange: null 그리드와 일반 블록 리스트 전달
        // 테스트용 블록 생성 (그리드에 속하지 않은 독립 블록)
        var block1 = CreateTestBlock(0, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        var block2 = CreateTestBlock(1, 0, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.FORE_LEFTRIGHT);

        var initialblocks = new List<UI_Match_Block> { block1, block2 };

        Dictionary<(int, int), UI_Match_Block> nullgrid = null;

        // Act: null 그리드로 체인 반응 처리
        // 예외 발생하지 않고, 연쇄 반응 없이 초기 블록만 반환해야 함
        var finaldestroylist = _processor.ProcessChainReaction(initialblocks, nullgrid);

        // Assert: 예외 발생하지 않고 초기 블록만 반환
        Assert.IsNotNull(finaldestroylist, "Result should not be null");
        Assert.AreEqual(2, finaldestroylist.Count, "Should return only initial blocks when grid is null");
        Assert.Contains(block1, finaldestroylist, "Should contain first initial block");
        Assert.Contains(block2, finaldestroylist, "Should contain second initial block");
    }

    [Test]
    public void ShouldReturnInitialBlocksWhenGridIsEmpty()
    {
        // Arrange: 빈 그리드(블록이 하나도 없음)와 특수 블록 리스트 전달
        var emptygrid = new Dictionary<(int, int), UI_Match_Block>();

        // 테스트용 특수 블록 생성 (그리드에 속하지 않은 독립 블록)
        var foreblock = CreateTestBlock(2, 2, EBLOCKCOLORTYPE.RED, EMATCHTYPE.FORE_LEFTRIGHT);
        var fiveblock = CreateTestBlock(3, 3, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.FIVE);
        var crossblock = CreateTestBlock(4, 4, EBLOCKCOLORTYPE.GREEN, EMATCHTYPE.CROSS_THREE);

        var initialblocks = new List<UI_Match_Block> { foreblock, fiveblock, crossblock };

        // Act: 빈 그리드로 체인 반응 처리
        // 연쇄 반응이 일어날 블록이 없으므로 초기 블록만 반환해야 함
        var finaldestroylist = _processor.ProcessChainReaction(initialblocks, emptygrid);

        // Assert: 연쇄 반응 없이 초기 블록만 반환
        Assert.IsNotNull(finaldestroylist, "Result should not be null");
        Assert.AreEqual(3, finaldestroylist.Count, "Should return only initial blocks when grid is empty");
        Assert.Contains(foreblock, finaldestroylist, "Should contain FORE block");
        Assert.Contains(fiveblock, finaldestroylist, "Should contain FIVE block");
        Assert.Contains(crossblock, finaldestroylist, "Should contain CROSS block");

        // 중복 없음 확인
        var uniqueblocks = finaldestroylist.Distinct().ToList();
        Assert.AreEqual(finaldestroylist.Count, uniqueblocks.Count, "No duplicate blocks");
    }
}