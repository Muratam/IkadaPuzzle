using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

/// <summary>
/// ステージ通信マネージャー
/// </summary>

public class WWWManager {

    public IEnumerator RegisterStage(Action<bool> callback, string stage, int userId = -1, string stageName = null) {

        string url = "http://hogera.sakura.ne.jp/ikada_puzzle/ikada.php";

        WWWForm wwwForm = new WWWForm();

        wwwForm.AddField("keyword", "RegisterStage");
        wwwForm.AddField("stage", stage);
        if (userId != -1) wwwForm.AddField("user_id", userId);
        if (stageName != null) wwwForm.AddField("stage_name", stageName);

        WWW www = new WWW(url, wwwForm);

        yield return www;

        if (www.error != null) {
            Debug.LogWarning("WWWERROR: " + www.error);
            callback(false);
            yield break;
        } else if (!www.isDone) {
            Debug.LogWarning("WWWERROR: " + "UNDONE");
            callback(false);
            yield break;

        } else {

            var result = ParseJson(www.text);

            if (result == null || !(result is Dictionary<string, object>)) {
                callback(false);
                yield break;

            } else {
                var resultDictionary = (Dictionary<string, object>)result;

                if (resultDictionary.ContainsKey("result") && (string)resultDictionary["result"] == "ok") {
                    callback(true);
                    yield break;

                } else {
                    callback(false);
                    yield break;
                }
            } 

        }
    }

    private object ParseJson(string wwwText) {

        //Jsonから通信結果とエラー情報の取得//////////////////////////////////////
        Debug.Log(wwwText); //DEBUG: wwwの内容一覧表示
        var json = Json.Deserialize(wwwText) as Dictionary<string, object>;

        var error = (List<object>)json["error"];
        foreach (string tmp in error) {
            Debug.LogWarning("Error: " + (string)tmp);
        }

        return json["result"];
        //////////////////////////////////////////////////////////////////////////

    }

}
