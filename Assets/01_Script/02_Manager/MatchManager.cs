using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public struct SpecialBlockCreationRequest
{
    public (int x, int y) Point;
    public EMATCHTYPE Type;
    public EBLOCKCOLORTYPE Color;
}

public class MatchManager : MonoBehaviour
{
    public static event Action<int, int, EMATCHTYPE, EBLOCKCOLORTYPE> _match_complte_createblock_event;//매치 성공 후 생성되는 개별 블록 이벤트 
    public static event Action<int, int> _match_complte_block_event;//매치 성공한 블럭들 이벤트 처리
    public static event Action _user_move_match_complte;//유저가 블록 이동 후 매치 성공했을때 
    List<(int x, int y)> simurationkeylist = new List<(int x, int y)>
    {
        (0, 0),//오리지널
        (0, -1),//상
        (0, 1),//하
        (-1, 0),//좌
        (1, 0)//우
    };


    //매치 진행중인지 체크
    bool _ismatching;
    bool CheckMatching() => _ismatching;

    void OnEnable()
    {
        MatchFiledManager._match_complte_event += AllBlockMatch;
        MatchFiledManager._block_move_event += UserMoveBlockMatch;
        MatchFiledManager._match_setting_check_list.Add(CheckMatching);
        MatchFiledManager._matchsimuration_check_event += SimulationBlockMatch;
    }

    void OnDisable()
    {
        MatchFiledManager._match_complte_event -= AllBlockMatch;
        MatchFiledManager._block_move_event -= UserMoveBlockMatch;
        MatchFiledManager._matchsimuration_check_event -= SimulationBlockMatch;
    }

    void MatchComplte(List<UI_Match_Block> x_list, List<UI_Match_Block> y_list, UI_Match_Block usermoveblock = null)
    {
        var matchtype = GetMatchTypes(x_list, y_list);
        if (matchtype == EMATCHTYPE.THREE)
        {
            SetMatchBlock(x_list, y_list);
            return;
        }

        (int x, int y) middlepoint = (0, 0);
        EBLOCKCOLORTYPE _color = EBLOCKCOLORTYPE.MAX;
        switch (matchtype)
        {
            case EMATCHTYPE.FORE_UPDOWN:
            case EMATCHTYPE.FORE_LEFTRIGHT:
            case EMATCHTYPE.FIVE:
                var slotlist = x_list.Count > 0 ? x_list : y_list;
                middlepoint = usermoveblock == null ? GetMiddlePoint(slotlist) : usermoveblock.GetPoint();
                _color = matchtype == EMATCHTYPE.FIVE ? EBLOCKCOLORTYPE.FIVE : slotlist[0].GetBlockColorTypes();
                break;
            case EMATCHTYPE.CROSS_THREE:
            case EMATCHTYPE.CROSS_FOUR:
            case EMATCHTYPE.CROSS_FIVE:
                middlepoint = usermoveblock == null ? GetMiddlePoint(x_list, y_list) : usermoveblock.GetPoint();
                _color = x_list[0].GetBlockColorTypes();
                break;
        }
        SetMatchBlock(x_list, y_list);

        //특수블록으로 제거했을땐 블록이 생성되지 않도록 수정
        _match_complte_createblock_event?.Invoke(middlepoint.x, middlepoint.y, matchtype, _color);
    }

    void SetMatchBlock(List<UI_Match_Block> xlist, List<UI_Match_Block> ylist)
    {
        xlist.AddRange(ylist);
        var total = xlist.Distinct().ToList();

        //매치된 블럭들 처리하기
        for (int i = 0; i < total.Count; i++)
        {
            var point = total[i].GetPoint();
            _match_complte_block_event?.Invoke(point.x, point.y);
        }
    }

    (int x, int y) GetMiddlePoint(List<UI_Match_Block> slotlist)
    {
        var xmax = slotlist.Max(x => x.GetPoint().x);
        var xmin = slotlist.Min(x => x.GetPoint().x);

        var ymax = slotlist.Max(x => x.GetPoint().y);
        var ymin = slotlist.Min(x => x.GetPoint().y);
        return (Mathf.RoundToInt((xmin + xmax) * 0.5f), Mathf.RoundToInt((ymin + ymax) * 0.5f));
    }

    (int x, int y) GetMiddlePoint(List<UI_Match_Block> xslotlist, List<UI_Match_Block> yslotlist)
    {
        var commonSlot = xslotlist.Intersect(yslotlist).FirstOrDefault();
        if (commonSlot != null)
        {
            return commonSlot.GetPoint();
        }
        return (-1, -1);
    }

    EMATCHTYPE GetMatchTypes(List<UI_Match_Block> x_list, List<UI_Match_Block> y_list)
    {
        var crosslist = new List<UI_Match_Block>();
        crosslist.AddRange(x_list);
        crosslist.AddRange(y_list);

        var firstblockcolor = crosslist[0].GetBlockColorTypes();
        bool allsamecolor = crosslist.All(block => block.GetBlockColorTypes() == firstblockcolor);

        if (allsamecolor)
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
        }


        if (x_list.Count == 4)
        {
            return EMATCHTYPE.FORE_LEFTRIGHT;
        }
        if (y_list.Count == 4)
        {
            return EMATCHTYPE.FORE_UPDOWN;
        }

        if (x_list.Count == 5 || y_list.Count == 5)
        {
            return EMATCHTYPE.FIVE;
        }

        return EMATCHTYPE.THREE;
    }

    bool AllBlockMatch(Dictionary<(int, int), UI_Match_Block> matchblockdic, int width, int height)
    {
        bool successmatch = false;
        _ismatching = true;
        //블록이 아닌 슬롯의 정보를 저장하여 위치값 계산할 예정
        var maxcount = width * height;
        int key_y = 0;
        int key_x = 0;

        List<UI_Match_Block> xlist = new List<UI_Match_Block>();
        List<UI_Match_Block> ylist = new List<UI_Match_Block>();

        for (int i = 0; i < maxcount; i++)
        {
            var matchresult = GetMatchBlock(key_x, key_y, width, height, matchblockdic);

            //매치 성공
            if (matchresult.matchblocklist_x.Count >= 3 || matchresult.matchblocklist_y.Count >= 3)
            {
                //타입별 매칭 처리
                GetMatchTypeFuction(matchresult.matchblocklist_x, matchblockdic);
                GetMatchTypeFuction(matchresult.matchblocklist_y, matchblockdic);

                //매칭 성공하면 위치는 고정하고 매칭 성공 처리 진행
                xlist.AddRange(matchresult.matchblocklist_x);
                ylist.AddRange(matchresult.matchblocklist_y);

                //매치를 성공하고도 return하지 않는 이유는 모든 블록 매치를 체크해야하기 때문
                successmatch = true;
            }

            //매치 실패 시 다음 칸으로 이동
            key_x++;
            //크기만큼 도달했다면 다 확인한 것이기 때문에 y값을 증가시키고 x값 초기화
            if (key_x >= width)
            {
                key_x = 0;
                key_y++;
            }
        }
        if (successmatch)
        {
            MatchComplte(xlist.Distinct().ToList(), ylist.Distinct().ToList());
        }
        _ismatching = false;
        return successmatch;
    }

    /// <summary>
    /// 유저가 직접 이동해서 매칭하는 것
    /// </summary>
    async void UserMoveBlockMatch(Dictionary<(int, int), UI_Match_Block> matchblockdic, UI_Match_Block pointdown, UI_Match_Block pointenter, int width, int height)
    {
        _ismatching = true;
        var downpos = pointdown.GetPos();
        var enterpos = pointenter.GetPos();

        var downpoint = pointdown.GetPoint();
        var enterpoint = pointenter.GetPoint();

        pointdown.Swap(pointenter);

        await UniTask.WaitForSeconds(0.4f, cancellationToken: this.GetCancellationTokenOnDestroy());

        //색상 공략 5매치 일 경우 처리
        if (pointdown.GetBlockColorTypes() == EBLOCKCOLORTYPE.FIVE || pointenter.GetBlockColorTypes() == EBLOCKCOLORTYPE.FIVE)
        {
            //타입별 매칭 처리
            var list = new List<UI_Match_Block>();
            list.Add(pointdown);
            list.Add(pointenter);
            GetMatchTypeFuction(list, matchblockdic);

            //매칭 성공하면 위치는 고정하고 매칭 성공 처리 진행
            MatchComplte(list, new List<UI_Match_Block>(), pointenter);
            _user_move_match_complte?.Invoke();
            _ismatching = false;
            return;
        }

        var matchresult = GetMatchBlock(downpoint.x, downpoint.y, width, height, matchblockdic, true);
        var downpointcheck = matchresult.matchblocklist_x.Count >= 3 || matchresult.matchblocklist_y.Count >= 3;

        var matchresult_enter = GetMatchBlock(enterpoint.x, enterpoint.y, width, height, matchblockdic, true);
        var entercheck = matchresult_enter.matchblocklist_x.Count >= 3 || matchresult_enter.matchblocklist_y.Count >= 3;

        // 매치가 전혀 없으면 원위치하고 종료
        if (downpointcheck == false && entercheck == false)
        {
            pointdown.ChangePoint(downpoint.x, downpoint.y, downpos);
            pointenter.ChangePoint(enterpoint.x, enterpoint.y, enterpos);
            _ismatching = false;
            return;
        }

        // --- 1단계: 분석 (정보 수집) ---
        var creationRequests = new List<SpecialBlockCreationRequest>();
        if (downpointcheck)
        {
            var request = GetSpecialBlockCreationRequest(matchresult.matchblocklist_x, matchresult.matchblocklist_y, pointenter);
            if (request.HasValue) creationRequests.Add(request.Value);
        }
        if (entercheck)
        {
            var request = GetSpecialBlockCreationRequest(matchresult_enter.matchblocklist_x, matchresult_enter.matchblocklist_y, pointdown);
            if (request.HasValue) creationRequests.Add(request.Value);
        }

        var blocksToDestroy = new List<UI_Match_Block>();
        blocksToDestroy.AddRange(matchresult.matchblocklist_x);
        blocksToDestroy.AddRange(matchresult.matchblocklist_y);
        blocksToDestroy.AddRange(matchresult_enter.matchblocklist_x);
        blocksToDestroy.AddRange(matchresult_enter.matchblocklist_y);

        var distinctBlocksToDestroy = blocksToDestroy.Distinct().ToList();

        // 기존 특수 블록 효과 처리 (문제점 2 해결)
        var finalBlocksToDestroy = ProcessChainReaction(distinctBlocksToDestroy, matchblockdic);


        // --- 2단계: 파괴 ---
        SetMatchBlock(finalBlocksToDestroy, new List<UI_Match_Block>());

        // --- 3단계: 생성 (문제점 1 해결) ---
        foreach (var req in creationRequests)
        {
            _match_complte_createblock_event?.Invoke(req.Point.x, req.Point.y, req.Type, req.Color);
        }

        // 최종 처리
        _user_move_match_complte?.Invoke();
        _ismatching = false;
    }

    /// <summary>
    /// 매치 그룹을 평가해서 생성할 특수 블록 정보를 반환하는 함수
    /// </summary>
    SpecialBlockCreationRequest? GetSpecialBlockCreationRequest(List<UI_Match_Block> x_list, List<UI_Match_Block> y_list, UI_Match_Block usermoveblock)
    {
        var matchtype = GetMatchTypes(x_list, y_list);
        if (matchtype == EMATCHTYPE.THREE)
        {
            // 3매치는 특수 블록을 생성하지 않음
            return null;
        }

        (int x, int y) middlepoint = (0, 0);
        EBLOCKCOLORTYPE _color = EBLOCKCOLORTYPE.MAX;
        switch (matchtype)
        {
            case EMATCHTYPE.FORE_LEFTRIGHT:
            case EMATCHTYPE.FORE_UPDOWN:
            case EMATCHTYPE.FIVE:
                var slotlist = x_list.Count > 0 ? x_list : y_list;
                middlepoint = usermoveblock.GetPoint();
                _color = matchtype == EMATCHTYPE.FIVE ? EBLOCKCOLORTYPE.FIVE : slotlist[0].GetBlockColorTypes();
                break;
            case EMATCHTYPE.CROSS_THREE:
            case EMATCHTYPE.CROSS_FOUR:
            case EMATCHTYPE.CROSS_FIVE:
                middlepoint = usermoveblock.GetPoint();
                _color = x_list[0].GetBlockColorTypes();
                break;
        }

        return new SpecialBlockCreationRequest { Point = middlepoint, Type = matchtype, Color = _color };
    }

    bool SimulationBlockMatch(Dictionary<(int, int), UI_Match_Block> matchblockdic, int width, int height)
    {
        _ismatching = true;


        //블록이 아닌 슬롯의 정보를 저장하여 위치값 계산할 예정
        var maxcount = matchblockdic.Count;
        var simurationcount = simurationkeylist.Count;
        int key_y = 0;
        int key_x = 0;

        for (int i = 0; i < maxcount; i++)
        {
            for (int point = 0; point < simurationcount; point++)
            {
                if (matchblockdic.TryGetValue((key_x, key_y), out var origin) == false)
                {
                    continue;
                }

                var nextkeyx = key_x + simurationkeylist[point].x;
                var nextkeyy = key_y + simurationkeylist[point].y;
                if (matchblockdic.TryGetValue((nextkeyx, nextkeyy), out var nextblock) == false)
                {
                    continue;
                }

                //시뮬레이션 이동
                origin.Swap(nextblock, true);

                //시뮬레이션 시작
                var matchresult = GetMatchBlock(nextkeyx, nextkeyy, width, height, matchblockdic, true);

                //다시 원상복구
                origin.Swap(nextblock, true);

                //매치 성공
                if (matchresult.matchblocklist_x.Count >= 3 || matchresult.matchblocklist_y.Count >= 3)
                {
                    _ismatching = false;
                    return true;
                }

                //실패시 반복
            }
            //매치 실패 시 다음 칸으로 이동
            key_x++;
            //크기만큼 도달했다면 다 확인한 것이기 때문에 y값을 증가시키고 x값 초기화
            if (key_x >= width)
            {
                key_x = 0;
                key_y++;
            }
        }

        _ismatching = false;
        return false;
    }

    EMATCHING MatingBlock((int x, int y) key, bool max, Dictionary<(int, int), UI_Match_Block> matchblockdic, List<UI_Match_Block> matching)
    {
        if (matchblockdic.ContainsKey(key) == false)
        {
            return max ? EMATCHING.MAX_MATCHEND : EMATCHING.NONE_SLOT;
        }

        if (matchblockdic[key] == null)
        {
            return EMATCHING.STOP;
        }

        if (matching.Count <= 0)
        {
            matching.Add(matchblockdic[key]);
            return EMATCHING.ING;
        }

        //이미 잡힌 매칭이 있을때
        var types = matchblockdic[key].GetBlockColorTypes();
        matching.Add(matchblockdic[key]);

        // 매칭 잡힌 타입과 현재 타입이 같은지 체크
        if (matching[0].GetBlockColorTypes() != types)
        {
            if (max)
            {
                return EMATCHING.MAX_NOMATCHEND;
            }

            return EMATCHING.STOP;
        }

        return max ? EMATCHING.MAX_MATCHEND : EMATCHING.ING;
    }

    List<UI_Match_Block> GetMatchList(bool xory, int key_x, int key_y, int startvalue, int maxcount, Dictionary<(int, int), UI_Match_Block> matchblockdic)
    {
        List<UI_Match_Block> matching = new List<UI_Match_Block>();
        List<UI_Match_Block> matchblocklist = new List<UI_Match_Block>();
        var key = (0, 0);

        for (int i = startvalue; i < maxcount; i++)
        {
            if (xory)
            {
                key = (i, key_y);
            }
            else
            {
                key = (key_x, i);
            }

            var checkmaxindex = i == maxcount - 1;
            var state = MatingBlock(key, checkmaxindex, matchblockdic, matching);

            if (state == EMATCHING.NONE_SLOT)
            {
                if (matching.Count >= 3)
                {
                    matchblocklist.AddRange(matching.GetRange(0, matching.Count));
                }
                matching.Clear();
            }

            if (checkmaxindex && matching.Count >= 3)
            {
                if (state == EMATCHING.MAX_MATCHEND)
                {
                    matchblocklist.AddRange(matching.GetRange(0, matching.Count));
                }
                else if (state == EMATCHING.MAX_NOMATCHEND)
                {
                    if (matching.Count > 3)
                    {
                        matchblocklist.AddRange(matching.GetRange(0, matching.Count - 1));
                    }
                }
            }

            if (state == EMATCHING.STOP)
            {
                if (matching.Count >= 4)
                {
                    // matching의 처음부터 Count-1개만큼 가져와서 추가
                    matchblocklist.AddRange(matching.GetRange(0, matching.Count - 1));
                    break;
                }

                if (matching.Count >= 2)
                {
                    matching.RemoveRange(0, matching.Count - 1);
                }
            }
        }

        return matchblocklist;
    }

    (List<UI_Match_Block> matchblocklist_x, List<UI_Match_Block> matchblocklist_y) GetMatchBlock(int key_x, int key_y, int width, int height, Dictionary<(int, int), UI_Match_Block> matchblockdic, bool isall = false)
    {
        List<UI_Match_Block> matchblocklist_x = GetMatchList(true, key_x, key_y, isall ? 0 : key_x, width, matchblockdic);
        List<UI_Match_Block> matchblocklist_y = GetMatchList(false, key_x, key_y, isall ? 0 : key_y, height, matchblockdic);

        //3개 이상이 아니면 매치가 되지 않은 것이기 때문에 Clear
        if (matchblocklist_x.Count <= 2)
        {
            matchblocklist_x.Clear();
        }
        if (matchblocklist_y.Count <= 2)
        {
            matchblocklist_y.Clear();
        }

        return (matchblocklist_x.Distinct().ToList(), matchblocklist_y.Distinct().ToList());
    }

    bool GetMatchTypeFuction(List<UI_Match_Block> breakblocklist, Dictionary<(int, int), UI_Match_Block> matchblockdic)
    {
        bool isspecial = false;

        // 매치되는 블록들이 모두 동일한 색상인지 확인
        if (breakblocklist.Count == 0)
        {
            return isspecial;
        }

        var maxcount = breakblocklist.Count;
        for (int i = 0; i < maxcount; i++)
        {
            var blocktypes = breakblocklist[i].GetBlockMatchTypes();
            switch (blocktypes)
            {
                //한줄 모두 파괴 
                case EMATCHTYPE.FORE_UPDOWN:
                    breakblocklist.AddRange(SetForeMatch(breakblocklist[i], matchblockdic));
                    isspecial = true;
                    break;
                //같은 색상 모두 파괴
                case EMATCHTYPE.FIVE:
                    var color = i == 0 ? breakblocklist[1].GetBlockColorTypes() : breakblocklist[0].GetBlockColorTypes();
                    breakblocklist.AddRange(SetFiveMatch(color, matchblockdic));
                    break;
                //3x3 파괴
                case EMATCHTYPE.CROSS_THREE:
                    breakblocklist.AddRange(Set_Corss_Match(-1, 2, breakblocklist[i], matchblockdic));
                    break;
                //x,y한줄씩 파괴
                case EMATCHTYPE.CROSS_FOUR:
                    breakblocklist.AddRange(Set_Corss_Match(-3, 4, breakblocklist[i], matchblockdic));
                    break;
                //전체 파괴
                case EMATCHTYPE.CROSS_FIVE:
                    breakblocklist.AddRange(Set_Corss_Match(-6, 7, breakblocklist[i], matchblockdic));
                    break;
            }
        }
        return isspecial;
    }

    List<UI_Match_Block> ProcessChainReaction(List<UI_Match_Block> initialBlocks, Dictionary<(int, int), UI_Match_Block> matchblockdic)
    {
        var finalDestroyList = new HashSet<UI_Match_Block>(initialBlocks);
        var processQueue = new Queue<(UI_Match_Block block, EBLOCKCOLORTYPE inheritedColor)>();

        // 초기 블록들을 큐에 추가
        foreach (var block in initialBlocks)
        {
            if (IsSpecialBlock(block))
            {
                processQueue.Enqueue((block, block.GetBlockColorTypes()));
            }
        }

        while (processQueue.Count > 0)
        {
            var (currentBlock, inheritedColor) = processQueue.Dequeue();
            var blocksAffectedByEffect = new List<UI_Match_Block>();

            switch (currentBlock.GetBlockMatchTypes())
            {
                case EMATCHTYPE.FORE_LEFTRIGHT:
                case EMATCHTYPE.FORE_UPDOWN:
                    blocksAffectedByEffect.AddRange(SetForeMatch(currentBlock, matchblockdic));
                    break;
                case EMATCHTYPE.FIVE:
                    // FORE 블록에서 색상을 물려받았는지 확인
                    var colorToMatch = inheritedColor != EBLOCKCOLORTYPE.FIVE ? inheritedColor : currentBlock.GetBlockColorTypes();
                    if (colorToMatch == EBLOCKCOLORTYPE.FIVE)
                    {
                        // FIVE 블록이 다른 FIVE블록과 만나 파괴되는 경우,
                        // 임의의 색상(예: RED)을 지정하거나 다른 규칙 필요. 여기서는 RED로 가정
                        colorToMatch = EBLOCKCOLORTYPE.RED;
                    }
                    blocksAffectedByEffect.AddRange(SetFiveMatch(colorToMatch, matchblockdic));
                    break;
                case EMATCHTYPE.CROSS_THREE:
                    blocksAffectedByEffect.AddRange(Set_Corss_Match(-1, 2, currentBlock, matchblockdic));
                    break;
                case EMATCHTYPE.CROSS_FOUR:
                    blocksAffectedByEffect.AddRange(Set_Corss_Match(-3, 4, currentBlock, matchblockdic));
                    break;
                case EMATCHTYPE.CROSS_FIVE:
                    blocksAffectedByEffect.AddRange(Set_Corss_Match(-6, 7, currentBlock, matchblockdic));
                    break;
            }

            foreach (var affectedBlock in blocksAffectedByEffect)
            {
                // 아직 최종 파괴 목록에 없고, 큐에도 없는 새로운 특수 블록이라면
                if (finalDestroyList.Contains(affectedBlock) == false && IsSpecialBlock(affectedBlock))
                {
                    // FIVE 블록을 위한 색상 상속
                    var colorToInherit = currentBlock.GetBlockMatchTypes() == EMATCHTYPE.FIVE ? inheritedColor : affectedBlock.GetBlockColorTypes();
                    processQueue.Enqueue((affectedBlock, colorToInherit));
                }
                finalDestroyList.Add(affectedBlock);
            }
        }
        return finalDestroyList.ToList();
    }

    bool IsSpecialBlock(UI_Match_Block block)
    {
        var type = block.GetBlockMatchTypes();
        return type != EMATCHTYPE.THREE;
    }


    //4칸 매치
    List<UI_Match_Block> SetForeMatch(UI_Match_Block currentBlock, Dictionary<(int, int), UI_Match_Block> matchblockdic)
    {
        var breaklist = new List<UI_Match_Block>();
        var currentPoint = currentBlock.GetPoint();
        var blockType = currentBlock.GetBlockMatchTypes();

        if (blockType == EMATCHTYPE.FORE_LEFTRIGHT)
        {
            // x축 방향 한줄 파괴 - 같은 y좌표의 모든 블록
            foreach (var block in matchblockdic)
            {
                if (block.Key.Item2 == currentPoint.y && block.Value != null)
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
                if (block.Key.Item1 == currentPoint.x && block.Value != null)
                {
                    breaklist.Add(block.Value);
                }
            }
        }

        return breaklist;
    }

    List<UI_Match_Block> SetFiveMatch(EBLOCKCOLORTYPE colortpye, Dictionary<(int, int), UI_Match_Block> matchblockdic)
    {
        var colorlist = matchblockdic.Where(x => x.Value != null).Where(x => x.Value.GetBlockColorTypes() == colortpye).Select(x => x.Value).ToList();
        return colorlist;
    }

    List<UI_Match_Block> Set_Corss_Match(int startindex, int endindex, UI_Match_Block boomblock, Dictionary<(int, int), UI_Match_Block> matchblockdic)
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