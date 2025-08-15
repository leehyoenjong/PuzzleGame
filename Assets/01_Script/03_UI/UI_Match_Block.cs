using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_Match_Block : MonoBehaviour
{
    [Header("기본")]
    [SerializeField] Image _image;
    [SerializeField] MoveContoller _movecontroller;
    [SerializeField] RectTransform _rt;
    public Vector2 GetPos() => _rt.anchoredPosition;
    public MoveContoller GetMoveController() => _movecontroller;

    [Header("블록 타입")]
    [SerializeField] EMATCHTYPE _blocktype;
    public EMATCHTYPE GetBlockMatchTypes() => _blocktype;

    [Header("점수")]
    [SerializeField] int _score;
    public int GetScore() => _score;

    //타입
    [SerializeField] EBLOCKCOLORTYPE _colortypes;
    public EBLOCKCOLORTYPE GetBlockColorTypes() => _colortypes;

    [SerializeField] int _x, _y;
    public (int x, int y) GetPoint() => (_x, _y);

    public static event Action<UI_Match_Block> _mathcomplte_event;
    public static event Action<int, int, UI_Match_Block> _move_block_event;
    public static event Action<UI_Match_Block> _point_down_event;
    public static event Action<UI_Match_Block> _point_enter_event;
    public static event Action _point_up_event;

    void OnEnable()
    {
        MatchManager._match_complte_block_event += ComplteMatch;
        MatchFiledManager._no_match_block_event += NoMatchTypeChange;
    }

    void OnDisable()
    {
        MatchManager._match_complte_block_event -= ComplteMatch;
        MatchFiledManager._no_match_block_event -= NoMatchTypeChange;
    }

    public void Setting(Vector2 createpos)
    {
        _rt.anchoredPosition = createpos;
        SettingColorTypes();
    }

    void NoMatchTypeChange()
    {
        //기본 블록만 변화하도록
        if (_blocktype != EMATCHTYPE.THREE)
        {
            return;
        }

        SettingColorTypes();
    }

    public void SettingColorTypes(EBLOCKCOLORTYPE colortypes)
    {
        _colortypes = colortypes;
        _image.color = GetColor();
    }

    void SettingColorTypes()
    {
        var ran = UnityEngine.Random.Range(0, (int)EBLOCKCOLORTYPE.MAX);
        _colortypes = (EBLOCKCOLORTYPE)ran;
        _image.color = GetColor();
    }

    public void ResetPoint()
    {
        _x = -1;
        _y = -1;
    }

    public void ChangePoint(int x, int y, Vector2 movepoint, bool isdirectmove = false)
    {
        _move_block_event?.Invoke(x, y, this);
        _x = x;
        _y = y;

        if (isdirectmove)
        {
            _movecontroller.SetPosition(movepoint);
        }
        else
        {
            _movecontroller.MoveTo(movepoint);
        }
    }

    public void Swap(UI_Match_Block swapblock, bool isdirectmove = false)
    {
        var originx = _x;
        var originy = _y;
        var originpos = GetPos();

        var swappoint = swapblock.GetPoint();
        var swappos = swapblock.GetPos();

        ResetPoint();
        swapblock.ResetPoint();

        ChangePoint(swappoint.x, swappoint.y, swappos, isdirectmove);
        swapblock.ChangePoint(originx, originy, originpos, isdirectmove);
    }

    public void Event_Point_Down()
    {
        _point_down_event?.Invoke(this);
    }

    public void Event_Point_Up()
    {
        _point_up_event?.Invoke();
    }

    public void Event_Point_Enter()
    {
        _point_enter_event?.Invoke(this);
    }

    void ComplteMatch(int x, int y)
    {
        if (GetPoint().x != x || GetPoint().y != y)
        {
            return;
        }
        _mathcomplte_event?.Invoke(this);
    }

    Color GetColor()
    {
        switch (_colortypes)
        {
            case EBLOCKCOLORTYPE.RED:
                return Color.red;
            case EBLOCKCOLORTYPE.BLUE:
                return Color.blue;
            case EBLOCKCOLORTYPE.YELLOW:
                return Color.yellow;
            case EBLOCKCOLORTYPE.PINK:
                return Color.magenta;
            case EBLOCKCOLORTYPE.GREEN:
                return Color.green;
            default:
                return Color.black;
        }
    }
}