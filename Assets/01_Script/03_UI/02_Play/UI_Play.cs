using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_Play : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _clearcondition_text;

    void OnEnable()
    {
        MatchFiledManager._load_chapter_event += SettingClearCondtion;
    }

    void OnDisable()
    {
        MatchFiledManager._load_chapter_event -= SettingClearCondtion;
    }

    void SettingClearCondtion(St_ChapterData chapterdata)
    {
        _clearcondition_text.text = chapterdata._clear_condition.GetConditionExplain();
    }
}
