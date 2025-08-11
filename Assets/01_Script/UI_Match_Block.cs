using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_Match_Block : MonoBehaviour
{
    [SerializeField] Image _image;
    [SerializeField] MoveContoller _movecontroller;
    [SerializeField] RectTransform _rt;
    public Vector2 GetPos() => _rt.anchoredPosition;
    public MoveContoller GetMoveController() => _movecontroller;
    ETYPE _types;

    public static event Action<UI_Match_Block> _matchevent;

    public void Setting(Vector2 createpos)
    {
        _rt.anchoredPosition = createpos;

        var ran = UnityEngine.Random.Range(0, (int)ETYPE.MAX);
        _types = (ETYPE)ran;
        _image.color = GetColor();
    }

    public void Event_Point_Enter()
    {
        _matchevent?.Invoke(this);
    }

    Color GetColor()
    {
        switch (_types)
        {
            case ETYPE.RED:
                return Color.red;
            case ETYPE.BLUE:
                return Color.blue;
            case ETYPE.YELLOW:
                return Color.yellow;
            case ETYPE.PINK:
                return Color.magenta;
            case ETYPE.GREEN:
                return Color.green;
            default:
                return Color.black;
        }
    }
}


public enum ETYPE
{
    RED,
    BLUE,
    YELLOW,
    PINK,
    GREEN,
    MAX
}