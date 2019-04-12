using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using System;


// メインの(3D表示の)パズルゲーム画面の実装

public class StageManager : IkadaCore
{
    public static int StageMax => SystemData.StageName.Length;
    private static int currentStageIndex = 0;
    public static int CurrentStageIndex
    {
        set { currentStageIndex = Mathf.Clamp(value, 0, StageMax - 1); }
        get { return currentStageIndex; }
    }
    public static string BaseStageName => "IkadaData/" + CurrentStageIndex;
    protected static string DataMovedTime(int index) { return "MovedTime" + index; }
    protected static string DataCurrentIndex => "CurrentIndex";

    [SerializeField] GameObject Wave;
    [SerializeField] GameObject Barrier;
    [SerializeField] TransitionUI UIClear;
    [SerializeField] TransitionUI UIGo;
    [SerializeField] GameObject GoalTarget;
    [SerializeField] GameObject Hint;
    [SerializeField] Text HintText;
    protected GameObject gocamera;
    protected LerpTransform lerpPlayer;
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
    protected void SetCamera(bool viewIsFromPlayer)
    {
        if (this.viewIsFromPlayer == viewIsFromPlayer) return;
        this.viewIsFromPlayer = viewIsFromPlayer;
        var playerEulerAngle = new Vector3(30, 0, 0);
        var normalEulerAngle = new Vector3(90, 270, 0);
        var playerPosition = new Vector3(0, 4, -4f);
        var normalPosition = new Vector3(-0.2f, 9.5f, -0.3f);
        var lerp = gocamera.GetComponent<LerpTransform>();
        lerp.SetParent(viewIsFromPlayer ? Player.transform : null);
        lerp.EulerAngles = viewIsFromPlayer ? playerEulerAngle : normalEulerAngle;
        lerp.LocalPosition = viewIsFromPlayer ? playerPosition : normalPosition;
    }
    // 光らせる
    protected void SetLighting()
    {
        float intensity = 1f - 0.7f * (float)CurrentStageIndex / StageMax;
        RenderSettings.skybox.SetFloat("_Exposure", intensity);
        GameObject.Find("Directional light").GetComponent<Light>().intensity = intensity;
    }
    // タイルをシュッと浮かび上がれらせたりする
    static readonly Vector3 DisFloatdiffVec = new Vector3(0, -0.5f, 0);
    protected void SummonTiles(Queue<Pos> poses, GameObject go, float lerpTime = 0.15f, Vector3 diffVec = new Vector3())
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
    protected Queue<LerpTransform> SummonedTiles = new Queue<LerpTransform>();
    protected void VanishTiles()
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

    void Awake()
    {
        UIGo = GameObject.Find("Canvas/Go").GetComponent<TransitionUI>();
        UIClear = GameObject.Find("Canvas/Clear").GetComponent<TransitionUI>();
        UIGo.gameObject.SetActive(false);
        UIClear.gameObject.SetActive(false);

        Player = GameObject.Find("Player");
        lerpPlayer = Player.GetComponent<LerpTransform>();
        gocamera = GameObject.Find("Main Camera");
        var goWorld = GameObject.Find("World");
        var bBackScene = GameObject.Find("BackScene").GetComponent<Button>();
        bBackScene.onClick.AddListener(() =>
        {
            if (CurrentMode == PlayMode.Story) Application.LoadLevel("StageSelect");
            else if (CurrentMode == PlayMode.Edit) Application.LoadLevel("StageEdit");
            else if (CurrentMode == PlayMode.Online) Application.LoadLevel("OnlineStage");
        });
        GameObject.Find("Canvas/Reset").GetComponent<Button>().onClick.AddListener(() => { InitTiles(BaseStageName); });
        bBackScene.transform.Find("Text").GetComponent<Text>().text = BackSceneText;
        Player.transform.SetParent(goWorld.transform);
        Stage.transform.SetParent(goWorld.transform);
        InitTiles(BaseStageName);
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
            lerp.LerpTime = lerpPlayer.LerpTime;
            lerp.Position = GetPositionFromPuzzlePosition(x, y);
            lerp.DestroyWhenFinished = true;
        };
        MoveTileProcess(x1, y1);
        MoveTileProcess(x2, y2);
    }


    void InitTiles(string FileName)
    {
        if (CurrentMode == PlayMode.Story)
        {
            if (FileName != "") Read(FileName);
        }
        else if (CurrentMode == PlayMode.Edit)
            InitialStrTileMap = EditStageData.Current.MakeUpStageMap();
        else
            InitialStrTileMap = SystemData.ConvertStageMap(OnlineStageSelectManager.OnlineStage.StageMap);
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
                        tileobj.tile = new Tile(Tile.TileType.Water);
                        break;
                    case "##":
                        tileobj = Instantiate<TileObject>(WallTile, GetPositionFromPuzzlePosition(x, y), new Quaternion());
                        tileobj.tile = new Tile(Tile.TileType.Wall);
                        break;
                    case "[]":
                        tileobj = Instantiate<TileObject>(FloorTile, GetPositionFromPuzzlePosition(x, y), new Quaternion());
                        tileobj.tile = new Tile(Tile.TileType.Normal);
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
                        tileobj.tile = new Tile(Tile.TileType.Ikada, inacross, exacross);
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

        lerpPlayer.Position = Player.transform.position = GetPositionFromPuzzlePosition(px, py);
        lerpPlayer.EulerAngles = new Vector3(0, 180, 0);
        prePlayerDirection = new Across(false, true, false, false, false);
        isEntering = true;
        UIGo.gameObject.SetActive(true);
        UIGo.ReStart();
        UIGo.GetComponent<AudioSource>().Play();
        Queue<Pos> pos = new Queue<Pos>();
        foreach (var i in Enumerable.Range(0, 8)) pos.Enqueue(new Pos(px - i, py));
        SummonTiles(pos, FloorTile.gameObject, 0.3f);
        StaticSaveData.Set(DataCurrentIndex, CurrentStageIndex);
        GameObject.Find("StageIndex/Text").GetComponent<Text>().text
            = CurrentMode == PlayMode.Story ? "Stage " + CurrentStageIndex
            : CurrentMode == PlayMode.Edit ? EditStageData.Current.Name
            : OnlineStageSelectManager.OnlineStage.StageName;
        SetCamera(true);
        SetLighting();
        MovedTime = 0;
        WaitTime = Time.time;
        Hint.SetActive(false);

    }

    Across prePlayerDirection = new Across(false, true, false, false, false);
    void MovePlayer()
    {
        if (Input.GetKeyDown(KeyCode.X)) { InitTiles(BaseStageName); return; }
        if (!lerpPlayer.LerpFinished) return;
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
            lerpPlayer.EulerAngles = new Vector3(0, inx == 0 ? 0 : inx * 180, 0);
            return;
        }
        int Angle = playerDirection.OneDirectionToAngle() * 90;
        lerpPlayer.EulerAngles = new Vector3(0, Angle, 0);
        prePlayerDirection = playerDirection;
        if (MoveCharacters(dx, dy) != MoveType.DidntMove) MovedTime++;
        else
        {
            var pos = Player.transform.position;
            pos += new Vector3(-playerDirection.Vertical, 0, playerDirection.Horizontal) * tileSize / 2;
            pos += new Vector3(0, 0.5f, 0);
            Instantiate(Barrier, pos, Player.transform.rotation);
        }
        lerpPlayer.Position = GetPositionFromPuzzlePosition(px, py)
            + tileSize * 0.36f * new Vector3(-1 * (PlayerTilePos.T ? 1 : PlayerTilePos.B ? -1 : 0), 0,
            PlayerTilePos.R ? 1 : PlayerTilePos.L ? -1 : 0);

    }
    bool isEntering = true;
    bool isGoaled = false;
    float WaitTime;
    void EnterGame()
    {
        if (Time.time - WaitTime < 1f) return;
        if (!lerpPlayer.LerpFinished) return;
        px--;
        lerpPlayer.Position = GetPositionFromPuzzlePosition(px, py);
        if (px != w - 1) return;
        isEntering = false;
        GameObject.Find("Reset/Text").GetComponent<Text>().text = "リセット\n(Xキー)";
        GameObject.Find("BackScene/Text").GetComponent<Text>().text = BackSceneText;
        if (UIGo.gameObject.activeSelf) UIGo.Vanish();
        SetCamera(false);
        if (CurrentMode == PlayMode.Story)
        {
            if (CurrentStageIndex < SystemData.Hints.Count())
            {
                Hint.SetActive(true);
                HintText.text = SystemData.Hints[CurrentStageIndex];
            }
        }

        VanishTiles();
    }
    void Goaled()
    {
        if (Time.time - WaitTime < 1f) return;
        if (!lerpPlayer.LerpFinished) return;
        px--;
        lerpPlayer.Position = GetPositionFromPuzzlePosition(px, py);
        if (px != -8) return;
        VanishTiles();
        if (UIClear.gameObject.activeSelf) UIClear.Vanish();
        isGoaled = false;
        if (CurrentMode == PlayMode.Story)
        {
            CurrentStageIndex++;
            InitTiles(BaseStageName);
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
        lerpPlayer.EulerAngles = new Vector3(0, 180, 0);
        prePlayerDirection = new Across(false, true, false, false, false);
        Queue<Pos> pos = new Queue<Pos>();
        foreach (var i in Enumerable.Range(0, 20))
            pos.Enqueue(new Pos(px - i, py));
        SummonTiles(pos, FloorTile.gameObject, 0.3f);
        SetCamera(true);
        if (CurrentMode == PlayMode.Story)
        {
            int oldTime; StaticSaveData.Get(DataMovedTime(CurrentStageIndex), out oldTime);
            if (oldTime == 0 || MovedTime < oldTime)
                StaticSaveData.Set(DataMovedTime(CurrentStageIndex), MovedTime);
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
        if (lerpPlayer.LerpFinished && px == 0 && Tiles[px, py].tile.tileType == Tile.TileType.Normal)
        {
            AchieveGoal();
            return;
        }
        MovePlayer();
    }
}
