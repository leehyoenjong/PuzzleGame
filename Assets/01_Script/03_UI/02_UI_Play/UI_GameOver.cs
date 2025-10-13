using TMPro;
using UnityEngine;

public class UI_GameOver : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _title;
    [SerializeField] TextMeshProUGUI _gamedata;
    [SerializeField] GameObject _replay;

    string SCORETEXT = "SCORE: {0}\n\nMOVECOUNT: {1}\n\nBLOCKBRAKE: {2}";

    public void Setting(St_GameData gamedata, bool isclear)
    {
        _title.text = isclear ? "CLEAR!" : "FAILD";
        _gamedata.text = string.Format(SCORETEXT, gamedata._score, gamedata._movecount, gamedata._blockbreak);
        _replay.SetActive(isclear == false);
    }
}