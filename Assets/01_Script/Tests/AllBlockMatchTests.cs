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
