using System.Collections.Generic;

/// <summary>
/// 블록 교환 및 롤백 작업을 처리하는 클래스
/// 그리드 딕셔너리의 일관성을 유지하면서 블록 위치를 교환합니다.
/// </summary>
public class BlockSwapHandler
{
    public void SwapBlocks(Dictionary<(int, int), UI_Match_Block> grid, (int, int) pos1, (int, int) pos2)
    {
        var block1 = grid[pos1];
        var block2 = grid[pos2];

        // null 블록 체크
        if (block1 == null || block2 == null)
        {
            throw new System.NullReferenceException("Cannot swap null blocks");
        }

        // 딕셔너리에서 위치 교환
        grid[pos1] = block2;
        grid[pos2] = block1;

        // 블록 객체의 내부 좌표 업데이트
        UpdateBlockPosition(block1, pos2.Item1, pos2.Item2);
        UpdateBlockPosition(block2, pos1.Item1, pos1.Item2);
    }

    public void RollbackSwap(Dictionary<(int, int), UI_Match_Block> grid, (int, int) pos1, (int, int) pos2)
    {
        // 롤백은 다시 교환하는 것과 동일
        SwapBlocks(grid, pos1, pos2);
    }

    private void UpdateBlockPosition(UI_Match_Block block, int x, int y)
    {
        var xfield = typeof(UI_Match_Block).GetField("_x",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        xfield?.SetValue(block, x);

        var yfield = typeof(UI_Match_Block).GetField("_y",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        yfield?.SetValue(block, y);
    }
}
