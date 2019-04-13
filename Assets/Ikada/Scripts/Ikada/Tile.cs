using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

// 筏 / 水 / 壁 / 足場 を合わせて タイルと定義する
// それを統括する
// 今後の拡張のために全て9方向x{内,外}で管理する
public class Tile
{
    public enum TileType
    {
        Normal, Water, Ikada, Wall
    }
    public readonly TileType tileType;
    public readonly Across ExAcross;//外側
    public readonly Across InAcross;//筏用の内側8方向の出入りに用いる
    public Tile(TileType tileType, Across inAcross = null, Across exAcross = null)
    {
        this.tileType = tileType;
        switch (tileType)
        {
            case TileType.Normal:
                ExAcross = new Across(true);
                InAcross = new Across(true); break;
            case TileType.Water:
            case TileType.Wall:
                ExAcross = new Across(false);
                InAcross = new Across(false); break;
            case TileType.Ikada:
                ExAcross = new Across(exAcross.Mat);
                InAcross = new Across(inAcross.Mat); break;
        }
    }
}
