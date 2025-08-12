using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public static event Action<int, int, EMATCHTYPE> _match_complte_createblock_event;//매치 성공 후 생성되는 개별 블록 이벤트 
    public static event Action<int, int> _match_complte_block_event;//매치 성공한 블럭들 이벤트 처리

    void OnEnable()
    {
        MatchFiledManager._match_complte_event += MatchComplte;
    }

    void OnDisable()
    {
        MatchFiledManager._match_complte_event -= MatchComplte;
    }

    void MatchComplte(List<UI_Match_Block> x_list, List<UI_Match_Block> y_list)
    {
        var matchtype = GetMatchTypes(x_list, y_list);

        (int x, int y) middlepoint = (0, 0);

        switch (matchtype)
        {
            case EMATCHTYPE.THREE:
            case EMATCHTYPE.FORE:
            case EMATCHTYPE.FIVE:
                var slotlist = x_list.Count > 0 ? x_list : y_list;
                middlepoint = GetMiddlePoint(slotlist);
                break;
            case EMATCHTYPE.CROSS_THREE:
            case EMATCHTYPE.CROSS_FOUR:
            case EMATCHTYPE.CROSS_FIVE:
                middlepoint = GetMiddlePoint(x_list, y_list);
                break;
        }
        SetMatchBlock(x_list, y_list);
        _match_complte_createblock_event?.Invoke(middlepoint.Item1, middlepoint.Item2, matchtype);
    }

    void SetMatchBlock(List<UI_Match_Block> xlist, List<UI_Match_Block> ylist)
    {
        //매치된 블럭들 처리하기
        for (int i = 0; i < xlist.Count; i++)
        {
            var point = xlist[i].GetPoint();
            _match_complte_block_event?.Invoke(point.x, point.y);
        }

        for (int i = 0; i < ylist.Count; i++)
        {
            var point = ylist[i].GetPoint();
            _match_complte_block_event?.Invoke(point.x, point.y);
        }
    }

    (int, int) GetMiddlePoint(List<UI_Match_Block> slotlist)
    {
        var xmax = slotlist.Max(x => x.GetPoint().x);
        var xmin = slotlist.Min(x => x.GetPoint().x);

        var ymax = slotlist.Max(x => x.GetPoint().y);
        var ymin = slotlist.Min(x => x.GetPoint().y);
        return (Mathf.RoundToInt((xmin + xmax) * 0.5f), Mathf.RoundToInt((ymin + ymax) * 0.5f));
    }


    (int, int) GetMiddlePoint(List<UI_Match_Block> xslotlist, List<UI_Match_Block> yslotlist)
    {
        var commonSlot = xslotlist.Intersect(yslotlist).FirstOrDefault();
        if (commonSlot != null)
        {
            return commonSlot.GetPoint();
        }
        return (-1, -1); // Or handle as an error/exception
    }


    EMATCHTYPE GetMatchTypes(List<UI_Match_Block> x_list, List<UI_Match_Block> y_list)
    {
        if (x_list.Count == 3 && y_list.Count == 3)
        {
            return EMATCHTYPE.CROSS_THREE;
        }
        if (x_list.Count == 4 && y_list.Count == 4)
        {
            return EMATCHTYPE.CROSS_FOUR;
        }
        if (x_list.Count == 5 && y_list.Count == 5)
        {
            return EMATCHTYPE.CROSS_FIVE;
        }

        if (x_list.Count == 4 || y_list.Count == 4)
        {
            return EMATCHTYPE.FORE;
        }
        if (x_list.Count == 5 || y_list.Count == 5)
        {
            return EMATCHTYPE.FIVE;
        }

        return EMATCHTYPE.THREE;
    }
}