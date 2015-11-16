using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public struct Vec2 { public int x, y; public Vec2(int _x, int _y) { x = _x; y = _y; } };
public struct Pair<T> { public T x, y; public Pair(T _x, T _y) { x = _x; y = _y; } };
	

public class IkadaManager : TileManager {

	[SerializeField] GameObject Wave;
	[SerializeField] GameObject FrameTile;
	protected GameObject gocamera;
	protected FloatingWater flWater;
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
		} else {
			move.SetParent(null);
			isPlayerView = false;
		}
		move.Position = CameraPosAng[n].x;
		move.Rotation = Quaternion.Euler(CameraPosAng[n].y);
	}
	protected void SetLighting() {
		float intensity = 1f - (float)CurrentStageIndex / StageMax + 0.1f;
		RenderSettings.skybox.SetFloat("_Exposure", intensity);
		GameObject.Find("Directional light").GetComponent<Light>().intensity = intensity;
	}


	protected virtual void Awake () {
		Player = GameObject.Find("Player");
		gocamera = GameObject.Find("Main Camera");
		flWater = GameObject.Find("Water").GetComponent<FloatingWater>();
		GameObject.Find("ToEditor").GetComponent<Button>().onClick.AddListener(() => { Application.LoadLevel("StageEdit"); });
		CameraPosAng.Foreach((i, cam) => {
			var button = GameObject.Find("Camera" + i).GetComponent<Button>();
			button.onClick.AddListener(() => { SetCamera(int.Parse(button.name.Replace("Camera", ""))); });
		});		
		InitTiles(BaseStageName);
		Player.transform.SetParent(flWater.transform);
		Stage.transform.SetParent(flWater.transform);
		DestPlayerPosition = Player.transform.position;
		DestPlayerEuler =  Player.transform.rotation.eulerAngles;		
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
		var goWave1 = Instantiate(Wave, Tiles[x1, y1].transform.position + new Vector3(0, 0.5f,0), Quaternion.Euler(270,0,0)) as GameObject;
		var goWave2 = Instantiate(Wave, Tiles[x2, y2].transform.position + new Vector3(0, 0.5f, 0), Quaternion.Euler(270, 0, 0)) as GameObject;
		goWave1.transform.SetParent(Tiles[x1,y1].transform);
		goWave2.transform.SetParent(Tiles[x2, y2].transform);
		MovedTileList.Add(new Vec2(x1, y1));
		MovedTileList.Add(new Vec2(x2, y2));
	}
	List<Vec2> MovedTileList = new List<Vec2>();


	bool FrameMade = false;
	protected virtual void InitTiles(string FileName) {
		if (FileName != "") Read(FileName);
		foreach (var t in Tiles) if (t != null) Destroy(t.gameObject);
		const int FrameSize = 1;
		if (!FrameMade) {
			FrameMade = true;
			for (int x = -FrameSize; x < w + FrameSize; x++) {
				for (int y = -FrameSize; y < h + FrameSize; y++) {
					if (0 <= x && x < w && 0 <= y && y < h) continue;
					Instantiate(FrameTile, GetPositionFromPuzzlePosition(x, y), new Quaternion());
				}
			}
		}

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
		SetLighting();
		DestPlayerPosition = Player.transform.position = GetPositionFromPuzzlePosition(px, py);
	}
	protected Vector3 Lerp(Vector3 Base, Vector3 Dest, float Per) {
		return Base * (1-Per) + Dest *  Per;
	}

	protected const float LerpTime = 0.3f;
	protected float LerpingTime = LerpTime;
	protected Vector3 DestPlayerPosition ;
	protected Vector3 DestPlayerEuler;
	Across prePlayerDirection = new Across(false,false,false,true,false);
	protected virtual void MovePlayer() {
		LerpingTime += Time.deltaTime;
		if (LerpingTime < LerpTime) {
			float per = LerpingTime / LerpTime;
			Player.transform.position = Lerp(Player.transform.position, DestPlayerPosition, per);
			Player.transform.rotation = Quaternion.Euler(Lerp(Player.transform.rotation.eulerAngles, DestPlayerEuler, per));
			foreach(var v in MovedTileList)
				Tiles[v.x, v.y].transform.position = Lerp(Tiles[v.x,v.y].transform.position,GetPositionFromPuzzlePosition(v.x,v.y),per);
		} else {
			int dx = Input.GetKey(KeyCode.RightArrow) ? 1 :
					 Input.GetKey(KeyCode.LeftArrow) ? -1 : 0;
			int dy = Input.GetKey(KeyCode.UpArrow) ? 1 :
					 Input.GetKey(KeyCode.DownArrow) ? -1 : 0;
			if (dx == 0 && dy == 0) return;
			LerpingTime = 0f;
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
			int Angle = playerDirection.R ? 0 : playerDirection.B ? 90 : playerDirection.L ? 180 : 270;
			if (Player.transform.rotation.eulerAngles.y > 350) Player.transform.rotation = Quaternion.Euler(0, 0, 0);
			if (prePlayerDirection.R && playerDirection.T) Player.transform.rotation = Quaternion.Euler(0,358,0);
			else if (prePlayerDirection.T && playerDirection.R) Angle = 358;
			DestPlayerEuler = new Vector3(0, Angle, 0);
			if (isPlayerView && !(playerDirection & prePlayerDirection).HaveDirection) {
				prePlayerDirection = playerDirection;
				return;
			}
			prePlayerDirection = playerDirection;
			
			MovedTileList.Clear();
			MoveCharacters(dx,dy);
			DestPlayerPosition =  GetPositionFromPuzzlePosition(px, py);
			DestPlayerPosition += tileSize * 0.36f * new Vector3(
				-1 * (PlayerTilePos.T ? 1 : PlayerTilePos.B ? -1 : 0),0,
				PlayerTilePos.R ? 1 : PlayerTilePos.L ? -1 : 0);
		}
	}

	protected virtual void Update() {
		MovePlayer();
		DestPlayerPosition.y = flWater.GetLocalFloating();
		if (Input.GetKeyDown(KeyCode.R)) InitTiles(BaseStageName);
		else if (Input.GetKeyDown(KeyCode.N) ||
			px == 0 && Tiles[px,py].tile.tileType == Tile.TileType.Normal) {
			CurrentStageIndex++; InitTiles(BaseStageName);
		}
		else if (Input.GetKeyDown(KeyCode.B)) {
			CurrentStageIndex--; InitTiles(BaseStageName);
		} else if (Input.GetKeyDown(KeyCode.Escape)) {
			Application.LoadLevel("StageSelect");
		}
	}
}
