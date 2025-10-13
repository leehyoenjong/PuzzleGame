using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 매치 타입에 따라 특수 블록 생성 요청을 만드는 팩토리 클래스
/// 4-매치, 5-매치, 십자 패턴에 대한 특수 블록 생성을 담당합니다.
/// </summary>
public class SpecialBlockFactory
{
    public SpecialBlockCreationRequest? CreateRequest(
        List<UI_Match_Block> xlist,
        List<UI_Match_Block> ylist,
        EMATCHTYPE matchtype,
        UI_Match_Block usermoveblock = null)
    {
        if (matchtype == EMATCHTYPE.THREE)
        {
            return null;
        }

        // null 리스트 방어 코드
        if (xlist == null && ylist == null)
        {
            return null;
        }

        // 유효한 블록만 필터링 (위치가 (-1, -1)인 블록 제거)
        var validxlist = xlist?.Where(b => b != null && b.GetPoint() != (-1, -1)).ToList() ?? new List<UI_Match_Block>();
        var validylist = ylist?.Where(b => b != null && b.GetPoint() != (-1, -1)).ToList() ?? new List<UI_Match_Block>();

        // 유효한 블록이 없으면 생성 불가
        if (validxlist.Count == 0 && validylist.Count == 0)
        {
            Debug.LogWarning($"[SpecialBlockFactory] 유효한 블록이 없음! 모든 블록이 (-1, -1) 상태. MatchType: {matchtype}");
            return null;
        }

        (int x, int y) spawnpoint = CalculateSpawnPoint(validxlist, validylist, matchtype, usermoveblock);
        EBLOCKCOLORTYPE color = DetermineColor(validxlist, validylist, matchtype);

        // 디버그 로그: 특수 블록 생성 정보
        LogSpecialBlockCreation(validxlist, validylist, matchtype, usermoveblock, spawnpoint, color);

        return new SpecialBlockCreationRequest
        {
            Point = spawnpoint,
            Type = matchtype,
            Color = color
        };
    }

    private (int x, int y) CalculateSpawnPoint(
        List<UI_Match_Block> xlist,
        List<UI_Match_Block> ylist,
        EMATCHTYPE matchtype,
        UI_Match_Block usermoveblock)
    {
        // usermoveblock이 유효하고 실제 매치 리스트에 포함되어 있을 때만 사용
        if (usermoveblock != null)
        {
            var usermovepoint = usermoveblock.GetPoint();

            // (-1, -1)은 무효한 위치이므로 무시
            if (usermovepoint == (-1, -1))
            {
                // 무시하고 중간점 계산으로 fallback
            }
            else
            {
                bool isinxlist = xlist.Contains(usermoveblock);
                bool isinylist = ylist.Contains(usermoveblock);

                if (isinxlist || isinylist)
                {
                    return usermovepoint;
                }
                // 리스트에 없으면 무시하고 중간점 계산으로 fallback
            }
        }

        switch (matchtype)
        {
            case EMATCHTYPE.FORE_LEFTRIGHT:
            case EMATCHTYPE.FORE_UPDOWN:
                return CalculateMiddlePoint(xlist.Count > 0 ? xlist : ylist);

            case EMATCHTYPE.FIVE:
                // FIVE는 모든 고유 블록을 합쳐서 중간점 계산 (L자형 포함)
                var allblocks = xlist.Union(ylist).Distinct().ToList();
                return CalculateMiddlePoint(allblocks);

            case EMATCHTYPE.CROSS_THREE:
            case EMATCHTYPE.CROSS_FOUR:
            case EMATCHTYPE.CROSS_FIVE:
                return CalculateIntersectionPoint(xlist, ylist);

            default:
                return (0, 0);
        }
    }

    private EBLOCKCOLORTYPE DetermineColor(
        List<UI_Match_Block> xlist,
        List<UI_Match_Block> ylist,
        EMATCHTYPE matchtype)
    {
        if (matchtype == EMATCHTYPE.FIVE)
        {
            return EBLOCKCOLORTYPE.FIVE;
        }

        var slotlist = xlist.Count > 0 ? xlist : ylist;
        return slotlist[0].GetBlockColorTypes();
    }

    private (int x, int y) CalculateMiddlePoint(List<UI_Match_Block> slotlist)
    {
        if (slotlist == null || slotlist.Count == 0)
        {
            Debug.LogError("[SpecialBlockFactory] CalculateMiddlePoint: 빈 리스트 전달됨!");
            return (-1, -1);
        }

        var xmax = slotlist.Max(block => block.GetPoint().x);
        var xmin = slotlist.Min(block => block.GetPoint().x);
        var ymax = slotlist.Max(block => block.GetPoint().y);
        var ymin = slotlist.Min(block => block.GetPoint().y);

        return ((xmin + xmax) / 2, (ymin + ymax) / 2);
    }

    private (int x, int y) CalculateIntersectionPoint(List<UI_Match_Block> xlist, List<UI_Match_Block> ylist)
    {
        var commonslot = xlist.Intersect(ylist).FirstOrDefault();
        if (commonslot != null)
        {
            return commonslot.GetPoint();
        }

        // Fallback: 교차점을 찾지 못하면 전체 블록의 중간점 사용
        Debug.LogWarning($"[SpecialBlockFactory] 교차점을 찾지 못함! Fallback으로 중간점 계산. xlist={xlist.Count}, ylist={ylist.Count}");
        var allblocks = xlist.Union(ylist).Distinct().ToList();
        return CalculateMiddlePoint(allblocks);
    }

    private void LogSpecialBlockCreation(
        List<UI_Match_Block> xlist,
        List<UI_Match_Block> ylist,
        EMATCHTYPE matchtype,
        UI_Match_Block usermoveblock,
        (int x, int y) spawnpoint,
        EBLOCKCOLORTYPE color)
    {
        var xpositions = string.Join(", ", xlist.Select(b => $"({b.GetPoint().x},{b.GetPoint().y})"));
        var ypositions = string.Join(", ", ylist.Select(b => $"({b.GetPoint().x},{b.GetPoint().y})"));
        var usermove = usermoveblock != null ? $"({usermoveblock.GetPoint().x},{usermoveblock.GetPoint().y})" : "NULL";

        // 모든 고유 블록 위치 계산
        var allblocks = xlist.Union(ylist).Distinct().ToList();
        var allpositions = string.Join(", ", allblocks.Select(b => $"({b.GetPoint().x},{b.GetPoint().y})"));

        // 위치 범위 계산
        if (allblocks.Count > 0)
        {
            var xmin = allblocks.Min(b => b.GetPoint().x);
            var xmax = allblocks.Max(b => b.GetPoint().x);
            var ymin = allblocks.Min(b => b.GetPoint().y);
            var ymax = allblocks.Max(b => b.GetPoint().y);

            Debug.Log($"[SpecialBlockFactory] 특수 블록 생성:\n" +
                      $"  MatchType: {matchtype}\n" +
                      $"  XList ({xlist.Count}개): [{xpositions}]\n" +
                      $"  YList ({ylist.Count}개): [{ypositions}]\n" +
                      $"  All Unique ({allblocks.Count}개): [{allpositions}]\n" +
                      $"  UserMoveBlock: {usermove}\n" +
                      $"  Position Range: X({xmin}~{xmax}), Y({ymin}~{ymax})\n" +
                      $"  Expected Middle: ({(xmin + xmax) / 2}, {(ymin + ymax) / 2})\n" +
                      $"  Actual SpawnPoint: ({spawnpoint.x}, {spawnpoint.y})\n" +
                      $"  Color: {color}");

            // 경고: 예상 위치와 실제 위치가 다를 경우
            var expectedmiddle = ((xmin + xmax) / 2, (ymin + ymax) / 2);
            if (usermoveblock == null && spawnpoint != expectedmiddle)
            {
                Debug.LogWarning($"[SpecialBlockFactory] ⚠️ 위치 불일치 감지!\n" +
                                $"  예상 중간점: {expectedmiddle}\n" +
                                $"  실제 생성점: {spawnpoint}\n" +
                                $"  MatchType: {matchtype}");
            }
        }
    }
}