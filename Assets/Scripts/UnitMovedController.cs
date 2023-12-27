using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovedController : MonoBehaviour
{
    #region Private Serializable Fields
    [SerializeField]
    GameObject moved_blue_prefab;
    [SerializeField]
    GameObject moved_pink_prefab;
    [SerializeField]
    GameObject moved_tile_prefab;
    [SerializeField]
    GameObject moved_arrow_prefab;
    
    #endregion
    
    
    #region Private Fields
    int[,] initMovedData = new int[,]
    {
        { 0,2,2,2,2,0 },
        { 0,2,2,2,2,0 },
        { 0,0,0,0,0,0 },
        { 0,0,0,0,0,0 },
        { 0,1,1,1,1,0 },
        { 0,1,1,1,1,0 },
    };
    
    List<GameObject>[,] movedUnitData;

    float old_x;
    float old_y;
    Vector2 old_2vec;
    float moved_x;
    float moved_y;
    Vector2 moved_2vec;

    float arrow_x;
    float arrow_y;
    #endregion
    
    GameObject arrow_instance;
    
    #region MonoBehavior CallBacks
    void Start(){
        initMoved();
    }
    #endregion
    
    
    #region Public Methods
    // 棋譜の初期化。
    public void initMoved()
    {
        movedUnitData = new List<GameObject>[6,6];

        // すでに存在している子オブジェクトを削除
        // GameObject[] childObj = GameObject.FindGameObjectsWithTag("movedUnit");
        // foreach(GameObject child in childObj){
        //     Destroy(child);
        // }

        // タイルの配置
        for(int i=0; i<6; i++){
            for(int j=0; j<6; j++){
                float x = -125 + (i * 50);
                float y = -125 + (j * 50);
                GameObject child = Instantiate(moved_tile_prefab, new Vector3(x,y,0), Quaternion.identity);
                child.transform.SetParent(this.transform, false);
                // child.transform.SetParent(this.transform, true);
            }
        }

        
        for(int i=0; i<6; i++){
            for(int j=0; j<6; j++){
                float x = -125 + (i * 50);
                float y = 125 - (j * 50);

                if(movedUnitData[j,i] == null){
                    movedUnitData[j,i] = new List<GameObject>();
                }
                
                if(initMovedData[j, i] == 2){
                    GameObject moved_instance = Instantiate(moved_pink_prefab, new Vector3(x,y,0), Quaternion.identity);
                    moved_instance.transform.SetParent(this.transform, false);

                    if(moved_instance != null){
                        movedUnitData[j,i].Add(moved_instance);
                    }

                    // child.name = "Moved_Pink"+ (j+1) + (i+1);
                    // movedUnitData[j,i].Add(child);
                }else if(initMovedData[j, i] == 1){
                    GameObject moved_instance = Instantiate(moved_blue_prefab, new Vector3(x,y,0), Quaternion.identity);
                    moved_instance.transform.SetParent(this.transform, false);

                    // if(movedUnitData[j,i] == null){
                    //     movedUnitData[j,i] = new List<GameObject>();
                    // }

                    if(moved_instance != null){
                        movedUnitData[j,i].Add(moved_instance);
                    }
                    // child.name = "Moved_Blue"+ (j+1) + (i+1);
                    // movedUnitData[j,i].Add(child);
                }
                
            }
        }

        arrow_instance = Instantiate(moved_arrow_prefab, new Vector3(0,0,0) , Quaternion.identity);
        arrow_instance.transform.SetParent(this.transform, false);
        arrow_instance.SetActive(false);
    }

    public void UpdateMoved(int from_y, int from_x, int to_y, int to_x, int nowTurn)
    {
        if(!(1<=from_y&&from_y<=6)) return;
        if(!(1<=from_x&&from_x<=6)) return;
        if(!(1<=to_y&&to_y<=6)) return;
        if(!(1<=to_y&&to_y<=6)) return;

        //受け取った座標を０〜５に
        from_y = (7 - from_y) - 1;
        // from_y -= 1;
        from_x -= 1;
        to_y = (7 - to_y) - 1;
        // to_y -= 1;
        to_x -= 1;

        // 受け取った座標は（７ーY座標）をすることでMoved関連のUI座標に変換可能。Xはそのままでおk
        // 消すのではなく移動させることはできないか？
        // リストで管理しよう。リストの要素指定でDestroy可能なことは確認済み。
        // https://qiita.com/otochan/items/28c3ecf7377ba56187c5
        // [Unity] 動的に生成したオブジェクトへのアクセス(List<>での保持、追加、削除)

        //配列とmovedの処理がおかしい気がする。X Yとかローカル座標とか。
        old_x = -125 + ((from_x) * 50);
        old_y = 125 - ((from_y) * 50);
        old_2vec = new Vector2(old_x,old_y);

        // Unityの座標に変換した場所へ新たに作成するため。
        moved_x = -125 + ((to_x) * 50);
        moved_y = 125 - ((to_y) * 50);
        moved_2vec = new Vector2(moved_x,moved_y);

        // if(nowTurn == 0)
        // {
            // これは良くない。名前の指定方法がおかしい。
            // 配列に現状の盤面を格納。その配列のGameObjectを削除するようにする。
            // Destroy(GameObject.Find("Moved_Blue"+(from_y+1)+(from_x+1)));

            // GameObject moved_instance = Instantiate(moved_blue_prefab, new Vector3(moved_x,moved_y,0), Quaternion.identity);

        if(movedUnitData[from_y,from_x] != null && movedUnitData[to_y,to_x] != null){
            if(movedUnitData[to_y,to_x].Count == 0
            && movedUnitData[from_y,from_x].Count != 0){
                //座標移動
                movedUnitData[from_y,from_x][0].transform.localPosition 
                = new Vector3(moved_x, moved_y, 0);
                //リスト更新
                movedUnitData[to_y,to_x].Add(movedUnitData[from_y,from_x][0]);
                movedUnitData[from_y,from_x].Clear();
            }else if(movedUnitData[to_y,to_x].Count != 0
            && movedUnitData[from_y,from_x].Count != 0){
                //撃破した敵のユニットを削除
                Destroy(movedUnitData[to_y,to_x][0]);
                movedUnitData[to_y,to_x].Clear();
                if(movedUnitData[from_y,from_x][0] != null){
                    //座標移動
                    movedUnitData[from_y,from_x][0].transform.localPosition 
                    = new Vector3(moved_x, moved_y, 0);
                }
                //リスト更新
                movedUnitData[to_y,to_x].Add(movedUnitData[from_y,from_x][0]);
                movedUnitData[from_y,from_x].Clear();
            }
        }
            // 位置ずれが起きたら下記が怪しい。
            // child.transform.SetParent(this.transform, false);
            // child.name = "Moved_Blue"+ (to_y+1) + (to_x+1);
        // }
        // else if(nowTurn == 1)
        // {
        //     Destroy(GameObject.Find("Moved_Pink"+(from_y+1)+(from_x+1)));

        //     GameObject child = Instantiate(moved_pink_prefab, new Vector3(moved_x,moved_y,0), Quaternion.identity);
        //     child.transform.SetParent(this.transform, false);
        //     child.name = "Moved_Pink"+ (to_y+1) + (to_x+1);
        // }
        
        // 矢印Imageの削除と表示
        // GameObject[] arrowObj = GameObject.FindGameObjectsWithTag("movedArrow");
        // foreach(GameObject arrow in arrowObj){
        //     Destroy(arrow);
        // }
        if(!arrow_instance.activeSelf){
            arrow_instance.SetActive(true);
        }

        // arrow_x = -125 + (((from_x+to_x)/2) * 50);
        // arrow_y = 125 + (((from_y+to_y)/2) * 50);
        arrow_x = (old_x + moved_x)/2;
        arrow_y = (old_y + moved_y)/2;
        
        arrow_instance.transform.localPosition = new Vector3(arrow_x,arrow_y,0);
        if(nowTurn == 0){
            var diff = moved_2vec - old_2vec;
            arrow_instance.transform.rotation = Quaternion.FromToRotation(Vector2.right, diff);
        }else if(nowTurn == 1){
            var diff = old_2vec - moved_2vec;
            arrow_instance.transform.rotation = Quaternion.FromToRotation(Vector2.right, diff);
        }
        
    }

    // ターン切り替えの際に親要素（棋譜全体）を上下反転する
    public void TurnMovedImage()
    {
        Transform tf =  this.transform;
        tf.Rotate(0f,0f,180f);
    }

    #endregion

}
