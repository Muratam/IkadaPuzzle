using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// エディットステージに関するデータ
public class EditStageData
{
    public const int h = StageMapUtil.h, w = StageMapUtil.w;
    public static EditStageData Current = null;
    public readonly int LocalID;
    public int ServerID;
    public string StageMap;
    public string Name;
    public EditStageData(int index)
    {
        LocalID = index;
        Load();
    }
    public void Load()
    {
        Dictionary<string, object> dict;
        SaveData.Instance.Get("EditStage" + LocalID, out dict);
        ServerID = dict != null ? (dict["ServerID"] is int) ? (int)(dict["ServerID"]) : (int)(long)(dict["ServerID"]) : -1;
        StageMap = dict != null ? (string)(dict["StageMap"]) : "";
        Name = dict != null ? (string)(dict["Name"]) : "";
        if (Name == null || Name == "") Name = "EditStage " + LocalID;
    }
    public void Save()
    {
        var dict = new Dictionary<string, object>();
        dict["ServerID"] = ServerID;
        dict["StageMap"] = StageMap;
        dict["Name"] = Name;
        string data = MiniJSON.Json.Serialize(dict);
        SaveData.Instance.Set("EditStage" + LocalID, data);
    }
}
