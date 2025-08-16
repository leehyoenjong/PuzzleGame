using System;
using UnityEngine;

public class GameConditionManager : MonoBehaviour
{
    St_GameClearCondition _clear_condition;
    St_GameOverCondtion _over_condition;
    public static Func<int> _overcondition_count;//조건 클리어 갯수

    void OnEnable()
    {
        GameManager._check_clear_condition_event += CheckGameClear;
        GameManager._check_over_condition_event += CheckGameOver;
        MatchFiledManager._load_chapter_event += SettingCondition;
        _overcondition_count += GetConditionCount;
    }

    void OnDisable()
    {
        GameManager._check_clear_condition_event += CheckGameClear;
        GameManager._check_over_condition_event -= CheckGameOver;
        MatchFiledManager._load_chapter_event -= SettingCondition;
        _overcondition_count -= GetConditionCount;
    }

    void SettingCondition(St_ChapterData chapterdata)
    {
        _clear_condition = chapterdata._clear_condition;
        _over_condition = chapterdata._over_condition;
    }

    bool CheckGameClear(St_GameData gamedata)
    {
        switch (_clear_condition._condtion)
        {
            case EGAMECLEARCONDITION.SCORE:
                return gamedata._score >= _clear_condition._score;
            case EGAMECLEARCONDITION.BLOCKBRAKE:
                return gamedata._blockbreak >= _clear_condition._blockbreak;
        }


        return false;
    }

    bool CheckGameOver(St_GameData gamedata)
    {
        switch (_over_condition._condtion)
        {
            case EGAMEOVERCONDITION.MOVECOUNT:
                return gamedata._movecount <= 0;
        }
        return false;
    }

    int GetConditionCount()
    {
        switch (_over_condition._condtion)
        {
            case EGAMEOVERCONDITION.MOVECOUNT:
                return _over_condition._movecount;
        }

        return -1;
    }
}