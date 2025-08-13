using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] St_GameData _gamedata;
    [SerializeField] GameObject _gameoverpopup;
    public static event Func<St_GameData, bool> _check_clear_condition_event;
    public static event Func<St_GameData, bool> _check_over_condition_event;
    bool _isover;
    bool GetOver() => _isover;

    void OnEnable()
    {
        ScoreManager._add_score_event += AddScoreData;
        MoveCountManager._movecount_event += AddMoveCountData;
        UI_Match_Block._mathcomplte_event += AddBlockBreakData;
        MatchFiledManager._replay_complte_event += CheckGameClearAndOver;
        MatchFiledManager._match_setting_check_list.Add(GetOver);
        BlockControllerManager._block_controller_check_list.Add(GetOver);
    }

    void OnDisable()
    {
        _isover = false;
        ScoreManager._add_score_event -= AddScoreData;
        MoveCountManager._movecount_event -= AddMoveCountData;
        UI_Match_Block._mathcomplte_event -= AddBlockBreakData;
        MatchFiledManager._replay_complte_event -= CheckGameClearAndOver;
    }

    void AddScoreData(int score)
    {
        _gamedata._score = score;
    }

    void AddMoveCountData(int totalcount)
    {
        _gamedata._movecount = totalcount;
    }

    void AddBlockBreakData(UI_Match_Block block)
    {
        _gamedata._blockbreak++;
    }

    void CheckGameClearAndOver()
    {
        _isover = false;
        if ((bool)_check_clear_condition_event?.Invoke(_gamedata))
        {
            _isover = true;

            //게임 클리어 처리
            Instantiate(_gameoverpopup, null).GetComponent<UI_GameOver>().Setting(_gamedata, true);
            return;
        }


        if ((bool)_check_over_condition_event?.Invoke(_gamedata))
        {
            _isover = true;

            //게임 오버 처리
            Instantiate(_gameoverpopup, null).GetComponent<UI_GameOver>().Setting(_gamedata, false);
            return;
        }
    }
}