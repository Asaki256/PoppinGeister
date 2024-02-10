using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    static public int PlayerNum = 1;

    [SerializeField] GameObject userSettingPanel;
    [SerializeField] UserSettingManager userSettingManager;

    // ホーム(タイトル)画面の初期化処理
    void Start()
    {
        userSettingManager.InitHomeUserSetting();
    }

    public void OpenUserSettingPanel()
    {
        if(userSettingPanel)
        {
            userSettingPanel.SetActive(true);
            userSettingManager.InitUserSetting();
        }
    }
    public void CloseUserSettingPanel()
    {
        if(userSettingPanel)
        {
            userSettingManager.SaveUserSetting();
            userSettingPanel.SetActive(false);
        }
    }

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
