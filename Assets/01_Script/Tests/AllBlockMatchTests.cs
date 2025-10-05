using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[TestFixture]
public class AllBlockMatchTests
{
    private Dictionary<(int, int), UI_Match_Block> _testgrid;
    private MatchDetector _matchdetector;

    [SetUp]
    public void SetUp()
    {
        _testgrid = new Dictionary<(int, int), UI_Match_Block>();
        _matchdetector = new MatchDetector();
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
    public void ShouldNotDuplicateProcessSameMatch()
    {
        // Arrange: Create single 3-match [R][R][R]
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED);

        // Act: Simulate AllBlockMatch scanning all positions with duplicate prevention
        var processedblocks = new HashSet<UI_Match_Block>();
        var allmatches = new HashSet<UI_Match_Block>();
        int matchprocesscount = 0;

        for (int x = 0; x < 3; x++)
        {
            // Skip if this position's block is already processed
            if (_testgrid.ContainsKey((x, 0)) && processedblocks.Contains(_testgrid[(x, 0)]))
            {
                continue;
            }

            var horizontalmatches = _matchdetector.DetectHorizontalMatch(_testgrid, (x, 0));

            if (horizontalmatches != null && horizontalmatches.Count >= 3)
            {
                // Check if any block in this match was already processed
                bool alreadyprocessed = false;
                foreach (var pos in horizontalmatches)
                {
                    if (_testgrid.ContainsKey(pos) && processedblocks.Contains(_testgrid[pos]))
                    {
                        alreadyprocessed = true;
                        break;
                    }
                }

                if (!alreadyprocessed)
                {
                    // This simulates GetMatchTypeFuction being called
                    matchprocesscount++;

                    foreach (var pos in horizontalmatches)
                    {
                        if (_testgrid.ContainsKey(pos))
                        {
                            var block = _testgrid[pos];
                            allmatches.Add(block);
                            processedblocks.Add(block);
                        }
                    }
                }
            }
        }

        // Assert: Should process match only ONCE
        Assert.AreEqual(3, allmatches.Count, "Should have 3 unique blocks");
        Assert.AreEqual(1, matchprocesscount, "Should process same match only once");
    }

    [Test]
    public void ShouldDetectMatchAtBottomOfGrid()
    {
        // Arrange: Create 3x3 grid with horizontal match at bottom (y=0)
        // Row 0 (bottom): [R][R][R]
        // Row 1: [B][G][Y]
        // Row 2 (top): [P][G][B]
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(0, 1, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(1, 1, EBLOCKCOLORTYPE.GREEN);
        CreateBlockAt(2, 1, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(0, 2, EBLOCKCOLORTYPE.PINK);
        CreateBlockAt(1, 2, EBLOCKCOLORTYPE.GREEN);
        CreateBlockAt(2, 2, EBLOCKCOLORTYPE.BLUE);

        // Act: Simulate AllBlockMatch scanning entire grid
        var processedblocks = new HashSet<UI_Match_Block>();
        var foundmatches = new List<List<UI_Match_Block>>();

        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                // Skip if this position's block is already processed
                if (_testgrid.ContainsKey((x, y)) && processedblocks.Contains(_testgrid[(x, y)]))
                {
                    continue;
                }

                var horizontalmatches = _matchdetector.DetectHorizontalMatch(_testgrid, (x, y));

                if (horizontalmatches != null && horizontalmatches.Count >= 3)
                {
                    var matchblocks = new List<UI_Match_Block>();
                    foreach (var pos in horizontalmatches)
                    {
                        if (_testgrid.ContainsKey(pos))
                        {
                            var block = _testgrid[pos];
                            matchblocks.Add(block);
                            processedblocks.Add(block);
                        }
                    }
                    foundmatches.Add(matchblocks);
                }
            }
        }

        // Assert: Should detect exactly one match at bottom
        Assert.AreEqual(1, foundmatches.Count, "Should detect exactly one match");
        Assert.AreEqual(3, foundmatches[0].Count, "Match should contain 3 blocks");
        Assert.IsTrue(foundmatches[0].Contains(_testgrid[(0, 0)]), "Should contain (0,0)");
        Assert.IsTrue(foundmatches[0].Contains(_testgrid[(1, 0)]), "Should contain (1,0)");
        Assert.IsTrue(foundmatches[0].Contains(_testgrid[(2, 0)]), "Should contain (2,0)");
    }

    [Test]
    public void ShouldDetectMultipleMatchesSimultaneously()
    {
        // Arrange: Create grid with TWO separate matches
        // Row 0: [R][R][R]  <- horizontal match
        // Row 1: [B][Y][P]
        // Row 2: [B][P][Y]
        // Row 3: [B][Y][P]
        // Column 0 has vertical match [B][B][B]
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(0, 1, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(1, 1, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(2, 1, EBLOCKCOLORTYPE.PINK);
        CreateBlockAt(0, 2, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(1, 2, EBLOCKCOLORTYPE.PINK);
        CreateBlockAt(2, 2, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(0, 3, EBLOCKCOLORTYPE.BLUE);
        CreateBlockAt(1, 3, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(2, 3, EBLOCKCOLORTYPE.PINK);

        // Act: Simulate AllBlockMatch scanning entire grid
        var processedblocks = new HashSet<UI_Match_Block>();
        var foundmatches = new List<List<UI_Match_Block>>();

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                if (_testgrid.ContainsKey((x, y)) && processedblocks.Contains(_testgrid[(x, y)]))
                {
                    continue;
                }

                // Check horizontal
                var horizontalmatches = _matchdetector.DetectHorizontalMatch(_testgrid, (x, y));
                if (horizontalmatches != null && horizontalmatches.Count >= 3)
                {
                    var matchblocks = new List<UI_Match_Block>();
                    foreach (var pos in horizontalmatches)
                    {
                        if (_testgrid.ContainsKey(pos))
                        {
                            matchblocks.Add(_testgrid[pos]);
                            processedblocks.Add(_testgrid[pos]);
                        }
                    }
                    foundmatches.Add(matchblocks);
                }

                // Check vertical
                var verticalmatches = _matchdetector.DetectVerticalMatch(_testgrid, (x, y));
                if (verticalmatches != null && verticalmatches.Count >= 3)
                {
                    var matchblocks = new List<UI_Match_Block>();
                    foreach (var pos in verticalmatches)
                    {
                        if (_testgrid.ContainsKey(pos))
                        {
                            matchblocks.Add(_testgrid[pos]);
                            processedblocks.Add(_testgrid[pos]);
                        }
                    }
                    foundmatches.Add(matchblocks);
                }
            }
        }

        // Assert: Should detect BOTH matches
        Assert.AreEqual(2, foundmatches.Count, "Should detect two separate matches");

        // One should be horizontal RED match
        var horizontalmatch = foundmatches.Find(m => m.Contains(_testgrid[(0, 0)]));
        Assert.IsNotNull(horizontalmatch, "Should find horizontal match");
        Assert.AreEqual(3, horizontalmatch.Count, "Horizontal match should have 3 blocks");

        // One should be vertical BLUE match
        var verticalmatch = foundmatches.Find(m => m.Contains(_testgrid[(0, 1)]));
        Assert.IsNotNull(verticalmatch, "Should find vertical match");
        Assert.AreEqual(3, verticalmatch.Count, "Vertical match should have 3 blocks");
    }

    [Test]
    public void ShouldHandleOverlappingLShapeMatches()
    {
        // Arrange: Create L-shape with overlapping matches
        // Row 0: [R][R][R]  <- horizontal match
        // Row 1: [R][Y][P]
        // Row 2: [R][P][Y]  <- (0,0), (0,1), (0,2) form vertical match
        // L-shape: horizontal [R][R][R] + vertical [R][R][R] overlap at (0,0)
        CreateBlockAt(0, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(1, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(2, 0, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(0, 1, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(1, 1, EBLOCKCOLORTYPE.YELLOW);
        CreateBlockAt(2, 1, EBLOCKCOLORTYPE.PINK);
        CreateBlockAt(0, 2, EBLOCKCOLORTYPE.RED);
        CreateBlockAt(1, 2, EBLOCKCOLORTYPE.PINK);
        CreateBlockAt(2, 2, EBLOCKCOLORTYPE.YELLOW);

        // Act: Simulate AllBlockMatch scanning entire grid
        var processedblocks = new HashSet<UI_Match_Block>();
        var foundmatches = new List<List<UI_Match_Block>>();

        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                if (_testgrid.ContainsKey((x, y)) && processedblocks.Contains(_testgrid[(x, y)]))
                {
                    continue;
                }

                // Check horizontal
                var horizontalmatches = _matchdetector.DetectHorizontalMatch(_testgrid, (x, y));
                if (horizontalmatches != null && horizontalmatches.Count >= 3)
                {
                    var matchblocks = new List<UI_Match_Block>();
                    foreach (var pos in horizontalmatches)
                    {
                        if (_testgrid.ContainsKey(pos))
                        {
                            matchblocks.Add(_testgrid[pos]);
                            processedblocks.Add(_testgrid[pos]);
                        }
                    }
                    foundmatches.Add(matchblocks);
                }

                // Check vertical
                var verticalmatches = _matchdetector.DetectVerticalMatch(_testgrid, (x, y));
                if (verticalmatches != null && verticalmatches.Count >= 3)
                {
                    var matchblocks = new List<UI_Match_Block>();
                    foreach (var pos in verticalmatches)
                    {
                        if (_testgrid.ContainsKey(pos))
                        {
                            matchblocks.Add(_testgrid[pos]);
                            processedblocks.Add(_testgrid[pos]);
                        }
                    }
                    foundmatches.Add(matchblocks);
                }
            }
        }

        // Assert: L-shape creates TWO separate matches (horizontal + vertical)
        // This is expected behavior - overlapping matches are handled separately
        Assert.AreEqual(2, foundmatches.Count, "Should detect both horizontal and vertical matches in L-shape");

        // Find horizontal match
        var horizontalmatch = foundmatches.Find(m => m.Contains(_testgrid[(1, 0)]) && m.Contains(_testgrid[(2, 0)]));
        Assert.IsNotNull(horizontalmatch, "Should find horizontal match");
        Assert.AreEqual(3, horizontalmatch.Count, "Horizontal match should have 3 blocks");

        // Find vertical match
        var verticalmatch = foundmatches.Find(m => m.Contains(_testgrid[(0, 1)]) && m.Contains(_testgrid[(0, 2)]));
        Assert.IsNotNull(verticalmatch, "Should find vertical match");
        Assert.AreEqual(3, verticalmatch.Count, "Vertical match should have 3 blocks");

        // Verify all 5 unique RED blocks are in processedblocks
        Assert.AreEqual(5, processedblocks.Count, "Should process all 5 unique RED blocks");
        Assert.IsTrue(processedblocks.Contains(_testgrid[(0, 0)]), "Should process (0,0)");
        Assert.IsTrue(processedblocks.Contains(_testgrid[(1, 0)]), "Should process (1,0)");
        Assert.IsTrue(processedblocks.Contains(_testgrid[(2, 0)]), "Should process (2,0)");
        Assert.IsTrue(processedblocks.Contains(_testgrid[(0, 1)]), "Should process (0,1)");
        Assert.IsTrue(processedblocks.Contains(_testgrid[(0, 2)]), "Should process (0,2)");
    }

    private void CreateBlockAt(int x, int y, EBLOCKCOLORTYPE color)
    {
        var gameobject = new GameObject($"Block_{x}_{y}");
        var block = gameobject.AddComponent<UI_Match_Block>();

        var colorfield = typeof(UI_Match_Block).GetField("_colortypes",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        colorfield?.SetValue(block, color);

        _testgrid.Add((x, y), block);
    }
}
