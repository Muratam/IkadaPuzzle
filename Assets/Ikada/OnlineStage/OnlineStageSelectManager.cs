using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class OnlineStageSelectManager : StageSelectManager
{
    public static List<StageInfo> OnlineStageInfos => SceneSelectManager.EditStageInfos;
    static int OnlineStageMax => OnlineStageInfos.Count;
    static int onLineCurrentStageIndex = 0;
    static int OnlineCurrentStageIndex
    {
        get { return onLineCurrentStageIndex; }
        set
        {
            if (value < 0) value = 0;
            if (value >= OnlineStageMax) value = OnlineStageMax - 1;
            onLineCurrentStageIndex = value;
        }
    }
    public static StageInfo OnlineStage => SceneSelectManager.EditStageInfos[OnlineCurrentStageIndex];
    void Awake()
    {
        SetUp();
    }
    protected override int TileLen => OnlineStageMax;
    protected override int CurrentTile => OnlineCurrentStageIndex;
    protected override string StageNameString => OnlineStage.StageName;
}
