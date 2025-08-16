using UnityEngine;

public class UI_Btn_CreatePopup : MonoBehaviour
{
    [SerializeField] GameObject _createpopup;

    public void Btn_Create()
    {
        Instantiate(_createpopup, null);
    }
}
