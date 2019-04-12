// セーブデータ
using UnityEngine;
using System.IO;
using System.Linq;
using MiniJSON;
using System.Collections.Generic;

public static class StaticSaveData
{
    public static string path => saveUtil.path;
    static SaveUtil saveUtil = new SaveUtil(Application.productName + ".dat");
    public static bool AutoSaveWhenSetIsCalled { get { return saveUtil.AutoSaveWhenSetIsCalled; } set { saveUtil.AutoSaveWhenSetIsCalled = value; } }
    public static void LoadData() { saveUtil.LoadData(); }
    public static void SaveData() { saveUtil.SaveData(); }
    public static void Set<T>(string name, T t) { saveUtil.Set(name, t); }
    public static void Get(string name, out int t) { saveUtil.Get(name, out t); }
    public static void Get(string name, out long t) { saveUtil.Get(name, out t); }
    public static void Get(string name, out float t) { saveUtil.Get(name, out t); }
    public static void Get(string name, out double t) { saveUtil.Get(name, out t); }
    public static void Get(string name, out string t) { saveUtil.Get(name, out t); }
    public static void Get(string name, out List<object> t) { saveUtil.Get(name, out t); }
    public static void Get(string name, out object[] t) { saveUtil.Get(name, out t); }
    public static void Get(string name, out Dictionary<string, object> t) { saveUtil.Get(name, out t); }
}

public class SaveUtil
{
    public readonly string path = "";
    Dictionary<string, object> data = new Dictionary<string, object>();
    public bool AutoSaveWhenSetIsCalled = true;
    public SaveUtil(string DataName, bool _AutoSaveWhenSetIsCalled = true)
    {
        if (Application.persistentDataPath[Application.persistentDataPath.Length - 1] != '/')
            path = Application.persistentDataPath + "/" + DataName;
        else path = Application.persistentDataPath + "/" + DataName;
        LoadData();
    }


    public void LoadData()
    {
        if (!File.Exists(path))
        {
            data = new Dictionary<string, object>();
            SaveData(); return;
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

    public void SaveData()
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
        if (AutoSaveWhenSetIsCalled) SaveData();
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

