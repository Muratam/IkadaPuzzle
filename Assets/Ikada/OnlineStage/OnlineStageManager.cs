using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class OnlineStageManager : StageSelectManager
{
    public static List<Pair<string>> OnlineStages_Name_Data { get { return SceneSelectManager.EditStages_Name_Data; } }
    static int OnlineStageMax { get { return OnlineStages_Name_Data.Count; } }
    static int onlinecurrentstageindex = 0;
    static int OnlineCurrentStageIndex
    {
        get { return onlinecurrentstageindex; }
        set
        {
            if (value < 0) value = 0;
            if (value >= OnlineStageMax) value = OnlineStageMax - 1;
            onlinecurrentstageindex = value;
        }
    }
    public static Pair<string> OnlineStage { get { return SceneSelectManager.EditStages_Name_Data[OnlineCurrentStageIndex]; } }


    protected virtual void Awake()
    {
        //{ int ci; StaticSaveData.Get(DATA_CURRENTINDEX, out ci); CurrentStageIndex = ci; }
        //REP(StageMax, i => StaticSaveData.Get(DATA_MOVEDTIME(i), out MovedTime[i]));
        Player = GameObject.Find("Player");
        lerpPlayer = Player.GetComponent<LerpTransform>();
        gocamera = GameObject.Find("Main Camera");
        (TransParticle = GameObject.Find("TransParticle")).SetActive(false);
        flWater = GameObject.Find("Water").GetComponent<FloatingWater>();
        flWater.transform.position = new Vector3(0, 0, StageMax / 1.5f);
        flWater.transform.localScale = new Vector3(40, 3 * StageMax, 1);
        Player.transform.SetParent(flWater.transform);
        Stage.transform.SetParent(flWater.transform);
        GameObject.Find("Canvas/BackScene").GetComponent<Button>().onClick.AddListener(() =>
        {
            if (!alreadyStageSelected) Application.LoadLevel("SceneSelect");
        });
        InitTiles();
        lerpPlayer.Init(true);
        SetUIs();
    }
    protected virtual void InitTiles()
    {
        REP(OnlineStageMax, i =>
        {
            var floor = Instantiate(WallTile, GetPositionFromPuzzlePosition(i, h / 2) + WallFloorDiffVec, new Quaternion()) as TileObject;
            floor.transform.SetParent(Stage.transform);
        });
        px = OnlineCurrentStageIndex; py = h / 2;
        Player.transform.position = GetPositionFromPuzzlePosition(px, py);
    }

    protected virtual void MovePlayer()
    {
        if (lerpPlayer.LerpFinished)
        {
            if (!LerpFinishedOnce) LerpFinishedOnce = true;
            int dx = Input.GetKey(KeyCode.RightArrow) ? 1 : Input.GetKey(KeyCode.LeftArrow) ? -1 : 0;
            if (dx == 0) return;
            if (dx == -1 && px == 0) return;
            if (dx == 1 && px == OnlineStageMax - 1) return;
            px += dx;
            lerpPlayer.EulerAngles = new Vector3(0, dx == predx ? 0 : 180, 0);
            lerpPlayer.Position = GetPositionFromPuzzlePosition(px, py);
            OnlineCurrentStageIndex = px;
            predx = dx;
            SetUIs();
        }
    }

    protected virtual void SetUIs()
    {
        SetLighting();
        var UIs = GameObject.Find("UIs").GetComponent<TransitionUI>();
        UIs.name = "OldUIs";
        var UIs2 = (Instantiate(UIs.gameObject, UIs.transform.position, new Quaternion()) as GameObject).GetComponent<TransitionUI>();
        UIs2.name = "UIs";
        UIs2.AwakePosition = UIs.AwakePosition;
        UIs2.transform.SetParent(UIs.transform.parent);
        GameObject.Find("UIs/StageName").GetComponent<Text>().text = OnlineStage.x;
        UIs.Vanish();
    }

    protected virtual void Update()
    {
        if (!alreadyStageSelected)
        {
            MovePlayer();
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Return))
            {
                alreadyStageSelected = true;
                Queue<Vec2> pos = new Queue<Vec2>();
                REP(15, i => pos.Enqueue(new Vec2(px, py + 1 + i)));
                SummonTiles(pos, WallTile.gameObject, 0.25f, WallFloorDiffVec);
                lerpPlayer.EulerAngles = new Vector3(0, predx * -90, 0);
                DecidedTime = Time.time;
            }
        }
        else GoToStage();
        gocamera.transform.position = new Vector3(GetPositionFromPuzzlePosition(0, h / 2).x + 2.5f, 2.5f, Player.transform.position.z + 0.0f);
    }

}
