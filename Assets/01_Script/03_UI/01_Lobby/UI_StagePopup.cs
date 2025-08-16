using TMPro;
using UnityEngine;

public class UI_StagePopup : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _stagenumber;
    [SerializeField] TextMeshProUGUI _clearcondition;
    [SerializeField] TextMeshProUGUI _gameovercondtion;
    St_ChapterData _chapterdata;
    public void Setting(St_ChapterData chapterdata)
    {
        _chapterdata = chapterdata;
        _stagenumber.text = "STAGE " + chapterdata.GetChapterNumber();
        _clearcondition.text = chapterdata._clear_condition.GetConditionExplain();
        _gameovercondtion.text = chapterdata._over_condition.GetConditionExplain();
    }

    public void Btn_Play()
    {
        MatchFiledManager._getchapterdata += () => _chapterdata;
    }
}