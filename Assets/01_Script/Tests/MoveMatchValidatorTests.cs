using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[TestFixture]
public class MoveMatchValidatorTests
{
    private MoveMatchValidator _movematchvalidator;
    private MatchDetector _matchdetector;
    private Dictionary<(int, int), UI_Match_Block> _testgrid;

    [SetUp]
    public void SetUp()
    {
        _matchdetector = new MatchDetector();
        _movematchvalidator = new MoveMatchValidator(_matchdetector);
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

    [Test]
    public void ShouldValidateMoveCreatesMatch()
    {
        // Arrange: Create a grid where swapping (0,0) and (1,0) creates a match
        // (0,0)=RED, (1,0)=BLUE, (2,0)=RED, (3,0)=RED
        // After swap: (0,0)=BLUE, (1,0)=RED, (2,0)=RED, (3,0)=RED -> 3-match at (1,0)
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        CreateBlockAt(3, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);

        // Act: Validate move between (0,0) and (1,0)
        var result = _movematchvalidator.ValidateMove(_testgrid, (0, 0), (1, 0));

        // Assert: Should return true (move creates match)
        Assert.IsTrue(result);
    }

    [Test]
    public void ShouldHandleFiveBlockSpecialCase()
    {
        // Arrange: FIVE 블록은 어떤 블록과도 교환 가능 (항상 매치 생성)
        // (0,0)=FIVE, (1,0)=RED (일반 블록)
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.FIVE);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);

        // Act: FIVE 블록과 일반 블록 교환
        var result = _movematchvalidator.ValidateMove(_testgrid, (0, 0), (1, 0));

        // Assert: FIVE 블록은 항상 매치를 생성해야 함
        Assert.IsTrue(result);
    }

    [Test]
    public void ShouldReturnMatchResultsForBothBlocks()
    {
        // Arrange: 두 블록 모두 매치를 생성하는 경우
        // (0,0)=RED, (1,0)=BLUE, (2,0)=RED, (3,0)=RED
        // (0,1)=BLUE, (1,1)=RED, (2,1)=BLUE
        // (0,2)=BLUE
        // 교환 후: (0,0)에서 세로 매치, (1,0)에서 가로 매치
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        CreateBlockAt(3, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        CreateBlockAt(0, 1, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
        CreateBlockAt(1, 1, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        CreateBlockAt(2, 1, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
        CreateBlockAt(0, 2, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);

        // Act: (0,0) RED와 (1,0) BLUE 교환
        // 결과: (0,0)=BLUE는 세로 매치, (1,0)=RED는 가로 매치
        var result = _movematchvalidator.ValidateMove(_testgrid, (0, 0), (1, 0));

        // Assert: 매치가 생성되어야 함
        Assert.IsTrue(result);
    }

    // Phase 5.1: 잘못된 입력 처리

    [Test]
    public void ShouldReturnFalseWhenBothPositionsAreIdentical()
    {
        // Arrange: 3x3 그리드 생성
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                CreateBlockAt(x, y, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
            }
        }

        // Act: 같은 위치 (1,1)과 (1,1) 교환 시도
        var result = _movematchvalidator.ValidateMove(_testgrid, (1, 1), (1, 1));

        // Assert: 같은 위치를 교환할 수 없으므로 false 반환
        Assert.IsFalse(result, "Cannot swap a block with itself");
    }

    [Test]
    public void ShouldReturnFalseWhenPositionsAreNotAdjacent()
    {
        // Arrange: 5x5 그리드 생성
        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                CreateBlockAt(x, y, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
            }
        }

        // Act & Assert: 대각선 위치 - (1,1)과 (2,2)
        var diagonalresult = _movematchvalidator.ValidateMove(_testgrid, (1, 1), (2, 2));
        Assert.IsFalse(diagonalresult, "Cannot swap diagonal blocks");

        // Act & Assert: 2칸 떨어진 가로 위치 - (1,1)과 (3,1)
        var horizontaldistantresult = _movematchvalidator.ValidateMove(_testgrid, (1, 1), (3, 1));
        Assert.IsFalse(horizontaldistantresult, "Cannot swap blocks 2 cells apart horizontally");

        // Act & Assert: 2칸 떨어진 세로 위치 - (1,1)과 (1,3)
        var verticaldistantresult = _movematchvalidator.ValidateMove(_testgrid, (1, 1), (1, 3));
        Assert.IsFalse(verticaldistantresult, "Cannot swap blocks 2 cells apart vertically");

        // Act & Assert: 인접한 가로 위치는 허용 (매치가 없어도 인접성은 true)
        // 단, 이 테스트는 인접성만 검증하므로 별도로 확인 필요
    }

    [Test]
    public void ShouldReturnFalseWhenOnePositionIsOutOfGrid()
    {
        // Arrange: 3x3 그리드 생성 (0,0) ~ (2,2)
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                CreateBlockAt(x, y, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
            }
        }

        // Act & Assert: 첫 번째 위치가 그리드 밖이고 인접함 - (-1, 0)과 (0, 0)
        // 인접하지만 그리드에 없으므로 false 반환해야 함
        var result1 = _movematchvalidator.ValidateMove(_testgrid, (-1, 0), (0, 0));
        Assert.IsFalse(result1, "Should return false when first position is out of grid");

        // Act & Assert: 두 번째 위치가 그리드 밖이고 인접함 - (2, 0)과 (3, 0)
        // 그리드는 0~2까지이므로 3은 밖, 인접하므로 그리드 검증 필요
        var result2 = _movematchvalidator.ValidateMove(_testgrid, (2, 0), (3, 0));
        Assert.IsFalse(result2, "Should return false when second position is out of grid");

        // Act & Assert: y 좌표가 그리드 밖이고 인접함 - (0, 2)과 (0, 3)
        var result3 = _movematchvalidator.ValidateMove(_testgrid, (0, 2), (0, 3));
        Assert.IsFalse(result3, "Should return false when position is out of grid vertically");
    }

    [Test]
    public void ShouldReturnFalseWhenBothPositionsAreOutOfGrid()
    {
        // Arrange: 3x3 그리드 생성 (0,0) ~ (2,2)
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                CreateBlockAt(x, y, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
            }
        }

        // Act & Assert: 두 위치 모두 그리드 밖이고 인접함 - (-1, 0)과 (-2, 0)
        var result1 = _movematchvalidator.ValidateMove(_testgrid, (-1, 0), (-2, 0));
        Assert.IsFalse(result1, "Should return false when both positions are out of grid");

        // Act & Assert: 두 위치 모두 그리드 밖이고 인접함 - (3, 0)과 (4, 0)
        var result2 = _movematchvalidator.ValidateMove(_testgrid, (3, 0), (4, 0));
        Assert.IsFalse(result2, "Should return false when both positions are out of grid");

        // Act & Assert: 두 위치 모두 그리드 밖이고 인접함 (세로) - (0, -1)과 (0, -2)
        var result3 = _movematchvalidator.ValidateMove(_testgrid, (0, -1), (0, -2));
        Assert.IsFalse(result3, "Should return false when both positions are out of grid vertically");
    }

    [Test]
    public void ShouldReturnFalseWhenOnePositionHasNullBlock()
    {
        // Arrange: 3x3 그리드 생성, 일부 위치에 null 블록 배치
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                CreateBlockAt(x, y, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
            }
        }

        // (1, 1) 위치에 null 블록 배치 (그리드에는 키가 존재하지만 값이 null)
        _testgrid[(1, 1)] = null;

        // Act & Assert: 첫 번째 위치가 null 블록 - (1, 1)과 (1, 0)
        var result1 = _movematchvalidator.ValidateMove(_testgrid, (1, 1), (1, 0));
        Assert.IsFalse(result1, "Should return false when first position has null block");

        // Act & Assert: 두 번째 위치가 null 블록 - (1, 2)과 (1, 1)
        var result2 = _movematchvalidator.ValidateMove(_testgrid, (1, 2), (1, 1));
        Assert.IsFalse(result2, "Should return false when second position has null block");

        // (2, 2) 위치에도 null 블록 배치
        _testgrid[(2, 2)] = null;

        // Act & Assert: 두 위치 모두 null 블록 (인접하지 않음)
        // (1, 1)과 (2, 2)는 인접하지 않으므로 인접성 검증에서 먼저 false 반환
        var result3 = _movematchvalidator.ValidateMove(_testgrid, (1, 1), (2, 2));
        Assert.IsFalse(result3, "Should return false when positions are not adjacent (even if both null)");
    }

    // Phase 5.2: FIVE 블록 특수 케이스

    [Test]
    public void ShouldReturnTrueWhenSwappingTwoFiveBlocks()
    {
        // Arrange: 두 개의 FIVE 블록 배치
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.FIVE);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.FIVE);

        // Act: 두 FIVE 블록 교환
        var result = _movematchvalidator.ValidateMove(_testgrid, (0, 0), (1, 0));

        // Assert: FIVE 블록끼리 교환도 항상 true여야 함
        Assert.IsTrue(result, "Two FIVE blocks should always be swappable");
    }

    [Test]
    public void ShouldReturnTrueWhenSwappingFiveBlockWithForeBlock()
    {
        // Arrange: FIVE 블록과 FORE 블록 배치
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.FIVE);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.FORE_LEFTRIGHT);

        // Act: FIVE 블록과 FORE 블록 교환
        var result = _movematchvalidator.ValidateMove(_testgrid, (0, 0), (1, 0));

        // Assert: FIVE 블록은 특수 블록(FORE)과도 항상 교환 가능해야 함
        Assert.IsTrue(result, "FIVE block should be swappable with FORE block");
    }

    [Test]
    public void ShouldReturnTrueWhenSwappingFiveBlockWithCrossBlock()
    {
        // Arrange: FIVE 블록과 CROSS 블록 배치
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.FIVE);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.CROSS_THREE);

        // Act: FIVE 블록과 CROSS 블록 교환
        var result = _movematchvalidator.ValidateMove(_testgrid, (0, 0), (1, 0));

        // Assert: FIVE 블록은 특수 블록(CROSS)과도 항상 교환 가능해야 함
        Assert.IsTrue(result, "FIVE block should be swappable with CROSS block");
    }

    [Test]
    public void ShouldReturnTrueWhenSwappingNormalBlockWithFiveBlockBidirectional()
    {
        // Arrange: 일반 블록과 FIVE 블록 배치
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.FIVE);

        // Act & Assert: 일반 블록 → FIVE 블록 (정방향)
        var result1 = _movematchvalidator.ValidateMove(_testgrid, (0, 0), (1, 0));
        Assert.IsTrue(result1, "Normal block should be swappable with FIVE block (forward)");

        // Act & Assert: FIVE 블록 → 일반 블록 (역방향)
        var result2 = _movematchvalidator.ValidateMove(_testgrid, (1, 0), (0, 0));
        Assert.IsTrue(result2, "FIVE block should be swappable with normal block (backward)");
    }

    // Phase 5.3: 양방향 매치 검증

    [Test]
    public void ShouldReturnTrueWhenOnlyFirstBlockCreatesMatch()
    {
        // Arrange: 첫 번째 블록 교환 후에만 매치가 생성되는 그리드
        // (0,0)=RED, (1,0)=BLUE, (2,0)=RED, (3,0)=RED
        // 교환 후: (0,0)=BLUE (매치 없음), (1,0)=RED (가로 3-매치 생성)
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        CreateBlockAt(3, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);

        // Act: (0,0) RED와 (1,0) BLUE 교환
        var result = _movematchvalidator.ValidateMove(_testgrid, (0, 0), (1, 0));

        // Assert: 첫 번째 블록 위치에서만 매치 생성되므로 true
        Assert.IsTrue(result, "Should return true when first block position creates match after swap");
    }

    [Test]
    public void ShouldReturnTrueWhenOnlySecondBlockCreatesMatch()
    {
        // Arrange: 두 번째 블록 교환 후에만 매치가 생성되는 그리드
        // (0,0)=RED, (1,0)=RED, (2,0)=RED, (3,0)=BLUE
        // 교환 후: (0,0)=RED (매치 없음), (3,0)=RED는 (0,0), (1,0), (2,0)과 가로 4-매치 생성
        // 하지만 인접해야 하므로 (2,0)과 (3,0) 교환
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
        CreateBlockAt(3, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);

        // Act: (2,0) BLUE와 (3,0) RED 교환
        // 교환 후: (2,0)=RED는 (0,0), (1,0), (3,0)과 가로 4-매치 생성
        //         (3,0)=BLUE는 매치 없음
        var result = _movematchvalidator.ValidateMove(_testgrid, (2, 0), (3, 0));

        // Assert: 두 번째 블록이 이동한 위치에서 매치 생성되므로 true
        Assert.IsTrue(result, "Should return true when second block position creates match after swap");
    }

    [Test]
    public void ShouldReturnTrueWhenBothBlocksCreateMatch()
    {
        // Arrange: 양쪽 블록 모두 매치를 생성하는 그리드
        // 이미 ShouldReturnMatchResultsForBothBlocks 테스트가 존재하지만, 명시적으로 추가
        // (0,0)=RED, (1,0)=BLUE, (2,0)=RED, (3,0)=RED
        // (0,1)=BLUE, (1,1)=RED, (2,1)=BLUE
        // (0,2)=BLUE
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        CreateBlockAt(3, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        CreateBlockAt(0, 1, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
        CreateBlockAt(1, 1, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        CreateBlockAt(2, 1, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
        CreateBlockAt(0, 2, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);

        // Act: 교환 후 양쪽 모두 매치 생성
        var result = _movematchvalidator.ValidateMove(_testgrid, (0, 0), (1, 0));

        // Assert: 양쪽 모두 매치되므로 true
        Assert.IsTrue(result, "Should return true when both positions create matches");
    }

    [Test]
    public void ShouldReturnFalseWhenNeitherBlockCreatesMatch()
    {
        // Arrange: 교환 후에도 매치가 생성되지 않는 그리드
        // (0,0)=RED, (1,0)=BLUE, (2,0)=GREEN
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.GREEN, EMATCHTYPE.THREE);

        // Act: 교환 후 매치 없음
        var result = _movematchvalidator.ValidateMove(_testgrid, (0, 0), (1, 0));

        // Assert: 매치가 없으므로 false
        Assert.IsFalse(result, "Should return false when neither position creates a match");
    }

    [Test]
    public void ShouldReturnTrueWhenSwapCreatesCrossMatch()
    {
        // Arrange: 교환 후 십자 매치가 생성되는 그리드
        // 진짜 십자 패턴을 만들기 위한 BLUE 블록 배치
        // (1,0)=BLUE, (0,1)=BLUE, (1,1)=RED, (2,1)=BLUE, (1,2)=BLUE
        // (1,1) RED와 (1,0) BLUE 교환 시
        // 교환 후: (1,1)=BLUE는 가로 (0,1)-(1,1)-(2,1) 3-매치 + 세로 (1,0)-(1,1)-(1,2) 3-매치 = 십자
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
        CreateBlockAt(0, 1, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
        CreateBlockAt(1, 1, EBLOCKCOLORTYPE.RED, EMATCHTYPE.THREE);
        CreateBlockAt(2, 1, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);
        CreateBlockAt(1, 2, EBLOCKCOLORTYPE.BLUE, EMATCHTYPE.THREE);

        // Act: (1,1) RED와 (1,0) BLUE 교환
        // 교환 후: (1,1) 위치에 BLUE가 오면서 가로 3-매치 + 세로 3-매치 십자 생성
        var result = _movematchvalidator.ValidateMove(_testgrid, (1, 1), (1, 0));

        // Assert: 교환 후 십자 매치 생성되므로 true
        Assert.IsTrue(result, "Should return true when swap creates cross match pattern");
    }

    private void CreateBlockAt(int x, int y, EBLOCKCOLORTYPE colortype, EMATCHTYPE matchtype)
    {
        var gameobject = new GameObject($"Block_{x}_{y}");
        var block = gameobject.AddComponent<UI_Match_Block>();

        // Use reflection to set private fields for testing
        var colorfield = typeof(UI_Match_Block).GetField("_colortypes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        colorfield?.SetValue(block, colortype);

        var matchtypefield = typeof(UI_Match_Block).GetField("_blocktype",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        matchtypefield?.SetValue(block, matchtype);

        _testgrid[(x, y)] = block;
    }
}
