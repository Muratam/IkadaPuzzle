using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

// ステージセレクト画面
public struct StageInfo
{
    public string StageName;
    public string StageMap;
    public StageInfo(string stageName, string stageMap)
    {
        this.StageName = stageName;
        this.StageMap = stageMap;
    }
};

public class SceneSelectManager : MonoBehaviour
{
    public static List<StageInfo> EditStageInfos = null;
    GameObject audio;
    [SerializeField] Button BnToStoryMode = null;
    [SerializeField] Button BnToEditMode = null;
    [SerializeField] Button BnToOnlineMode = null;

    void GoToStoryMode()
    {
        EditStageData.Current = null;
        EditStageInfos = null;
        Application.LoadLevel("StageSelect");
    }
    void GotoEditMode()
    {
        EditStageData.Current = null;
        EditStageInfos = null;
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
            if (EditStageInfos == null) EditStageInfos = new List<StageInfo>();
            EditStageInfos.Clear();
            foreach (var line in dic)
            {
                if (line.Key == "result") continue;
                if (!(line.Value is Dictionary<string, object>)) continue;
                var stageData = (Dictionary<string, object>)line.Value;
                EditStageInfos.Add(new StageInfo((string)stageData["stage_name"], (string)stageData["stage"]));
            }
            if (EditStageInfos.Count > 24)
            {
                var copy = EditStageInfos.ToArray();
                EditStageInfos.Clear();
                int max = Mathf.Min(24, copy.Length);
                foreach (var i in Enumerable.Range(0, max)) EditStageInfos.Add(copy[copy.Length - 1 - i]);
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
