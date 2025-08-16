using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Btn_SceneMove : MonoBehaviour
{
    [SerializeField] string _scenename;

    public void Btn_SceneMove()
    {
        SceneManager.LoadScene(_scenename);
    }
}