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
        
        // SetBlockData 메서드가 있다고 가정하고 설정
        // 실제 구현에서는 block의 내부 필드를 직접 설정해야 할 수 있습니다
        var blockdata = new St_BlockData { _colortypes = color, _blocktypes = EBLOCKTYPE.NORMAL };
        block.SetBlockData(blockdata, x, y, matchtype);
        
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
        
        // Act
        var finaldestroylist = _processor.ProcessChainReaction(initialblocks, _testgrid);
        
        // Assert: (1,1) 블록은 두 FORE 블록 모두의 영향을 받지만 한 번만 포함되어야 함
        var intersectionblock = _testgrid[(1, 1)];
        var intersectioncount = finaldestroylist.Count(block => block == intersectionblock);
        
        Assert.AreEqual(1, intersectioncount, "Intersection block should appear only once in final destroy list");
        
        // 모든 블록이 포함되어야 함 (3x3 = 9개)
        Assert.AreEqual(9, finaldestroylist.Count);
        
        // 중복이 없어야 함
        var uniqueblocks = finaldestroylist.Distinct().ToList();
        Assert.AreEqual(finaldestroylist.Count, uniqueblocks.Count, "No duplicate blocks should exist in final destroy list");
    }

    [Test]
    public void ShouldHandleCircularChainReferences()
    {
        // Arrange: 순환 참조가 발생할 수 있는 상황
        // 두 개의 FORE 블록이 서로를 활성화시킬 수 있는 배치
        // 3x3 그리드에서 FORE_LEFTRIGHT와 FORE_UPDOWN가 교차하는 배치
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                EMATCHTYPE matchtype = EMATCHTYPE.THREE;
                if (x == 1 && y == 1) matchtype = EMATCHTYPE.FORE_LEFTRIGHT; // 가운데 가로 FORE
                else if (x == 1 && y == 2) matchtype = EMATCHTYPE.FORE_UPDOWN; // 아래쪽 세로 FORE
                
                var block = CreateTestBlock(x, y, EBLOCKCOLORTYPE.RED, matchtype);
                _testgrid[(x, y)] = block;
            }
        }

        // 첫 번째 FORE 블록만으로 시작
        var initialblocks = new List<UI_Match_Block> { _testgrid[(1, 1)] }; // FORE_LEFTRIGHT
        
        // Act - 무한 루프가 발생하지 않아야 함
        var finaldestroylist = _processor.ProcessChainReaction(initialblocks, _testgrid);
        
        // Assert: 모든 블록이 포함되고 무한 루프가 발생하지 않아야 함
        Assert.AreEqual(9, finaldestroylist.Count); // 3x3 = 9개 모든 블록
        
        // 두 FORE 블록 모두 포함되어야 함
        Assert.Contains(_testgrid[(1, 1)], finaldestroylist); // FORE_LEFTRIGHT
        Assert.Contains(_testgrid[(1, 2)], finaldestroylist); // FORE_UPDOWN
        
        // 중복이 없어야 함 (순환 참조로 인한 중복 처리 방지)
        var uniqueblocks = finaldestroylist.Distinct().ToList();
        Assert.AreEqual(finaldestroylist.Count, uniqueblocks.Count, "Circular references should not cause duplicate processing");
    }
}