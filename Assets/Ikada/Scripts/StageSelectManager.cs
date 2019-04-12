using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class StageSelectManager : IkadaManager
{
    protected static readonly Vector3 WallFloorDiffVec = new Vector3(0, -0.5f, 0);
    [SerializeField] protected TransitionUI UIs;
    protected int[] MovedTime = new int[StageMax];
    protected virtual void InitTiles()
    {
        REP(StageMax, i =>
        {
            var floor = Instantiate(WallTile, GetPositionFromPuzzlePosition(i, h / 2) + WallFloorDiffVec, new Quaternion()) as TileObject;
            floor.transform.SetParent(Stage.transform);
        });
        px = CurrentStageIndex; py = h / 2;
        Player.transform.position = GetPositionFromPuzzlePosition(px, py);
    }

    protected virtual void Awake()
    {
        { int ci; StaticSaveData.Get(DATA_CURRENTINDEX, out ci); CurrentStageIndex = ci; }
        REP(StageMax, i => StaticSaveData.Get(DATA_MOVEDTIME(i), out MovedTime[i]));
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
        SetUI();
    }
    protected int predx = 1;
    protected GameObject TransParticle;
    protected bool LerpFinishedOnce = false;
    protected virtual void MovePlayer()
    {
        if (!lerpPlayer.LerpFinished) return;
        if (!LerpFinishedOnce)
        {
            LerpFinishedOnce = true;
            //SetLighting();
        }
        int dx = Input.GetKey(KeyCode.RightArrow) ? 1 :
                    Input.GetKey(KeyCode.LeftArrow) ? -1 : 0;
        if (dx == 0) return;
        if (dx == -1 && px == 0) return;
        if (dx == 1 && px == StageMax - 1) return;
        px += dx;
        lerpPlayer.EulerAngles = new Vector3(0, dx == predx ? 0 : 180, 0);
        lerpPlayer.Position = GetPositionFromPuzzlePosition(px, py);
        CurrentStageIndex = px;
        predx = dx;
        SetUI();
    }
    protected virtual void SetUI()
    {
        SetLighting();
        var UIs = GameObject.Find("UIs").GetComponent<TransitionUI>();
        UIs.name = "OldUIs";
        var UIs2 = (Instantiate(UIs.gameObject, UIs.transform.position, new Quaternion()) as GameObject).GetComponent<TransitionUI>();
        UIs2.name = "UIs";
        UIs2.AwakePosition = UIs.AwakePosition;
        UIs2.transform.SetParent(UIs.transform.parent);
        GameObject.Find("UIs/StageIndex").GetComponent<Text>().text = "Stage " + CurrentStageIndex;
        GameObject.Find("UIs/StageName").GetComponent<Text>().text = StageName[CurrentStageIndex].Replace(".txt", "");
        GameObject.Find("UIs/MovedTime").GetComponent<Text>().text = MovedTime[CurrentStageIndex] == 0 ? "--" : "" + MovedTime[CurrentStageIndex];
        UIs.Vanish();
    }

    protected bool alreadyStageSelected = false;
    protected float DecidedTime = 0f;
    protected virtual void GoToStage()
    {
        if (Time.time - DecidedTime < 1f) return;
        if (!lerpPlayer.LerpFinished) return;
        py++;
        lerpPlayer.Position = GetPositionFromPuzzlePosition(px, py);
        if (py == h / 2 + 7 - 1) TransParticle.SetActive(true);
        else if (py == h / 2 + 7) Application.LoadLevel("Template");
    }
    protected virtual void Update()
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
            Queue<Vec2> pos = new Queue<Vec2>();
            REP(15, i => pos.Enqueue(new Vec2(px, py + 1 + i)));
            SummonTiles(pos, WallTile.gameObject, 0.25f, WallFloorDiffVec);
            lerpPlayer.EulerAngles = new Vector3(0, predx * -90, 0);
            DecidedTime = Time.time;
        }
        else if (Input.GetKeyDown(KeyCode.X)) Application.LoadLevel("SceneSelect");
        gocamera.transform.position = new Vector3(GetPositionFromPuzzlePosition(0, h / 2).x + 2.5f, 2.5f, Player.transform.position.z + 0.0f);
    }
}
