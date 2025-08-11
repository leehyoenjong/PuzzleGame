using UnityEngine;

public class UI_Match_Slot : MonoBehaviour
{
    [SerializeField] RectTransform _rt;
    public Vector2 GetPos() => _rt.anchoredPosition;
    int _x, _y;

    public void Setting(int x, int y)
    {
        _x = x;
        _y = y;
    }
}
