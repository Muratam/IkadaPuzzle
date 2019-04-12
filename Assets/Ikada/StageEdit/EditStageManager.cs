using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using UnityEngine.UI;

// ステージエディット画面

public class EditStageManager : IkadaCore
{
    EditStageData[] EditStageDatas = new EditStageData[10];
    int EditStageMax => EditStageDatas.Length;

    GameObject FileList;
    GameObject FileElm;
    InputField WriteInput;
    TileObject EditTile;

    void FileListOpen()
    {
        FileList.transform.parent.gameObject.SetActive(true);
        foreach (var c in FileList.GetComponentsInChildren<Button>()) { if (c.gameObject.name == "temp") Destroy(c.gameObject); }
        var files = EditStageDatas;
        files.ToList().ForEach(f =>
        {
            var fe = Instantiate(FileElm) as GameObject;
            fe.transform.SetParent(FileList.transform);
            fe.transform.Find("Text").GetComponent<Text>().text = f.Name;
            fe.name = "temp";
            fe.GetComponent<Button>().onClick.AddListener(() =>
            {
                InitTiles(f);//"IkadaData/" + f);
                FileListClose();
            });
        });
        FileList.GetComponent<RectTransform>().sizeDelta =
            new Vector2(FileList.GetComponent<RectTransform>().sizeDelta.x,
                        Mathf.Max(72 * (files.Length + 1) * 1.3f, 72 * 1.3f * 9));
    }
    void FileListClose()
    {
        FileList.transform.parent.gameObject.SetActive(false);
    }
    void ShowMessage(string message)
    {
        MessageUI.gameObject.SetActive(true);
        MessageUI.ReStart();
        MessageUI.transform.Find("Text").GetComponent<Text>().text = message;
    }
    void HideMessage()
    {
        if (!MessageUI.gameObject.activeSelf) return;
        if (!MessageUI.Vanished) MessageUI.Vanish();
    }
    TransitionUI MessageUI;
    void Awake()
    {
        FileList = GameObject.Find("FileList");
        FileElm = GameObject.Find("FileList/FileElm");
        FileElm.GetComponent<Button>().onClick.AddListener(() => { FileListClose(); });
        WriteInput = GameObject.Find("WriteInput").GetComponent<InputField>();
        MessageUI = GameObject.Find("Message").GetComponent<TransitionUI>(); MessageUI.gameObject.SetActive(false);
        GameObject.Find("Read").GetComponent<Button>().onClick.AddListener(() => { FileListOpen(); });
        GameObject.Find("Write").GetComponent<Button>().onClick.AddListener(() =>
        {
            EditStageData.Current.SetUpStageMap(Tiles);
            EditStageData.Current.Name = WriteInput.text == "" ? "Stage" + EditStageData.Current.LocalID : WriteInput.text;
            EditStageData.Current.SetMembers();
            ShowMessage("ステージを\n 保存しました。");
            Debug.Log("Saved");
        });
        GameObject.Find("Reset").GetComponent<Button>().onClick.AddListener(() => { InitTiles(null); });
        GameObject.Find("Play").GetComponent<Button>().onClick.AddListener(() =>
        {
            GameObject.Find("Write").GetComponent<Button>().onClick.Invoke();
            Application.LoadLevel("Stage");
        });
        GameObject.Find("Publish").GetComponent<Button>().onClick.AddListener(() =>
        {
            GameObject.Find("Write").GetComponent<Button>().onClick.Invoke();
            StartCoroutine(GameObject.FindObjectOfType<WWWManager>().RegisterStage(id =>
            {
                Debug.Log("Publish" + id);
                if (id != -1)
                {
                    ShowMessage("ステージを\n投稿しました！");
                    EditStageData.Current.ServerID = id;
                }
                else
                    ShowMessage("失敗しました。\n\nステージ名が\n被っているかも\nしれません。\n\n通信環境も\n確認ください\nまた、全く同じ\nステージは\n投稿出来ません");
                EditStageData.Current.SetMembers();
            }, EditStageData.Current.Name, EditStageData.Current.StageMap, EditStageData.Current.ServerID));
        });
        GameObject.Find("Canvas/BackScene").GetComponent<Button>().onClick.AddListener(() =>
        {
            Application.LoadLevel("SceneSelect");
        });
        FileList.transform.parent.gameObject.SetActive(false);
        Player = GameObject.Find("Canvas/Player");
        foreach (var i in Enumerable.Range(0, EditStageMax))
            EditStageDatas[i] = new EditStageData(i);
        InitTiles(EditStageDatas[0]);
    }
    void Update()
    {
        int dx = Input.GetKeyDown(KeyCode.RightArrow) ? 1 :
                 Input.GetKeyDown(KeyCode.LeftArrow) ? -1 : 0;
        int dy = Input.GetKeyDown(KeyCode.UpArrow) ? 1 :
                 Input.GetKeyDown(KeyCode.DownArrow) ? -1 : 0;
        MoveCharacters(dx, dy);
        UpdateMap();
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine(GameObject.FindObjectOfType<WWWManager>().GetAllStage(dic =>
            {
                Debug.Assert(dic != null, dic);
                foreach (var line in dic)
                {
                    if (line.Key == "result") return;
                    if (!(line.Value is Dictionary<string, object>)) return;
                    var stageData = (Dictionary<string, object>)line.Value;
                    Debug.Log(line.Key + " ID:" + stageData["id"] + ":StageName:" + stageData["stage_name"]);
                    Debug.Log((string)stageData["stage"]);
                };
            }));
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            foreach (var i in Enumerable.Range(0, 50))
                StartCoroutine(GameObject.FindObjectOfType<WWWManager>().DeleteStage(res => { Debug.Log(res + "  " + i); }, "", i));
        }
        else if (Input.anyKeyDown) HideMessage();
    }

    void InitTiles(EditStageData est)
    {
        if (est != null)
        {
            InitialStrTileMap = est.MakeUpStageMap();
            WriteInput.text = est.Name;
        }
        foreach (var t in Tiles) if (t != null) Destroy(t.gameObject);
        foreach (var x in Enumerable.Range(0, w))

        {
            foreach (var y in Enumerable.Range(0, h))
            {
                string str = InitialStrTileMap[x, y];
                TileObject tileobj;
                switch (str)
                {
                    case "..":
                        tileobj = Instantiate(WaterTile, GetPositionFromPuzzlePosition(x, y), new Quaternion()) as TileObject;
                        tileobj.tile = new Tile(Tile.TileType.Water);
                        break;
                    case "##":
                        tileobj = Instantiate(WallTile, GetPositionFromPuzzlePosition(x, y), new Quaternion()) as TileObject;
                        tileobj.tile = new Tile(Tile.TileType.Wall); break;
                    case "[]":
                        tileobj = Instantiate(FloorTile, GetPositionFromPuzzlePosition(x, y), new Quaternion()) as TileObject;
                        tileobj.tile = new Tile(Tile.TileType.Normal); break;
                    default:
                        tileobj = Instantiate(IkadalTile, GetPositionFromPuzzlePosition(x, y), new Quaternion()) as TileObject;
                        var In = AlphabetLib.FromAlphabetToBool5(str[0]);
                        Across inacross = new Across(In[0], In[1], In[2], In[3], In[4]);
                        var Ex = AlphabetLib.FromAlphabetToBool5(str[1]);
                        Across exacross = new Across(Ex[0], Ex[1], Ex[2], Ex[3], Ex[4]);
                        tileobj.tile = new Tile(Tile.TileType.Ikada, inacross, exacross);
                        tileobj.GetComponentsInChildren<Button>().ToList().ForEach(b => AddButtonClick(tileobj, b));
                        break;
                }
                if (tileobj != null)
                {
                    tileobj.transform.SetParent(Stage.transform);
                    tileobj.SetInitButtonState();
                    Tiles[x, y] = tileobj;
                    tileobj.ForClickX = x;
                    tileobj.ForClickY = y;
                    tileobj.GetComponent<Button>().onClick.AddListener(() =>
                        SwitchTile(tileobj, tileobj.ForClickX, tileobj.ForClickY)
                    );
                }
            }
        }
        px = w - 1; py = h - 1;
        Player.transform.position = GetPositionFromPuzzlePosition(w - 1, h - 1);
    }

    void UpdateMap()
    {
        Player.transform.position = GetPositionFromPuzzlePosition(px, py);
        float dim = 0.36f;
        Player.transform.position += tileSize * new Vector3(PlayerTilePos.R ? dim : PlayerTilePos.L ? -dim : 0, PlayerTilePos.T ? dim : PlayerTilePos.B ? -dim : 0, 0);
        Player.transform.position += tileSize * new Vector3(0, dim, 0);
    }

}
