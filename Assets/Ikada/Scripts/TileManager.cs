using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using UnityEngine.UI;

public struct Pos { public int x, y; public Pos(int _x, int _y) { x = _x; y = _y; } };

public class TileManager : MonoBehaviour
{
    public const int w = SystemData.w, h = SystemData.h;
    protected int EditStageMax { get { return EditStageDatas.Length; } }
    EditStageData[] EditStageDatas = new EditStageData[10];

    [SerializeField] protected TileObject IkadalTile;
    [SerializeField] protected TileObject FloorTile;
    [SerializeField] protected TileObject WallTile;
    [SerializeField] protected TileObject WaterTile;
    [SerializeField] protected GameObject Stage;
    protected TileObject[,] Tiles = new TileObject[w, h];
    protected virtual int tileSize { get { return 120; } } //120

    protected virtual Vector3 GetPositionFromPuzzlePosition(int x, int y)
    {
        return tileSize * new Vector3(x - w / 2 + 0.5f, y - h / 2 + 0.5f - 0.8f, 0);
    }
    protected virtual void SwapTileMaps(int x1, int y1, int x2, int y2)
    {
        var tmp = Tiles[x1, y1];
        Tiles[x1, y1] = Tiles[x2, y2];
        Tiles[x2, y2] = tmp;
        Tiles[x1, y1].transform.position = GetPositionFromPuzzlePosition(x1, y1);
        Tiles[x2, y2].transform.position = GetPositionFromPuzzlePosition(x2, y2);
        Tiles[x1, y1].ForClickX = x1; Tiles[x1, y1].ForClickY = y1;
        Tiles[x2, y2].ForClickX = x2; Tiles[x2, y2].ForClickY = y2;
    }

    protected bool IsInRange(int x, int y) { return (x >= 0 && y >= 0 && x < Tiles.GetLength(0) && y < Tiles.GetLength(1)); }

    protected string[,] InitialStrTileMap;

    protected void Read(string DataName)
    {
        var textAsset = Resources.Load(DataName) as TextAsset;
        try
        {
            var lines = textAsset.text.Split('\n');
            InitialStrTileMap = new string[w, h];
            foreach (var y in Enumerable.Range(0, h))
            {
                var r = lines[y];
                var read = r.Split(' ');
                foreach (var x in Enumerable.Range(0, w))
                    InitialStrTileMap[x, h - 1 - y] = read[x];
            }
        }
        catch
        {
            Debug.Log("Strange Map !!");
            InitialStrTileMap = SystemData.DefaultTileMap;
        }
    }
    protected void Write(string DataName)
    {
        using (FileStream f = new FileStream(DataName, FileMode.Create, FileAccess.Write))
        using (StreamWriter writer = new StreamWriter(f))
        {
            foreach (var y in Enumerable.Range(0, h))
            {
                string str = "";
                foreach (var x in Enumerable.Range(0, w))
                {
                    var tileobj = Tiles[x, h - 1 - y].tile;
                    switch (tileobj.tileType)
                    {
                        case Tile.TileType.Normal://[]
                            str += "[]"; break;
                        case Tile.TileType.Water://..
                            str += ".."; break;
                        case Tile.TileType.Wall://##
                            str += "##"; break;
                        case Tile.TileType.Ikada:
                            char c0 = AlphabetLib.ToAlphabetFromBool5(tileobj.InAcross.GetRLTBC());
                            char c1 = AlphabetLib.ToAlphabetFromBool5(tileobj.ExAcross.GetRLTBC());
                            str += c0 + "" + c1; break;
                    }
                    str += " ";
                }
                writer.WriteLine(str);
            }
        }
    }
    protected void SwitchTile(TileObject tileobj, int x, int y)
    {
        TileObject newTileObj = IkadalTile;
        Tile.TileType newTileType = Tile.TileType.Ikada;
        switch (tileobj.tile.tileType)
        {
            case Tile.TileType.Normal: newTileType = Tile.TileType.Wall; newTileObj = WallTile; break;
            case Tile.TileType.Water: newTileType = Tile.TileType.Normal; newTileObj = FloorTile; break;
            case Tile.TileType.Ikada: newTileType = Tile.TileType.Water; newTileObj = WaterTile; break;
            case Tile.TileType.Wall: newTileType = Tile.TileType.Ikada; newTileObj = IkadalTile; break;
        }
        newTileObj = Instantiate(newTileObj, GetPositionFromPuzzlePosition(x, y), new Quaternion()) as TileObject;
        newTileObj.tile = new Tile(newTileType, new Across(true), new Across(true));
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
    protected void AddButtonClick(TileObject tileobj, Button b)
    {
        b.onClick.AddListener(() =>
        {
            var c = b.image.color;
            var ia = tileobj.tile.InAcross;
            var ea = tileobj.tile.ExAcross;
            if (b.name[0] == 'i')
            {
                bool[] ac = ia.GetRLTBC();
                ac[b.name[1] - '0'] = !ac[b.name[1] - '0'];
                tileobj.tile = new Tile(Tile.TileType.Ikada,
                    new Across(ac[0], ac[1], ac[2], ac[3], ac[4]), ea);
            }
            else if (b.name[0] == 'e')
            {
                bool[] ac = ea.GetRLTBC();
                ac[b.name[1] - '0'] = !ac[b.name[1] - '0'];
                tileobj.tile = new Tile(Tile.TileType.Ikada,
                    ia, new Across(ac[0], ac[1], ac[2], ac[3], ac[4]));
            }
            tileobj.SetInitButtonState();
        });
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

    //* キャラクターの搭乗可能判定。 :ExAcross
    protected bool CanAcrossRide(int x, int y, Across Direction, Across Position)
    {
        if (!IsInRange(x, y)) return false;
        if (!Direction.HaveDirection) return false;
        if (!Position.HaveDirection) return false;
        //if (!(Direction & Position).HaveDirection) return false;
        if (!Across.NearlyEqualDirection(Direction, Position)) return false;
        int Desx = x + Direction.Horizontal;
        int Desy = y + Direction.Vertical;
        if (!IsInRange(Desx, Desy)) return false;
        var tile = Tiles[x, y].tile;
        var DesTile = Tiles[Desx, Desy].tile;
        return (Direction & tile.ExAcross).HaveDirection &&
            (Direction.ReversePosition() & DesTile.ExAcross).HaveDirection;
    }
    // キャラクターが筏の内部へ入れるか判定 :InAcross
    protected bool CanGoToInside(int x, int y, Across Direction, Across Position)
    {
        if (!IsInRange(x, y)) return false;
        if (!Direction.HaveDirection) return false;
        if (!Position.HaveDirection) return false;
        if (Position != Direction.ReversePosition()) return false;
        var tile = Tiles[x, y].tile;
        return (Position & tile.InAcross) == Position;
    }
    // キャラクターが筏の内部から出られるか判定 :InAcross
    protected bool CanGoFromInside(int x, int y, Across Direction)
    {
        if (!IsInRange(x, y)) return false;
        if (!Direction.HaveDirection) return false;
        var tile = Tiles[x, y].tile;
        return (Direction & tile.InAcross) == Direction;
    }

    // 1 上陸出来るなら上陸する
    // 2 下が筏で目の前が水->動かせる
    // 3 下が筏で目の前が筏->上陸できないかつ動かせる(行き先が水なら)なら押す
    protected enum MoveType { Moved, Pushed, DidntMove }
    protected MoveType MoveIkada(int x, int y, Across Direction)
    {
        if (!Direction.HaveDirection) return MoveType.DidntMove;
        if (Direction.HaveTiltDirection) return MoveType.DidntMove;
        //if (CanAcrossRide(x, y, Direction, Position)) return false;
        var tile = Tiles[x, y].tile;
        if (tile.tileType != Tile.TileType.Ikada) return MoveType.DidntMove;
        if (!(tile.ExAcross & Direction).HaveDirection) return MoveType.DidntMove;
        int Desx = x + Direction.Horizontal;
        int Desy = y + Direction.Vertical;
        if (!IsInRange(Desx, Desy)) return MoveType.DidntMove;
        var DesTile = Tiles[Desx, Desy].tile;
        if (DesTile.tileType == Tile.TileType.Water)
        {
            px += Direction.Horizontal;
            py += Direction.Vertical;
            SwapTileMaps(Desx, Desy, x, y);
            return MoveType.Moved;
        }
        else if (DesTile.tileType == Tile.TileType.Ikada)
        {
            int Des2x = Desx + Direction.Horizontal;
            int Des2y = Desy + Direction.Vertical;
            if (!IsInRange(Des2x, Des2y)) return MoveType.DidntMove;
            var Des2Tile = Tiles[Des2x, Des2y].tile;
            if (Des2Tile.tileType == Tile.TileType.Water && (tile.ExAcross & Direction).HaveDirection)
            {
                SwapTileMaps(Desx, Desy, Des2x, Des2y);
                return MoveType.Pushed;
            }
        }
        return MoveType.DidntMove;
    }

    protected MoveType MoveCharacters(int dx, int dy)
    {
        Across direction = new Across(dx == 1, dx == -1, dy == 1, dy == -1, false);
        var centerPosition = new Across(false, false, false, false, true);
        //基本的に内側に行かせて、行けないときのみ端にする
        var mtlist = new Stack<MoveType>();
        while (true)
        {
            int prepx = px, prepy = py;
            var prepos = PlayerTilePos;
            if (isPlayerInside())
            {
                if (CanGoFromInside(px, py, direction))
                {
                    PlayerTilePos = direction;
                }
                else mtlist.Push(MoveIkada(px, py, direction));
            }
            else
            {
                if (CanGoToInside(px, py, direction, PlayerTilePos))
                    PlayerTilePos = centerPosition;
                else if (CanAcrossRide(px, py, direction, PlayerTilePos))
                {
                    mtlist.Push(MoveType.Moved);
                    PlayerTilePos = PlayerTilePos.ReversePosition();
                    px += dx; py += dy;
                    break;
                }
                else
                {
                    mtlist.Push(MoveIkada(px, py, direction));
                    if (mtlist.Peek() != MoveType.DidntMove) break;
                }
            }
            if (prepx == px && prepy == py && prepos == PlayerTilePos) break;
            if (isPlayerInside()) { break; }
        }

        if ((Tiles[px, py].tile.InAcross & PlayerTilePos).HaveDirection) { PlayerTilePos = new Across(false, false, false, false, true); }
        if (mtlist.Contains(MoveType.Moved)) return MoveType.Moved;
        else if (mtlist.Contains(MoveType.Pushed)) return MoveType.Pushed;
        else return MoveType.DidntMove;
    }


    GameObject FileList;
    GameObject FileElm;
    InputField WriteInput;
    TileObject EditTile;

    protected string[] ListUpFiles()
    {
        string path = Application.dataPath;
        switch (Application.platform)
        {
            case RuntimePlatform.OSXPlayer: path += "/../../"; break;
            default: path += "/../"; break;
        }
        return System.IO.Directory.GetFiles(path + "IkadaData/", "*", SearchOption.TopDirectoryOnly)
                .Select(s => Path.GetFileName(s)).ToArray();
    }

    void FileListOpen()
    {
        FileList.transform.parent.gameObject.SetActive(true);
        foreach (var c in FileList.GetComponentsInChildren<Button>()) { if (c.gameObject.name == "temp") Destroy(c.gameObject); }
        var files = EditStageDatas;//.Select(s => s.Name).ToArray(); //ListUpFiles();
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
            Application.LoadLevel("Template");
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
    protected GameObject Player;
    protected Across PlayerTilePos = new Across(false, false, false, false, true);
    protected bool isPlayerInside() { return PlayerTilePos.C; }
    protected int px, py;
    protected virtual void UpdateMap()
    {
        Player.transform.position = GetPositionFromPuzzlePosition(px, py);
        float dim = 0.36f;
        Player.transform.position += tileSize * new Vector3(PlayerTilePos.R ? dim : PlayerTilePos.L ? -dim : 0, PlayerTilePos.T ? dim : PlayerTilePos.B ? -dim : 0, 0);
        Player.transform.position += tileSize * new Vector3(0, dim, 0);
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
}

