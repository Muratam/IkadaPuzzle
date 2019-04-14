using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using System;


// メインの(3D表示の)パズルゲーム画面の実装

public class StageManager : IkadaCore
{
    [SerializeField] TileObject IkadalTile;
    [SerializeField] TileObject FloorTile;
    [SerializeField] TileObject WallTile;
    [SerializeField] TileObject WaterTile;
    [SerializeField] GameObject Stage;
    [SerializeField] GameObject Player;
    [SerializeField] GameObject Wave;
    [SerializeField] GameObject Barrier;
    [SerializeField] TransitionUI UIClear;
    [SerializeField] TransitionUI UIGo;
    [SerializeField] GameObject GoalTarget;
    [SerializeField] GameObject Hint;
    [SerializeField] Text HintText;
    [SerializeField] GameObject Camera;
    LerpTransform LerpPlayer;
    public enum PlayMode { Story, Edit, Online }
    public PlayMode CurrentMode
    {
        get
        {
            if (EditStageData.Current != null) return PlayMode.Edit;
            else if (OnlineStageSelectManager.OnlineStageInfos != null) return PlayMode.Online;
            else return PlayMode.Story;
        }
    }

    int movedTime = 0;
    int MovedTime
    {
        get { return movedTime; }
        set
        {
            movedTime = value;
            GameObject.Find("MovedTime/Text").GetComponent<Text>().text = "" + value;
        }
    }
    bool viewIsFromPlayer = false;
    // プレイヤーにフォーカス可能
    void SetCamera(bool viewIsFromPlayer)
    {
        if (this.viewIsFromPlayer == viewIsFromPlayer) return;
        this.viewIsFromPlayer = viewIsFromPlayer;
        var playerEulerAngle = new Vector3(30, 0, 0);
        var normalEulerAngle = new Vector3(90, 270, 0);
        var playerPosition = new Vector3(0, 4, -4f);
        var normalPosition = new Vector3(-0.2f, 9.5f, -0.3f);
        var lerp = Camera.GetComponent<LerpTransform>();
        lerp.SetParent(viewIsFromPlayer ? Player.transform : null);
        lerp.EulerAngles = viewIsFromPlayer ? playerEulerAngle : normalEulerAngle;
        lerp.LocalPosition = viewIsFromPlayer ? playerPosition : normalPosition;
    }
    // タイルをシュッと浮かび上がれらせたりする
    static readonly Vector3 DisFloatdiffVec = new Vector3(0, -0.5f, 0);
    void SummonTiles(Queue<Pos> poses, GameObject go, float lerpTime = 0.15f, Vector3 diffVec = new Vector3())
    {
        if (poses.Count == 0) return;
        var pos = poses.Dequeue();
        var lerp = (Instantiate(go, GetPositionFromPuzzlePosition(pos.x, pos.y) + DisFloatdiffVec + diffVec, new Quaternion()) as GameObject).AddComponent<LerpTransform>();
        lerp.transform.SetParent(Stage.transform);
        lerp.Position = GetPositionFromPuzzlePosition(pos.x, pos.y) + diffVec;
        lerp.LerpTime = lerpTime;
        lerp.AddAction(() =>
        {
            SummonTiles(poses, go, lerpTime, diffVec);
        });
        SummonedTiles.Enqueue(lerp);
    }
    Queue<LerpTransform> SummonedTiles = new Queue<LerpTransform>();
    void VanishTiles()
    {
        foreach (var lerp in SummonedTiles)
        {
            lerp.LocalPosition = lerp.transform.localPosition + DisFloatdiffVec;
            lerp.ClearActions();
            var _lerp = lerp;
            lerp.AddAction(() => Destroy(_lerp.gameObject));
        }
        SummonedTiles.Clear();
    }


    string BackSceneText
    {
        get
        {
            if (CurrentMode == PlayMode.Story) return "ステージ選択へ";
            if (CurrentMode == PlayMode.Edit) return "ステージエディターへ";
            return "ステージ選択へ";
        }
    }

    [SerializeField] GameObject PlayerObject;
    void ToggleHidePlayer()
    {
        if (PlayerObject == null) return;
        PlayerObject.SetActive(!PlayerObject.activeSelf);
    }

    void Awake()
    {
        UIGo = GameObject.Find("Canvas/Go").GetComponent<TransitionUI>();
        UIClear = GameObject.Find("Canvas/Clear").GetComponent<TransitionUI>();
        UIGo.gameObject.SetActive(false);
        UIClear.gameObject.SetActive(false);
        LerpPlayer = Player.GetComponent<LerpTransform>();
        var goWorld = GameObject.Find("World");
        var bBackScene = GameObject.Find("BackScene").GetComponent<Button>();
        bBackScene.onClick.AddListener(() =>
        {
            if (CurrentMode == PlayMode.Story) Application.LoadLevel("StageSelect");
            else if (CurrentMode == PlayMode.Edit) Application.LoadLevel("StageEdit");
            else if (CurrentMode == PlayMode.Online) Application.LoadLevel("OnlineStage");
        });
        GameObject.Find("Canvas/Reset").GetComponent<Button>().onClick.AddListener(() => { InitGame(GameData.BaseStageName); });
        GameObject.Find("Canvas/ToggleHide").GetComponent<Button>().onClick.AddListener(ToggleHidePlayer);
        bBackScene.transform.Find("Text").GetComponent<Text>().text = BackSceneText;
        Player.transform.SetParent(goWorld.transform);
        Stage.transform.SetParent(goWorld.transform);
        InitGame(GameData.BaseStageName);
    }
    protected override int tileSize => 1;
    protected override Vector3 GetPositionFromPuzzlePosition(int x, int y)
    {
        return tileSize * new Vector3(
            -1 * (y - h / 2 + 0.5f),
            0,
            x - w / 2 + 0.5f);
    }
    protected override void SwapTileMaps(int x1, int y1, int x2, int y2)
    {
        var tmp = Tiles[x1, y1];
        Tiles[x1, y1] = Tiles[x2, y2];
        Tiles[x2, y2] = tmp;
        Action<int, int> MoveTileProcess = (x, y) =>
        {
            var goWave = Instantiate(Wave, Tiles[x, y].transform.position + new Vector3(0, 0.5f, 0), Quaternion.Euler(270, 0, 0)) as GameObject;
            goWave.transform.SetParent(Tiles[x, y].transform);
            var lerp = Tiles[x, y].GetComponent<LerpTransform>() ? Tiles[x, y].GetComponent<LerpTransform>() : Tiles[x, y].gameObject.AddComponent<LerpTransform>();
            lerp.LerpTime = LerpPlayer.LerpTime;
            lerp.Position = GetPositionFromPuzzlePosition(x, y);
            lerp.DestroyWhenFinished = true;
        };
        MoveTileProcess(x1, y1);
        MoveTileProcess(x2, y2);
    }

    string[,] InitialStrTileMap;

    void InitTiles(string fileName)
    {
        if (CurrentMode == PlayMode.Story)
        {
            if (fileName != "") InitialStrTileMap = StageMapUtil.ReadFile(fileName);
        }
        else if (CurrentMode == PlayMode.Edit)
            InitialStrTileMap = StageMapUtil.Split(EditStageData.Current.StageMap);
        else
            InitialStrTileMap = StageMapUtil.Split(OnlineStageSelectManager.OnlineStage.Map);
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
                        tileobj = Instantiate<TileObject>(WaterTile, GetPositionFromPuzzlePosition(x, y), new Quaternion());
                        tileobj.Tile = new Tile(Tile.TileType.Water);
                        break;
                    case "##":
                        tileobj = Instantiate<TileObject>(WallTile, GetPositionFromPuzzlePosition(x, y), new Quaternion());
                        tileobj.Tile = new Tile(Tile.TileType.Wall);
                        break;
                    case "[]":
                        tileobj = Instantiate<TileObject>(FloorTile, GetPositionFromPuzzlePosition(x, y), new Quaternion());
                        tileobj.Tile = new Tile(Tile.TileType.Normal);
                        if (x == 0)
                        {
                            var goal = Instantiate<GameObject>(GoalTarget);
                            goal.transform.SetParent(tileobj.transform);
                            goal.transform.localPosition = new Vector3(0, 0.7f, 0);
                        }

                        break;
                    default:
                        tileobj = Instantiate(IkadalTile, GetPositionFromPuzzlePosition(x, y), new Quaternion()) as TileObject;
                        var In = AlphabetLib.FromAlphabetToBool5(str[0]);
                        Across inacross = new Across(In[0], In[1], In[2], In[3], In[4]);
                        var Ex = AlphabetLib.FromAlphabetToBool5(str[1]);
                        Across exacross = new Across(Ex[0], Ex[1], Ex[2], Ex[3], Ex[4]);
                        tileobj.Tile = new Tile(Tile.TileType.Ikada, inacross, exacross);
                        break;
                }
                if (tileobj == null) continue;
                tileobj.transform.SetParent(Stage.transform);
                tileobj.SetInitGoIkadaState();
                Tiles[x, y] = tileobj;
            }
        }
        px = w - 1; py = h - 1;
        px += 8;
    }
    void InitGame(string fileName)
    {
        InitTiles(fileName);
        LerpPlayer.Position = Player.transform.position = GetPositionFromPuzzlePosition(px, py);
        LerpPlayer.EulerAngles = new Vector3(0, 180, 0);
        prePlayerDirection = new Across(false, true, false, false, false);
        isEntering = true;
        UIGo.gameObject.SetActive(true);
        UIGo.ReStart();
        UIGo.GetComponent<AudioSource>().Play();
        Queue<Pos> pos = new Queue<Pos>();
        foreach (var i in Enumerable.Range(0, 8)) pos.Enqueue(new Pos(px - i, py));
        SummonTiles(pos, FloorTile.gameObject, 0.3f);
        SaveData.Instance.Set(GameData.DataCurrentIndex, GameData.CurrentStageIndex);
        GameObject.Find("StageIndex/Text").GetComponent<Text>().text
            = CurrentMode == PlayMode.Story ? "Stage " + GameData.CurrentStageIndex
            : CurrentMode == PlayMode.Edit ? EditStageData.Current.Name
            : OnlineStageSelectManager.OnlineStage.Name;
        SetCamera(true);
        MovedTime = 0;
        WaitTime = Time.time;
        Hint.SetActive(false);
        Atmosphere.AdjustLighting();

    }

    Across prePlayerDirection = new Across(false, true, false, false, false);
    void MovePlayer()
    {
        if (Input.GetKeyDown(KeyCode.X)) { InitGame(GameData.BaseStageName); return; }
        if (Input.GetKeyDown(KeyCode.Z)) { ToggleHidePlayer(); return; }
        if (!LerpPlayer.LerpFinished) return;
        int dx = Input.GetKey(KeyCode.RightArrow) ? 1 :
                 Input.GetKey(KeyCode.LeftArrow) ? -1 : 0;
        int dy = Input.GetKey(KeyCode.UpArrow) ? 1 :
                 Input.GetKey(KeyCode.DownArrow) ? -1 : 0;
        if (dx == 0 && dy == 0) return;
        int inx = dx, iny = dy;
        if (viewIsFromPlayer)
        {
            if (prePlayerDirection.Horizontal != 0)
            {
                int ddy = dy;
                dy = -prePlayerDirection.Horizontal * dx;
                dx = prePlayerDirection.Horizontal * ddy;
            }
            else if (prePlayerDirection.B)
            {
                dx *= -1; dy *= -1;
            }
        }

        var playerDirection = new Across(dx == 1, dx == -1, dy == 1, dy == -1, false);
        if (viewIsFromPlayer && !(playerDirection & prePlayerDirection).HaveDirection)
        {
            prePlayerDirection = playerDirection;
            LerpPlayer.EulerAngles = new Vector3(0, inx == 0 ? 0 : inx * 180, 0);
            return;
        }
        int Angle = playerDirection.OneDirectionToAngle() * 90;
        LerpPlayer.EulerAngles = new Vector3(0, Angle, 0);
        prePlayerDirection = playerDirection;
        if (MoveCharacters(dx, dy) != MoveType.DidntMove) MovedTime++;
        else
        {
            var pos = Player.transform.position;
            pos += new Vector3(-playerDirection.Vertical, 0, playerDirection.Horizontal) * tileSize / 2;
            pos += new Vector3(0, 0.5f, 0);
            Instantiate(Barrier, pos, Player.transform.rotation);
        }
        LerpPlayer.Position = GetPositionFromPuzzlePosition(px, py)
            + tileSize * 0.36f * new Vector3(-1 * (PlayerTilePos.T ? 1 : PlayerTilePos.B ? -1 : 0), 0,
            PlayerTilePos.R ? 1 : PlayerTilePos.L ? -1 : 0);

    }
    bool isEntering = true;
    bool isGoaled = false;
    float WaitTime;
    void EnterGame()
    {
        if (Time.time - WaitTime < 1f) return;
        if (!LerpPlayer.LerpFinished) return;
        px--;
        LerpPlayer.Position = GetPositionFromPuzzlePosition(px, py);
        if (px != w - 1) return;
        isEntering = false;
        GameObject.Find("Reset/Text").GetComponent<Text>().text = "リセット\n(Xキー)";
        GameObject.Find("BackScene/Text").GetComponent<Text>().text = BackSceneText;
        if (UIGo.gameObject.activeSelf) UIGo.Vanish();
        SetCamera(false);
        if (CurrentMode == PlayMode.Story)
        {
            if (GameData.CurrentStageIndex < StoryData.Hints.Count())
            {
                Hint.SetActive(true);
                HintText.text = StoryData.Hints[GameData.CurrentStageIndex];
            }
        }

        VanishTiles();
    }
    void Goaled()
    {
        if (Time.time - WaitTime < 1f) return;
        if (!LerpPlayer.LerpFinished) return;
        px--;
        LerpPlayer.Position = GetPositionFromPuzzlePosition(px, py);
        if (px != -8) return;
        VanishTiles();
        if (UIClear.gameObject.activeSelf) UIClear.Vanish();
        isGoaled = false;
        if (CurrentMode == PlayMode.Story)
        {
            GameData.CurrentStageIndex++;
            InitGame(GameData.BaseStageName);
        }
        else if (CurrentMode == PlayMode.Edit)
            Application.LoadLevel("StageEdit");
        else if (CurrentMode == PlayMode.Online)
            Application.LoadLevel("OnlineStage");
    }
    void AchieveGoal()
    {
        WaitTime = Time.time;
        isGoaled = true;
        Hint.SetActive(false);
        UIClear.gameObject.SetActive(true);
        UIClear.ReStart();
        UIClear.GetComponent<AudioSource>().Play();
        LerpPlayer.EulerAngles = new Vector3(0, 180, 0);
        prePlayerDirection = new Across(false, true, false, false, false);
        Queue<Pos> pos = new Queue<Pos>();
        foreach (var i in Enumerable.Range(0, 20))
            pos.Enqueue(new Pos(px - i, py));
        SummonTiles(pos, FloorTile.gameObject, 0.3f);
        SetCamera(true);
        if (CurrentMode == PlayMode.Story)
        {
            int oldTime;
            SaveData.Instance.Get(GameData.DataMovedTime(GameData.CurrentStageIndex), out oldTime);
            if (oldTime == 0 || MovedTime < oldTime)
                SaveData.Instance.Set(GameData.DataMovedTime(GameData.CurrentStageIndex), MovedTime);
        }
    }

    void Update()
    {
        if (isEntering)
        {
            GameObject.Find("Reset/Text").GetComponent<Text>().text = "リセット";
            GameObject.Find("BackScene/Text").GetComponent<Text>().text = BackSceneText + "\n(Xキー)";
            if (Input.GetKeyDown(KeyCode.X))
                GameObject.Find("BackScene").GetComponent<Button>().onClick.Invoke();
            else EnterGame();
            return;
        }
        if (isGoaled)
        {
            Goaled();
            return;
        }
        if (LerpPlayer.LerpFinished && px == 0 && Tiles[px, py].Tile.tileType == Tile.TileType.Normal)
        {
            AchieveGoal();
            return;
        }
        MovePlayer();
    }
}
