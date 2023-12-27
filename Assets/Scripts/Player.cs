using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//MonoBehaviorはGameObjectにアタッチするためのクラス。今回は通常のクラスとしてのみ扱うので、必要ない
public class Player
{
    public bool IsHuman;//プレイヤーが人間かどうか
    public int PlayerNo;//ユニット判断のための変数
    public bool IsGoal;//ゲームの終了判定

    public Player(bool isplayer, int playerno)
    {
        this.IsHuman = isplayer;
        this.PlayerNo = playerno;
    }

    public string GetPlayerName()
    {
        string ret = "";
        string playerName = PlayerNo + "P";
        ret = playerName;

        return ret;
    }
}