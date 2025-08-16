using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_Chapter", menuName = "SO_Chapter", order = 0)]
public class SO_Chapter : ScriptableObject
{
    public List<St_ChapterData> _chapterdata = new List<St_ChapterData>();

    public St_ChapterData GetChapterData(int chapternumber)
    {
        return _chapterdata.FirstOrDefault(x => x._chapternumber == chapternumber);
    }
}