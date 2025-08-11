using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UI_Match_Filed : MonoBehaviour
{
    [SerializeField] int Width;
    [SerializeField] int Height;
    [SerializeField] GameObject _matchslot;
    [SerializeField] GameObject _matchblock;
    [SerializeField] Transform _slotparent;
    [SerializeField] Transform _blockparent;

    Dictionary<(int x, int y), UI_Match_Slot> _matchslotdic = new Dictionary<(int, int), UI_Match_Slot>();
    Dictionary<(int x, int y), UI_Match_Block> _matchblockdic = new Dictionary<(int, int), UI_Match_Block>();

    void Start()
    {
        Setting();
    }

    public void Setting()
    {
        CreateMatchSlot();
        CreateMatchBlock();
    }

    void CreateMatchSlot()
    {
        // _matchslot의 사이즈를 가져오기
        var slotRectTransform = _matchslot.GetComponent<RectTransform>();
        var slotSize = slotRectTransform.sizeDelta.x;

        // 전체 그리드의 중앙점 계산
        var totalWidth = (Width - 1) * slotSize;
        var totalHeight = (Height - 1) * slotSize;
        var startX = -totalWidth / 2f;
        var startY = totalHeight / 2f;

        for (int y = 0; y < Width; y++)
        {
            for (int x = 0; x < Height; x++)
            {
                var matchslotobject = Instantiate(_matchslot, _slotparent);
                var matchslot = matchslotobject.GetComponent<UI_Match_Slot>();

                // 위치 계산
                var matchslotrect = matchslotobject.GetComponent<RectTransform>();
                float posx = startX + x * slotSize;
                float posy = startY - y * slotSize;

                matchslotrect.anchoredPosition = new Vector2(posx, posy);
                matchslot.Setting(x, y);
                _matchslotdic.Add((x, y), matchslot);
            }
        }
    }

    void CreateMatchBlock()
    {
        //최상위 위치의 슬롯
        var toppoint = new Dictionary<(int, int), UI_Match_Slot>();
        for (int i = 0; i < Width; i++)
        {
            var key = (i, 0);
            toppoint.Add(key, _matchslotdic[key]);
        }

        var blocklist = new List<UI_Match_Block>();
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
                var blockobject = Instantiate(_matchblock, _blockparent);
                var block = blockobject.GetComponent<UI_Match_Block>();

                var createpoint = toppoint[toppointkey].GetPos() + new Vector2(0, 150);
                block.Setting(createpoint);
                blocklist.Add(block);
            }
        }
        MoveMatchBlock(blocklist).Forget();
    }

    async UniTaskVoid MoveMatchBlock(List<UI_Match_Block> blocklist)
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
    }
}