using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MatchFiledManager : MonoBehaviour
{
    [SerializeField] int Width;
    [SerializeField] int Height;
    [SerializeField] GameObject _matchslot;
    [SerializeField] GameObject[] _matchblock;
    [SerializeField] Transform _slotparent;
    [SerializeField] Transform _blockparent;

    Dictionary<(int x, int y), UI_Match_Slot> _matchslotdic = new Dictionary<(int, int), UI_Match_Slot>();
    Dictionary<(int x, int y), UI_Match_Block> _matchblockdic = new Dictionary<(int, int), UI_Match_Block>();

    public static event Action<Dictionary<(int, int), UI_Match_Block>, int, int> _match_complte_event;//매치 성공 이벤트 
    public static event Func<Dictionary<(int, int), UI_Match_Block>, int, int, bool> _matchsimuration_check_event;//매치 시뮬레이션 체크 이벤트
    public static event Action<Dictionary<(int, int), UI_Match_Block>, UI_Match_Block, UI_Match_Block, int, int> _block_move_event;//이동 진행시 이벤트

    public static List<Func<bool>> _match_setting_check_list = new List<Func<bool>>();//블록 생성, 이동와 같은 것들을 진행해도 되는지 체크하는 이벤트
    public static event Action _no_match_block_event;//매치되는 블록 없을때 이벤트
    public static event Action _replay_complte_event;//모든 준비가 완료되었고 이제 시작해도 될때

    float _slotsize;//슬롯 사이즈 캐싱
    bool _issetting;//셋팅중인지 체크하는 변수
    bool GetSetting() => _issetting;

    void OnEnable()
    {
        BlockControllerManager._block_controller_check_list.Add(GetSetting);
        MatchManager._match_complte_createblock_event += CreateMatchBlock;
        BlockControllerManager._move_block_event += WaitAndMove;
        UI_Match_Block._move_block_event += ChangeIDX;
        UI_Match_Block._mathcomplte_event += RemoveIDX;
    }

    void OnDisable()
    {
        BlockControllerManager._block_controller_check_list.Remove(GetSetting);
        MatchManager._match_complte_createblock_event -= CreateMatchBlock;
        BlockControllerManager._move_block_event -= WaitAndMove;
        UI_Match_Block._move_block_event -= ChangeIDX;
        UI_Match_Block._mathcomplte_event -= RemoveIDX;
        _match_setting_check_list.Clear();
    }

    void Start()
    {
        Setting().Forget();
    }

    async UniTaskVoid Setting()
    {
        _issetting = true;
        SettingSlotSize();
        CreateMatchSlot();
        var createblockresult = CreateMatchBlock();
        await MoveMatchBlock(createblockresult.blocklist);
        WaitAndMove();
    }


    async UniTaskVoid FiledReSetting()
    {
        _issetting = true;
        await UniTask.WaitUntil(() => _match_setting_check_list.All(x => x.Invoke() == false), cancellationToken: this.GetCancellationTokenOnDestroy());

        //새롭게 생성해야할 블록이 있는지 체크
        var blockList = _matchblockdic.Values.Where(x => x != null).ToList();
        if (blockList.Count == Width * Height)
        {
            //매치 가능한 블록이 있는지 체크
            var checksimurationmatchblock = (bool)_matchsimuration_check_event?.Invoke(_matchblockdic, Width, Height);

            //매치 가능한 블록이 없다면
            if (checksimurationmatchblock == false)
            {
                //특수 블록 제외 모든 블록 재셋팅
                _no_match_block_event?.Invoke();
                WaitAndMove();
                return;
            }
            _replay_complte_event?.Invoke();
            _issetting = false;
            return;
        }

        //먼저 기존에 남아있던 블록 아래로 이동 
        await MoveMatchBlock(blockList);

        //이동 후 빈칸에 맞춰 블록 생성 진행
        var createblockresult = CreateMatchBlock();
        if (createblockresult.newblockcount <= 0)
        {
            _issetting = false;
            return;
        }

        //한번 더 생성된 것들 포함하여 이동 진행
        await MoveMatchBlock(createblockresult.blocklist);
        WaitAndMove();
    }

    void SettingSlotSize()
    {
        // _matchslot의 사이즈를 가져오기
        var slotRectTransform = _matchslot.GetComponent<RectTransform>();
        _slotsize = slotRectTransform.sizeDelta.x;
    }

    void CreateMatchSlot()
    {
        // 전체 그리드의 중앙점 계산
        var totalWidth = (Width - 1) * _slotsize;
        var totalHeight = (Height - 1) * _slotsize;
        var startX = -totalWidth / 2f;
        var startY = totalHeight / 2f;

        for (int y = 0; y < Width; y++)
        {
            for (int x = 0; x < Height; x++)
            {
                var matchslotobject = Instantiate(_matchslot, _slotparent);
                var matchslot = matchslotobject.GetComponent<UI_Match_Slot>();

                // 위치 계산
                float posx = startX + x * _slotsize;
                float posy = startY - y * _slotsize;

                var matchslotrect = matchslotobject.GetComponent<RectTransform>();
                matchslotrect.anchoredPosition = new Vector2(posx, posy);
                matchslot.Setting(x, y);

                var key = (x, y);
                if (_matchslotdic.ContainsKey(key) == false)
                {
                    _matchslotdic.Add(key, null);
                }
                if (_matchblockdic.ContainsKey(key) == false)
                {
                    _matchblockdic.Add(key, null);
                }
                _matchslotdic[key] = matchslot;
            }
        }
    }

    //전체 돌면서 생성
    (List<UI_Match_Block> blocklist, int newblockcount) CreateMatchBlock()
    {
        //최상위 위치의 슬롯
        var toppoint = new Dictionary<(int, int), UI_Match_Slot>();
        for (int i = 0; i < Width; i++)
        {
            var key = (i, 0);
            toppoint.Add(key, _matchslotdic[key]);
        }

        var blocklist = new List<UI_Match_Block>();
        int basicblockindx = (int)EMATCHTYPE.THREE;
        int newblockcount = 0;
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var key = (x, y);
                if (_matchblockdic.ContainsKey(key) && _matchblockdic[key] != null)
                {
                    blocklist.Add(_matchblockdic[key]);
                    continue;
                }
                //최상위 위에서 생성되서 내려오도록 할 예정
                var toppointkey = (x, 0);

                //최상위 위쪽에다 블록 생성
                var blockobject = Instantiate(_matchblock[basicblockindx], _blockparent);
                var block = blockobject.GetComponent<UI_Match_Block>();

                var createpoint = toppoint[toppointkey].GetPos() + new Vector2(0, 150);
                block.Setting(createpoint);
                blocklist.Add(block);
                newblockcount++;
            }
        }
        return (blocklist, newblockcount);
    }

    //특정 위치 개별 생성
    void CreateMatchBlock(int x, int y, EMATCHTYPE matchtype)
    {
        // 전체 그리드의 중앙점 계산
        var totalWidth = (Width - 1) * _slotsize;
        var totalHeight = (Height - 1) * _slotsize;
        var startX = -totalWidth / 2f;
        var startY = totalHeight / 2f;

        // 위치 계산
        float posx = startX + x * _slotsize;
        float posy = startY - y * _slotsize;

        var blockobject = Instantiate(_matchblock[(int)matchtype], _blockparent);
        var block = blockobject.GetComponent<UI_Match_Block>();
        var movepoint = new Vector2(posx, posy);
        block.Setting(movepoint);
        block.ChangePoint(x, y, movepoint, true);
    }

    //전체 이동
    async void WaitAndMove()
    {
        await UniTask.WaitForSeconds(0.25f, cancellationToken: this.GetCancellationTokenOnDestroy());
        _match_complte_event?.Invoke(_matchblockdic, Width, Height);
        FiledReSetting().Forget();
    }

    //개별 이동
    void WaitAndMove(UI_Match_Block pointdownblock, UI_Match_Block enterblock)
    {
        _block_move_event?.Invoke(_matchblockdic, pointdownblock, enterblock, Width, Height);
        FiledReSetting().Forget();
    }

    async UniTask<bool> MoveMatchBlock(List<UI_Match_Block> blocklist)
    {
        if (blocklist.Count <= 0)
        {
            return true;
        }

        //y값이 낮은 순으로 정렬
        blocklist.Sort((a, b) => a.GetPos().y.CompareTo(b.GetPos().y));

        int movecount = 0;

        for (int y = Height - 1; y >= 0; y--)
        {
            movecount = 0;
            for (int x = 0; x < Width; x++)
            {
                var key = (x, y);
                //밑에서부터 체크하기 때문에 블록이 있다면 밑에 자리가 있는 것이기에 continue
                if (_matchblockdic.ContainsKey(key) && _matchblockdic[key] != null)
                {
                    continue;
                }

                //이동 위치
                var targetpos = _matchslotdic[key].GetPos();

                //blocklist가 x값은 같고 y값은 큰값만 가져올 것
                var block = blocklist.FirstOrDefault(x => x != null && x.GetPos().x == targetpos.x && x.GetPoint().y < key.y);
                if (block == null || block == default)
                {
                    continue;
                }

                //이동한 블록 제거
                block.ChangePoint(x, y, targetpos);
                movecount++;

                if (targetpos == block.GetPos())
                {
                    movecount--;
                }

                blocklist.Remove(block);
                if (blocklist.Count <= 0)
                {
                    return true;
                }
            }

            if (movecount > 0)
            {
                //계단식으로 떨어지는 느낌을 주기 위해 0.25초 딜레이
                await UniTask.WaitForSeconds(0.15f, cancellationToken: this.GetCancellationTokenOnDestroy());
            }
        }
        return true;
    }

    void ChangeIDX(int x, int y, UI_Match_Block block)
    {
        if (_matchblockdic.TryGetValue((x, y), out var origin))
        {
            _matchblockdic[(x, y)] = block;
            if (Application.isEditor)
            {
                block.name = $"{x}_{y}";
            }
        }

        var point = block.GetPoint();
        if (point.x == -1)
        {
            return;
        }
        _matchblockdic[(point.x, point.y)] = origin;

        if (origin != null && Application.isEditor)
        {
            _matchblockdic[(point.x, point.y)].name = $"{x}_{y}";
        }
    }

    void RemoveIDX(UI_Match_Block block)
    {
        _matchblockdic[block.GetPoint()] = null;
    }
}