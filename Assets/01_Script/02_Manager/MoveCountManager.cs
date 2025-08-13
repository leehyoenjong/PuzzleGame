using System;
using UnityEngine;

public class MoveCountManager : MonoBehaviour
{
    public static event Action<int> _movecount_event;
    int _currentmovecount;

    void Start()
    {
        MatchManager._user_move_match_complte += AddMoveCountData;
        int conditioncount = (int)GameConditionManager._overcondition_count?.Invoke();
        _movecount_event?.Invoke(conditioncount);
    }

    void OnDisable()
    {
        MatchManager._user_move_match_complte += AddMoveCountData;
    }

    void AddMoveCountData()
    {
        _currentmovecount--;
        int conditioncount = (int)GameConditionManager._overcondition_count?.Invoke();

        var totalcount = conditioncount + _currentmovecount;
        _movecount_event?.Invoke(totalcount);
    }
}