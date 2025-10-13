using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 매치된 블록들을 분석하여 매치 타입(3-매치, 4-매치, 5-매치, 십자)을 분류하는 클래스
/// </summary>
public class MatchTypeClassifier
{
    public EMATCHTYPE ClassifyMatchType(List<UI_Match_Block> xlist, List<UI_Match_Block> ylist)
    {
        // Handle null or empty lists
        if (xlist == null) xlist = new List<UI_Match_Block>();
        if (ylist == null) ylist = new List<UI_Match_Block>();

        // Filter out null blocks from both lists
        xlist = xlist.Where(block => block != null).ToList();
        ylist = ylist.Where(block => block != null).ToList();

        var crosslist = new List<UI_Match_Block>();
        crosslist.AddRange(xlist);
        crosslist.AddRange(ylist);

        // If no blocks at all, cannot classify
        if (crosslist.Count == 0)
            return EMATCHTYPE.THREE; // Default fallback

        var firstblockcolor = crosslist[0].GetBlockColorTypes();
        bool allsamecolor = crosslist.All(block => block.GetBlockColorTypes() == firstblockcolor);

        // 1순위: CROSS 패턴 검증 (교차점이 정확히 1개, 같은 색상, 교차점이 중앙)
        if (allsamecolor && xlist.Count > 0 && ylist.Count > 0)
        {
            var intersection = xlist.Intersect(ylist).ToList();

            // 교차점이 없는 경우 (평행선): 각각 독립적으로 분류
            if (intersection.Count == 0)
            {
                // xlist 기준으로만 분류
                if (xlist.Count >= 5)
                    return EMATCHTYPE.FIVE;
                if (xlist.Count == 4)
                    return EMATCHTYPE.FORE_LEFTRIGHT;
                return EMATCHTYPE.THREE;
            }

            // CROSS는 교차점이 정확히 1개 있어야 함
            if (intersection.Count == 1)
            {
                var intersectionpoint = intersection[0].GetPoint();

                // 교차점이 리스트 내에서 몇 번째 인덱스인지 확인
                var intersectionblock = intersection[0];
                int xindex = xlist.IndexOf(intersectionblock);
                int yindex = ylist.IndexOf(intersectionblock);

                // CROSS 조건: 교차점이 양쪽 리스트 모두에서 중간 인덱스에 위치해야 함
                // (정십자, T자형, 역T자형: 둘 다 중간 인덱스)
                // L자형, ㄱ자형, ㄴ자형: 한쪽 이상이 끝 인덱스 → FIVE
                bool isxmiddle = xindex > 0 && xindex < xlist.Count - 1;
                bool isymiddle = yindex > 0 && yindex < ylist.Count - 1;

                if (isxmiddle && isymiddle)
                {
                    // CROSS 판정: 양쪽이 비슷한 길이일 때만 십자로 판정
                    // 5H + 5V 또는 5H + 4V 또는 4H + 5V 등
                    if (xlist.Count >= 5 && ylist.Count >= 5)
                    {
                        return EMATCHTYPE.CROSS_FIVE;
                    }
                    if (xlist.Count == 4 && ylist.Count == 4)
                    {
                        return EMATCHTYPE.CROSS_FOUR;
                    }
                    if (xlist.Count == 3 && ylist.Count == 3)
                    {
                        return EMATCHTYPE.CROSS_THREE;
                    }
                }
            }
        }

        // 2순위: 5-매치 이상 판정 (고유 블록 5개 이상, 같은 색상)
        var uniqueblocks = xlist.Union(ylist).Distinct().ToList();
        if (allsamecolor && uniqueblocks.Count >= 5)
        {
            return EMATCHTYPE.FIVE;
        }

        // 3순위: 4-매치 일직선
        if (xlist.Count == 4)
        {
            return EMATCHTYPE.FORE_LEFTRIGHT;
        }
        if (ylist.Count == 4)
        {
            return EMATCHTYPE.FORE_UPDOWN;
        }

        // 4순위: 3-매치
        return EMATCHTYPE.THREE;
    }

    /// <summary>
    /// 주어진 블록이 리스트의 끝(edge)이 아닌 위치에 있는지 확인
    /// 끝이 아니면 true (중앙 또는 중간 어딘가)
    /// 첫 번째 또는 마지막 블록이면 false
    /// </summary>
    private bool IsNotEdgeBlock(List<UI_Match_Block> blocklist, (int x, int y) point)
    {
        if (blocklist.Count < 3)
            return false;

        // 가로 정렬인지 세로 정렬인지 확인
        var firstpoint = blocklist[0].GetPoint();
        var lastpoint = blocklist[blocklist.Count - 1].GetPoint();

        if (firstpoint.y == lastpoint.y)
        {
            // 가로 정렬: x 좌표로 끝 확인
            var xpositions = blocklist.Select(b => b.GetPoint().x).OrderBy(x => x).ToList();
            int minx = xpositions.First();
            int maxx = xpositions.Last();

            // 끝이 아니면 true
            return point.x != minx && point.x != maxx;
        }
        else
        {
            // 세로 정렬: y 좌표로 끝 확인
            var ypositions = blocklist.Select(b => b.GetPoint().y).OrderBy(y => y).ToList();
            int miny = ypositions.First();
            int maxy = ypositions.Last();

            // 끝이 아니면 true
            return point.y != miny && point.y != maxy;
        }
    }

    /// <summary>
    /// 주어진 블록이 리스트의 중앙에 위치하는지 확인
    /// 홀수 개: 정확히 중앙 블록
    /// 짝수 개: 중앙 2개 중 하나
    /// </summary>
    private bool IsCenterBlock(List<UI_Match_Block> blocklist, (int x, int y) point)
    {
        if (blocklist.Count < 3)
            return false;

        // 가로 정렬인지 세로 정렬인지 확인
        var firstpoint = blocklist[0].GetPoint();
        var lastpoint = blocklist[blocklist.Count - 1].GetPoint();

        if (firstpoint.y == lastpoint.y)
        {
            // 가로 정렬: x 좌표로 중앙 확인
            var xpositions = blocklist.Select(b => b.GetPoint().x).OrderBy(x => x).ToList();
            int middleindex1 = (xpositions.Count - 1) / 2;
            int middleindex2 = xpositions.Count / 2;

            return point.x == xpositions[middleindex1] || point.x == xpositions[middleindex2];
        }
        else
        {
            // 세로 정렬: y 좌표로 중앙 확인
            var ypositions = blocklist.Select(b => b.GetPoint().y).OrderBy(y => y).ToList();
            int middleindex1 = (ypositions.Count - 1) / 2;
            int middleindex2 = ypositions.Count / 2;

            return point.y == ypositions[middleindex1] || point.y == ypositions[middleindex2];
        }
    }
}