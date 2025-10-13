using System.Collections.Generic;

/// <summary>
/// 사용자 블록 이동이 유효한 매치를 생성하는지 검증하는 클래스
/// FIVE 블록 특수 케이스와 양방향 매치 검증을 담당합니다.
/// </summary>
public class MoveMatchValidator
{
    private readonly MatchDetector _matchdetector;

    public MoveMatchValidator(MatchDetector matchdetector)
    {
        _matchdetector = matchdetector;
    }

    public bool ValidateMove(Dictionary<(int, int), UI_Match_Block> grid, (int, int) pos1, (int, int) pos2)
    {
        // 같은 위치는 교환할 수 없음
        if (pos1 == pos2)
        {
            return false;
        }

        // 인접한 위치인지 검증 (가로 또는 세로로 1칸 차이만 허용)
        int xdiff = System.Math.Abs(pos1.Item1 - pos2.Item1);
        int ydiff = System.Math.Abs(pos1.Item2 - pos2.Item2);

        bool isadjacent = (xdiff == 1 && ydiff == 0) || (xdiff == 0 && ydiff == 1);
        if (!isadjacent)
        {
            return false;
        }

        // 그리드에 두 위치가 모두 존재하는지 검증
        if (!grid.ContainsKey(pos1) || !grid.ContainsKey(pos2))
        {
            return false;
        }

        var block1 = grid[pos1];
        var block2 = grid[pos2];

        // null 블록 검증
        if (block1 == null || block2 == null)
        {
            return false;
        }

        // FIVE 블록은 어떤 블록과도 교환 가능
        if (block1.GetBlockMatchTypes() == EMATCHTYPE.FIVE || block2.GetBlockMatchTypes() == EMATCHTYPE.FIVE)
        {
            return true;
        }

        // Swap blocks temporarily in dictionary
        grid[pos1] = block2;
        grid[pos2] = block1;

        // Update block internal positions temporarily
        UpdateBlockPosition(block1, pos2.Item1, pos2.Item2);
        UpdateBlockPosition(block2, pos1.Item1, pos1.Item2);

        // Check if either position creates a match by scanning full line
        bool hasmatch = HasMatchAtPosition(grid, pos1) || HasMatchAtPosition(grid, pos2);

        // Swap back
        grid[pos1] = block1;
        grid[pos2] = block2;
        UpdateBlockPosition(block1, pos1.Item1, pos1.Item2);
        UpdateBlockPosition(block2, pos2.Item1, pos2.Item2);

        return hasmatch;
    }

    private bool HasMatchAtPosition(Dictionary<(int, int), UI_Match_Block> grid, (int, int) position)
    {
        if (!grid.ContainsKey(position))
            return false;

        var block = grid[position];
        var color = block.GetBlockColorTypes();

        // Check horizontal line (both directions)
        int horizontalcount = 1;
        int x = position.Item1;
        int y = position.Item2;

        // Check left
        int leftx = x - 1;
        while (grid.ContainsKey((leftx, y)) && grid[(leftx, y)].GetBlockColorTypes() == color)
        {
            horizontalcount++;
            leftx--;
        }

        // Check right
        int rightx = x + 1;
        while (grid.ContainsKey((rightx, y)) && grid[(rightx, y)].GetBlockColorTypes() == color)
        {
            horizontalcount++;
            rightx++;
        }

        if (horizontalcount >= 3)
            return true;

        // Check vertical line (both directions)
        int verticalcount = 1;

        // Check down
        int downy = y - 1;
        while (grid.ContainsKey((x, downy)) && grid[(x, downy)].GetBlockColorTypes() == color)
        {
            verticalcount++;
            downy--;
        }

        // Check up
        int upy = y + 1;
        while (grid.ContainsKey((x, upy)) && grid[(x, upy)].GetBlockColorTypes() == color)
        {
            verticalcount++;
            upy++;
        }

        return verticalcount >= 3;
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
