using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    static public int PlayerNum = 1;

    [SerializeField] GameObject userSettingPanel;
    [SerializeField] UserSettingManager userSettingManager;
    [SerializeField] TextMeshProUGUI homeUserName;
    [SerializeField] Image homeUserColorImage;
    [SerializeField] Image homeUserIconItemImage;

    // ホーム(タイトル)画面の初期化処理
    void Start()
    {
        UpdateHomeUserSetting();
    }

    public void UpdateHomeUserSetting()
    {
        userSettingManager.InitUserSetting(ref homeUserName, ref homeUserColorImage, ref homeUserIconItemImage);
    }

    public void OpenUserSettingPanel()
    {
        if(userSettingPanel)
        {
            userSettingPanel.SetActive(true);
            userSettingManager.InitUserSettingPanel();
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
