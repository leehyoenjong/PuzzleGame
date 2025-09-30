using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        (int x, int y) spawnpoint = CalculateSpawnPoint(xlist, ylist, matchtype, usermoveblock);
        EBLOCKCOLORTYPE color = DetermineColor(xlist, ylist, matchtype);

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
        if (usermoveblock != null)
        {
            return usermoveblock.GetPoint();
        }

        switch (matchtype)
        {
            case EMATCHTYPE.FORE_LEFTRIGHT:
            case EMATCHTYPE.FORE_UPDOWN:
            case EMATCHTYPE.FIVE:
                return CalculateMiddlePoint(xlist.Count > 0 ? xlist : ylist);

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
        var xmax = slotlist.Max(block => block.GetPoint().x);
        var xmin = slotlist.Min(block => block.GetPoint().x);
        var ymax = slotlist.Max(block => block.GetPoint().y);
        var ymin = slotlist.Min(block => block.GetPoint().y);

        return (Mathf.RoundToInt((xmin + xmax) * 0.5f), Mathf.RoundToInt((ymin + ymax) * 0.5f));
    }

    private (int x, int y) CalculateIntersectionPoint(List<UI_Match_Block> xlist, List<UI_Match_Block> ylist)
    {
        var commonslot = xlist.Intersect(ylist).FirstOrDefault();
        if (commonslot != null)
        {
            return commonslot.GetPoint();
        }
        return (-1, -1);
    }
}