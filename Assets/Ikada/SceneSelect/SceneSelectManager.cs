using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
public class SceneSelectManager : MonoBehaviour {
	public static List<Pair<string>> EditStages_Name_Data = null;
	GameObject audio;
	void Start() {
		DontDestroyOnLoad(audio);
		audio.name = "Audio2";
	}

	void Awake () {
		audio = GameObject.Find("Audio");
		var audio2 = GameObject.Find("Audio2");
		if (audio2 != null) Destroy(audio2);

		GameObject.Find("ToStoryMode").GetComponent<Button>().onClick.AddListener(() => {
			TileManager.EditStageData.Current = null;
			EditStages_Name_Data = null;
			Application.LoadLevel("StageSelect");
		});
		GameObject.Find("ToEditMode").GetComponent<Button>().onClick.AddListener(() => {
			TileManager.EditStageData.Current = null;
			EditStages_Name_Data = null;
			Application.LoadLevel("StageEdit");	
		});
		GameObject.Find("ToOnlineMode").GetComponent<Button>().onClick.AddListener(() => {
			StartCoroutine(GameObject.FindObjectOfType<WWWManager>().GetAllStage(dic => {
				if (dic != null) {
					if (EditStages_Name_Data == null) EditStages_Name_Data = new List<Pair<string>>();
					EditStages_Name_Data.Clear();
					dic.Foreach(line => {
						if (line.Key != "result") {
							if (line.Value is Dictionary<string, object>) {
								var stageData = (Dictionary<string, object>)line.Value;
								EditStages_Name_Data.Add(new Pair<string>((string)stageData["stage_name"],(string)stageData["stage"]));
								//stageData.Key..."id", "stage_name", "stage"
								//stageData.Valueはobject型なので、型変換が必要なので注意。たぶん。
								//Debug.Log(line.Key + ":StageName:" + stageData["stage_name"]);
							}
						}
					});
					if (EditStages_Name_Data.Count > 24) {
						var copy = EditStages_Name_Data.ToArray();
						EditStages_Name_Data.Clear();
						int max = Mathf.Min(24,copy.Length);
						for (int i = 0;i < max ; i++) {
							EditStages_Name_Data.Add(copy[copy.Length - 1 - i]);
						}
					}
					TileManager.EditStageData.Current = null;
					Application.LoadLevel("OnlineStage");	
				} else {
					if (!Message.gameObject.activeSelf) Message.gameObject.SetActive(true);
					Message.ReStart();
					MessageText.text = "ステージの取得に\n失敗しました。\n\n接続を確認して\n再度アクセス\nしてください";
				}
			}));
		});
		Message = GameObject.Find("Message").GetComponent<TransitionUI>();
		MessageText = Message.transform.Find("Text").GetComponent<Text>();
		Message.gameObject.SetActive(false);
	}
	TransitionUI Message;
	Text MessageText;
	void Update() {
		if (Input.anyKeyDown) Message.gameObject.SetActive(false);
	}
	
}
