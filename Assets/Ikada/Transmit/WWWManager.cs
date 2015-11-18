using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;

/// <summary>
/// ステージ通信マネージャー
/// </summary>

public class WWWManager : MonoBehaviour {

    private string url = "http://hogera.sakura.ne.jp/muratam/ikadapuzzle/ikada.php";

    public IEnumerator RegisterStage(Action<bool> callback, string stage, string stageName, int userId = -1) {

        WWWForm wwwForm = new WWWForm();

        wwwForm.AddField("keyword", "RegisterStage");
        wwwForm.AddField("stage", stage);
        wwwForm.AddField("stage_name", stageName);
        if (userId != -1) wwwForm.AddField("user_id", userId);

        WWW www = new WWW(url, wwwForm);

        yield return www;

        var result = ParseJson(www);   

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

    public IEnumerator GetAllStage(Action<Dictionary<string, object>> callback) {

        WWWForm wwwForm = new WWWForm();

        wwwForm.AddField("keyword", "GetAllStage");

        WWW www = new WWW(url, wwwForm);
        yield return www;

        var result = ParseJson(www);

        if (result == null || !(result is Dictionary<string, object>)) {
            callback(null);
            yield break;

        } else {
            var resultDictionary = (Dictionary<string, object>)result;

            if (resultDictionary.ContainsKey("result") && (string)resultDictionary["result"] == "ok") {
                callback(resultDictionary);
                yield break;

            } else {
                callback(null);
                yield break;
            }
        }

    }
    public IEnumerator DeleteStage(Action<int> callback, string stageName, int stageId = -1) {

        WWWForm wwwForm = new WWWForm();

        wwwForm.AddField("keyword", "DeleteStage");
        if (stageName != "") wwwForm.AddField("stage_name", stageName);
        if (stageId != -1) wwwForm.AddField("id", stageId);
        //stage_nameとidを両方指定した場合は、idで削除するステージを検索します

        WWW www = new WWW(url, wwwForm);
        yield return www;

        var result = ParseJson(www);

        if (result == null || !(result is Dictionary<string, object>)) {
            callback(-1);
            yield break;

        } else {
            var resultDictionary = (Dictionary<string, object>)result;

            if (resultDictionary.ContainsKey("result") && (string)resultDictionary["result"] == "ok") {
                if (resultDictionary.ContainsKey("delete_count")) {
                    callback((int)(long)resultDictionary["delete_count"]);
                    yield break;
                } else {
                    callback(-1);
                    yield break;
                }

            } else {
                callback(-1);
                yield break;
            }
        }

    }

    //エラーチェックとパース
    private object ParseJson(WWW www) {

        if (www.error != null) {
            Debug.LogWarning("WWWERROR: " + www.error);
            return null;
        } else if (!www.isDone) {
            Debug.LogWarning("WWWERROR: " + "UNDONE");
            return null;
        } else if (www.text == null) {
            return null;

        } 


        //Jsonから通信結果とエラー情報の取得//////////////////////////////////////
        Debug.Log(www.text); //DEBUG: wwwの内容一覧表示
        var json = Json.Deserialize(www.text) as Dictionary<string, object>;

        var error = (List<object>)json["error"];
        foreach (string tmp in error) {
            Debug.LogWarning("Error: " + (string)tmp);
        }

        return json["result"];
        //////////////////////////////////////////////////////////////////////////

    }

}
