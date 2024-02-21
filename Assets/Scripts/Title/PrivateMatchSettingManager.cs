using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PrivateMatchSettingManager : MonoBehaviour
{
    // 上部の部屋状態メッセージ
    [SerializeField] TextMeshProUGUI roomStateMessage;
    // あいことば入力欄
    [SerializeField] TMP_InputField keyWordInputField;
    [SerializeField] TextMeshProUGUI keywordText;
    [SerializeField] TextMeshProUGUI keywordCopiedText;
    // 1Pユーザ情報
    [SerializeField] TextMeshProUGUI firstUserName;
    [SerializeField] TextMeshProUGUI firstUserComment;
    [SerializeField] Image firstUserIconColor;
    [SerializeField] Image firstUserIconItem;
    [SerializeField] TextMeshProUGUI firstUserReadyText;
    bool firstUserReadyFlag = false;
    // 2Pユーザ情報
    [SerializeField] TextMeshProUGUI secondUserName;
    [SerializeField] TextMeshProUGUI secondUserComment;
    [SerializeField] Image secondUserIconColor;
    [SerializeField] Image secondUserIconItem;
    [SerializeField] TextMeshProUGUI secondUserReadyText;
    bool secondUserReadyFlag = false;
    // ユーザ情報保持オブジェクト
    [SerializeField] UserSettingManager usm;

    void Start()
    {
        InitFirstUserInfo();
    }

    void InitFirstUserInfo()
    {
        // firstUserName.text = usm.;
        // firstUserComment.text = ;
        // firstUserIconColor.color = ;
        // firstUserIconItem = ;
        // firstUserReadyText = "対戦相手を探しています...";
    }

}
