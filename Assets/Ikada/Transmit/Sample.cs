using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class Sample : MonoBehaviour {
    public Button button1;
    public Button button2;


    // Use this for initialization
    void Start () {

        button1.onClick.AddListener(() => {
            StartCoroutine(GameObject.FindObjectOfType<WWWManager>().RegisterStage(isSuccess => {

                //通信処理の成否を受け取る任意の処理
                if (isSuccess) {
                    Debug.Log("成功しました！");
                } else {
                    Debug.Log("失敗しましたあ");
                }

            }, "stagedata01", "stagename01"));
        });

    }

    // Update is called once per frame
    void Update () {
	
	}
}
