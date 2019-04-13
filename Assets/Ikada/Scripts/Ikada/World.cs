using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    // ステージ後半ほど暗くする
    void SetLighting()
    {
        var light = GameObject.Find("Directional light").GetComponent<Light>();
        float intensity = 1f - 0.7f * (float)GameData.CurrentStageIndex / GameData.StageMax;
        RenderSettings.skybox.SetFloat("_Exposure", intensity);
        light.intensity = intensity;
    }
    void Start()
    {
        SetLighting();
    }
}
