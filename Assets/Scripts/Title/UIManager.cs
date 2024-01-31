using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    static public int PlayerNum = 1;

    [SerializeField]
    GameObject PanelUIObj;

    public void OpenKeyInputPanel()
    {
        
    }

    public void VsCPU()
    {
        PlayerNum = 1;
        SceneManager.LoadScene("MainScene");
    }

    public void VsPlayer()
    {
        PlayerNum = 2;
        SceneManager.LoadScene("MainScene");
    }
}
