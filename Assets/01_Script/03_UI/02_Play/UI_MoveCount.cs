using System;
using TMPro;
using UnityEngine;

public class UI_MoveCount : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _movecount;
    string MOVECOUNTSTR = "MOVECOUNT: {0}";

    void OnEnable()
    {
        MoveCountManager._movecount_event += SetText;
    }

    void OnDisable()
    {
        MoveCountManager._movecount_event -= SetText;
    }

    void SetText(int totalcount)
    {
        _movecount.text = string.Format(MOVECOUNTSTR, totalcount);
    }
}
