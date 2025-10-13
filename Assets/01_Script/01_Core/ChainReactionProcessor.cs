using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 특수 블록의 연쇄 반응을 처리하는 클래스
/// FORE, FIVE, CROSS 타입 블록의 효과와 연쇄 반응을 담당합니다.
/// </summary>
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
        // null 블록 리스트 방어 코드
        if (initialblocks == null)
        {
            return new List<UI_Match_Block>();
        }

        var finaldestroylist = new HashSet<UI_Match_Block>();
        var processedblocks = new HashSet<UI_Match_Block>();
        var processqueue = new Queue<(UI_Match_Block block, EBLOCKCOLORTYPE inheritedcolor)>();

        UnityEngine.Debug.Log($"[ProcessChainReaction] Starting with {initialblocks.Count} initial blocks");

        // 초기 블록들을 처리
        foreach (var block in initialblocks)
        {
            // null 블록은 건너뛰기
            if (block == null)
            {
                UnityEngine.Debug.Log($"[ProcessChainReaction] Skipping null block");
                continue;
            }

            finaldestroylist.Add(block);
            UnityEngine.Debug.Log($"[ProcessChainReaction] Added initial block at ({block.GetPoint().x}, {block.GetPoint().y}): {block.GetBlockMatchTypes()}");

            if (IsSpecialBlock(block))
            {
                // FIVE 블록이 일반 블록과 함께 매치될 때 색상 결정
                EBLOCKCOLORTYPE inheritedcolor;
                if (block.GetBlockMatchTypes() == EMATCHTYPE.FIVE)
                {
                    // 초기 블록 리스트에서 일반 블록(THREE)의 색상을 찾기
                    var regularblock = initialblocks.FirstOrDefault(b => b != null && b.GetBlockMatchTypes() == EMATCHTYPE.THREE);
                    if (regularblock != null)
                    {
                        // 일반 블록의 색상 사용
                        inheritedcolor = regularblock.GetBlockColorTypes();
                    }
                    else
                    {
                        // 일반 블록이 없으면 FIVE 블록 자신의 색상 사용
                        // (FIVE끼리 매치되거나 FIVE 단독인 경우)
                        inheritedcolor = block.GetBlockColorTypes();
                    }
                }
                else
                {
                    // FIVE가 아닌 특수 블록은 자신의 색상 사용
                    inheritedcolor = block.GetBlockColorTypes();
                }

                processqueue.Enqueue((block, inheritedcolor));
                UnityEngine.Debug.Log($"[ProcessChainReaction] Enqueued special block at ({block.GetPoint().x}, {block.GetPoint().y})");
            }
        }

        while (processqueue.Count > 0)
        {
            var (currentblock, inheritedcolor) = processqueue.Dequeue();
            UnityEngine.Debug.Log($"[ProcessChainReaction] Processing block at ({currentblock.GetPoint().x}, {currentblock.GetPoint().y}): {currentblock.GetBlockMatchTypes()}");

            // 이미 처리한 블록은 건너뛰기 (순환 참조 방지)
            if (processedblocks.Contains(currentblock))
            {
                UnityEngine.Debug.Log($"[ProcessChainReaction] Skipping already processed block at ({currentblock.GetPoint().x}, {currentblock.GetPoint().y})");
                continue;
            }

            processedblocks.Add(currentblock);

            var blocksaffectedbyeffect = new List<UI_Match_Block>();

            switch (currentblock.GetBlockMatchTypes())
            {
                case EMATCHTYPE.FORE_LEFTRIGHT:
                case EMATCHTYPE.FORE_UPDOWN:
                    blocksaffectedbyeffect.AddRange(ProcessForeMatch(currentblock, matchblockdic));
                    UnityEngine.Debug.Log($"[ProcessChainReaction] FORE effect affected {blocksaffectedbyeffect.Count} blocks");
                    break;
                case EMATCHTYPE.FIVE:
                    // 상속받은 색상 확인
                    EBLOCKCOLORTYPE colortomatch;
                    if (inheritedcolor == EBLOCKCOLORTYPE.FIVE)
                    {
                        // FIVE 색상이면 fallback으로 RED 사용
                        colortomatch = EBLOCKCOLORTYPE.RED;
                    }
                    else
                    {
                        // 일반 색상이면 그대로 사용
                        colortomatch = inheritedcolor;
                    }
                    blocksaffectedbyeffect.AddRange(ProcessFiveMatch(colortomatch, matchblockdic));
                    UnityEngine.Debug.Log($"[ProcessChainReaction] FIVE effect affected {blocksaffectedbyeffect.Count} blocks with color {colortomatch}");
                    break;
                case EMATCHTYPE.CROSS_THREE:
                    blocksaffectedbyeffect.AddRange(ProcessCrossMatch(-1, 2, currentblock, matchblockdic));
                    UnityEngine.Debug.Log($"[ProcessChainReaction] CROSS_THREE effect affected {blocksaffectedbyeffect.Count} blocks");
                    break;
                case EMATCHTYPE.CROSS_FOUR:
                    blocksaffectedbyeffect.AddRange(ProcessCrossMatch(-3, 4, currentblock, matchblockdic));
                    UnityEngine.Debug.Log($"[ProcessChainReaction] CROSS_FOUR effect affected {blocksaffectedbyeffect.Count} blocks");
                    break;
                case EMATCHTYPE.CROSS_FIVE:
                    blocksaffectedbyeffect.AddRange(ProcessCrossMatch(-6, 7, currentblock, matchblockdic));
                    UnityEngine.Debug.Log($"[ProcessChainReaction] CROSS_FIVE effect affected {blocksaffectedbyeffect.Count} blocks");
                    break;
            }

            foreach (var affectedblock in blocksaffectedbyeffect)
            {
                bool wasadded = finaldestroylist.Add(affectedblock);
                UnityEngine.Debug.Log($"[ProcessChainReaction] Affected block at ({affectedblock.GetPoint().x}, {affectedblock.GetPoint().y}): {affectedblock.GetBlockMatchTypes()}, Added: {wasadded}");

                // 새로운 특수 블록이고 아직 처리되지 않았다면 큐에 추가
                if (IsSpecialBlock(affectedblock) && !processedblocks.Contains(affectedblock))
                {
                    // FIVE 블록을 위한 색상 상속
                    EBLOCKCOLORTYPE colortoinherit;
                    if (affectedblock.GetBlockMatchTypes() == EMATCHTYPE.FIVE)
                    {
                        // FIVE 블록은 현재 블록의 색상을 상속
                        colortoinherit = inheritedcolor;
                    }
                    else
                    {
                        // 다른 특수 블록은 자신의 색상 사용
                        colortoinherit = affectedblock.GetBlockColorTypes();
                    }
                    processqueue.Enqueue((affectedblock, colortoinherit));
                    UnityEngine.Debug.Log($"[ProcessChainReaction] Enqueued special block at ({affectedblock.GetPoint().x}, {affectedblock.GetPoint().y})");
                }
            }
        }

        UnityEngine.Debug.Log($"[ProcessChainReaction] Final destroy list count: {finaldestroylist.Count}");
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

        // null 그리드 방어 코드
        if (matchblockdic == null)
        {
            return breaklist;
        }

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
        // null 그리드 방어 코드
        if (matchblockdic == null)
        {
            return new List<UI_Match_Block>();
        }

        var colorlist = matchblockdic.Where(x => x.Value != null).Where(x => x.Value.GetBlockColorTypes() == colortype).Select(x => x.Value).ToList();
        return colorlist;
    }

    private List<UI_Match_Block> ProcessCrossMatch(int startindex, int endindex, UI_Match_Block boomblock, Dictionary<(int, int), UI_Match_Block> matchblockdic)
    {
        var breaklist = new List<UI_Match_Block>();

        // null 그리드 방어 코드
        if (matchblockdic == null)
        {
            return breaklist;
        }

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