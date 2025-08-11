using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_Match_Filed : MonoBehaviour
{
    [SerializeField] int Width;
    [SerializeField] int Height;
    [SerializeField] GameObject _matchslot;
    [SerializeField] GameObject _matchblock;
    [SerializeField] Transform _parent;

    Dictionary<(int x, int y), UI_Match_Slot> _matchslotdic = new Dictionary<(int, int), UI_Match_Slot>();
    Dictionary<(int x, int y), UI_Match_Block> _matchblockdic = new Dictionary<(int, int), UI_Match_Block>();


    void Start()
    {
        Setting();
    }

    public void Setting()
    {
        CreateMatchSlot();
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
                var matchslotobject = Instantiate(_matchslot, _parent);
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
        var createcount = _matchslotdic.Count - _matchblockdic.Count;
        for (int i = 0; i < createcount; i++)
        {
            var blockobject = Instantiate(_matchblock, null);
        }
    }

    void MoveMatchBlock()
    {

    }
}
