using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
        LoadData();
    }


    [SerializeField] SO_Chapter _chapterdata;
    public SO_Chapter GetChapterData() => _chapterdata;
    public St_ChapterData GetChapterData(int chapternumber) => _chapterdata.GetChapterData(chapternumber);


    void LoadData()
    {
        _chapterdata = Resources.Load<SO_Chapter>("SO_Chapter");
    }
}