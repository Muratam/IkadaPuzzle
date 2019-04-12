using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Sample : MonoBehaviour
{
    public Button button1;
    public Button button2;
    public Button button3;

    void Start()
    {

        button1.onClick.AddListener(() =>
        {
            StartCoroutine(GameObject.FindObjectOfType<WWWManager>().RegisterStage(id =>
            {
                //通信処理の成否を受け取る任意の処理
                if (id != -1)
                    Debug.Log("成功しました！ id:" + id);
                else
                    Debug.Log("失敗しましたあ id:" + id);
            }, "test", "[] [] [] [] .. .. []"));
        });

        button2.onClick.AddListener(() =>
        {
            StartCoroutine(GameObject.FindObjectOfType<WWWManager>().GetAllStage(dic =>
            {
                Debug.Assert(dic != null, dic);
                //通信処理の成否を受け取る任意の処理
                Debug.Log("成功しました！");
                foreach (var line in dic)
                {
                    if (line.Key == "result") continue;
                    if (!(line.Value is Dictionary<string, object>)) continue;
                    var stageData = (Dictionary<string, object>)line.Value;
                    //stageData.Key..."id", "stage_name", "stage"
                    //stageData.Valueはobject型なので、型変換が必要なので注意。たぶん。
                    Debug.Log(line.Key + ":StageName:" + stageData["stage_name"]);
                }
            }));
        });

        button3.onClick.AddListener(() =>
        {
            StartCoroutine(GameObject.FindObjectOfType<WWWManager>().DeleteStage(delete_count =>
            {
                //通信処理の成否を受け取る任意の処理 -1...何らかの失敗 0...該当行なし 1...削除成功
                if (delete_count != -1)
                    Debug.Log("削除行数：" + delete_count.ToString());
                else
                    Debug.Log("失敗しましたあ");
            }, "test"));
        });
    }
}
