using System.Collections.Generic;

public struct CrossMatchResult
{
    public List<(int, int)> horizontalmatches;
    public List<(int, int)> verticalmatches;
}

public class MatchDetector
{
    public List<(int, int)> DetectHorizontalMatch(Dictionary<(int, int), UI_Match_Block> grid, (int x, int y) startposition)
    {
        if (!grid.ContainsKey(startposition))
            return null;

        var startblock = grid[startposition];
        var matches = new List<(int, int)> { startposition };

        var startcolor = startblock.GetBlockColorTypes();
        int currentx = startposition.x + 1;
        int y = startposition.y;

        // Check right direction
        while (grid.ContainsKey((currentx, y)))
        {
            var block = grid[(currentx, y)];
            if (block.GetBlockColorTypes() == startcolor)
            {
                matches.Add((currentx, y));
                currentx++;
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
        if (!grid.ContainsKey(startposition))
            return null;

        var startblock = grid[startposition];
        var matches = new List<(int, int)> { startposition };

        var startcolor = startblock.GetBlockColorTypes();
        int x = startposition.x;
        int currenty = startposition.y + 1;

        // Check upward direction
        while (grid.ContainsKey((x, currenty)))
        {
            var block = grid[(x, currenty)];
            if (block.GetBlockColorTypes() == startcolor)
            {
                matches.Add((x, currenty));
                currenty++;
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
        if (!grid.ContainsKey(centerposition))
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
            if (block.GetBlockColorTypes() == color)
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
            if (block.GetBlockColorTypes() == color)
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
            if (block.GetBlockColorTypes() == color)
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
            if (block.GetBlockColorTypes() == color)
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