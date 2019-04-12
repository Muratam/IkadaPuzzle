using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EditStageData
{
    public const int h = SystemData.h, w = SystemData.w;
    public static EditStageData Current = null;
    public readonly int LocalID;
    public int ServerID { get; set; }
    public string StageMap { get; private set; }
    public string Name { get; set; }
    public EditStageData(int index)
    {
        LocalID = index;
        GetMembers();
    }
    public void GetMembers()
    {
        Dictionary<string, object> dict;
        StaticSaveData.Get("EditStage" + LocalID, out dict);
        ServerID = dict != null ? (dict["ServerID"] is int) ? (int)(dict["ServerID"]) : (int)(long)(dict["ServerID"]) : -1;
        StageMap = dict != null ? (string)(dict["StageMap"]) : "";
        Name = dict != null ? (string)(dict["Name"]) : "";
        if (Name == null || Name == "") Name = "EditStage " + LocalID;
    }
    public void SetMembers()
    {
        var dict = new Dictionary<string, object>();
        dict["ServerID"] = ServerID;
        dict["StageMap"] = StageMap;
        dict["Name"] = Name;
        string data = MiniJSON.Json.Serialize(dict);
        StaticSaveData.Set("EditStage" + LocalID, data);
    }
    public string[,] MakeUpStageMap()
    {
        Current = this;
        return SystemData.ConvertStageMap(StageMap);
    }
    public void SetUpStageMap(TileObject[,] Tiles)
    {
        StageMap = "";
        foreach (var y in Enumerable.Range(0, h))
        {
            foreach (var x in Enumerable.Range(0, w))
            {
                var tileobj = Tiles[x, h - 1 - y].tile;
                switch (tileobj.tileType)
                {
                    case Tile.TileType.Normal://[]
                        StageMap += "[]"; break;
                    case Tile.TileType.Water://..
                        StageMap += ".."; break;
                    case Tile.TileType.Wall://##
                        StageMap += "##"; break;
                    case Tile.TileType.Ikada:
                        char c0 = AlphabetLib.ToAlphabetFromBool5(tileobj.InAcross.GetRLTBC());
                        char c1 = AlphabetLib.ToAlphabetFromBool5(tileobj.ExAcross.GetRLTBC());
                        StageMap += c0 + "" + c1; break;
                }
                StageMap += " ";

            }
            StageMap += "\n";
        }
    }
}
