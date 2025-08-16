using TMPro;
using UnityEngine;

public class UI_StageInfo : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _chapternumber;
    [SerializeField] GameObject _popup;

    St_ChapterData _chapterdata;
    public void Setting(St_ChapterData chapterdata)
    {
        if (chapterdata._chapterdata == null)
        {
            return;
        }
        _chapterdata = chapterdata;
        _chapternumber.text = chapterdata.GetChapterNumber();
    }

    public void Btn_View()
    {
        Instantiate<GameObject>(_popup, null).GetComponent<UI_StagePopup>().Setting(_chapterdata);
    }
}