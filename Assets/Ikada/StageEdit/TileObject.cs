﻿using UnityEngine;
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

// エディット用。タイルとクリック箇所を持つ。
public class TileObject : MonoBehaviour
{
    public Tile tile;
    public int ForClickX, ForClickY;
    public void SetInitButtonState()
    {
        if (tile.tileType != Tile.TileType.Ikada) return;
        Action<Image, bool> SetColor = (image, b) =>
        {
            var c = image.color;
            image.color = b ?
                new Color(c.r, c.g, c.b, 1f) :
                new Color(c.r, c.g, c.b, 0.15f);
        };
        var eb = tile.ExAcross.GetRLTBC();
        var ib = tile.InAcross.GetRLTBC();
        for (int i = 0; i < 4; i++)
            SetColor(transform.Find("e" + i).gameObject.GetComponent<Image>(), eb[i]);
        for (int i = 0; i < 4; i++)
            SetColor(transform.Find("e" + i + "/i" + i).gameObject.GetComponent<Image>(), ib[i]);
        SetColor(transform.Find("t0").GetComponent<Image>(), tile.InAcross.C);
    }

    public void SetInitGoIkadaState()
    {
        if (tile.tileType != Tile.TileType.Ikada) return;
        var eb = tile.ExAcross.GetRLTBC();
        var ib = tile.InAcross.GetRLTBC();
        for (int i = 0; i < 4; i++)
            transform.Find("e" + i).gameObject.SetActive(eb[i]);
        for (int i = 0; i < 4; i++)
            transform.Find("e" + i + "/i" + i).gameObject.SetActive(ib[i]);
        transform.Find("t0").gameObject.SetActive(tile.InAcross.C);
    }
}
