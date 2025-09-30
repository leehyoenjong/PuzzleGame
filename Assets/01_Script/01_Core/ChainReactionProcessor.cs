using System.Collections.Generic;
using System.Linq;

public class ChainReactionProcessor
{
    public List<UI_Match_Block> ProcessEffect(UI_Match_Block specialblock, Dictionary<(int, int), UI_Match_Block> matchblockdic, EBLOCKCOLORTYPE? targetcolor = null)
    {
        var affectedblocks = new List<UI_Match_Block>();
        var blocktype = specialblock.GetBlockMatchTypes();

        switch (blocktype)
        {
            case EMATCHTYPE.FORE_LEFTRIGHT:
            case EMATCHTYPE.FORE_UPDOWN:
                affectedblocks.AddRange(ProcessForeMatch(specialblock, matchblockdic));
                break;
            case EMATCHTYPE.FIVE:
                var colortype = targetcolor ?? specialblock.GetBlockColorTypes();
                affectedblocks.AddRange(ProcessFiveMatch(colortype, matchblockdic));
                break;
            case EMATCHTYPE.CROSS_THREE:
                affectedblocks.AddRange(ProcessCrossMatch(-1, 2, specialblock, matchblockdic));
                break;
            case EMATCHTYPE.CROSS_FOUR:
                affectedblocks.AddRange(ProcessCrossMatch(-3, 4, specialblock, matchblockdic));
                break;
            case EMATCHTYPE.CROSS_FIVE:
                affectedblocks.AddRange(ProcessCrossMatch(-6, 7, specialblock, matchblockdic));
                break;
        }

        return affectedblocks;
    }

    public List<UI_Match_Block> ProcessChainReaction(List<UI_Match_Block> initialblocks, Dictionary<(int, int), UI_Match_Block> matchblockdic)
    {
        var finaldestroylist = new HashSet<UI_Match_Block>(initialblocks);
        var processqueue = new Queue<(UI_Match_Block block, EBLOCKCOLORTYPE inheritedcolor)>();

        // 초기 블록들을 큐에 추가
        foreach (var block in initialblocks)
        {
            if (IsSpecialBlock(block))
            {
                processqueue.Enqueue((block, block.GetBlockColorTypes()));
            }
        }

        while (processqueue.Count > 0)
        {
            var (currentblock, inheritedcolor) = processqueue.Dequeue();
            var blocksaffectedbyeffect = new List<UI_Match_Block>();

            switch (currentblock.GetBlockMatchTypes())
            {
                case EMATCHTYPE.FORE_LEFTRIGHT:
                case EMATCHTYPE.FORE_UPDOWN:
                    blocksaffectedbyeffect.AddRange(ProcessForeMatch(currentblock, matchblockdic));
                    break;
                case EMATCHTYPE.FIVE:
                    // FORE 블록에서 색상을 물려받았는지 확인
                    var colortomatch = inheritedcolor != EBLOCKCOLORTYPE.FIVE ? inheritedcolor : currentblock.GetBlockColorTypes();
                    if (colortomatch == EBLOCKCOLORTYPE.FIVE)
                    {
                        // FIVE 블록이 다른 FIVE블록과 만나 파괴되는 경우,
                        // 임의의 색상(예: RED)을 지정하거나 다른 규칙 필요. 여기서는 RED로 가정
                        colortomatch = EBLOCKCOLORTYPE.RED;
                    }
                    blocksaffectedbyeffect.AddRange(ProcessFiveMatch(colortomatch, matchblockdic));
                    break;
                case EMATCHTYPE.CROSS_THREE:
                    blocksaffectedbyeffect.AddRange(ProcessCrossMatch(-1, 2, currentblock, matchblockdic));
                    break;
                case EMATCHTYPE.CROSS_FOUR:
                    blocksaffectedbyeffect.AddRange(ProcessCrossMatch(-3, 4, currentblock, matchblockdic));
                    break;
                case EMATCHTYPE.CROSS_FIVE:
                    blocksaffectedbyeffect.AddRange(ProcessCrossMatch(-6, 7, currentblock, matchblockdic));
                    break;
            }

            foreach (var affectedblock in blocksaffectedbyeffect)
            {
                // 아직 최종 파괴 목록에 없고, 큐에도 없는 새로운 특수 블록이라면
                if (finaldestroylist.Contains(affectedblock) == false && IsSpecialBlock(affectedblock))
                {
                    // FIVE 블록을 위한 색상 상속
                    var colortoinherit = currentblock.GetBlockMatchTypes() == EMATCHTYPE.FIVE ? inheritedcolor : affectedblock.GetBlockColorTypes();
                    processqueue.Enqueue((affectedblock, colortoinherit));
                }
                finaldestroylist.Add(affectedblock);
            }
        }
        return finaldestroylist.ToList();
    }

    private bool IsSpecialBlock(UI_Match_Block block)
    {
        var type = block.GetBlockMatchTypes();
        return type != EMATCHTYPE.THREE;
    }

    private List<UI_Match_Block> ProcessForeMatch(UI_Match_Block currentblock, Dictionary<(int, int), UI_Match_Block> matchblockdic)
    {
        var breaklist = new List<UI_Match_Block>();
        var currentpoint = currentblock.GetPoint();
        var blocktype = currentblock.GetBlockMatchTypes();

        if (blocktype == EMATCHTYPE.FORE_LEFTRIGHT)
        {
            // x축 방향 한줄 파괴 - 같은 y좌표의 모든 블록
            foreach (var block in matchblockdic)
            {
                if (block.Key.Item2 == currentpoint.y && block.Value != null)
                {
                    breaklist.Add(block.Value);
                }
            }
        }
        else // FORE_UPDOWN
        {
            // y축 방향 한줄 파괴 - 같은 x좌표의 모든 블록
            foreach (var block in matchblockdic)
            {
                if (block.Key.Item1 == currentpoint.x && block.Value != null)
                {
                    breaklist.Add(block.Value);
                }
            }
        }

        return breaklist;
    }

    private List<UI_Match_Block> ProcessFiveMatch(EBLOCKCOLORTYPE colortype, Dictionary<(int, int), UI_Match_Block> matchblockdic)
    {
        var colorlist = matchblockdic.Where(x => x.Value != null).Where(x => x.Value.GetBlockColorTypes() == colortype).Select(x => x.Value).ToList();
        return colorlist;
    }

    private List<UI_Match_Block> ProcessCrossMatch(int startindex, int endindex, UI_Match_Block boomblock, Dictionary<(int, int), UI_Match_Block> matchblockdic)
    {
        var breaklist = new List<UI_Match_Block>();
        var point = boomblock.GetPoint();

        for (int y = startindex; y < endindex; y++)
        {
            for (int x = startindex; x < endindex; x++)
            {
                var keyx = point.x + x;
                var keyy = point.y + y;

                if (matchblockdic.TryGetValue((keyx, keyy), out var block) == false)
                {
                    continue;
                }
                breaklist.Add(block);
            }
        }

        return breaklist;
    }
}