using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlockControllerManager : MonoBehaviour
{
    UI_Match_Block _point_down_block;

    public static event Action<UI_Match_Block, UI_Match_Block> _move_block_event;
    public static List<Func<bool>> _block_controller_check_list = new List<Func<bool>>();//블록을 조정해도 되는지 확인하는 이벤트ㄴ

    void OnEnable()
    {
        UI_Match_Block._point_down_event += PointDown;
        UI_Match_Block._point_enter_event += PointEnter;
        UI_Match_Block._point_up_event += PointUp;
    }

    void OnDisable()
    {
        UI_Match_Block._point_down_event -= PointDown;
        UI_Match_Block._point_enter_event -= PointEnter;
        UI_Match_Block._point_up_event -= PointUp;
        _block_controller_check_list.Clear();
    }

    void PointDown(UI_Match_Block block)
    {
        if (_block_controller_check_list.All(x => x.Invoke() == false) == false)
        {
            return;
        }

        _point_down_block = block;
    }

    void PointUp()
    {
        _point_down_block = null;
    }

    void PointEnter(UI_Match_Block enterblock)
    {
        if (_point_down_block == null)
        {
            return;
        }
        var checkx = Mathf.Abs(_point_down_block.GetPoint().x - enterblock.GetPoint().x) >= 2;
        var checky = Mathf.Abs(_point_down_block.GetPoint().y - enterblock.GetPoint().y) >= 2;
        if (checkx || checky)
        {
            _point_down_block = null;
            return;
        }

        _move_block_event?.Invoke(_point_down_block, enterblock);
        _point_down_block = null;
    }
}