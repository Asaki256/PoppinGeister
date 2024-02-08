using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class UserSettingManager : MonoBehaviour
{

    [SerializeField] ToggleGroup colorToggleGroup;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame

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
                string colorCodeStr = ColorUtility.ToHtmlStringRGB(colorCircle.GetComponent<Image>().color);
            }
        }
    }
}
