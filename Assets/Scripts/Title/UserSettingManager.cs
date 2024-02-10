using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class UserSettingManager : MonoBehaviour
{

    [SerializeField] ToggleGroup colorToggleGroup;
    [SerializeField] ToggleGroup iconItemToggleGroup;
    [SerializeField] TMP_InputField userNameInputField;
    [SerializeField] TMP_InputField userCommentInputField;
    [SerializeField] Image userColorImage;
    [SerializeField] Image userIconItemImage;
    [SerializeField] TextMeshProUGUI homeUserName;
    [SerializeField] Image homeUserColorImage;
    [SerializeField] Image homeUserIconItemImage;

    // トグル選択時にカラーコード取得
    public void onClickColorToggle()
    {
        // Toggleの仕様なのか、2回呼び出しがされる。
        // ラジオボタンOn側とOff側の双方から呼び出されてるぽい。
        // 変数を更新後、戻るボタン押下時にまとめて保存するため、パフォーマンス影響は小
        if(colorToggleGroup.AnyTogglesOn()){
            Toggle selectedColorToggle = colorToggleGroup.ActiveToggles().FirstOrDefault();
            if(selectedColorToggle.isOn && selectedColorToggle){
                GameObject colorCircle = selectedColorToggle.transform.Find("IsSelectCircle/BGCircle/ColorCircle").gameObject;

                userColorImage.color = colorCircle.GetComponent<Image>().color;
            }
        }
    }

    // トグル選択時にアイコン番号取得
    public void onClickIconToggle()
    {
        // Toggleの仕様なのか、2回呼び出しがされる。
        // ラジオボタンOn側とOff側の双方から呼び出されてるぽい。
        // 変数を更新後、戻るボタン押下時にまとめて保存するため、パフォーマンス影響は小
        if(iconItemToggleGroup.AnyTogglesOn()){
            Toggle selectedIconToggle = iconItemToggleGroup.ActiveToggles().FirstOrDefault();
            if(selectedIconToggle.isOn && selectedIconToggle){
                string iconName = selectedIconToggle.gameObject.name;
                updateUserIcon(iconName);
            }
        }
    }

    // 戻るボタン押下時に呼び出すメソッド
    public void SaveUserSetting()
    {
        string colorCodeStr = "55B8FF";
        string iconName = "SelectIcon_none";

        PlayerPrefs.SetString("UserName", userNameInputField.text);
        PlayerPrefs.SetString("UserComment", userCommentInputField.text);

        if(iconItemToggleGroup.AnyTogglesOn()){
            Toggle selectedIconToggle = iconItemToggleGroup.ActiveToggles().FirstOrDefault();
            if(selectedIconToggle.isOn && selectedIconToggle){
                iconName = selectedIconToggle.gameObject.name;
            }
        }
        PlayerPrefs.SetString("IconName", iconName);

        if(colorToggleGroup.AnyTogglesOn()){
            Toggle selectedColorToggle = colorToggleGroup.ActiveToggles().FirstOrDefault();
            if(selectedColorToggle.isOn && selectedColorToggle){
                GameObject colorCircle = selectedColorToggle.transform.Find("IsSelectCircle/BGCircle/ColorCircle").gameObject;
                // Color型からカラーコードへ変換(例：「FFFFFF」の文字列.#がつかないことに注意)
                colorCodeStr = ColorUtility.ToHtmlStringRGB(colorCircle.GetComponent<Image>().color);
            }
        }
        PlayerPrefs.SetString("ColorCode", colorCodeStr);
        // 更新情報の保存処理
        PlayerPrefs.Save();

        // ホーム画面のユーザ設定の更新
        homeUserName.text = userNameInputField.text;
        updateHomeUserIcon(iconName);
        updateHomeUserIconColor(colorCodeStr);
    }

    // ユーザ設定パネルが開く時に実行するメソッド
    public void InitUserSetting()
    {
        // PlayerPrefsから各値を取得(取得できなかった場合、デフォルトの値をセット)
        string userNameStr = PlayerPrefs.GetString("UserName", "NoName");
        string userCommentStr = PlayerPrefs.GetString("UserComment", "よろしくお願いします。");
        string iconNameStr = PlayerPrefs.GetString("IconName", "SelectIcon_none");
        string colorCodeStr = PlayerPrefs.GetString("ColorCode", "55B8FF");

        // 取得した値を元に、ゲーム内のUIに初期値をセットする
        userNameInputField.text = userNameStr;
        userCommentInputField.text = userCommentStr;
        updateUserIcon(iconNameStr);
        updateUserIconColor(colorCodeStr);

        // アイコンアイテムの種類トグル設定
        Transform itemParent = iconItemToggleGroup.transform;
        var itemChildren = new Transform[itemParent.childCount];
        var itemChildIndex = 0;
        foreach(Transform child in itemParent)
        {
            itemChildren[itemChildIndex++] = child;

            if(iconNameStr == child.gameObject.name)
            {
                child.gameObject.GetComponent<Toggle>().isOn = true;
            }
        }

        // アイコンの色トグル設定
        Transform colorParent = colorToggleGroup.transform;
        var colorChildren = new Transform[colorParent.childCount];
        var colorChildIndex = 0;
        foreach(Transform child in colorParent)
        {
            colorChildren[colorChildIndex++] = child;

            Color color = child.Find("IsSelectCircle/BGCircle/ColorCircle").gameObject.GetComponent<Image>().color;
            if(colorCodeStr == ColorUtility.ToHtmlStringRGB(color)){
                child.gameObject.GetComponent<Toggle>().isOn = true;
            }
        }
    }

    // ホーム画面のユーザ設定表示
    public void InitHomeUserSetting()
    {
        string userNameStr = PlayerPrefs.GetString("UserName", "NoName");
        string iconNameStr = PlayerPrefs.GetString("IconName", "SelectIcon_none");
        string colorCodeStr = PlayerPrefs.GetString("ColorCode", "55B8FF");

        homeUserName.text = userNameStr;
        updateHomeUserIcon(iconNameStr);
        updateHomeUserIconColor(colorCodeStr);
    }

    // パスからSprite型の画像を取得するメソッド
    public static Sprite LoadSprite(string path)
    {
        try
        {
            var rawData = System.IO.File.ReadAllBytes(path);
            Texture2D texture2D = new Texture2D(0, 0);
            texture2D.LoadImage(rawData);
            var sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height),
                new Vector2(0.5f, 0.5f), 100f);
            return sprite;
        }
        catch (System.Exception)
        {
            return null;
        }
    }

    // ユーザアイコンアイテムの変更メソッド
    public void updateUserIcon(string iconName)
    {
        if(iconName == "SelectIcon_none"){
            var c = userIconItemImage.color;
            userIconItemImage.color = new Color(c.r, c.g, c.b, 0);

            userIconItemImage.sprite = null;
        }else{
            var c = userIconItemImage.color;
            userIconItemImage.color = new Color(c.r, c.g, c.b, 1);

            if(iconName == "SelectIcon_diviner"){
                userIconItemImage.sprite = LoadSprite("Assets/Sprites/IconItem_diviner.png");
            }else if(iconName == "SelectIcon_death"){
                userIconItemImage.sprite = LoadSprite("Assets/Sprites/IconItem_death.png");
            }else if(iconName == "SelectIcon_king"){
                userIconItemImage.sprite = LoadSprite("Assets/Sprites/IconItem_king.png");
            }
        }
    }

    // ユーザアイコンアイテムの変更メソッド(ホーム)
    public void updateHomeUserIcon(string iconName)
    {
        if(iconName == "SelectIcon_none"){
            var c = homeUserIconItemImage.color;
            homeUserIconItemImage.color = new Color(c.r, c.g, c.b, 0);

            homeUserIconItemImage.sprite = null;
        }else{
            var c = homeUserIconItemImage.color;
            homeUserIconItemImage.color = new Color(c.r, c.g, c.b, 1);

            if(iconName == "SelectIcon_diviner"){
                homeUserIconItemImage.sprite = LoadSprite("Assets/Sprites/IconItem_diviner.png");
            }else if(iconName == "SelectIcon_death"){
                homeUserIconItemImage.sprite = LoadSprite("Assets/Sprites/IconItem_death.png");
            }else if(iconName == "SelectIcon_king"){
                homeUserIconItemImage.sprite = LoadSprite("Assets/Sprites/IconItem_king.png");
            }
        }
    }

    // アイコンの色変更メソッド
    public void updateUserIconColor(string colorCode)
    {
        Color color = new Color(0.33f, 0.72f, 1f, 1f);
        // outキーワードで参照渡しにする
        if (ColorUtility.TryParseHtmlString("#"+colorCode, out color))
        {
            // Color型への変換成功（colorにColor型の色が代入される）
            userColorImage.color = color;
        }
        else
        {
            // Color型への変換失敗（colorはColor型の初期値のまま）
            userColorImage.color = color;
        }
    }

    // アイコンの色変更メソッド(ホーム)
    public void updateHomeUserIconColor(string colorCode)
    {
        Color color = new Color(0.33f, 0.72f, 1f, 1f);
        // outキーワードで参照渡しにする
        if (ColorUtility.TryParseHtmlString("#"+colorCode, out color))
        {
            // Color型への変換成功（colorにColor型の色が代入される）
            homeUserColorImage.color = color;
        }
        else
        {
            // Color型への変換失敗（colorはColor型の初期値のまま）
            homeUserColorImage.color = color;
        }
    }
}
