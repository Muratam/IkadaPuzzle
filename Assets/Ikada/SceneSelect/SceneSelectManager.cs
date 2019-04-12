using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

// ステージセレクト画面
public class SceneSelectManager : MonoBehaviour
{
    public static List<Pair<string>> EditStages_Name_Data = null;
    GameObject audio;
    [SerializeField] Button BnToStoryMode = null;
    [SerializeField] Button BnToEditMode = null;
    [SerializeField] Button BnToOnlineMode = null;

    void GoToStoryMode()
    {
        EditStageData.Current = null;
        EditStages_Name_Data = null;
        Application.LoadLevel("StageSelect");
    }
    void GotoEditMode()
    {
        EditStageData.Current = null;
        EditStages_Name_Data = null;
        Application.LoadLevel("StageEdit");
    }
    void GotoOnlineMode()
    {
        StartCoroutine(GameObject.FindObjectOfType<WWWManager>().GetAllStage(dic =>
        {
            if (dic == null)
            {
                if (!Message.gameObject.activeSelf) Message.gameObject.SetActive(true);
                Message.ReStart();
                MessageText.text = "ステージの取得に\n失敗しました。\n\n接続を確認して\n再度アクセス\nしてください";
                return;
            }
            if (EditStages_Name_Data == null) EditStages_Name_Data = new List<Pair<string>>();
            EditStages_Name_Data.Clear();
            foreach (var line in dic)
            {
                if (line.Key == "result") continue;
                if (!(line.Value is Dictionary<string, object>)) continue;
                var stageData = (Dictionary<string, object>)line.Value;
                EditStages_Name_Data.Add(new Pair<string>((string)stageData["stage_name"], (string)stageData["stage"]));
                //stageData.Key..."id", "stage_name", "stage"
                //stageData.Valueはobject型なので、型変換が必要なので注意。たぶん。
                //Debug.Log(line.Key + ":StageName:" + stageData["stage_name"]);
            }
            if (EditStages_Name_Data.Count > 24)
            {
                var copy = EditStages_Name_Data.ToArray();
                EditStages_Name_Data.Clear();
                int max = Mathf.Min(24, copy.Length);
                foreach (var i in Enumerable.Range(0, max)) EditStages_Name_Data.Add(copy[copy.Length - 1 - i]);
            }
            EditStageData.Current = null;
            Application.LoadLevel("OnlineStage");
        }));
    }

    void Start()
    {
        DontDestroyOnLoad(audio);
        audio.name = "Audio2";
    }

    void Awake()
    {
        audio = GameObject.Find("Audio");
        var audio2 = GameObject.Find("Audio2");
        if (audio2 != null) Destroy(audio2);
        BnToStoryMode.onClick.AddListener(this.GoToStoryMode);
        BnToEditMode.onClick.AddListener(this.GotoEditMode);
        BnToOnlineMode.onClick.AddListener(this.GotoOnlineMode);
        Message = GameObject.Find("Message").GetComponent<TransitionUI>();
        MessageText = Message.transform.Find("Text").GetComponent<Text>();
        Message.gameObject.SetActive(false);
    }
    TransitionUI Message;
    Text MessageText;
    void Update()
    {
        if (Input.anyKeyDown) Message.gameObject.SetActive(false);
    }

}
