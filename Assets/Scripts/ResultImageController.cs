using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class ResultImageController : MonoBehaviour
{
    GameObject[] childBars;
    TextMeshProUGUI childText_main;
    TextMeshProUGUI childText_detail;
    RectTransform rectTransform;

    void Start()
    {
        childBars = GameObject.FindGameObjectsWithTag("ResultImage_bar");
        childText_main = GameObject.FindGameObjectWithTag("ResultText_main").GetComponent<TextMeshProUGUI>();
        childText_detail = GameObject.FindGameObjectWithTag("ResultText_detail").GetComponent<TextMeshProUGUI>();
        
        rectTransform = transform.GetComponent<RectTransform>();

        this.gameObject.SetActive(false);
    }

    public void DisplayResultImage(int winPlayerNum, int atcPlayerNum, int difPlayerNum, int atcUnitNum, int difUnitNum){
        // 非表示から表示に更新
        this.gameObject.SetActive(true);

        // 引数を元にテキストを更新
        childText_main.text = winPlayerNum + "P WIN!";
        if(difUnitNum == -1){
            childText_detail.text = atcPlayerNum+"Pの【"+GetUnitName(atcUnitNum)+"】が"
                                    +difPlayerNum+"Pの陣地に侵入した";
        }else{
            childText_detail.text = atcPlayerNum+"Pの【"+GetUnitName(atcUnitNum)+"】が"
                                    +difPlayerNum+"Pの【"+GetUnitName(difUnitNum)+"】を倒した";
        }
        
        // 勝利プレイヤーに基づく色の変更
        // 1P勝利の場合
        if(winPlayerNum == 1){
            Color color = GetColor("#4299F5");
            foreach(var childBar in childBars){
                childBar.GetComponent<UnityEngine.UI.Image>().color = color;
            }
        }else if(winPlayerNum == 2){
            Color color = GetColor("#F542EA");
            foreach(var childBar in childBars){
                childBar.GetComponent<UnityEngine.UI.Image>().color = color;
            }
        }

        // 展開アニメーション
        rectTransform.localScale = Vector3.zero;
        rectTransform.DOScale(1f, 1f)
            .SetEase(Ease.OutBack, 5f)
            .SetLink(gameObject);
        
    }

    public void DeleteResultImage(){
        // 勝敗演出の削除処理
        this.gameObject.SetActive(false);
    }

    private Color32 white_color = new Color32(255,255,255,255);
    //カラーコードの文字列からColorクラスに変換
    public Color GetColor(string colorCode)
    {
        Color color = default(Color);
        if (ColorUtility.TryParseHtmlString(colorCode, out color))
        {
           //Colorを生成できた場合その色を返す
            return color;
        }
        else
        { 
           //失敗した場合は白を返す
            return white_color;
        }
    }

    private string GetUnitName(int unitNum){
        switch (unitNum)
        {
            case 1:
                return "市民";
            case 2:
                return "王";
            case 3:
                return "死神";
            case 4:
                return "狩人";
            case 5:
                return "占い師";
        }
        return "";
    }
}
