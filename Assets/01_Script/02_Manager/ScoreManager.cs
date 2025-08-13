using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static event Action<int> _add_score_event;//스코어 획득 이벤트
    int _currentscore;

    void OnEnable()
    {
        UI_Match_Block._mathcomplte_event += AddScore;
    }

    void OnDisable()
    {
        UI_Match_Block._mathcomplte_event -= AddScore;
    }

    void AddScore(UI_Match_Block block)
    {
        _currentscore += block.GetScore();
        _add_score_event?.Invoke(_currentscore);
    }
}
