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

    public static event Action<List<UI_Match_Block>, List<UI_Match_Block>> _match_complte_event;//매치 성공 이벤트 

    float _slotsize;//슬롯 사이즈 캐싱

    void OnEnable()
    {
        MatchManager._match_complte_createblock_event += CreateMatchBlock;
    }

    void OnDisable()
    {
        MatchManager._match_complte_createblock_event -= CreateMatchBlock;
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

        await UniTask.WaitForSeconds(0.5f, cancellationToken: this.GetCancellationTokenOnDestroy());
        AllBlockMatch();
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

    void AllBlockMatch()
    {
        //블록이 아닌 슬롯의 정보를 저장하여 위치값 계산할 예정
        var matchblocklist_x = new List<UI_Match_Block>();
        var matchblocklist_y = new List<UI_Match_Block>();

        var maxcount = _matchblockdic.Count;

        int key_y = 0;
        int key_x = 0;

        for (int i = 0; i < maxcount; i++)
        {
            matchblocklist_x.Clear();
            matchblocklist_y.Clear();
            
            for (int x = key_x; x < Width; x++)
            {
                //더 옆으로 갈 곳이 없기 때문에 종료
                var nextkey = (x + 1, key_y);
                if (nextkey.Item1 >= Width)
                {
                    break;
                }

                var key = (x, key_y);
                if (_matchblockdic[key] == null || _matchblockdic[nextkey] == null)
                {
                    continue;
                }

                var types = _matchblockdic[key].GetBlockColorTypes();
                var nexttypes = _matchblockdic[nextkey].GetBlockColorTypes();

                //타입이 맞지 않으면 더 확인할 필요 없기 때문에 종료
                if (types != nexttypes)
                {
                    break;
                }

                //매칭 되었기 때문에 저장
                if (matchblocklist_x.Contains(_matchblockdic[key]) == false)
                {
                    matchblocklist_x.Add(_matchblockdic[key]);
                }
                if (matchblocklist_x.Contains(_matchblockdic[nextkey]) == false)
                {
                    matchblocklist_x.Add(_matchblockdic[nextkey]);
                }
            }

            for (int y = key_y; y < Height; y++)
            {
                //더 옆으로 갈 곳이 없기 때문에 종료
                var nextkey = (key_x, y + 1);
                if (nextkey.Item2 >= Height)
                {
                    break;
                }

                var key = (key_x, y);
                if (_matchblockdic[key] == null || _matchblockdic[nextkey] == null)
                {
                    continue;
                }

                var types = _matchblockdic[key].GetBlockColorTypes();
                var nexttypes = _matchblockdic[nextkey].GetBlockColorTypes();

                //타입이 맞지 않으면 더 확인할 필요 없기 때문에 종료
                if (types != nexttypes)
                {
                    break;
                }

                //타입이 맞다면 
                if (matchblocklist_y.Contains(_matchblockdic[key]) == false)
                {
                    matchblocklist_y.Add(_matchblockdic[key]);
                }
                if (matchblocklist_y.Contains(_matchblockdic[nextkey]) == false)
                {
                    matchblocklist_y.Add(_matchblockdic[nextkey]);
                }
            }

            //3개 이상이 아니면 매치가 되지 않은 것이기 때문에 Clear
            if (matchblocklist_x.Count <= 0)
            {
                matchblocklist_x.Clear();
            }
            if (matchblocklist_y.Count <= 0)
            {
                matchblocklist_y.Clear();
            }

            //매치 성공
            if (matchblocklist_x.Count >= 3 || matchblocklist_y.Count >= 3)
            {
                _match_complte_event?.Invoke(matchblocklist_x, matchblocklist_y);
                //매치를 성공하고도 return하지 않는 이유는 모든 블록 매치를 체크해야하기 때문
            }

            //매치 실패 시 다음 칸으로 이동
            key_x++;
            //크기만큼 도달했다면 다 확인한 것이기 때문에 y값을 증가시키고 x값 초기화
            if (key_x >= Width)
            {
                key_x = 0;
                key_y++;
            }
        }
    }
}