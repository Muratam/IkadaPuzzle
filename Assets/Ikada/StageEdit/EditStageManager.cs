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
    [SerializeField] EditableTileObject IkadalTile;
    [SerializeField] EditableTileObject FloorTile;
    [SerializeField] EditableTileObject WallTile;
    [SerializeField] EditableTileObject WaterTile;
    [SerializeField] GameObject Stage;
    [SerializeField] GameObject Player;

    GameObject FileList;
    GameObject FileElm;
    InputField WriteInput;
    EditableTileObject EditTile;

    void OpenFileList()
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
                CloseFileList();
            });
        });
        FileList.GetComponent<RectTransform>().sizeDelta =
            new Vector2(FileList.GetComponent<RectTransform>().sizeDelta.x,
                        Mathf.Max(72 * (files.Length + 1) * 1.3f, 72 * 1.3f * 9));
    }
    void CloseFileList()
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
        FileElm.GetComponent<Button>().onClick.AddListener(() => { CloseFileList(); });
        WriteInput = GameObject.Find("WriteInput").GetComponent<InputField>();
        MessageUI = GameObject.Find("Message").GetComponent<TransitionUI>(); MessageUI.gameObject.SetActive(false);
        GameObject.Find("Read").GetComponent<Button>().onClick.AddListener(() => { OpenFileList(); });
        GameObject.Find("Write").GetComponent<Button>().onClick.AddListener(() =>
        {
            EditStageData.Current.StageMap = StageMapUtil.TileObjsToString(Tiles);
            EditStageData.Current.Name = WriteInput.text == "" ? "Stage" + EditStageData.Current.LocalID : WriteInput.text;
            EditStageData.Current.Save();
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
                EditStageData.Current.Save();
            }, EditStageData.Current.Name, EditStageData.Current.StageMap, EditStageData.Current.ServerID));
        });
        GameObject.Find("Canvas/BackScene").GetComponent<Button>().onClick.AddListener(() =>
        {
            Application.LoadLevel("SceneSelect");
        });
        FileList.transform.parent.gameObject.SetActive(false);
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

    string[,] InitialStrTileMap;
    void InitTiles(EditStageData est)
    {
        if (est != null)
        {
            EditStageData.Current = est;
            InitialStrTileMap = StageMapUtil.Split(est.StageMap);
            WriteInput.text = est.Name;
        }
        foreach (var t in Tiles) if (t != null) Destroy(t.gameObject);
        foreach (var x in Enumerable.Range(0, w))

        {
            foreach (var y in Enumerable.Range(0, h))
            {
                string str = InitialStrTileMap[x, y];
                EditableTileObject tileobj;
                switch (str)
                {
                    case "..":
                        tileobj = Instantiate(WaterTile, GetPositionFromPuzzlePosition(x, y), new Quaternion()) as EditableTileObject;
                        tileobj.Tile = new Tile(Tile.TileType.Water);
                        break;
                    case "##":
                        tileobj = Instantiate(WallTile, GetPositionFromPuzzlePosition(x, y), new Quaternion()) as EditableTileObject;
                        tileobj.Tile = new Tile(Tile.TileType.Wall); break;
                    case "[]":
                        tileobj = Instantiate(FloorTile, GetPositionFromPuzzlePosition(x, y), new Quaternion()) as EditableTileObject;
                        tileobj.Tile = new Tile(Tile.TileType.Normal); break;
                    default:
                        tileobj = Instantiate(IkadalTile, GetPositionFromPuzzlePosition(x, y), new Quaternion()) as EditableTileObject;
                        var In = AlphabetLib.FromAlphabetToBool5(str[0]);
                        var inacross = new Across(In[0], In[1], In[2], In[3], In[4]);
                        var Ex = AlphabetLib.FromAlphabetToBool5(str[1]);
                        var exacross = new Across(Ex[0], Ex[1], Ex[2], Ex[3], Ex[4]);
                        tileobj.Tile = new Tile(Tile.TileType.Ikada, inacross, exacross);
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
    void SwitchTile(EditableTileObject tileobj, int x, int y)
    {
        var newTileObj = IkadalTile;
        var newTileType = Tile.TileType.Ikada;
        switch (tileobj.Tile.tileType)
        {
            case Tile.TileType.Normal: newTileType = Tile.TileType.Wall; newTileObj = WallTile; break;
            case Tile.TileType.Water: newTileType = Tile.TileType.Normal; newTileObj = FloorTile; break;
            case Tile.TileType.Ikada: newTileType = Tile.TileType.Water; newTileObj = WaterTile; break;
            case Tile.TileType.Wall: newTileType = Tile.TileType.Ikada; newTileObj = IkadalTile; break;
        }
        newTileObj = Instantiate(newTileObj, GetPositionFromPuzzlePosition(x, y), new Quaternion()) as EditableTileObject;
        newTileObj.Tile = new Tile(newTileType, new Across(true), new Across(true));
        newTileObj.transform.SetParent(Stage.transform);
        newTileObj.SetInitButtonState();
        if (newTileType == Tile.TileType.Ikada)
            newTileObj.GetComponentsInChildren<Button>().ToList().ForEach(b => AddButtonClick(newTileObj, b));
        newTileObj.ForClickX = x; newTileObj.ForClickY = y;
        newTileObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            SwitchTile(newTileObj, newTileObj.ForClickX, newTileObj.ForClickY);
        });
        Tiles[x, y] = newTileObj;
        Destroy(tileobj.gameObject);
    }
    void AddButtonClick(EditableTileObject tileobj, Button b)
    {
        b.onClick.AddListener(() =>
        {
            var c = b.image.color;
            var ia = tileobj.Tile.InAcross;
            var ea = tileobj.Tile.ExAcross;
            if (b.name[0] == 'i')
            {
                bool[] ac = ia.GetRLTBC();
                ac[b.name[1] - '0'] = !ac[b.name[1] - '0'];
                tileobj.Tile = new Tile(Tile.TileType.Ikada,
                    new Across(ac[0], ac[1], ac[2], ac[3], ac[4]), ea);
            }
            else if (b.name[0] == 'e')
            {
                bool[] ac = ea.GetRLTBC();
                ac[b.name[1] - '0'] = !ac[b.name[1] - '0'];
                tileobj.Tile = new Tile(Tile.TileType.Ikada,
                    ia, new Across(ac[0], ac[1], ac[2], ac[3], ac[4]));
            }
            tileobj.SetInitButtonState();
        });
    }
    void UpdateMap()
    {
        Player.transform.position = GetPositionFromPuzzlePosition(px, py);
        float dim = 0.36f;
        Player.transform.position += tileSize * new Vector3(PlayerTilePos.R ? dim : PlayerTilePos.L ? -dim : 0, PlayerTilePos.T ? dim : PlayerTilePos.B ? -dim : 0, 0);
        Player.transform.position += tileSize * new Vector3(0, dim, 0);
    }

}
