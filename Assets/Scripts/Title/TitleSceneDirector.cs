using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleSceneDirector : MonoBehaviour
{
    public bool[] isPlayer;//プレイヤーの時はisPlayer[0]がtrueになる
    Player[] player;
    int nowTurn;
    int blueCount, pinkCount;

    //ゲームモード ゲーム中の場面切り替え用の変数
    enum MODE
    {
        NONE = -1,
        WAIT_TURN_START,//1端末でのプレイヤー変更の際の待機時間のモード
        MOVE_SELECT,//ユニット選択をするモード
        FIELD_UPDATE,//ユニット移動後のデータ更新をするモード
        WAIT_TURN_END,//ターン終了を待つモード
        TURN_CHANGE,//ターンの切り替えモード
    }

    //モード
    MODE nowMode;//上記のENUM型のものしか入れられない変数
    MODE nextMode;//モードを次に切り替えるための変数

    //ウェイトの定義(CPU操作時のアニメーション用)
    float waitTime;

    //フィールドの状態
    public int[,] tileData = new int[,]
    {
        //移動可能かの判定＋オブジェクト配置に使用する
        //ゴールを4と8として定義
        //0は見えない壁
        //ゴールに出られるかどうかを、自分のプレイヤー番号x4の時とする
        //1Pは、奥の4ばんのゴールから出られる

        //手前(カメラからの視線)
        { 0,8,0,0,0,0,8,0 },
        { 0,2,1,1,1,1,2,0 },
        { 0,1,1,1,1,1,1,0 },
        { 0,1,1,1,1,1,1,0 },
        { 0,1,1,1,1,1,1,0 },
        { 0,1,1,1,1,1,1,0 },
        { 0,2,1,1,1,1,2,0 },
        { 0,4,0,0,0,0,4,0 },
    };

    //ユニットの初期配置
    int[,] initUnitData = new int[,]
    {
        //手前
        { 0,0,0,0,0,0,0,0 },
        { 0,0,1,1,1,1,0,0 },
        { 0,0,1,1,1,1,0,0 },
        { 0,0,0,0,0,0,0,0 },
        { 0,0,0,0,0,0,0,0 },
        { 0,0,2,2,2,2,0,0 },
        { 0,0,2,2,2,2,0,0 },
        { 0,0,0,0,0,0,0,0 },
    };

    //ユニット最大数
    const int UNIT_MAX = 8;

    //フィールド上のユニット
    public List<GameObject>[,] unitData;//上下関係をリストで再現する

    //ユニット選択モードで使う
    GameObject selectUnit;//選択したユニットのオブジェクト
    int oldX, oldY;//選択したユニットがどこのマスから選ばれたかの座標


    private void Start()
    {
        //ユニットとフィールドを作成
        List<int> p1rnd = getRandomList(UNIT_MAX, UNIT_MAX / 2);
        List<int> p2rnd = getRandomList(UNIT_MAX, UNIT_MAX / 2);
        int p1unit = 0;//Unitが今何番めのカウントなのか
        int p2unit = 0;
        blueCount = 8;
        pinkCount = 8;

        //ランダムの数値が一致した時に赤を生成
        //フィールドのサイズ分だけ現在のフィールドの状態の変数を作成
        unitData = new List<GameObject>[tileData.GetLength(0), tileData.GetLength(1)];


        //プレイヤー設定
        player = new Player[2];
        player[0] = new Player(false, 1);
        player[1] = new Player(false, 2);


        //タイルとユニットの初期化
        for (int i = 0; i < tileData.GetLength(0); i++)//Yの座標
        {
            for (int j = 0; j < tileData.GetLength(1); j++)//Xの座標
            {
                //キューブの真ん中の原点が0.5あるので、それ分ずらす
                float x = j - (tileData.GetLength(1) / 2 - 0.5f); //列(x)
                float y = i - (tileData.GetLength(0) / 2 - 0.5f); //行(y)

                //タイルの配置
                string resname = "";

                //1：通常タイル、2：ゴールタイル、4：1Pのゴール、8：2Pのゴール
                int no = tileData[i, j];
                if (4 == no || 8 == no) no = 5;
                resname = "Cube (" + no + ")";

                resourcesInstantiate(resname, new Vector3(x, 0, y), Quaternion.identity);

                //ユニット配置
                unitData[i, j] = new List<GameObject>();

                //ユニットの向き(デフォルト)
                Vector3 angle = new Vector3(0, 0, 0);
                int unittype = UnitController.TYPE_CITIZEN;

                List<int> unitrnd = new List<int>();
                int unitnum = -1;
                resname = "Unit1";

                //1Pユニット配置
                if (1 == initUnitData[i, j])
                {
                    unitrnd = p1rnd;
                    unitnum = p1unit++;//何番めのユニットなのか 代入した後に+1
                }
                else if (2 == initUnitData[i, j])
                {
                    resname = "Unit2";
                    unitrnd = p2rnd;
                    unitnum = p2unit++;
                    angle.y = 180;
                }
                else
                {
                    resname = "";
                }

                //unitrndとunitnumが一致した時に赤を配置
                //unitrndにunitnumと一致する値があった場合そのindexを返す
                //なかった場合は-1なので、0~7までのunitnumを全て調べる
                if (-1 < unitrnd.IndexOf(unitnum))
                {
                    unittype = UnitController.TYPE_CITIZEN;
                }

                GameObject unit = resourcesInstantiate(
                    resname,
                    new Vector3(x, 0f, y),
                    Quaternion.Euler(angle));

                if (null != unit)
                {
                    unit.GetComponent<UnitController>().PlayerNo = initUnitData[i, j];
                    unit.GetComponent<UnitController>().Type = unittype;

                    unitData[i, j].Add(unit);
                }
            }
        }
        nowTurn = 0;
        nextMode = MODE.MOVE_SELECT;
    }

    private void Update()
    {
        if (isWait()) return;
        if(blueCount==0 || pinkCount==0) return;

        mode();

        if (MODE.NONE != nextMode) initMode(nextMode);
    }

    //Resourcesからゲームオブジェクトを作成する関数
    GameObject resourcesInstantiate(string name, Vector3 pos, Quaternion ang)
    {
        GameObject prefab = (GameObject)Resources.Load(name);

        if (null == prefab)
        {
            return null;
        }

        return Instantiate(prefab, pos, ang);
    }

    List<int> getRandomList(int range, int count)
    {
        List<int> ret = new List<int>();

        //0,1,2,3,4,5,6,7
        //毎回赤のユニットの配置が変わる
        //2,1,0,6などのリストが返される

        if (range < count)//無限ループ対策
        {
            Debug.LogError("リスト作成エラー");
            return ret;
        }

        while (true)
        {
            int no = Random.Range(0, range);
            if (-1 == ret.IndexOf(no))//retのリストにnoと同じ値が入っていない場合、追加
            {
                ret.Add(no);
            }

            if (count <= ret.Count)//ret.Countが4以上の場合
            {
                break;
            }
        }

        return ret;
    }

    bool isWait()
    {
        bool ret = false;

        if (0 < waitTime)
        {
            waitTime -= Time.deltaTime;
            ret = true;
        }

        return ret;
    }

    /// <summary>
    /// メインモード
    /// </summary>
    void mode()
    {
        if (MODE.MOVE_SELECT == nowMode)
        {
            SelectMode();
        }
        else if (MODE.FIELD_UPDATE == nowMode)
        {
            fieldUpdateMode();
        }
        else if (MODE.TURN_CHANGE == nowMode)
        {
            turnChangeMode();
        }
    }

    /// <summary>
    /// 次のモードの初期化処理(モードが変わるときに呼び出される)
    /// </summary>
    /// <param name="next">次のモード</param>
    void initMode(MODE next)
    {
        if (MODE.MOVE_SELECT == next)
        {
            selectUnit = null;

            if (!player[nowTurn].IsHuman)
            {
                waitTime = 0.5f;
            }
        }
        else if (MODE.WAIT_TURN_END == next)
        {

        }

        nowMode = next;
        nextMode = MODE.NONE;
    }

    void SelectMode()
    {
        //CPUの処理(そのうち機械学習でさせたいー)
        if (!player[nowTurn].IsHuman)
        {
            while (true)
            {
                selectUnit = null;

                //ユニットランダムで選択する
                oldX = Random.Range(0, unitData.GetLength(1));
                oldY = Random.Range(0, unitData.GetLength(0));

                if (0 < unitData[oldY, oldX].Count
                    && player[nowTurn].PlayerNo == unitData[oldY, oldX][0].GetComponent<UnitController>().PlayerNo)
                {
                    selectUnit = unitData[oldY, oldX][0];
                }

                //移動先のタイルをランダムで選択する
                if (null != selectUnit)
                {
                    int rndx = Random.Range(0, unitData.GetLength(1));
                    int rndy = Random.Range(0, unitData.GetLength(0));

                    if (movableTile(oldX, oldY, rndx, rndy))
                    {
                        Vector3 tpos = new Vector3(rndx - (tileData.GetLength(1) / 2 - 0.5f),
                            0f, rndy - (tileData.GetLength(0) / 2 - 0.5f));

                        unitData[oldY, oldX].Clear();
                        selectUnit.transform.position = tpos;
                        unitData[rndy, rndx].Add(selectUnit);

                        break;
                    }
                }
            }

            nextMode = MODE.FIELD_UPDATE;
            return;
        }

        //プレイヤーの処理
        GameObject hitobj = null;

        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                hitobj = hit.collider.gameObject;
            }
        }

        if (null == hitobj) return;

        Vector3 pos = hitobj.transform.position;

        //position x,yからそれぞれ配列の番号に変換
        int x = (int)(pos.x + (tileData.GetLength(1)) / 2 - 0.5f);
        int y = (int)(pos.z + (tileData.GetLength(0)) / 2 - 0.5f);

        //ユニット選択
        //配列の中身の確認
        //現在のunitData
        //今のターンのプレイヤーと、ユニットのデータから取得したプレイヤーナンバーが同じかどうか
        if (0 < unitData[y, x].Count
            && player[nowTurn].PlayerNo == unitData[y, x][0].GetComponent<UnitController>().PlayerNo)
        {
            if (null != selectUnit)
            {
                selectUnit.GetComponent<UnitController>().Select(false);
            }

            selectUnit = unitData[y, x][0];//一番下のユニットなので0番を取得
            oldX = x;
            oldY = y;

            selectUnit.GetComponent<UnitController>().Select();
        }
        //移動先タイル選択
        else if (null != selectUnit)
        {
            if (movableTile(oldX, oldY, x, y))
            {
                unitData[oldY, oldX].Clear();
                pos.y = 0f;
                selectUnit.transform.position = pos;

                unitData[y, x].Add(selectUnit);

                nextMode = MODE.FIELD_UPDATE;
            }
        }
    }

    void fieldUpdateMode()
    {
        for (int i = 0; i < unitData.GetLength(0); i++)
        {
            for (int j = 0; j < unitData.GetLength(1); j++)
            {
                //ゴールしてたら消す
                if (1 == unitData[i, j].Count && player[nowTurn].PlayerNo * 4 == tileData[i, j])
                {
                    //青ならば勝利
                    if (UnitController.TYPE_CITIZEN == unitData[i, j][0].GetComponent<UnitController>().Type)
                    {
                        player[nowTurn].IsGoal = true;
                    }

                    if(unitData[i,j][0].GetComponent<UnitController>().PlayerNo == 1){
                        blueCount--;
                    }else{
                        pinkCount--;
                    }

                    Destroy(unitData[i, j][0]);
                    unitData[i, j].RemoveAt(0);
                }

                //２つ置いてあったら古いユニットを消す
                if (1 < unitData[i, j].Count)
                {
                    if(unitData[i,j][0].GetComponent<UnitController>().PlayerNo == 1){
                        blueCount--;
                    }else{
                        pinkCount--;
                    }
                    Destroy(unitData[i, j][0]);
                    unitData[i, j].RemoveAt(0);
                }
            }
        }

        nextMode = MODE.TURN_CHANGE;
    }

    void turnChangeMode()
    {
        nextMode = MODE.MOVE_SELECT;

        int oldturn = nowTurn;
        nowTurn = getNextTurn();

        //次がプレイヤーだったら=人間同士の場合
        if (player[oldturn].IsHuman && player[nowTurn].IsHuman)
        {
            nextMode = MODE.WAIT_TURN_END;
        }
    }

    //そこへ移動可能かどうか=今の場所から一ます分離れているかの判定
    bool movableTile(int oldx, int oldy, int x, int y)
    {
        bool ret = false;

        //差分を取得する=差の絶対値を取得
        int dx = Mathf.Abs(oldx - x);
        int dy = Mathf.Abs(oldy - y);

        //斜めは進めない
        if (1 < dx + dy)
        {
            ret = false;
        }
        //壁以外かつ誰も乗っていないまたは相手がいるか
        else if (1 == tileData[y, x]  //普通のタイル
            || 2 == tileData[y, x] //ゴールタイル
            || player[nowTurn].PlayerNo * 4 == tileData[y, x])
        {
            //誰も乗っていない
            if (0 == unitData[y, x].Count)
            {
                ret = true;
            }
            //誰か乗っている(仲間or相手)
            else
            {
                //相手が乗っている場合
                if (unitData[y, x][0].GetComponent<UnitController>().PlayerNo != player[nowTurn].PlayerNo)
                {
                    ret = true;
                }
            }
        }
        return ret;
    }

    /// <summary>
    /// 次のターンのプレイヤーインデックス取得。0と1をぐるぐる回す
    /// </summary>
    /// <returns></returns>
    int getNextTurn()
    {
        int ret = nowTurn;

        ret++;
        if (1 < ret) ret = 0;

        return ret;
    }
}