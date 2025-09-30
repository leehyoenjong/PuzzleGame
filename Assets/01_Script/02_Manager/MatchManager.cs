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

    // Match detection logic
    private MatchDetector _matchdetector;
    private MatchTypeClassifier _matchtypeclassifier;
    private SpecialBlockFactory _specialblockfactory;
    private ChainReactionProcessor _chainreactionprocessor;

    //매치 진행중인지 체크
    bool _ismatching;
    bool CheckMatching() => _ismatching;

    void Awake()
    {
        _matchdetector = new MatchDetector();
        _matchtypeclassifier = new MatchTypeClassifier();
        _specialblockfactory = new SpecialBlockFactory();
        _chainreactionprocessor = new ChainReactionProcessor();
    }

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
        var matchtype = _matchtypeclassifier.ClassifyMatchType(x_list, y_list);

        SetMatchBlock(x_list, y_list);

        var creationrequest = _specialblockfactory.CreateRequest(x_list, y_list, matchtype, usermoveblock);
        if (creationrequest.HasValue)
        {
            var request = creationrequest.Value;
            _match_complte_createblock_event?.Invoke(request.Point.x, request.Point.y, request.Type, request.Color);
        }
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
            var matchtype = _matchtypeclassifier.ClassifyMatchType(matchresult.matchblocklist_x, matchresult.matchblocklist_y);
            var request = _specialblockfactory.CreateRequest(matchresult.matchblocklist_x, matchresult.matchblocklist_y, matchtype, pointenter);
            if (request.HasValue) creationRequests.Add(request.Value);
        }
        if (entercheck)
        {
            var matchtype = _matchtypeclassifier.ClassifyMatchType(matchresult_enter.matchblocklist_x, matchresult_enter.matchblocklist_y);
            var request = _specialblockfactory.CreateRequest(matchresult_enter.matchblocklist_x, matchresult_enter.matchblocklist_y, matchtype, pointdown);
            if (request.HasValue) creationRequests.Add(request.Value);
        }

        var blocksToDestroy = new List<UI_Match_Block>();
        blocksToDestroy.AddRange(matchresult.matchblocklist_x);
        blocksToDestroy.AddRange(matchresult.matchblocklist_y);
        blocksToDestroy.AddRange(matchresult_enter.matchblocklist_x);
        blocksToDestroy.AddRange(matchresult_enter.matchblocklist_y);

        var distinctBlocksToDestroy = blocksToDestroy.Distinct().ToList();

        // 기존 특수 블록 효과 처리 (문제점 2 해결)
        var finalBlocksToDestroy = _chainreactionprocessor.ProcessChainReaction(distinctBlocksToDestroy, matchblockdic);


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


    (List<UI_Match_Block> matchblocklist_x, List<UI_Match_Block> matchblocklist_y) GetMatchBlock(int key_x, int key_y, int width, int height, Dictionary<(int, int), UI_Match_Block> matchblockdic, bool isall = false)
    {
        var matchblocklist_x = new List<UI_Match_Block>();
        var matchblocklist_y = new List<UI_Match_Block>();

        // Use MatchDetector for detection
        var horizontalpositions = _matchdetector.DetectHorizontalMatch(matchblockdic, (key_x, key_y));
        var verticalpositions = _matchdetector.DetectVerticalMatch(matchblockdic, (key_x, key_y));

        // Convert positions to blocks
        if (horizontalpositions != null)
        {
            foreach (var pos in horizontalpositions)
            {
                if (matchblockdic.ContainsKey(pos))
                {
                    matchblocklist_x.Add(matchblockdic[pos]);
                }
            }
        }

        if (verticalpositions != null)
        {
            foreach (var pos in verticalpositions)
            {
                if (matchblockdic.ContainsKey(pos))
                {
                    matchblocklist_y.Add(matchblockdic[pos]);
                }
            }
        }

        return (matchblocklist_x.Distinct().ToList(), matchblocklist_y.Distinct().ToList());
    }

    bool GetMatchTypeFuction(List<UI_Match_Block> breakblocklist, Dictionary<(int, int), UI_Match_Block> matchblockdic)
    {
        // 매치되는 블록들이 모두 동일한 색상인지 확인
        if (breakblocklist.Count == 0)
        {
            return false;
        }

        // 특수 블록이 있는지 확인
        bool hasspecialblock = breakblocklist.Any(block => 
        {
            var type = block.GetBlockMatchTypes();
            return type != EMATCHTYPE.THREE;
        });

        if (hasspecialblock)
        {
            // ChainReactionProcessor를 사용하여 체인 반응 처리
            var finalblocks = _chainreactionprocessor.ProcessChainReaction(breakblocklist, matchblockdic);
            
            // 원래 리스트를 교체
            breakblocklist.Clear();
            breakblocklist.AddRange(finalblocks);
            
            return true;
        }

        return false;
    }

}