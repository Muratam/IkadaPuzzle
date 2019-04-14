using UnityEngine;
using System.IO;
using System.Linq;
using MiniJSON;
using System.Collections.Generic;


// セーブデータのパス・変換
public class SaveData
{
    public readonly string path = "";
    public static SaveData Instance = new SaveData(Application.productName + ".dat");
    Dictionary<string, object> data = new Dictionary<string, object>();
    public SaveData(string DataName)
    {
        path = Application.persistentDataPath + "/" + DataName;
        LoadData();
    }

    public void LoadData()
    {
        if (!File.Exists(path))
        {
            data = new Dictionary<string, object>();
            Save();
            return;
        }
        else
        {
            using (FileStream f = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(f))
            {
                data = Json.Deserialize(reader.ReadToEnd()) as Dictionary<string, object>;
            }
        }
    }

    public void Save()
    {
        using (FileStream f = new FileStream(path, FileMode.Create, FileAccess.Write))
        using (StreamWriter writer = new StreamWriter(f))
        {
            writer.Write(Json.Serialize(data));
        }
    }

    public void Set<T>(string name, T t)
    {
        data[name] = t;
        Save();
    }
    public void Get(string name, out int t)
    {
        if (data.ContainsKey(name))
        {
            if (data[name] is int) t = (int)data[name];
            else t = (int)((long)data[name]);
        }
        else t = 0;
    }
    public void Get(string name, out long t)
    {
        if (data.ContainsKey(name)) t = (long)data[name];
        else t = 0;
    }
    public void Get(string name, out float t)
    {
        if (data.ContainsKey(name))
        {
            if (data[name] is float) t = (float)data[name];
            else t = (float)((double)data[name]);
        }
        else t = 0;
    }
    public void Get(string name, out double t)
    {
        if (data.ContainsKey(name)) t = (double)data[name];
        else t = 0;
    }
    public void Get(string name, out string t)
    {
        if (data.ContainsKey(name)) t = (string)data[name];
        else t = null;
    }
    public void Get(string name, out List<object> t)
    {
        if (data.ContainsKey(name))
        {
            if (data[name] is object[]) t = ((object[])data[name]).ToList();
            else t = ((List<object>)data[name]);
        }
        else t = null;
    }
    public void Get(string name, out object[] t)
    {
        if (data.ContainsKey(name))
        {
            if (data[name] is object[]) t = ((object[])data[name]);
            else t = ((List<object>)data[name]).ToArray();
        }
        else t = null;
    }

    public void Get(string name, out Dictionary<string, object> t)
    {
        if (data.ContainsKey(name))
            t = Json.Deserialize((string)data[name]) as Dictionary<string, object>;
        else t = null;
    }
}

