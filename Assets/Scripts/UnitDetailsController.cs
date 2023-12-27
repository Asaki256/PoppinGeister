using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDetailsController : MonoBehaviour
{
    private GameObject unitDetail = null;
    private int activeUnitType = -1;

    // ユニットの詳細ポップアップ表示メソッド
    public void DetailOpen(int unitType)
    {
        switch (unitType)
        {
            case UnitController.TYPE_CITIZEN:
                unitDetail = this.transform.Find("CitizenDetail").gameObject;
                break;
            case UnitController.TYPE_KING:
                unitDetail = this.transform.Find("KingDetail").gameObject;
                break;
            case UnitController.TYPE_DEATH:
                unitDetail = this.transform.Find("DeathDetail").gameObject;
                break;
            case UnitController.TYPE_HUNTER:
                unitDetail = this.transform.Find("HunterDetail").gameObject;
                break;
            case UnitController.TYPE_DIVINER:
                unitDetail = this.transform.Find("DivinerDetail").gameObject;
                break;
        }
        unitDetail.SetActive(true);
        activeUnitType = unitType;
    }

    // ユニットの詳細ポップアップ非表示メソッド
    public void DetailClose()
    {
        if (unitDetail == null) return;
        unitDetail.SetActive(false);
    }

    //必要そうなら実装する
    public void DetailCloseAll()
    {

    }
}
