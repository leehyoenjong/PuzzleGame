using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public static event Action<int, int, EMATCHTYPE> _match_complte_createblock_event;//매치 성공 후 생성되는 개별 블록 이벤트 
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

    void MatchComplte(List<UI_Match_Block> x_list, List<UI_Match_Block> y_list, bool isspecial)
    {
        var matchtype = GetMatchTypes(x_list, y_list);
        if (matchtype == EMATCHTYPE.THREE)
        {
            SetMatchBlock(x_list, y_list);
            return;
        }

        (int x, int y) middlepoint = (0, 0);
        switch (matchtype)
        {
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

        //특수블록으로 제거했을땐 블록이 생성되지 않도록 수정
        if (isspecial == false)
        {
            _match_complte_createblock_event?.Invoke(middlepoint.Item1, middlepoint.Item2, matchtype);
        }
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
        return (-1, -1);
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

    void AllBlockMatch(Dictionary<(int, int), UI_Match_Block> matchblockdic, int width, int height)
    {
        _ismatching = true;
        //블록이 아닌 슬롯의 정보를 저장하여 위치값 계산할 예정
        var maxcount = matchblockdic.Count;
        int key_y = 0;
        int key_x = 0;

        for (int i = 0; i < maxcount; i++)
        {
            var matchresult = GetMatchBlock(key_x, key_y, width, height, matchblockdic);

            //매치 성공
            if (matchresult.matchblocklist_x.Count >= 3 || matchresult.matchblocklist_y.Count >= 3)
            {
                //타입별 매칭 처리
                var checkspecial = GetMatchTypeFuction(matchresult.matchblocklist_x, matchblockdic) || GetMatchTypeFuction(matchresult.matchblocklist_y, matchblockdic);

                //매칭 성공하면 위치는 고정하고 매칭 성공 처리 진행
                MatchComplte(matchresult.matchblocklist_x, matchresult.matchblocklist_y, checkspecial);
                //매치를 성공하고도 return하지 않는 이유는 모든 블록 매치를 체크해야하기 때문
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

        await UniTask.WaitForSeconds(0.25f, cancellationToken: this.GetCancellationTokenOnDestroy());

        var matchresult = GetMatchBlock(downpoint.x, downpoint.y, width, height, matchblockdic, true);
        var downpointcheck = matchresult.matchblocklist_x.Count >= 3 || matchresult.matchblocklist_y.Count >= 3;
        //매치 성공
        if (downpointcheck)
        {
            //타입별 매칭 처리
            var checkspecial = GetMatchTypeFuction(matchresult.matchblocklist_x, matchblockdic) || GetMatchTypeFuction(matchresult.matchblocklist_y, matchblockdic);

            //매칭 성공하면 위치는 고정하고 매칭 성공 처리 진행
            MatchComplte(matchresult.matchblocklist_x, matchresult.matchblocklist_y, checkspecial);
        }

        var matchresult_enter = GetMatchBlock(enterpoint.x, enterpoint.y, width, height, matchblockdic, true);
        var entercheck = matchresult_enter.matchblocklist_x.Count >= 3 || matchresult_enter.matchblocklist_y.Count >= 3;
        if (entercheck)
        {
            //타입별 매칭 처리
            var checkspecial = GetMatchTypeFuction(matchresult_enter.matchblocklist_x, matchblockdic) || GetMatchTypeFuction(matchresult_enter.matchblocklist_y, matchblockdic);

            //매칭 성공하면 위치는 고정하고 매칭 성공 처리 진행
            MatchComplte(matchresult_enter.matchblocklist_x, matchresult_enter.matchblocklist_y, checkspecial);
        }

        //매칭 성공 시 원상복구 막기 
        if (downpointcheck || entercheck)
        {
            _user_move_match_complte?.Invoke();
            _ismatching = false;
            return;
        }

        //매칭 실패 시 원상복구
        pointdown.ChangePoint(downpoint.x, downpoint.y, downpos);
        pointenter.ChangePoint(enterpoint.x, enterpoint.y, enterpos);
        _ismatching = false;
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
                var origin = matchblockdic[(key_x, key_y)];

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

        var maxcount = breakblocklist.Count;
        for (int i = 0; i < maxcount; i++)
        {
            var blocktypes = breakblocklist[i].GetBlockTypes();
            switch (blocktypes)
            {
                //한줄 모두 파괴 
                case EMATCHTYPE.FORE:
                    breakblocklist.AddRange(SetForeMatch(i, breakblocklist, matchblockdic));
                    isspecial = true;
                    break;
                    // //같은 색상 모두 파괴
                    // case EMATCHTYPE.FIVE:
                    //     breakblocklist.AddRange(SetForeMatch(i, blocklist, matchblockdic));
                    //     break;
                    // //3x3 파괴
                    // case EMATCHTYPE.CROSS_THREE:
                    //     breakblocklist.AddRange(SetForeMatch(i, blocklist, matchblockdic));
                    //     break;
                    // //x,y한줄씩 파괴
                    // case EMATCHTYPE.CROSS_FOUR:
                    //     breakblocklist.AddRange(SetForeMatch(i, blocklist, matchblockdic));
                    //     break;
                    // //전체 파괴
                    // case EMATCHTYPE.CROSS_FIVE:
                    //     breakblocklist.AddRange(SetForeMatch(i, blocklist, matchblockdic));
                    //     break;
            }
        }
        return isspecial;
    }

    //4칸 매치
    List<UI_Match_Block> SetForeMatch(int index, List<UI_Match_Block> blocklist, Dictionary<(int, int), UI_Match_Block> matchblockdic)
    {
        var breaklist = new List<UI_Match_Block>();
        //x줄인지 y줄인지 확인
        var currentBlock = blocklist[index];
        var currentPoint = currentBlock.GetPoint();

        bool isXLine = false;
        for (int j = 0; j < blocklist.Count; j++)
        {
            if (index == j) continue;
            var otherPoint = blocklist[j].GetPoint();

            // x값이 다르면 x축 방향 라인
            if (currentPoint.x != otherPoint.x)
            {
                isXLine = true;
                break;
            }
        }

        if (isXLine)
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
        else
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
}