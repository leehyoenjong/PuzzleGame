using TMPro;
using UnityEngine;

public class UI_Score : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _score;
    string SCORETEXT = "SCORE: {0}";

    void OnEnable()
    {
        ScoreManager._add_score_event += SettingScore;
        SettingScore(0);
    }

    void OnDisable()
    {
        ScoreManager._add_score_event -= SettingScore;
    }

    void SettingScore(int score)
    {
        _score.text = string.Format(SCORETEXT, score);
    }
}