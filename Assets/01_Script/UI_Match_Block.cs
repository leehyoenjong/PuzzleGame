using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_Match_Block : MonoBehaviour
{
    [Header("블록 타입")]
    [SerializeField] EMATCHTYPE _blocktype;
    public EMATCHTYPE GetBlockTypes() => _blocktype;

    [Header("기본")]
    [SerializeField] Image _image;
    [SerializeField] MoveContoller _movecontroller;
    [SerializeField] RectTransform _rt;
    public Vector2 GetPos() => _rt.anchoredPosition;
    public MoveContoller GetMoveController() => _movecontroller;

    //타입
    [SerializeField] EBLOCKCOLORTYPE _colortypes;
    public EBLOCKCOLORTYPE GetBlockColorTypes() => _colortypes;

    public static event Action<int, int, UI_Match_Block> _move_block_event;
    public static event Action<UI_Match_Block> _point_down_event;
    public static event Action<UI_Match_Block> _point_enter_event;
    public static event Action _point_up_event;


    [SerializeField] int _x, _y;
    public (int x, int y) GetPoint() => (_x, _y);

    void OnEnable()
    {
        MatchManager._match_complte_block_event += ComplteMatch;
    }

    void OnDisable()
    {
        MatchManager._match_complte_block_event -= ComplteMatch;
    }

    public void Setting(int x, int y, Vector2 createpos)
    {
        _x = x;
        _y = y;
        _rt.anchoredPosition = createpos;

        var ran = UnityEngine.Random.Range(0, (int)EBLOCKCOLORTYPE.MAX);
        _colortypes = (EBLOCKCOLORTYPE)ran;
        _image.color = GetColor();
    }

    public void ChangePoint(int x, int y, Vector2 movepoint)
    {
        _x = x;
        _y = y;
        _movecontroller.MoveTo(movepoint);
        _move_block_event?.Invoke(x, y, this);
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

        Destroy(this.gameObject);
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