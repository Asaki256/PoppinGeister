using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    //constにすることで再代入ができない固有の数になる
    public const int TYPE_CITIZEN = 1;  // 市民
    public const int TYPE_KING = 2;     // 王
    public const int TYPE_DEATH = 3;    // 死神
    public const int TYPE_HUNTER = 4;   // 狩人
    public const int TYPE_DIVINER = 5;  // 占い師

    const float SELECT_POS_Y = 0.3f;//選択時のユニットの高さ(固定)

    //どちらのPlayerのユニットか(1 or 2)
    public int PlayerNo;
    public int Type;

    /// <summary>
    /// 選択時の動作
    /// </summary>
    /// <param name="select">選択true or 非選択false</param>
    /// <returns>アニメーション秒数</returns>
    public float Select(bool select = true)//デフォルトtrue
    {
        float ret = 0;//戻り値の初期値 アニメーション秒数は現在は0で指定

        Vector3 pos = new Vector3(
            transform.position.x,
            SELECT_POS_Y,
            transform.position.z);
            

        if (!select)//非選択の場合、位置をデフォルトに戻す
        {
            pos = new Vector3(transform.position.x, 0f, transform.position.z);
        }
        transform.position = pos;
        return ret;
    }
}