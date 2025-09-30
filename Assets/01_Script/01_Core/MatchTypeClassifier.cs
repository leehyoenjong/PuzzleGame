using System.Collections.Generic;
using System.Linq;

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