using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameData
{
    public static int StageMax => StoryData.StageNames.Length;
    private static int currentStageIndex = 0;
    public static int CurrentStageIndex
    {
        set { currentStageIndex = Mathf.Clamp(value, 0, StageMax - 1); }
        get { return currentStageIndex; }
    }
    public static string BaseStageName => "IkadaData/" + CurrentStageIndex;
    public static string DataCurrentIndex => "CurrentIndex";
    public static string DataMovedTime(int index) { return "MovedTime" + index; }
}
