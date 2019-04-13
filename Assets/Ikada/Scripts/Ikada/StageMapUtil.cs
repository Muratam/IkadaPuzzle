using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using UnityEngine;

// 文字列のマップ情報から行列に変換したり読み込んだり
public class StageMapUtil
{
    // ステージは 10 x 8 のマップに固定する
    public const int w = 10, h = 8;
    // 読み込んだ一連のデータから行列に変換する
    public static string[,] Split(string StageMap)
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
                    InitialStrTileMap[x, h - 1 - y] = read[x];
            }
        }
        catch
        {
            // 不正なデータが送られてくる可能性がある
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
    public static string[,] ReadFile(string DataName)
    {
        var result = new string[w, h];
        var textAsset = Resources.Load(DataName) as TextAsset;
        try
        {
            var lines = textAsset.text.Split('\n');
            foreach (var y in Enumerable.Range(0, h))
            {
                var r = lines[y];
                var read = r.Split(' ');
                foreach (var x in Enumerable.Range(0, w))
                    result[x, h - 1 - y] = read[x];
            }
            return result;
        }
        catch
        {
            Debug.Log("Strange Map !!");
            return DefaultTileMap;
        }
    }
    public static void WriteFile(string path, Tile[,] tiles)
    {
        using (FileStream f = new FileStream(path, FileMode.Create, FileAccess.Write))
        using (StreamWriter writer = new StreamWriter(f))
        {
            writer.WriteLine(TilesToString(tiles));
        }
    }


    public static string TilesToString(Tile[,] tiles)
    {
        var result = "";
        foreach (var y in Enumerable.Range(0, h))
        {
            foreach (var x in Enumerable.Range(0, w))
            {
                var tile = tiles[x, h - 1 - y];
                switch (tile.tileType)
                {
                    case Tile.TileType.Normal://[]
                        result += "[]"; break;
                    case Tile.TileType.Water://..
                        result += ".."; break;
                    case Tile.TileType.Wall://##
                        result += "##"; break;
                    case Tile.TileType.Ikada:
                        char c0 = AlphabetLib.ToAlphabetFromBool5(tile.InAcross.GetRLTBC());
                        char c1 = AlphabetLib.ToAlphabetFromBool5(tile.ExAcross.GetRLTBC());
                        result += c0 + "" + c1; break;
                }
                result += " ";

            }
            result += "\n";
        }
        return result;
    }
    public static string TileObjsToString(TileObject[,] tileobjs)
    {
        var w = tileobjs.GetLength(0);
        var h = tileobjs.GetLength(1);
        var tiles = new Tile[w, h];
        foreach (var x in Enumerable.Range(0, w))
            foreach (var y in Enumerable.Range(0, h))
                tiles[x, y] = tileobjs[x, y].Tile;
        return TilesToString(tiles);
    }


}
