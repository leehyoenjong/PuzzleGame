using System;
using System.Collections.Generic;
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
    public static event Action<Dictionary<(int, int), UI_Match_Block>, UI_Match_Block, UI_Match_Block, int, int> _block_move_event;//이동 진행시 이벤트
    float _slotsize;//슬롯 사이즈 캐싱

    void OnEnable()
    {
        MatchManager._match_complte_createblock_event += CreateMatchBlock;
        BlockControllerManager._move_block_event += WaitAndMove;
        UI_Match_Block._move_block_event += ChangeIDX;
    }

    void OnDisable()
    {
        MatchManager._match_complte_createblock_event -= CreateMatchBlock;
        BlockControllerManager._move_block_event -= WaitAndMove;
        UI_Match_Block._move_block_event -= ChangeIDX;
    }

    void Start()
    {
        Setting().Forget();
    }

    public async UniTaskVoid Setting()
    {
        SettingSlotSize();
        CreateMatchSlot();
        var createblocklist = CreateMatchBlock();
        await MoveMatchBlock(createblocklist);
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
                _matchslotdic.Add((x, y), matchslot);
            }
        }
    }

    //전체 돌면서 생성
    List<UI_Match_Block> CreateMatchBlock()
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
                block.Setting(x, y, createpoint);
                blocklist.Add(block);
            }
        }
        return blocklist;
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
        block.Setting(x, y, new Vector2(posx, posy));

        _matchblockdic[(x, y)] = block;
    }

    //전체 이동
    async void WaitAndMove()
    {
        await UniTask.WaitForSeconds(0.5f, cancellationToken: this.GetCancellationTokenOnDestroy());
        _match_complte_event?.Invoke(_matchblockdic, Width, Height);
    }

    //개별 이동
    void WaitAndMove(UI_Match_Block pointdownblock, UI_Match_Block enterblock)
    {
        _block_move_event?.Invoke(_matchblockdic, pointdownblock, enterblock, Width, Height);
    }

    async UniTask<bool> MoveMatchBlock(List<UI_Match_Block> blocklist)
    {
        //y값이 낮은 순으로 정렬
        blocklist.Sort((a, b) => a.transform.position.y.CompareTo(b.transform.position.y));

        for (int y = Height - 1; y > -1; y--)
        {
            for (int x = 0; x < Width; x++)
            {
                var key = (x, y);
                if (_matchblockdic.ContainsKey(key) && _matchblockdic[key] != null)
                {
                    continue;
                }

                //이동 위치
                var targetpos = _matchslotdic[key].GetPos();

                //blocklist가 y값이 낮은 순으로 되어 있기 때문에 x값만 체크하여 아래로 이동
                var block = blocklist.Find(x => x.GetPos().x == targetpos.x);
                block.GetMoveController().MoveTo(targetpos);

                if (_matchblockdic.ContainsKey(key) == false)
                {
                    _matchblockdic.Add(key, null);
                }
                _matchblockdic[key] = block;
                blocklist.Remove(block);
            }

            //계단식으로 떨어지는 느낌을 주기 위해 0.1초 딜레이
            await UniTask.WaitForSeconds(0.1f, cancellationToken: this.GetCancellationTokenOnDestroy());
        }

        return true;
    }

    void ChangeIDX(int x, int y, UI_Match_Block block)
    {
        _matchblockdic[(x, y)] = block;
    }
}