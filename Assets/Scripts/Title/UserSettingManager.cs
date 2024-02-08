using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class UserSettingManager : MonoBehaviour
{

    [SerializeField] ToggleGroup colorToggleGroup;
    [SerializeField] ToggleGroup iconToggleGroup;

    void Start()
    {
        
    }



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
                // Color型からカラーコードへ変換
                string colorCodeStr = ColorUtility.ToHtmlStringRGB(colorCircle.GetComponent<Image>().color);
            }
        }
    }

    // トグル選択時にアイコン番号取得
    public void onClickIconToggle()
    {
        // Toggleの仕様なのか、2回呼び出しがされる。
        // ラジオボタンOn側とOff側の双方から呼び出されてるぽい。
        // 変数を更新後、戻るボタン押下時にまとめて保存するため、パフォーマンス影響は小
        if(colorToggleGroup.AnyTogglesOn()){
            Toggle selectedIconToggle = iconToggleGroup.ActiveToggles().FirstOrDefault();
            if(selectedIconToggle.isOn && selectedIconToggle){
                string iconName = selectedIconToggle.gameObject.name;
                Debug.Log(iconName);
            }
        }
    } 
}
