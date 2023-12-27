using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class MessageImageController : MonoBehaviour
{
    Color32 white_color = new Color32(255,255,255,255);
    GameObject[] childBars;
    TextMeshProUGUI childText;
    RectTransform rectTransform;
    CanvasGroup canvasGroup;
    [SerializeField]
    private float moveXPosition = 200f;

    void Start()
    {
        childBars = GameObject.FindGameObjectsWithTag("MessageImage_bar");
        childText = GameObject.FindGameObjectWithTag("MessageText").GetComponent<TextMeshProUGUI>();
        rectTransform = transform.GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        this.gameObject.SetActive(false);
    }

    public void DisplayMessageImage(string colorCode, string messageText)
    {
        this.gameObject.SetActive(true);

        // 色変更（引数を元に）
        Color color = GetColor(colorCode);
        foreach(var childBar in childBars){
            childBar.GetComponent<Image>().color = color;
        }

        // テキスト変更
        childText.text = messageText;

        // アニメーション
        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = new Vector2(moveXPosition*(-1), 0);
        var sequence = DOTween.Sequence();
        sequence.Append(canvasGroup.DOFade(1f, 0.5f))
            .Join(rectTransform.DOAnchorPos(new Vector2(0, 0), 0.5f))
            .AppendInterval(1f)
            .Append(canvasGroup.DOFade(0, 0.5f))
            .Join(rectTransform.DOAnchorPos(new Vector2(moveXPosition, 0), 0.5f))
            .SetEase(Ease.OutQuad);
    }

    //カラーコードの文字列からColorクラスに変換
    private Color GetColor(string colorCode)
    {
        Color color = default(Color);
        if (ColorUtility.TryParseHtmlString(colorCode, out color))
        {
           //Colorを生成できたらそれを返す
            return color;
        }
        else
        { 
           //失敗した場合は白を返す
            return white_color;
        }
    }
}
