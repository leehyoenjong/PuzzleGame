using System.Collections.Generic;

/// <summary>
/// 십자 매치 결과를 저장하는 구조체
/// </summary>
public struct CrossMatchResult
{
    public List<(int, int)> horizontalmatches;
    public List<(int, int)> verticalmatches;
}

/// <summary>
/// 그리드에서 매치 패턴을 감지하는 클래스
/// 수평, 수직, 십자 매치를 감지합니다.
/// </summary>
public class MatchDetector
{
    public List<(int, int)> DetectHorizontalMatch(Dictionary<(int, int), UI_Match_Block> grid, (int x, int y) startposition)
    {
        if (!grid.ContainsKey(startposition) || grid[startposition] == null)
            return null;

        var startblock = grid[startposition];
        var matches = new List<(int, int)>();

        var startcolor = startblock.GetBlockColorTypes();
        int y = startposition.y;

        // Check left direction (scan backwards from startposition)
        int leftx = startposition.x;
        while (grid.ContainsKey((leftx, y)))
        {
            var block = grid[(leftx, y)];
            if (block != null && block.GetBlockColorTypes() == startcolor)
            {
                matches.Insert(0, (leftx, y)); // Insert at beginning to maintain order
                leftx--;
            }
            else
            {
                break;
            }
        }

        // Check right direction (exclude startposition as it's already added)
        int rightx = startposition.x + 1;
        while (grid.ContainsKey((rightx, y)))
        {
            var block = grid[(rightx, y)];
            if (block != null && block.GetBlockColorTypes() == startcolor)
            {
                matches.Add((rightx, y));
                rightx++;
            }
            else
            {
                break;
            }
        }

        // Return matches only if at least 3 blocks
        return matches.Count >= 3 ? matches : null;
    }

    public List<(int, int)> DetectVerticalMatch(Dictionary<(int, int), UI_Match_Block> grid, (int x, int y) startposition)
    {
        if (!grid.ContainsKey(startposition) || grid[startposition] == null)
            return null;

        var startblock = grid[startposition];
        var matches = new List<(int, int)>();

        var startcolor = startblock.GetBlockColorTypes();
        int x = startposition.x;

        // Check downward direction (scan backwards from startposition)
        int downy = startposition.y;
        while (grid.ContainsKey((x, downy)))
        {
            var block = grid[(x, downy)];
            if (block != null && block.GetBlockColorTypes() == startcolor)
            {
                matches.Insert(0, (x, downy)); // Insert at beginning to maintain order
                downy--;
            }
            else
            {
                break;
            }
        }

        // Check upward direction (exclude startposition as it's already added)
        int upy = startposition.y + 1;
        while (grid.ContainsKey((x, upy)))
        {
            var block = grid[(x, upy)];
            if (block != null && block.GetBlockColorTypes() == startcolor)
            {
                matches.Add((x, upy));
                upy++;
            }
            else
            {
                break;
            }
        }

        // Return matches only if at least 3 blocks
        return matches.Count >= 3 ? matches : null;
    }

    public CrossMatchResult? DetectCrossMatch(Dictionary<(int, int), UI_Match_Block> grid, (int x, int y) centerposition)
    {
        if (!grid.ContainsKey(centerposition) || grid[centerposition] == null)
            return null;

        var centerblock = grid[centerposition];
        var centercolor = centerblock.GetBlockColorTypes();

        // Detect horizontal match through center
        var horizontalmatches = DetectFullHorizontalMatch(grid, centerposition, centercolor);

        // Detect vertical match through center
        var verticalmatches = DetectFullVerticalMatch(grid, centerposition, centercolor);

        // Check if both horizontal and vertical have at least 3 matches
        if (horizontalmatches != null && verticalmatches != null &&
            horizontalmatches.Count >= 3 && verticalmatches.Count >= 3)
        {
            return new CrossMatchResult
            {
                horizontalmatches = horizontalmatches,
                verticalmatches = verticalmatches
            };
        }

        return null;
    }

    private List<(int, int)> DetectFullHorizontalMatch(Dictionary<(int, int), UI_Match_Block> grid, (int x, int y) center, EBLOCKCOLORTYPE color)
    {
        var matches = new List<(int, int)> { center };
        int y = center.y;

        // Check left direction
        int leftx = center.x - 1;
        while (grid.ContainsKey((leftx, y)))
        {
            var block = grid[(leftx, y)];
            if (block != null && block.GetBlockColorTypes() == color)
            {
                matches.Insert(0, (leftx, y));
                leftx--;
            }
            else
            {
                break;
            }
        }

        // Check right direction
        int rightx = center.x + 1;
        while (grid.ContainsKey((rightx, y)))
        {
            var block = grid[(rightx, y)];
            if (block != null && block.GetBlockColorTypes() == color)
            {
                matches.Add((rightx, y));
                rightx++;
            }
            else
            {
                break;
            }
        }

        return matches.Count >= 3 ? matches : null;
    }

    private List<(int, int)> DetectFullVerticalMatch(Dictionary<(int, int), UI_Match_Block> grid, (int x, int y) center, EBLOCKCOLORTYPE color)
    {
        var matches = new List<(int, int)> { center };
        int x = center.x;

        // Check down direction
        int downy = center.y - 1;
        while (grid.ContainsKey((x, downy)))
        {
            var block = grid[(x, downy)];
            if (block != null && block.GetBlockColorTypes() == color)
            {
                matches.Insert(0, (x, downy));
                downy--;
            }
            else
            {
                break;
            }
        }

        // Check up direction
        int upy = center.y + 1;
        while (grid.ContainsKey((x, upy)))
        {
            var block = grid[(x, upy)];
            if (block != null && block.GetBlockColorTypes() == color)
            {
                matches.Add((x, upy));
                upy++;
            }
            else
            {
                break;
            }
        }

        return matches.Count >= 3 ? matches : null;
    }
}