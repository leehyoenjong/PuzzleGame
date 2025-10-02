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

        if (allsamecolor)
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

        if (xlist.Count == 4)
        {
            return EMATCHTYPE.FORE_LEFTRIGHT;
        }
        if (ylist.Count == 4)
        {
            return EMATCHTYPE.FORE_UPDOWN;
        }

        if (xlist.Count >= 5 || ylist.Count >= 5)
        {
            return EMATCHTYPE.FIVE;
        }

        return EMATCHTYPE.THREE;
    }
}