using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

// ステージ選択画面
public class StageSelectManager : MonoBehaviour
{
    public const int w = StageMapUtil.w, h = StageMapUtil.h;
    static readonly Vector3 WallFloorDiffVec = new Vector3(0, -0.5f, 0);
    [SerializeField] protected TransitionUI UIs;
    [SerializeField] protected LerpTransform LerpPlayer;
    [SerializeField] protected TileObject WallTile;
    [SerializeField] protected TileObject WaterTile;
    [SerializeField] protected GameObject Stage;
    [SerializeField] protected GameObject Player;
    [SerializeField] protected GameObject Camera;

    int[] movedTime = new int[GameData.StageMax];
    int tileSize = 1;
    protected virtual int TileLen => GameData.StageMax;
    protected virtual int CurrentTile
    {
        get { return GameData.CurrentStageIndex; }
        set { GameData.CurrentStageIndex = value; }
    }
    Vector3 GetPositionFromPuzzlePosition(int x, int y)
    {
        return tileSize * new Vector3(
            -1 * (y - h / 2 + 0.5f),
            0,
            x - w / 2 + 0.5f);
    }
    int px, py;
    protected virtual string StageNameString => StoryData.StageNames[GameData.CurrentStageIndex];
    void InitTiles()
    {
        foreach (var i in Enumerable.Range(0, TileLen))
        {
            var floor = Instantiate(WallTile, GetPositionFromPuzzlePosition(i, h / 2) + WallFloorDiffVec, new Quaternion()) as TileObject;
            floor.transform.SetParent(Stage.transform);
        }
    }
    protected void SetUp()
    {
        Player = GameObject.Find("Player");
        LerpPlayer = Player.GetComponent<LerpTransform>();
        Camera = GameObject.Find("Main Camera");
        (TransParticle = GameObject.Find("TransParticle")).SetActive(false);
        var world = GameObject.Find("World");
        Player.transform.SetParent(world.transform);
        Stage.transform.SetParent(world.transform);
        GameObject.Find("Canvas/BackScene").GetComponent<Button>().onClick.AddListener(() =>
        {
            if (!alreadyStageSelected) Application.LoadLevel("SceneSelect");
        });
        InitTiles();
        px = CurrentTile;
        py = h / 2;
        Player.transform.position = GetPositionFromPuzzlePosition(px, py);
        LerpPlayer.Init(true);
        SetUI();
    }
    void Awake()
    {
        {
            int ci;
            SaveData.Instance.Get(GameData.DataCurrentIndex, out ci);
            GameData.CurrentStageIndex = ci;
        }
        foreach (var i in Enumerable.Range(0, GameData.StageMax))
            SaveData.Instance.Get(GameData.DataMovedTime(i), out movedTime[i]);
        SetUp();
    }
    GameObject TransParticle;
    bool LerpFinishedOnce = false;
    void MovePlayer()
    {
        if (!LerpPlayer.LerpFinished) return;
        if (!LerpFinishedOnce) LerpFinishedOnce = true;
        int dx = Input.GetKey(KeyCode.RightArrow) ? 1 : Input.GetKey(KeyCode.LeftArrow) ? -1 : 0;
        if (dx == 0) return;
        if (dx == -1 && px == 0) return;
        if (dx == 1 && px == TileLen - 1) return;
        px += dx;
        LerpPlayer.EulerAngles = new Vector3(0, dx == 1 ? 0 : 180, 0);
        LerpPlayer.Position = GetPositionFromPuzzlePosition(px, py);
        CurrentTile = px;
        SetUI();
        Atmosphere.AdjustLighting();
    }

    void SetUI()
    {
        var UIs = GameObject.Find("UIs").GetComponent<TransitionUI>();
        UIs.name = "OldUIs";
        var UIs2 = (Instantiate(UIs.gameObject, UIs.transform.position, new Quaternion()) as GameObject).GetComponent<TransitionUI>();
        UIs2.name = "UIs";
        UIs2.AwakePosition = UIs.AwakePosition;
        UIs2.transform.SetParent(UIs.transform.parent);
        var StageIndexText = GameObject.Find("UIs/StageIndex");
        if (StageIndexText != null) StageIndexText.GetComponent<Text>().text = "Stage " + CurrentTile;
        var StageNameText = GameObject.Find("UIs/StageName");
        if (StageNameText != null) StageNameText.GetComponent<Text>().text = StageNameString;
        var MovedTimeText = GameObject.Find("UIs/MovedTime");
        if (MovedTimeText != null)
        {
            var text = "--";
            if (movedTime[GameData.CurrentStageIndex] != 0) text = movedTime[GameData.CurrentStageIndex] + "歩";
            MovedTimeText.GetComponent<Text>().text = text;
        }
        UIs.Vanish();
    }

    bool alreadyStageSelected = false;
    float DecidedTime = 0f;
    void GoToStage()
    {
        if (Time.time - DecidedTime < 1f) return;
        if (!LerpPlayer.LerpFinished) return;
        py++;
        LerpPlayer.Position = GetPositionFromPuzzlePosition(px, py);
        if (py == h / 2 + 7 - 1) TransParticle.SetActive(true);
        else if (py == h / 2 + 7) Application.LoadLevel("Stage");
    }
    void Update()
    {
        if (alreadyStageSelected)
        {
            GoToStage();
            return;
        }
        MovePlayer();
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Return))
        {
            alreadyStageSelected = true;
            var pos = new Queue<Pos>();
            foreach (var i in Enumerable.Range(0, 15)) pos.Enqueue(new Pos(px, py + 1 + i));
            SummonTiles(pos, WallTile.gameObject, 0.25f, WallFloorDiffVec);
            LerpPlayer.EulerAngles = new Vector3(0, -90, 0);
            DecidedTime = Time.time;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.X)) Application.LoadLevel("SceneSelect");
        Camera.transform.position = new Vector3(GetPositionFromPuzzlePosition(0, h / 2).x + 2.5f, 2.5f, Player.transform.position.z + 0.0f);
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

}
