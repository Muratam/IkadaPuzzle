using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class StageSelectManager : IkadaManager {
	
	protected virtual void InitTiles() {
		REP(StageMax, i => {
			var floor = Instantiate(WallTile, GetPositionFromPuzzlePosition(i,h/2)-new Vector3(0,0.5f,0), new Quaternion()) as TileObject;
			floor.transform.SetParent(Stage.transform);
		});
		px = CurrentStageIndex; py = h/2;
		DestPlayerPosition = Player.transform.position = GetPositionFromPuzzlePosition(px,py);
	}
	
	protected virtual void Awake () {
		Player = GameObject.Find("Player");
		gocamera = GameObject.Find("Main Camera");
		flWater = GameObject.Find("Water").GetComponent<FloatingWater>();
		flWater.transform.position = new Vector3(0, 0, StageMax/1.5f);
		flWater.transform.localScale = new Vector3(18, 3 * StageMax, 1);
		Player.transform.SetParent(flWater.transform);
		Stage.transform.SetParent(flWater.transform);
		InitTiles();
		DestPlayerPosition = Player.transform.position;
		DestPlayerEuler = Player.transform.rotation.eulerAngles;
		SetLighting();
	}

	protected virtual void MovePlayer() {
		LerpingTime += Time.deltaTime;
		if (LerpingTime < LerpTime) {
			float per = LerpingTime / LerpTime;
			Player.transform.position = Lerp(Player.transform.position, DestPlayerPosition, per);
			Player.transform.rotation = Quaternion.Euler(Lerp(Player.transform.rotation.eulerAngles, DestPlayerEuler, per));
		} else {
			int dx = Input.GetKey(KeyCode.RightArrow) ? 1 :
					 Input.GetKey(KeyCode.LeftArrow) ? -1 : 0;
			if (dx == 0) return;
			if (dx == -1 && px == 0) return;
			if (dx == 1 && px == StageMax - 1) return;
			LerpingTime = 0f;
			px += dx;
			SetLighting();
			DestPlayerEuler = new Vector3(0, dx == 1 ? 0 : 180, 0);
			DestPlayerPosition = GetPositionFromPuzzlePosition(px, py);
			CurrentStageIndex = px;
		}	
	}

	protected virtual void Update () {
		MovePlayer();
		GameObject.Find("StageIndex").GetComponent<Text>().text = "Stage "+CurrentStageIndex;
		DestPlayerPosition.y = flWater.GetLocalFloating();
		gocamera.transform.position = Player.transform.position + new Vector3(1.6f,1.4f,0.5f);
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			Application.LoadLevel("Temprate");
		}
	}
}
