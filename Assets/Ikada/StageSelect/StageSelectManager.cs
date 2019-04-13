using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

// この継承に関して(StageSelectManager > StageManger)はあまり良くないパターン。
// 時間がなかったためステージ選択画面を楽に作成するためにStageMangerを再利用してしまった。
//
public class StageSelectManager : StageManager
{
    static readonly Vector3 WallFloorDiffVec = new Vector3(0, -0.5f, 0);
    [SerializeField] TransitionUI UIs;
    int[] MovedTime = new int[StageMax];
    protected virtual int TileLen => StageMax;
    protected virtual int CurrentTile
    {
        get { return CurrentStageIndex; }
        set { CurrentStageIndex = value; }
    }
    protected virtual string StageNameString => SystemData.StageName[CurrentStageIndex].Replace(".txt", "");
    void InitTiles()
    {
        foreach (var i in Enumerable.Range(0, TileLen))
        {
            var floor = Instantiate(WallTile, GetPositionFromPuzzlePosition(i, h / 2) + WallFloorDiffVec, new Quaternion()) as TileObject;
            floor.transform.SetParent(Stage.transform);
        }
        px = CurrentTile; py = h / 2;
        Player.transform.position = GetPositionFromPuzzlePosition(px, py);
    }
    protected void SetUp()
    {
        Player = GameObject.Find("Player");
        lerpPlayer = Player.GetComponent<LerpTransform>();
        gocamera = GameObject.Find("Main Camera");
        (TransParticle = GameObject.Find("TransParticle")).SetActive(false);
        var world = GameObject.Find("World");
        Player.transform.SetParent(world.transform);
        Stage.transform.SetParent(world.transform);
        GameObject.Find("Canvas/BackScene").GetComponent<Button>().onClick.AddListener(() =>
        {
            if (!alreadyStageSelected) Application.LoadLevel("SceneSelect");
        });
        InitTiles();
        lerpPlayer.Init(true);
        SetUI();
    }
    void Awake()
    {
        {
            int ci;
            StaticSaveData.Get(DataCurrentIndex, out ci);
            CurrentStageIndex = ci;
        }
        foreach (var i in Enumerable.Range(0, StageMax))
            StaticSaveData.Get(DataMovedTime(i), out MovedTime[i]);
        SetUp();
    }
    GameObject TransParticle;
    bool LerpFinishedOnce = false;
    void MovePlayer()
    {
        if (!lerpPlayer.LerpFinished) return;
        if (!LerpFinishedOnce) LerpFinishedOnce = true;
        int dx = Input.GetKey(KeyCode.RightArrow) ? 1 : Input.GetKey(KeyCode.LeftArrow) ? -1 : 0;
        if (dx == 0) return;
        if (dx == -1 && px == 0) return;
        if (dx == 1 && px == StageMax - 1) return;
        px += dx;
        lerpPlayer.EulerAngles = new Vector3(0, dx == 1 ? 0 : 180, 0);
        lerpPlayer.Position = GetPositionFromPuzzlePosition(px, py);
        CurrentTile = px;
        SetUI();
    }

    void SetUI()
    {
        SetLighting();
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
        if (MovedTimeText != null) MovedTimeText.GetComponent<Text>().text = MovedTime[CurrentStageIndex] == 0 ? "--" : MovedTime[CurrentStageIndex] + "歩";
        UIs.Vanish();
    }

    bool alreadyStageSelected = false;
    float DecidedTime = 0f;
    void GoToStage()
    {
        if (Time.time - DecidedTime < 1f) return;
        if (!lerpPlayer.LerpFinished) return;
        py++;
        lerpPlayer.Position = GetPositionFromPuzzlePosition(px, py);
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
            lerpPlayer.EulerAngles = new Vector3(0, -90, 0);
            DecidedTime = Time.time;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.X)) Application.LoadLevel("SceneSelect");
        gocamera.transform.position = new Vector3(GetPositionFromPuzzlePosition(0, h / 2).x + 2.5f, 2.5f, Player.transform.position.z + 0.0f);
    }
}
