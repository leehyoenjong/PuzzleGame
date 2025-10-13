using UnityEngine;

public class UI_Lobby : MonoBehaviour
{
    [SerializeField] Transform _stageload;

    void Start()
    {
        var stagelist = _stageload.GetComponentsInChildren<UI_StageInfo>(true);
        var maxcount = stagelist.Length;
        for (int i = 0; i < maxcount; i++)
        {
            stagelist[i].Setting(DataManager.instance.GetChapterData(i));
        }
    }
}