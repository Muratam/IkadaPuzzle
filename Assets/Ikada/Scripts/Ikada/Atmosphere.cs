using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Atmosphere
{
    // ステージ後半ほど暗くする
    public static void AdjustLighting(Light light = null)
    {
        if (light == null) light = GameObject.Find("Directional light").GetComponent<Light>();
        float intensity = 1f - 0.7f * (float)GameData.CurrentStageIndex / GameData.StageMax;
        RenderSettings.skybox.SetFloat("_Exposure", intensity);
        light.intensity = intensity;
    }
}
