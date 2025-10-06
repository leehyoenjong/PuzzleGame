using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 매치된 블록들을 분석하여 매치 타입(3-매치, 4-매치, 5-매치, 십자)을 분류하는 클래스
/// </summary>
public class MatchTypeClassifier
{
    public EMATCHTYPE ClassifyMatchType(List<UI_Match_Block> xlist, List<UI_Match_Block> ylist)
    {
        var crosslist = new List<UI_Match_Block>();
        crosslist.AddRange(xlist);
        crosslist.AddRange(ylist);

        var firstblockcolor = crosslist[0].GetBlockColorTypes();
        bool allsamecolor = crosslist.All(block => block.GetBlockColorTypes() == firstblockcolor);

        // 1순위: CROSS 패턴 검증 (교차점이 정확히 1개, 같은 색상, 교차점이 중앙)
        if (allsamecolor && xlist.Count > 0 && ylist.Count > 0)
        {
            var intersection = xlist.Intersect(ylist).ToList();

            // CROSS는 교차점이 정확히 1개 있어야 함
            if (intersection.Count == 1)
            {
                var intersectionpoint = intersection[0].GetPoint();

                // 교차점이 xlist와 ylist의 중앙에 위치하는지 확인
                bool iscenterofx = IsCenterBlock(xlist, intersectionpoint);
                bool iscenterofy = IsCenterBlock(ylist, intersectionpoint);

                // 교차점이 둘 다의 중앙이어야 진짜 CROSS
                if (iscenterofx && iscenterofy)
                {
                    if (xlist.Count == 3 && ylist.Count == 3)
                    {
                        return EMATCHTYPE.CROSS_THREE;
                    }
                    if (xlist.Count == 4 && ylist.Count == 4)
                    {
                        return EMATCHTYPE.CROSS_FOUR;
                    }
                    if (xlist.Count >= 5 && ylist.Count >= 5)
                    {
                        return EMATCHTYPE.CROSS_FIVE;
                    }
                }
            }
        }

        // 2순위: 5-매치 판정 (고유 블록 5개 이상, 같은 색상)
        var uniqueblocks = xlist.Union(ylist).Distinct().ToList();
        if (allsamecolor && uniqueblocks.Count >= 5)
        {
            return EMATCHTYPE.FIVE;
        }

        // 3순위: 4-매치
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