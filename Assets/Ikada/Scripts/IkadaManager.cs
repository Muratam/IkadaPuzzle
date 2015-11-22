using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public struct Vec2 { public int x, y; public Vec2(int _x, int _y) { x = _x; y = _y; } };
public struct Pair<T> { public T x, y; public Pair(T _x, T _y) { x = _x; y = _y; } };
	

public class IkadaManager : TileManager {

	[SerializeField] GameObject Wave;
	[SerializeField] GameObject Barrier;
	[SerializeField] TransitionUI UIClear;
	[SerializeField] TransitionUI UIGo;

	protected GameObject gocamera;
	protected FloatingWater flWater;
	protected LerpMove lmPlayer;
	protected static string DATA_MOVEDTIME(int index) { return "MovedTime" + index; }
	protected static string DATA_CURRENTINDEX { get { return "CurrentIndex"; } }
	public enum PlayMode {Story,Edit,Online }
	public PlayMode CurrentMode { 
		get {
			if (EditStageData.Current != null) return PlayMode.Edit;
			else if (OnlineStageManager.OnlineStages_Name_Data != null) return PlayMode.Online;
			else return PlayMode.Story;
		}
	}

	int movedTime = 0;
	int MovedTime {
		get {return movedTime; }
		set {
			movedTime = value; 
			GameObject.Find("MovedTime/Text").GetComponent<Text>().text = "" + value;
		}
	}
	bool isPlayerView = false;
	static Pair<Vector3>[] CameraPosAng = new Pair<Vector3>[] {
		new Pair<Vector3>(new Vector3(-0.2f,9.5f,-0.3f),new Vector3(90,270,0)),
		new Pair<Vector3>(new Vector3(5, 8, 0), new Vector3(60, 270, 0)),
		new Pair<Vector3>(new Vector3(0, 4, -4f), new Vector3(30, 0, 0)),//UnityChan
	};
	public static Pair<Vector3> PlayerCameraPosAng { get { return CameraPosAng[2]; } }

	protected void SetCamera(int n) {
		var move = gocamera.GetComponent<LerpMove>();
		if (n == 2) {
			move.SetParent(Player.transform);
			isPlayerView = true;
			move.LocalRotation = Quaternion.Euler(CameraPosAng[n].y);
		} else {
			move.SetParent(null);
			isPlayerView = false;
			move.LocalRotation = Quaternion.Euler(CameraPosAng[n].y);
		}
		move.LocalPosition = CameraPosAng[n].x;
	}
	protected void SetLighting() {
		float intensity = 1f -0.7f * (float)CurrentStageIndex / StageMax ;
		RenderSettings.skybox.SetFloat("_Exposure", intensity);
		GameObject.Find("Directional light").GetComponent<Light>().intensity = intensity;
	}

	static readonly Vector3 DisFloatDiffVec = new Vector3(0,-0.5f,0);
	protected void AfloatTiles(Queue<Vec2> _pos,GameObject _go,float _LerpTime= 0.15f,Vector3 DiffVec = new Vector3()) {
		if (_pos.Count == 0) return;
		var pos = _pos.Dequeue();
		var lm = (Instantiate(_go, GetPositionFromPuzzlePosition(pos.x, pos.y) + DisFloatDiffVec + DiffVec, new Quaternion()) as GameObject).AddComponent<LerpMove>();
		lm.transform.SetParent(Stage.transform);
		lm.Position = GetPositionFromPuzzlePosition(pos.x, pos.y) + DiffVec;
		lm.LerpTime = _LerpTime;
		lm.AddSelfAction((_lm) => { 
			AfloatTiles(_pos, _go,_LerpTime,DiffVec);
		});
		AfloatedTiles.Enqueue(lm);
	}
	protected Queue<LerpMove> AfloatedTiles = new Queue<LerpMove>();
	protected void DisfloatTiles() {
		foreach (var lm in AfloatedTiles) {
			lm.LocalPosition = lm.transform.localPosition + DisFloatDiffVec;
			lm.ClearSelfActions();
			lm.AddSelfAction((_lm)=> Destroy(_lm.gameObject));
		}
		AfloatedTiles.Clear();
	}


	string BackSceneText() {
		return CurrentMode == PlayMode.Story ? "ステージ選択へ" :
			CurrentMode == PlayMode.Edit ? "ステージエディターへ"
			: "ステージ選択へ";
	}

	protected virtual void Awake () {
		UIGo = GameObject.Find("Canvas/Go").GetComponent<TransitionUI>();
		UIClear = GameObject.Find("Canvas/Clear").GetComponent<TransitionUI>();
		UIGo.gameObject.SetActive(false);
		UIClear.gameObject.SetActive(false);

		Player = GameObject.Find("Player");
		lmPlayer = Player.GetComponent<LerpMove>();
		gocamera = GameObject.Find("Main Camera");
		flWater = GameObject.Find("Water").GetComponent<FloatingWater>();
		var bBackScene = GameObject.Find("BackScene").GetComponent<Button>();
		bBackScene.onClick.AddListener(() => {
			if (CurrentMode == PlayMode.Story) Application.LoadLevel("StageSelect");
			else if (CurrentMode == PlayMode.Edit) Application.LoadLevel("StageEdit");
			else if (CurrentMode == PlayMode.Online) Application.LoadLevel("OnlineStage");
		});
		GameObject.Find("Canvas/Reset").GetComponent<Button>().onClick.AddListener(() => { InitTiles(BaseStageName);});
		bBackScene.transform.FindChild("Text").GetComponent<Text>().text = BackSceneText();
		CameraPosAng.Foreach((i, cam) => {
			var button = GameObject.Find("Camera" + i).GetComponent<Button>();
			button.onClick.AddListener(() => {
				StaticSaveData.Set("CameraPos", int.Parse(button.name.Replace("Camera", "")));
				SetCamera(int.Parse(button.name.Replace("Camera", ""))); 
			});
		});
		
		InitTiles(BaseStageName);
		Player.transform.SetParent(flWater.transform);
		Stage.transform.SetParent(flWater.transform);
	}
	protected virtual void Start() { }
	protected override int tileSize { get { return 1; } } //120
	
	protected override Vector3 GetPositionFromPuzzlePosition(int x, int y) {
		return tileSize * new Vector3(
			-1 * ( y - h / 2 + 0.5f),
			flWater.GetLocalFloating(),
			x - w / 2 + 0.5f);
	}

	protected override void SwapTileMaps(int x1, int y1, int x2, int y2) {
		var tmp = Tiles[x1, y1];
		Tiles[x1, y1] = Tiles[x2, y2];
		Tiles[x2, y2] = tmp;
		Action<int, int> MoveTileProcess = (x, y) => {
			var goWave = Instantiate(Wave, Tiles[x, y].transform.position + new Vector3(0, 0.5f, 0), Quaternion.Euler(270, 0, 0)) as GameObject;
			goWave.transform.SetParent(Tiles[x, y].transform);
			var lm = Tiles[x, y].GetComponent<LerpMove>() ? Tiles[x, y].GetComponent<LerpMove>() : Tiles[x, y].gameObject.AddComponent<LerpMove>();
			lm.LerpTime = lmPlayer.LerpTime;
			lm.Position = GetPositionFromPuzzlePosition(x, y);
			lm.DestroyWhenFinished = true;
		};
		MoveTileProcess(x1,y1);
		MoveTileProcess(x2,y2);
	}


	protected virtual void InitTiles(string FileName) {
		if (CurrentMode == PlayMode.Story) {
			if (FileName != "") Read(FileName);
		} else if (CurrentMode == PlayMode.Edit) {
			InitialStrTileMap = EditStageData.Current.MakeUpStageMap();
		} else {
			InitialStrTileMap = ConvertStageMap (OnlineStageManager.OnlineStage.y);
		}
		foreach (var t in Tiles) if (t != null) Destroy(t.gameObject);
		REP (w,x=> {
			REP(h, y => {
				string str = InitialStrTileMap[x, y];
				TileObject tileobj;
				switch (str) {
					case "..":
						tileobj = Instantiate(WaterTile, GetPositionFromPuzzlePosition(x, y), new Quaternion()) as TileObject;
						tileobj.tile = new Tile(Tile.TileType.Water);
						break;
					case "##":
						tileobj = Instantiate(WallTile, GetPositionFromPuzzlePosition(x, y), new Quaternion()) as TileObject;
						tileobj.tile = new Tile(Tile.TileType.Wall); break;
					case "[]":
						tileobj = Instantiate(FloorTile, GetPositionFromPuzzlePosition(x, y), new Quaternion()) as TileObject;
						tileobj.tile = new Tile(Tile.TileType.Normal); break;
					default:
						tileobj = Instantiate(IkadalTile, GetPositionFromPuzzlePosition(x, y), new Quaternion()) as TileObject;
						var In = AlphabetLib.FromAlphabetToBool5(str[0]);
						Across inacross = new Across(In[0], In[1], In[2], In[3], In[4]);
						var Ex = AlphabetLib.FromAlphabetToBool5(str[1]);
						Across exacross = new Across(Ex[0], Ex[1], Ex[2], Ex[3], Ex[4]);
						tileobj.tile = new Tile(Tile.TileType.Ikada, inacross, exacross);
						break;
				}
				if (tileobj != null) {
					tileobj.transform.SetParent(Stage.transform);
					tileobj.SetInitgoIkadaState();
					Tiles[x, y] = tileobj;
				}
			});
		});
		px = w - 1; py = h - 1;
		px += 8;

		lmPlayer.Position = Player.transform.position = GetPositionFromPuzzlePosition(px, py);
		lmPlayer.Rotate = new Vector3(0,90 * prePlayerDirection.GetDiffOrderByLBRT(new Across(false, true, false, false, false)), 0);
		prePlayerDirection = new Across(false, true, false, false, false);
		isComingPlayer = true;
		UIGo.gameObject.SetActive(true);
		UIGo.ReStart();
		UIGo.GetComponent<AudioSource>().Play();
		Queue<Vec2> pos = new Queue<Vec2>();
		REP(8, i => pos.Enqueue(new Vec2(px-i, py)));
		AfloatTiles(pos, FloorTile.gameObject,0.3f);
		StaticSaveData.Set(DATA_CURRENTINDEX, CurrentStageIndex);
		GameObject.Find("StageIndex/Text").GetComponent<Text>().text 
			= CurrentMode == PlayMode.Story ? "Stage "+ CurrentStageIndex 
			: CurrentMode == PlayMode.Edit ?  EditStageData.Current.Name
			: OnlineStageManager.OnlineStage.x;
		SetCamera(2);
		SetLighting();
		MovedTime = 0;
		WaitTime = Time.time;
		
	}

	Across prePlayerDirection = new Across(false,true,false,false,false);
	protected virtual void MovePlayer() {
		if(Input.GetKeyDown(KeyCode.X)){
			InitTiles(BaseStageName);	
			return;
		} else if (Input.GetKeyDown(KeyCode.Z)) {
			if (isPlayerView) SetCamera(0);
			else SetCamera(2);	
			return;
		}
		if (lmPlayer.LerpFinished) {
			int dx = Input.GetKey(KeyCode.RightArrow) ? 1 :
					 Input.GetKey(KeyCode.LeftArrow) ? -1 : 0;
			int dy = Input.GetKey(KeyCode.UpArrow) ? 1 :
					 Input.GetKey(KeyCode.DownArrow) ? -1 : 0;
			if (dx == 0 && dy == 0) return;
			int inx = dx, iny = dy;
			if (isPlayerView) {
				if (prePlayerDirection.Horizontal != 0) {
					int ddy = dy;
					dy = -prePlayerDirection.Horizontal * dx;
					dx = prePlayerDirection.Horizontal * ddy;
				} else if (prePlayerDirection.B) {
					dx *= -1; dy *= -1;
				}
			}

			//0 R 90 B 180 L 270 T 
			Across playerDirection = new Across(dx == 1, dx == -1, dy == 1, dy == -1, false);
			if (isPlayerView && !(playerDirection & prePlayerDirection).HaveDirection) {
				prePlayerDirection = playerDirection;
				int Angle = inx == 1 ? 90 : inx == - 1 ? -90 :180 ; 
				lmPlayer.Rotate = new Vector3(0, Angle, 0);
				return;
			}else{
				int Angle = prePlayerDirection.GetDiffOrderByLBRT(playerDirection) * 90;
				if (Angle == 270) Angle = -90; else if (Angle == -270) Angle = 90;
				lmPlayer.Rotate = new Vector3(0, Angle, 0);
			}
			prePlayerDirection = playerDirection;
			if (MoveCharacters(dx, dy) != MoveType.DidntMove) MovedTime++;
			else { Instantiate(Barrier,Player.transform.position + new Vector3(-playerDirection.Vertical,0,playerDirection.Horizontal) * tileSize /2 + new Vector3(0,0.5f,0),Player.transform.rotation); }
			lmPlayer.Position = GetPositionFromPuzzlePosition(px, py)
				+ tileSize * 0.36f * new Vector3(-1 * (PlayerTilePos.T ? 1 : PlayerTilePos.B ? -1 : 0),0,
				PlayerTilePos.R ? 1 : PlayerTilePos.L ? -1 : 0);
		}
	}
	bool isComingPlayer = true;
	bool isGoaled = false;
	float WaitTime;
	void ComePlayer() {
		if (Time.time - WaitTime < 1f) return;
		if (lmPlayer.LerpFinished) {
			px--;
			lmPlayer.Position = GetPositionFromPuzzlePosition(px, py);
			if (px == w - 1) {
				isComingPlayer = false;
				GameObject.Find("Reset/Text").GetComponent<Text>().text = "リセット\n(Xキー)";
				GameObject.Find("BackScene/Text").GetComponent<Text>().text = BackSceneText();
				if (UIGo.gameObject.activeSelf) UIGo.Vanish();
				int CameraPos;
				StaticSaveData.Get("CameraPos",out CameraPos);
				SetCamera(CameraPos);
				DisfloatTiles();
			}
		}
	}
	void GoaledPlayer() {
		if (Time.time - WaitTime < 1f) return;
		if (lmPlayer.LerpFinished) {
			px--;
			lmPlayer.Position = GetPositionFromPuzzlePosition(px, py);
			if (px == -8) {
				DisfloatTiles();
				if (UIClear.gameObject.activeSelf) UIClear.Vanish();
				isGoaled = false;
				if (CurrentMode == PlayMode.Story) {
					CurrentStageIndex++;
					InitTiles(BaseStageName);
				} else if (CurrentMode == PlayMode.Edit){
					Application.LoadLevel("StageEdit");
				} else if( CurrentMode == PlayMode.Online){
					Application.LoadLevel("OnlineStage");	
				}
			}
		}	
	}
	
	protected virtual void Update() {
		if (isComingPlayer) {
			GameObject.Find("Reset/Text").GetComponent<Text>().text = "リセット";
			GameObject.Find("BackScene/Text").GetComponent<Text>().text = BackSceneText() + "\n(Xキー)";
			if (Input.GetKeyDown(KeyCode.X))
				GameObject.Find("BackScene").GetComponent<Button>().onClick.Invoke();
			else ComePlayer();
		}else if(isGoaled){
			GoaledPlayer();
		} else {
			if (lmPlayer.LerpFinished && px == 0 && Tiles[px, py].tile.tileType == Tile.TileType.Normal) {
				WaitTime = Time.time;
				isGoaled = true;
				UIClear.gameObject.SetActive(true);
				UIClear.ReStart();
				UIClear.GetComponent<AudioSource>().Play();
				lmPlayer.Rotate = new Vector3(0, 90 * prePlayerDirection.GetDiffOrderByLBRT(new Across(false, true, false, false, false)), 0);
				prePlayerDirection = new Across(false, true, false, false, false);
				Queue<Vec2> pos = new Queue<Vec2>();
				REP(20, i => pos.Enqueue(new Vec2(px - i, py)));
				AfloatTiles(pos, FloorTile.gameObject, 0.3f);
				SetCamera(2);
				if (CurrentMode == PlayMode.Story) {
					int oldTime; StaticSaveData.Get(DATA_MOVEDTIME(CurrentStageIndex), out oldTime);
					if (oldTime == 0 || MovedTime < oldTime)
						StaticSaveData.Set(DATA_MOVEDTIME(CurrentStageIndex), MovedTime);
				}
			} else MovePlayer();
		}
	}
}
