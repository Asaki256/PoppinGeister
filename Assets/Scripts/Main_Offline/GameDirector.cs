using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GameDirector : MonoBehaviour
{
    //プレイヤー
    //プレイヤーの時はisPlayer[0]がtrueになる
    public bool[] isPlayer;
    Player[] player;
    int nowTurn;

    //ゲームモード ゲーム中の場面切り替え用の変数
    enum MODE
    {
        NONE = -1,
        WAIT_TURN_START,//1端末でのプレイヤー変更の際の待機時間
        MOVE_SELECT,//ゲーム中の行動ユニット選択をする
        FIELD_UPDATE,//ユニット移動後のデータ更新をする
        WAIT_TURN_END,//ターン終了を待つ
        TURN_CHANGE,//ターンの切り替え
        START_SELECT_PRE,//開始時のユニットを自動配置
        FIRST_SELECT,//開始時のユニット並び替え
        USER_CHANGE,
        SECOND_SELECT,
        CPU_UNIT_CHANGE,
        GAME_START,
        TO_1P,
        GAME_JUDGE,
    }

    //モード
    MODE nowMode;//上記のENUM型のものしか入れられない変数
    MODE nextMode;//モードを次に切り替えるための変数

    //ウェイトの定義(CPU操作時のアニメーション用)
    float waitTime;

    //フィールドの状態
    int[,] tileData = new int[,]
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
    List<GameObject>[,] unitData;//上下関係をリストで再現する

    //ユニット選択モードで使う
    GameObject selectUnit;//選択したユニットのオブジェクト
    int oldX, oldY;//選択したユニットがどこのマスから選ばれたかの座標

    //ボタンなどのGameObject
    GameObject btnTurnEnd;
    GameObject btnMoveCancel;
    GameObject btnUserChange;
    GameObject btnUnitChangeComp;
    GameObject btnTurnChange;
    GameObject txtInfo;
    GameObject objCamera;
    GameObject canvasObj;
    TextMeshProUGUI turnText;
    public AnimationCurve startCurve;
    string p1unitname = "";
    string p2unitname = "";
    int p1unit = 0;//Unitが今何番めのカウントなのか
    int p2unit = 0;
    int p1unittype = UnitController.TYPE_CITIZEN;
    int p2unittype = UnitController.TYPE_CITIZEN;
    int startPreI = 0;
    int startPreJ = 0;
    int atcUnitNum = -1;
    int difUnitNum = -1;
    bool st_selectFlag1 = false;
    bool st_selectFlag2 = false;
    bool st_nextTurnFlag = false;
    bool st_firstFlag1 = true;
    bool st_firstFlag2 = true;
    GameObject st_changeUnit0 = null;
    GameObject st_changeUnit1 = null;
    bool firstUserChange = true;
    bool userChangeFlag = false;
    Vector3 selectUnitOldPos;
    int selectUnitX = 0, selectUnitOldX = 0,
        selectUnitY = 0, selectUnitOldY = 0;

    [SerializeField]
    UnitMovedController unitMovedController;
    [SerializeField]
    UnitDetailsController unitDetailsController;
    [SerializeField]
    ResultImageController resultImageController;
    [SerializeField]
    MessageImageController messageImageController;

    void Start()
    {
        // フレームレート固定
        Application.targetFrameRate = 60;

        //画面上のオブジェクト取得
        txtInfo = GameObject.Find("Info");
        objCamera = GameObject.Find("Main Camera");
        canvasObj = GameObject.Find("Canvas");
        btnTurnEnd = canvasObj.transform.Find("OkButton").gameObject;
        btnMoveCancel = canvasObj.transform.Find("CancelButton").gameObject;
        btnTurnChange = canvasObj.transform.Find("TurnChangeButton").gameObject;
        btnUnitChangeComp = canvasObj.transform.Find("ChangeCompButton").gameObject;
        turnText = GameObject.Find("TurnText").GetComponent<TextMeshProUGUI>();
        turnText.text = "";
        btnUserChange = canvasObj.transform.Find("UserChangeButton").gameObject;
        txtInfo.GetComponent<TextMeshProUGUI>().text = "";

        //ユニットとフィールドを作成
        List<int> p1rnd = getRandomList(UNIT_MAX, UNIT_MAX / 2);
        List<int> p2rnd = getRandomList(UNIT_MAX, UNIT_MAX / 2);

        //ランダムの数値が一致した時に赤を生成
        //フィールドのサイズ分だけ現在のフィールドの状態の変数を作成
        unitData = new List<GameObject>[tileData.GetLength(0), tileData.GetLength(1)];

        //プレイヤー設定
        player = new Player[2];
        player[0] = new Player(false, 1);
        player[1] = new Player(false, 2);

        for (int i = 0; i < TitleSceneDirector.PlayerNum; i++)
        {
            player[i].IsHuman = true;
        }

        //タイルとユニットの初期化
        for (int i = 0; i < tileData.GetLength(0); i++)//Yの座標
        {
            for (int j = 0; j < tileData.GetLength(1); j++)//Xの座標
            {
                //キューブの真ん中の原点が0.5あるので、それの分だけずらす
                float x = j - (tileData.GetLength(1) / 2 - 0.5f); //列(x)
                float y = i - (tileData.GetLength(0) / 2 - 0.5f); //行(y)

                //タイルの配置
                string resname = "";

                //1：通常タイル、2：ゴールタイル、4：1Pのゴール、8：2Pのゴール
                int no = tileData[i, j];
                if (4 == no || 8 == no) no = 5;
                resname = "Cube (" + no + ")";

                resourcesInstantiate(resname, new Vector3(x, 0, y), Quaternion.identity);
            }
        }
        nowTurn = 0;
        nextMode = MODE.START_SELECT_PRE;
    }

    void Update()
    {
        if (isWait()) return;

        inputMode();

        if (MODE.NONE != nextMode) initMode(nextMode);
    }

    void FixedUpdate()
    {
        if (isWait()) return;
        physicsMode();
    }

    //待ち時間 waitTimeの分だけ処理をストップできる
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
    void inputMode()
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
        else if (MODE.START_SELECT_PRE == nowMode)
        {
            startSelectPreMode();
        }
        else if (MODE.FIRST_SELECT == nowMode)
        {
            firstSelectMode();
        }
        else if (MODE.USER_CHANGE == nowMode)
        {
            userChangeMode();
        }
        else if (MODE.SECOND_SELECT == nowMode)
        {
            secondSelectMode();
        }
        else if (MODE.CPU_UNIT_CHANGE == nowMode)
        {
            cpuUnitChangeMode();
        }
        else if (MODE.TO_1P == nowMode)
        {
            to1PMode();
        }
        else if (MODE.GAME_JUDGE == nowMode)
        {
            gameJudge();
        }
        else if (MODE.GAME_START == nowMode)
        {
            nextMode = MODE.MOVE_SELECT;
        }
    }
    void physicsMode()
    {
    }

    /// <summary>
    /// 次のモードの初期化処理(モードが変わるときに呼び出される)
    /// モード遷移時のそのモードの初期の設定を行う
    /// </summary>
    /// <param name="next">次のモード</param>
    void initMode(MODE next)
    {
        if (MODE.WAIT_TURN_END == next)
        {
            btnMoveCancel.SetActive(true);
            btnTurnEnd.SetActive(true);
        }
        else if (MODE.CPU_UNIT_CHANGE == next)
        {
            for (int n = 0; n < 100; n++)
            {
                int[] y = new int[2];
                int[] x = new int[2];
                for (int i = 0; i < 2; i++)
                {
                    y[i] = Random.Range(5, 7);
                    x[i] = Random.Range(2, 6);
                }
                unitRandomChange(x[0], y[0], x[1], y[1]);//CPUのユニットをランダムに並び替える
            }

            //item非表示
            UnitItemOnOff(2, false);
            //数秒後に完了(3秒くらい)
            waitTime = 3f;
        }
        else if (MODE.USER_CHANGE == next)
        {
        }
        else if (MODE.FIRST_SELECT == next)
        {
            messageImageController.DisplayMessageImage("#ac6de7", player[nowTurn].PlayerNo + "Pのターン");
            waitTime = 3f;

            btnUnitChangeComp.SetActive(true);
        }
        else if (MODE.SECOND_SELECT == next)
        {
            messageImageController.DisplayMessageImage("#ac6de7", player[nowTurn].PlayerNo + "Pのターン");
            waitTime = 3f;

            UnitItemOnOff(2, true);
            btnUnitChangeComp.SetActive(true);
        }
        else if (MODE.GAME_START == next)
        {
            messageImageController.DisplayMessageImage("#FFAF40", "GAME START!");
            waitTime = 5f;

            //Unit1 item表示
            UnitItemOnOff(1, true);
        }
        else if (MODE.MOVE_SELECT == next)
        {
            turnText.text = (nowTurn + 1).ToString() + "Pのターン";
            //現在のターンのプレイヤーに対して
            //占い師の正面に位置している敵ユニットのアイテムを表示する
            DivinerSkill();

            btnTurnChange.SetActive(false);
            selectUnit = null;

            messageImageController.DisplayMessageImage("#ac6de7", player[nowTurn].PlayerNo + "Pのターン");
            waitTime = 3f;

            fieldUpdate();
        }
        else if (MODE.TO_1P == next)
        {
            UnitItemOffAll();
            btnUserChange.SetActive(true);
            unitDetailsController.DetailClose();
        }
        else if (MODE.GAME_JUDGE == next)
        {
            btnTurnEnd.SetActive(false);
            btnMoveCancel.SetActive(false);
            unitDetailsController.DetailClose();
        }
        else if (MODE.WAIT_TURN_START == next)
        {
            btnTurnChange.SetActive(true);
            if (player[nowTurn].IsHuman && player[getNextTurn()].IsHuman)
            {
                UnitItemOffAll();
                unitDetailsController.DetailClose();
            }
        }

        nowMode = next;
        nextMode = MODE.NONE;
    }
    void DebugUnitDataCount()
    {
        for (int i = 0; i < 8; i++)//Y
        {
            Debug.Log(unitData[i, 0].Count + ","
            + unitData[i, 1].Count + ","
            + unitData[i, 2].Count + ","
            + unitData[i, 3].Count + ","
            + unitData[i, 4].Count + ","
            + unitData[i, 5].Count + ","
            + unitData[i, 6].Count + ","
            + unitData[i, 7].Count + ","
            );
        }
    }

    void SelectMode()
    {
        //CPUの処理(そのうち機械学習でさせたいー)
        //ランダム選択後の移動がおかしいかも。のちに修正必須
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

                    if (movableTile(oldX, oldY, rndx, rndy, selectUnit.GetComponent<UnitController>().Type))
                    {
                        Vector3 tpos = new Vector3(rndx - (tileData.GetLength(1) / 2 - 0.5f),
                            0f, rndy - (tileData.GetLength(0) / 2 - 0.5f));

                        selectUnitOldPos = selectUnit.transform.position;
                        selectUnit.transform.position = tpos;

                        selectUnitX = rndx;
                        selectUnitOldX = oldX;
                        selectUnitY = rndy;
                        selectUnitOldY = oldY;

                        break;
                    }
                }
            }
            waitTime = 2.0f;

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
            // 少しでもパフォーマンス上げるため、コントローラーの変数宣言（それでもあんまり良くなさそう）
            UnitController unitCon;

            if (null != selectUnit)
            {
                DestroyTiles("MovableTile");
                unitCon = selectUnit.GetComponent<UnitController>();
                unitCon.Select(false);
                unitDetailsController.DetailClose();
            }

            selectUnit = unitData[y, x][0];//一番下のユニットなので0番を取得
            oldX = x;
            oldY = y;

            //進めるマスを表示
            unitCon = selectUnit.GetComponent<UnitController>();
            SetMovableTile(x, y, unitCon.Type);
            unitCon.Select();
            unitDetailsController.DetailOpen(unitCon.Type);
        }
        //移動先タイル選択
        else if (null != selectUnit)
        {
            if (movableTile(oldX, oldY, x, y, selectUnit.GetComponent<UnitController>().Type))
            {
                DestroyTiles("MovableTile");

                pos.y = 0f;

                selectUnitOldPos = selectUnit.transform.position;
                selectUnit.transform.position = pos;

                selectUnitX = x;
                selectUnitOldX = oldX;
                selectUnitY = y;
                selectUnitOldY = oldY;

                nextMode = MODE.FIELD_UPDATE;
            }
        }
    }

    void fieldUpdateMode()
    {
        nextMode = MODE.TURN_CHANGE;
    }

    void turnChangeMode()
    {
        atcUnitNum = selectUnit.GetComponent<UnitController>().Type;

        if ((player[nowTurn].IsHuman && player[getNextTurn()].IsHuman)
            || (player[nowTurn].IsHuman && !player[getNextTurn()].IsHuman))
        {
            nextMode = MODE.WAIT_TURN_END;
            return;
        }
        //手番が CPU->人間 の場合
        else if (!player[nowTurn].IsHuman && player[getNextTurn()].IsHuman)
        {
            //ユニットデータ移動の確定
            unitData[selectUnitOldY, selectUnitOldX].Clear();
            unitData[selectUnitY, selectUnitX].Add(selectUnit);
            //棋譜の更新
            unitMovedController.TurnMovedImage();
            unitMovedController.UpdateMoved(selectUnitOldY, selectUnitOldX, selectUnitY, selectUnitX, nowTurn);
            unitMovedController.TurnMovedImage();

            nextMode = MODE.GAME_JUDGE;
            return;
        }
        else
        {
            Debug.LogError("プレイヤー遷移時エラー");
            return;
        }

    }

    /// <summary>
    /// ゲーム開始時にユニットを並べるモード
    /// </summary>
    void startSelectPreMode()
    {
        p2unitname = "";
        p1unitname = "";

        //キューブの真ん中の原点が0.5あるので、それの分だけずらす
        float p1x = startPreJ - (tileData.GetLength(1) / 2 - 0.5f); //列(x)
        float p1y = startPreI - (tileData.GetLength(0) / 2 - 0.5f); //行(y)

        float p2x = (tileData.GetLength(1) - startPreJ - 1) - (tileData.GetLength(1) / 2 - 0.5f); //列(x)
        float p2y = (tileData.GetLength(0) - startPreI - 1) - (tileData.GetLength(0) / 2 - 0.5f); //行(y)

        //ユニット配置
        if (unitData[startPreI, startPreJ] == null)
        {
            unitData[startPreI, startPreJ] = new List<GameObject>();
        }

        if (unitData[tileData.GetLength(1) - startPreI - 1, tileData.GetLength(0) - startPreJ - 1] == null)
        {
            unitData[tileData.GetLength(1) - startPreI - 1, tileData.GetLength(0) - startPreJ - 1] = new List<GameObject>();
        }

        //ユニットの向き(デフォルト)
        Vector3 angle = new Vector3(0, 0, 0);

        //1Pユニット配置
        if (1 == initUnitData[startPreI, startPreJ])
        {

            p1unitname = "Unit1";

            if (p1unit >= 4 && p1unit <= 7)
            {
                p1unittype = UnitController.TYPE_CITIZEN;
            }
            else if (p1unit == 3)
            {
                p1unittype = UnitController.TYPE_KING;
            }
            else if (p1unit == 2)
            {
                p1unittype = UnitController.TYPE_DEATH;
            }
            else if (p1unit == 1)
            {
                p1unittype = UnitController.TYPE_HUNTER;
            }
            else if (p1unit == 0)
            {
                p1unittype = UnitController.TYPE_DIVINER;
            }

            GameObject unit = resourcesInstantiate(
            p1unitname,
            new Vector3(p1x, 0f, p1y),
            Quaternion.Euler(angle));

            if (null != unit)
            {
                unit.GetComponent<UnitController>().PlayerNo
                    = initUnitData[startPreI, startPreJ];
                unit.GetComponent<UnitController>().Type = p1unittype;

                //固有アイテムの生成処理
                GameObject item1 = (GameObject)resourcesInstantiate(
                    p1unittype + "_item",
                    unit.transform.position,
                    Quaternion.Euler(angle)
                    );
                //ユニットの子として追加
                if (item1 != null)
                {
                    item1.transform.parent = unit.transform;
                }

                unitData[startPreI, startPreJ].Add(unit);
            }

            p1unit++;//P1のユニット番号を＋1
            waitTime = 0.5f;
        }

        if (2 == initUnitData[tileData.GetLength(1) - startPreI - 1, tileData.GetLength(0) - startPreJ - 1])
        {
            angle.y = 180;
            p2unitname = "Unit2";

            if (p2unit >= 4)
            {
                p2unittype = UnitController.TYPE_CITIZEN;
            }
            else if (p2unit == 3)
            {
                p2unittype = UnitController.TYPE_KING;
            }
            else if (p2unit == 2)
            {
                p2unittype = UnitController.TYPE_DEATH;
            }
            else if (p2unit == 1)
            {
                p2unittype = UnitController.TYPE_HUNTER;
            }
            else if (p2unit == 0)
            {
                p2unittype = UnitController.TYPE_DIVINER;
            }

            GameObject unit = resourcesInstantiate(
            p2unitname,
            new Vector3(p2x, 0f, p2y),
            Quaternion.Euler(angle));

            if (null != unit)
            {
                unit.GetComponent<UnitController>().PlayerNo
                    = initUnitData[tileData.GetLength(1) - startPreI - 1, tileData.GetLength(0) - startPreJ - 1];
                unit.GetComponent<UnitController>().Type = p2unittype;

                //固有アイテムの生成処理
                GameObject item2 = (GameObject)resourcesInstantiate(
                    p2unittype + "_item",
                    unit.transform.position,
                    Quaternion.Euler(angle)
                    );
                //ユニットの子として追加
                if (item2 != null)
                {
                    item2.transform.parent = unit.transform;
                }

                unitData[tileData.GetLength(1) - startPreI - 1, tileData.GetLength(0) - startPreJ - 1].Add(unit);
            }


            p2unit++;//P2のユニット番号を＋1
            waitTime = 0.5f;
        }

        if (startPreI == tileData.GetLength(1) - 1 && startPreJ == tileData.GetLength(0) - 1)
        {
            nextMode = MODE.FIRST_SELECT;
            return;
        }

        if (startPreJ >= tileData.GetLength(1) - 1)
        {
            startPreI++;
            startPreJ = -1;
        }
        startPreJ++;
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

    //今回はランダムな初期配置とする
    //0~rangeの中でランダムな数をcount数だけ返す
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

    /// <summary>
    /// その位置から移動可能なタイルを表示する関数
    /// </summary>
    void SetMovableTile(int x, int y, int unittype)
    {
        //unittype = 1~5
        //TYPE_CITIZEN = 1;  // 市民
        //TYPE_KING = 2;     // 王
        //TYPE_DEATH = 3;    // 死神
        //TYPE_HUNTER = 4;   // 狩人
        //TYPE_DIVINER = 5;  // 占い師

        string resname = "MovableTile";
        float posX;
        float posY;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (unittype == UnitController.TYPE_CITIZEN
                || unittype == UnitController.TYPE_DIVINER)
                {
                    if (i == 1 && j == 1) continue;
                    else if (i == 1 && j == -1) continue;
                    else if (i == 0 && j == 0) continue;
                    else if (i == -1 && j == 1) continue;
                    else if (i == -1 && j == -1) continue;
                }
                else if (unittype == UnitController.TYPE_KING
                || unittype == UnitController.TYPE_DEATH)
                {
                    if (i == 0 && j == 0) continue;
                }
                else if (unittype == UnitController.TYPE_HUNTER)
                {
                    if (i == -1 && j == 0) continue;
                    else if (i == 0 && j == -1) continue;
                    else if (i == 0 && j == 0) continue;
                    else if (i == 0 && j == 1) continue;
                    else if (i == 1 && j == 0) continue;
                }

                if (tileData[y + i, x + j] == 1
                || tileData[y + i, x + j] == 2
                || tileData[y + i, x + j] == player[nowTurn].PlayerNo * 4)
                {
                    posX = x + j - (tileData.GetLength(1) / 2 - 0.5f);
                    posY = y + i - (tileData.GetLength(0) / 2 - 0.5f);

                    if (0 == unitData[y + i, x + j].Count)//誰も乗っていない場合
                    {
                        if (UnitController.TYPE_CITIZEN != unitData[y, x][0].GetComponent<UnitController>().Type
                    && player[nowTurn].PlayerNo * 4 == tileData[y + i, x + j])
                        { }
                        else
                        {
                            //そのタイル上に薄緑のオブジェクトを生成
                            resourcesInstantiate(resname, new Vector3(posX, 0.01f, posY), Quaternion.identity);
                        }
                    }
                    else//誰かが乗っている場合
                    {
                        //相手の場合
                        if (unitData[y + i, x + j][0].GetComponent<UnitController>().PlayerNo != player[nowTurn].PlayerNo)
                        {
                            if (UnitController.TYPE_DEATH == unitData[y, x][0].GetComponent<UnitController>().Type)
                            { }
                            else
                            {
                                //そのタイル上に薄緑のオブジェクトを生成
                                resourcesInstantiate(resname, new Vector3(posX, 0.01f, posY), Quaternion.identity);
                            }
                        }
                    }
                }
            }
        }
    }

    //そこへ移動可能かどうか=今の場所から一ます分離れているかの判定
    bool movableTile(int oldx, int oldy, int x, int y, int unittype)
    {
        bool ret = false;

        //差分を取得する=差の絶対値を取得
        int dx = Mathf.Abs(oldx - x);
        int dy = Mathf.Abs(oldy - y);

        if (unittype == UnitController.TYPE_CITIZEN
        || unittype == UnitController.TYPE_DIVINER)
        {
            //斜めは進めない
            if (1 < dx + dy)
            {
                ret = false;
            }
            //壁以外かつ誰も乗っていないまたは相手がいるか
            else if (1 == tileData[y, x]  //普通のタイル
                    || 2 == tileData[y, x] //ゴールに進めるタイル
                    || player[nowTurn].PlayerNo * 4 == tileData[y, x]) //そのプレイヤーのゴール
            {
                //誰も乗っていない
                if (0 == unitData[y, x].Count)
                {
                    //青のみゴールできるため、それ以外は省く
                    if (UnitController.TYPE_CITIZEN != unitData[oldy, oldx][0].GetComponent<UnitController>().Type
                        && player[nowTurn].PlayerNo * 4 == tileData[y, x])
                    {
                        ret = false;
                    }
                    else
                    {
                        ret = true;
                    }
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
        }
        else if (unittype == UnitController.TYPE_KING
        || unittype == UnitController.TYPE_DEATH)
        {
            //斜めも全部進める
            if (!(dx == 1 && dy == 1) && 1 != dx + dy)
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
                    //青のみゴールできるため、それ以外は省く
                    if (UnitController.TYPE_CITIZEN != unitData[oldy, oldx][0].GetComponent<UnitController>().Type
                        && player[nowTurn].PlayerNo * 4 == tileData[y, x])
                    {
                        ret = false;
                    }
                    else
                    {
                        ret = true;
                    }
                }
                //誰か乗っている(仲間or相手)
                else
                {
                    //死神は、誰かがいる場所には移動できない
                    if (UnitController.TYPE_DEATH == unitData[oldy, oldx][0].GetComponent<UnitController>().Type)
                    {
                        return false;
                    }

                    //相手が乗っている場合
                    if (unitData[y, x][0].GetComponent<UnitController>().PlayerNo != player[nowTurn].PlayerNo)
                    {
                        ret = true;
                    }
                }
            }
        }
        else if (unittype == UnitController.TYPE_HUNTER)
        {
            //斜めのみ進める
            if (!(dx == 1 && dy == 1))
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
                    //青のみゴールできるため、それ以外は省く
                    if (UnitController.TYPE_CITIZEN != unitData[oldy, oldx][0].GetComponent<UnitController>().Type
                        && player[nowTurn].PlayerNo * 4 == tileData[y, x])
                    {
                        ret = false;
                    }
                    else
                    {
                        ret = true;
                    }
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
        }

        return ret;
    }

    //シーン再読み込み
    public void RestartScene()
    {
        DOTween.Clear(true);
        SceneManager.LoadScene("MainScene");
    }

    //タイトルシーンへ遷移
    public void GoTitle()
    {
        DOTween.Clear(true);
        SceneManager.LoadScene("TitleScene");
    }

    public void UnitChangeComp()
    {
        // ユニット詳細パネル非表示
        unitDetailsController.DetailClose();

        st_nextTurnFlag = true;
    }

    /// <summary>
    /// ターン終了ボタン
    /// </summary>
    public void TurnEnd()
    {
        //次のプレイヤーのターン開始
        if (MODE.WAIT_TURN_START == nowMode)
        {
            //プレイヤー同士の場合の処理
            if (player[nowTurn].IsHuman && player[getNextTurn()].IsHuman)
            {
                //1Pのカメラ
                objCamera.transform.position = new Vector3(0, 5, -5);
                objCamera.transform.eulerAngles = new Vector3(50, 0, 0);

                //2Pのカメラ
                if (2 == player[nowTurn].PlayerNo)
                {
                    objCamera.transform.position = new Vector3(0, 5, 5);
                    objCamera.transform.eulerAngles = new Vector3(50, 180, 0);
                }
                UnitItemOnOff(player[nowTurn].PlayerNo, true);
            }

            nextMode = MODE.MOVE_SELECT;
        }
        //現在のプレイヤーのターン終了
        else if (MODE.WAIT_TURN_END == nowMode)
        {
            //プレイヤー同士の場合の処理
            if (player[nowTurn].IsHuman && player[getNextTurn()].IsHuman)
            {
                // 真上にカメラセット（プレイヤーごとに異なる角度）
                if (nowTurn == 0)
                {
                    objCamera.transform.position = new Vector3(0, 9, 0);
                    objCamera.transform.eulerAngles = new Vector3(90, 0, 0);
                }
                else if (nowTurn == 1)
                {
                    objCamera.transform.position = new Vector3(0, 9, 0);
                    objCamera.transform.eulerAngles = new Vector3(90, 180, 0);
                }

            }

            //ユニットデータ移動の確定
            unitData[selectUnitOldY, selectUnitOldX].Clear();
            unitData[selectUnitY, selectUnitX].Add(selectUnit);
            //棋譜の更新
            unitMovedController.UpdateMoved(selectUnitOldY, selectUnitOldX, selectUnitY, selectUnitX, nowTurn);

            fieldUpdate();

            nextMode = MODE.GAME_JUDGE;
        }
    }

    public void MoveCancel()
    {
        btnTurnEnd.SetActive(false);
        btnMoveCancel.SetActive(false);

        selectUnit.transform.position = selectUnitOldPos;
        selectUnit.GetComponent<UnitController>().Select(false);
        unitDetailsController.DetailClose();
        nextMode = MODE.MOVE_SELECT;
    }

    //全ユニットを取得
    //後でnowTurnのPlayerのユニットのみ取得するようにする
    List<GameObject> getUnits()
    {
        List<GameObject> ret = new List<GameObject>();

        for (int i = 0; i < unitData.GetLength(0); i++)
        {
            for (int j = 0; j < unitData.GetLength(1); j++)
            {
                if (1 > unitData[i, j].Count) continue;
                ret.AddRange(unitData[i, j]);
            }
        }
        return ret;
    }

    /// <summary>
    ///MovableTile全削除
    /// </summary>
    void DestroyTiles(string desttilename)
    {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag(desttilename);
        //if(tiles == null) return;
        foreach (GameObject tile in tiles)
        {
            Destroy(tile);
        }
    }

    void firstSelectMode()
    {
        if (st_nextTurnFlag)
        {
            initUnitChangeMode();

            nextMode = MODE.CPU_UNIT_CHANGE;
            int oldturn = nowTurn;
            nowTurn = getNextTurn();

            //次がプレイヤーだったら=人間同士の場合
            if (player[oldturn].IsHuman && player[nowTurn].IsHuman)
            {
                nextMode = MODE.USER_CHANGE;
            }
            st_nextTurnFlag = false;
            return;
        }

        unitSelectFunc();
    }

    void userChangeMode()
    {
        if (firstUserChange)
        {
            //カメラを真上に移動(これが固定なのが良くない可能性がある。)
            //
            objCamera.transform.position = new Vector3(0, 9, 0);
            objCamera.transform.eulerAngles = new Vector3(90, 0, 0);

            //ボタン表示
            btnUserChange.SetActive(true);

            UnitItemOffAll();
            unitDetailsController.DetailClose();

            //テキスト表示「1Pの方は2Pの方に端末を渡してください。」

            firstUserChange = false;
        }

        //ボタンで2Pのユニット並び替えモードへ(ボタンを押したときuserChangeFlagをtrueに)
        if (userChangeFlag)
        {
            objCamera.transform.position = new Vector3(0, 5, 5);
            objCamera.transform.eulerAngles = new Vector3(50, 180, 0);

            //ボタン非表示
            btnUserChange.SetActive(false);
            userChangeFlag = false;

            nextMode = MODE.SECOND_SELECT;
        }
    }

    public void OnUserChange()
    {
        userChangeFlag = true;
    }

    void secondSelectMode()
    {
        //1Pでの並び替えを2Pができるように修正。

        if (st_nextTurnFlag)
        {
            initUnitChangeMode();

            //カメラを真上に移動
            objCamera.transform.position = new Vector3(0, 9, 0);
            objCamera.transform.eulerAngles = new Vector3(90, 180, 0);

            nextMode = MODE.TO_1P;
            nowTurn = getNextTurn();
            return;
        }

        unitSelectFunc();
    }

    void cpuUnitChangeMode()
    {
        nowTurn = getNextTurn();
        nextMode = MODE.GAME_START;
    }

    void unitRandomChange(int x0, int y0, int x1, int y1)
    {
        if (x0 == x1 && y0 == y1) return;

        GameObject unit0;
        GameObject unit1;
        if (unitData[y0, x0][0] != null && unitData[y1, x1][0] != null)
        {
            unit0 = unitData[y0, x0][0];
            unit1 = unitData[y1, x1][0];
        }
        else
        {
            Debug.LogError("CPUオブジェクトが変更できません。");
            return;
        }
        unitData[y0, x0].Clear();
        unitData[y1, x1].Clear();

        Vector3 tmpUnit0Pos = unit0.transform.position;
        unit0.transform.position = unit1.transform.position;
        unit1.transform.position = tmpUnit0Pos;
        unitData[y0, x0].Add(unit1);
        unitData[y1, x1].Add(unit0);

        return;
    }

    void initUnitChangeMode()
    {
        DestroyTiles("SelectableTile");
        DestroyTiles("SelectableTile2");
        st_selectFlag1 = false;
        st_selectFlag2 = false;
        st_changeUnit0 = null;
        st_changeUnit1 = null;
        st_firstFlag1 = true;
        st_firstFlag2 = true;
        btnUnitChangeComp.SetActive(false);
    }

    void unitSelectFunc()
    {
        if (!st_selectFlag1 && !st_selectFlag2)
        {
            if (st_firstFlag1)
            {
                //黄色タイル配置
                for (int i = 0; i < tileData.GetLength(0); i++)// Y
                {
                    for (int j = 0; j < tileData.GetLength(1); j++)// X
                    {
                        //プレイヤー番号が一致している場合、タイルを配置
                        if (0 < unitData[i, j].Count && player[nowTurn].PlayerNo == unitData[i, j][0].GetComponent<UnitController>().PlayerNo)
                        {
                            float tileX = j - (tileData.GetLength(1) / 2 - 0.5f); //列(x)
                            float tileY = i - (tileData.GetLength(0) / 2 - 0.5f); //行(y)

                            string tilename = "SelectableTile";

                            resourcesInstantiate(tilename, new Vector3(tileX, 0.01f, tileY), Quaternion.identity);
                        }
                    }
                }
                st_firstFlag1 = false;
            }

            //ユニット選択処理
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

            if (unitData[y, x].Count == 0 || player[nowTurn].PlayerNo != unitData[y, x][0].GetComponent<UnitController>().PlayerNo) return;

            st_changeUnit0 = unitData[y, x][0];

            //一つ目のユニット(1unit)が選択されている場合
            if (st_changeUnit0 != null)
            {
                //黄色タイル破壊
                DestroyTiles("SelectableTile");
                st_selectFlag1 = true;
                // ユニット説明パネル表示
                unitDetailsController.DetailOpen(unitData[y, x][0].GetComponent<UnitController>().Type);

                return;
            }
        }
        else if (st_selectFlag1 && !st_selectFlag2)
        {
            //1unit以外の全てのunitの位置に選択可能タイルを表示(オレンジ)
            ////それ以外のunitを押すことができる+押した場合、選択フラグ2をtrueにしてunit情報を保存
            //一つ目に加えてそれ以外の二つ目のユニットが選択されている場合＝選択フラグ1true 2true
            if (st_firstFlag2)
            {
                // 黄色タイル配置
                for (int i = 0; i < tileData.GetLength(0); i++)// Y
                {
                    for (int j = 0; j < tileData.GetLength(1); j++)// X
                    {
                        //プレイヤー番号が一致している場合、黄色タイルを配置
                        if (0 < unitData[i, j].Count && player[nowTurn].PlayerNo == unitData[i, j][0].GetComponent<UnitController>().PlayerNo)
                        {
                            if (unitData[i, j][0] == st_changeUnit0) continue;
                            float tileX = j - (tileData.GetLength(1) / 2 - 0.5f); //列(x)
                            float tileY = i - (tileData.GetLength(0) / 2 - 0.5f); //行(y)

                            string tilename = "SelectableTile2";

                            resourcesInstantiate(tilename, new Vector3(tileX, 0.01f, tileY), Quaternion.identity);
                        }
                    }
                }
                st_firstFlag2 = false;
            }


            //ユニット選択処理
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

            if (unitData[y, x].Count == 0 || player[nowTurn].PlayerNo != unitData[y, x][0].GetComponent<UnitController>().PlayerNo) return;

            st_changeUnit1 = unitData[y, x][0];

            //1unitを押した場合、1,2unit=null 選択フラグ1をfalse
            if (st_changeUnit0 == st_changeUnit1)
            {
                // オレンジタイル破壊
                st_changeUnit0 = null;
                st_changeUnit1 = null;
                st_selectFlag1 = false;
                DestroyTiles("SelectableTile2");
                st_firstFlag1 = true;
                st_firstFlag2 = true;

                // ユニット詳細パネル非表示
                unitDetailsController.DetailClose();

                return;
            }
            else if (st_changeUnit1 != null)
            {
                //オレンジタイル破壊
                DestroyTiles("SelectableTile2");

                st_firstFlag1 = true;
                st_firstFlag2 = true;

                st_selectFlag2 = true;

                // ユニット詳細パネル非表示
                unitDetailsController.DetailClose();

                return;
            }
            return;
        }
        else if (st_selectFlag1 && st_selectFlag2)
        {
            //positionから配列番号に変換
            int x1 = (int)(st_changeUnit0.transform.position.x + (tileData.GetLength(1)) / 2 - 0.5f);
            int y1 = (int)(st_changeUnit0.transform.position.z + (tileData.GetLength(0)) / 2 - 0.5f);
            int x2 = (int)(st_changeUnit1.transform.position.x + (tileData.GetLength(1)) / 2 - 0.5f);
            int y2 = (int)(st_changeUnit1.transform.position.z + (tileData.GetLength(0)) / 2 - 0.5f);

            //unitData配列のデータを交換
            Vector3 tmpPos;
            unitData[y2, x2].Clear();
            unitData[y1, x1].Clear();

            //unitData内のunitの位置を交換
            tmpPos = st_changeUnit1.transform.position;
            st_changeUnit1.transform.position = st_changeUnit0.transform.position;
            st_changeUnit0.transform.position = tmpPos;
            unitData[y2, x2].Add(st_changeUnit0);
            unitData[y1, x1].Add(st_changeUnit1);

            st_selectFlag1 = false;
            st_selectFlag2 = false;
            st_changeUnit0 = null;
            st_changeUnit1 = null;
            //SEse効果音ならす
            waitTime = 1f;
        }
    }

    void to1PMode()
    {
        if (userChangeFlag)
        {
            btnUserChange.SetActive(false);

            //1P視点のカメラへ
            objCamera.transform.position = new Vector3(0, 5, -5);
            objCamera.transform.eulerAngles = new Vector3(50, 0, 0);

            nextMode = MODE.GAME_START;
        }
    }

    void UnitItemOffAll()
    {
        for (int i = 0; i < tileData.GetLength(0); i++)
        {
            for (int j = 0; j < tileData.GetLength(1); j++)
            {
                if (0 < unitData[i, j].Count)
                {
                    //親オブジェクトの子の中から最後のインデックスの子を取り出す
                    //この処理の方がFindよりもパフォーマンス良さそう
                    GameObject item = unitData[i, j][0].transform.GetChild(unitData[i, j][0].transform.childCount - 1).gameObject;
                    if (item == null) continue;

                    item.SetActive(false);
                }
            }
        }
    }

    void UnitItemOnAll()
    {
        for (int i = 0; i < tileData.GetLength(0); i++)
        {
            for (int j = 0; j < tileData.GetLength(1); j++)
            {
                if (0 < unitData[i, j].Count)
                {
                    //親オブジェクトの子の中から最後のインデックスの子を取り出す
                    //この処理の方がFindよりもパフォーマンス良さそう
                    GameObject item = unitData[i, j][0].transform.GetChild(unitData[i, j][0].transform.childCount - 1).gameObject;
                    if (item == null) continue;

                    if (item.activeSelf == false)
                    {
                        item.SetActive(true);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 選択されたユニット番号のユニットアイテムを表示
    /// </summary>
    /// <param name="unitNum"></param>
    void UnitItemOnOff(int unitNum, bool isSet)
    {
        for (int i = 0; i < tileData.GetLength(0); i++)
        {
            for (int j = 0; j < tileData.GetLength(1); j++)
            {
                if (0 < unitData[i, j].Count && unitNum == unitData[i, j][0].GetComponent<UnitController>().PlayerNo)
                {
                    //親オブジェクトの子の中から最後のインデックスの子を取り出す
                    //この処理の方がFindよりもパフォーマンス良さそう
                    GameObject item = unitData[i, j][0].transform.GetChild(unitData[i, j][0].transform.childCount - 1).gameObject;
                    if (item == null) continue;

                    item.SetActive(isSet);
                }
            }
        }
    }

    void DivinerSkill()
    {
        //現在のターンのプレイヤーに対して
        //占い師の正面に位置している敵ユニットのアイテムを表示する
        for (int i = 0; i < tileData.GetLength(0); i++)
        {
            for (int j = 0; j < tileData.GetLength(1); j++)
            {
                UnitController unitCntr;

                //ユニットが存在する場合
                if (unitData[i, j].Count > 0)
                {
                    unitCntr = unitData[i, j][0].GetComponent<UnitController>();
                }
                else continue;

                //そのユニットが占い師である
                //かつ そのユニットがそのターンのプレイヤーのものである場合
                if (UnitController.TYPE_DIVINER == unitCntr.Type
                 && nowTurn + 1 == unitCntr.PlayerNo)
                {
                    //占い師ユニットが1Pかつ正面のy+1配列に敵ユニット(2P)がいる場合、そのユニットのアイテムを表示
                    if (1 == unitCntr.PlayerNo
                     && 0 < unitData[i + 1, j].Count
                     && 2 == unitData[i + 1, j][0].GetComponent<UnitController>().PlayerNo)
                    {
                        GameObject item = unitData[i + 1, j][0].transform.GetChild(unitData[i + 1, j][0].transform.childCount - 1).gameObject;
                        if (item == null) continue;
                        item.SetActive(true);
                    }
                    //占い師ユニットが2Pかつ正面のy-1配列に敵ユニット(1P)がいる場合、そのユニットのアイテムを表示
                    else if (2 == unitCntr.PlayerNo
                     && 0 < unitData[i - 1, j].Count
                     && 1 == unitData[i - 1, j][0].GetComponent<UnitController>().PlayerNo)
                    {
                        GameObject item = unitData[i - 1, j][0].transform.GetChild(unitData[i - 1, j][0].transform.childCount - 1).gameObject;
                        if (item == null) continue;
                        item.SetActive(true);
                    }
                }
            }
        }
    }

    //初期ターン変数0
    //ゲームスタート時に1追加(1)
    //
    //ターンチェンジが行われ、プレイヤーが変更された際(次のプレイヤーの手番開始時)にターン変数に1追加(2)
    //現在のユニットデータを取得し、現在のターン変数番目の配列にそのデータを格納
    //前回のユニットの移動を簡易的な図として、画面左中央に表示

    //ユニット選択時のキャンセルボタンの実装
    //ユニットデータ変更の確定を、ターン変更時に行う。
    //ターン変更が押される前にキャンセルボタンが押された場合、
    //その時点ではユニットデータは変更されずにユニットの位置を元に戻す。
    void fieldUpdate()
    {
        for (int i = 0; i < unitData.GetLength(0); i++)
        {
            for (int j = 0; j < unitData.GetLength(1); j++)
            {
                //ゴール
                if (1 == unitData[i, j].Count && player[nowTurn].PlayerNo * 4 == tileData[i, j])
                {
                    //市民ならば勝利
                    if (UnitController.TYPE_CITIZEN == unitData[i, j][0].GetComponent<UnitController>().Type)
                    {
                        player[nowTurn].IsGoal = true;
                    }
                }

                //２つ置いてあったら古いユニットを消す
                if (1 < unitData[i, j].Count)
                {
                    //王が取られた場合
                    if (UnitController.TYPE_KING == unitData[i, j][0].GetComponent<UnitController>().Type)
                    {
                        difUnitNum = UnitController.TYPE_KING;
                        //王をとったプレイヤーの勝利
                        // =そのターンでないプレイヤーのIsGoalフラグをtrue
                        player[nowTurn].IsGoal = true;
                    }
                    else if (UnitController.TYPE_DEATH == unitData[i, j][0].GetComponent<UnitController>().Type)
                    {
                        difUnitNum = UnitController.TYPE_DEATH;
                        //死神を取られたプレイヤーの勝利
                        player[getNextTurn()].IsGoal = true;
                    }

                    Destroy(unitData[i, j][0]);
                    unitData[i, j].RemoveAt(0);
                }
            }
        }
    }

    void gameJudge()
    {
        nextMode = MODE.NONE;

        //現在のターンのプレイヤー
        if (player[nowTurn].IsGoal)
        {
            // 勝敗演出表示 
            resultImageController.DisplayResultImage(player[nowTurn].PlayerNo, player[nowTurn].PlayerNo, player[getNextTurn()].PlayerNo, atcUnitNum, difUnitNum);

            UnitItemOnAll();
        }
        //次のターンのプレイヤー
        else if (player[getNextTurn()].IsGoal)
        {
            resultImageController.DisplayResultImage(player[getNextTurn()].PlayerNo, player[nowTurn].PlayerNo, player[getNextTurn()].PlayerNo, atcUnitNum, difUnitNum);

            UnitItemOnAll();
        }
        else
        {
            nextMode = MODE.WAIT_TURN_START;
            //CPU戦の場合MOVE_SELECT
            if (!(player[nowTurn].IsHuman && player[getNextTurn()].IsHuman))
            {
                nextMode = MODE.MOVE_SELECT;
            }
            //次のターンへ
            nowTurn = getNextTurn();

            // 攻防のユニットNoのリセット
            atcUnitNum = -1;
            difUnitNum = -1;
        }
    }
}