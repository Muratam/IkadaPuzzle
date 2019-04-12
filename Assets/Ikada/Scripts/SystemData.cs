using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public static class SystemData
{
    public const int w = 10, h = 8;
    public static readonly string[] StageName = new string[]{
        "Tutorial1 操作方法.txt",
        "Tutorial2 壁.txt",
        "Tutorial3 乗り継ぎ.txt",
        "Tutorial4 押す(1).txt",
        "Tutorial5 押す(2).txt",
        "Tutorial6 足場(1).txt",
        "Tutorial7 足場(2).txt",
        "Tutorial8 足場(3).txt",
        "壁を越すには.txt",
        "急がば回れ.txt",
        "いかだを乗り継いで.txt",
        "乗り継ぐ順番.txt",
        "渡れるいかだはどれだ.txt",
        "上からか、右からか.txt",
        "さっきの友が今は敵.txt",
        "壁の向こうへ.txt",
        "島への障害物.txt",
        "島へ行くには.txt",
        "いかだの選択.txt",
        "門番いかだ.txt",
        "6つのいかだ.txt",
        "5つのいかだ.txt",
        "5連いかだの先へ.txt",
        "いかだ迷路.txt",
        "遠い向こう岸.txt",
        "いかだの壁.txt",
    };
    public static readonly string[] Hints = new string[]{
        "十字キーで操作してね。\n筏に乗ると水の上を渡れるよ！\n左端まで行こう！",
        "灰色の壁にぶつかると\nそれ以上薦めないよ！\nよく考えて移動しよう！",
        "筏は乗り継げるよ！",
        "筏の黄色の部分が繋がって\nないと乗り継げないよ！\nその場合先の筏を押すよ！ ",
        "筏は黄色の部分がある方向に移動できるよ！\n筏は一つまでしか押せないよ！",
        "筏は今乗っている部分と\n繋がっていないと\n乗り継げないよ！！！！！！ ",
        "筏は黄色の方向へ移動できる.\n筏は繋がっていないと\n乗り継げない！",
        "最後のチュートリアルステージ！\n筏の形をよく見てね！ ",
    };

    public static string[,] ConvertStageMap(string StageMap)
    {
        StageMap = StageMap.Replace("\r\n", "\n");
        var InitialStrTileMap = new string[w, h];
        try
        {
            var MapDatas = StageMap.Split('\n');
            foreach (var y in Enumerable.Range(0, h))
            {
                var r = MapDatas[y];
                var read = r.Split(' ');
                foreach (var x in Enumerable.Range(0, w))
                {
                    InitialStrTileMap[x, h - 1 - y] = read[x];
                }
            }
        }
        catch
        {
            Debug.Log("Strange Map !!");
            InitialStrTileMap = DefaultTileMap;
        }
        return InitialStrTileMap;
    }
    public static string[,] DefaultTileMap
    {
        get
        {
            string[,] strmap = new string[w, h];
            foreach (var y in Enumerable.Range(0, h))
                foreach (var x in Enumerable.Range(0, w))
                {
                    if (x == 0 || x == w - 1) strmap[x, y] = "[]";
                    else strmap[x, y] = "..";
                }
            return strmap;
        }
    }

}
