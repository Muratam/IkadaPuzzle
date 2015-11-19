using UnityEngine;
using System.Collections;

public class TransitionUI : MonoBehaviour {

	public float InitSize = 0.2f;
	public float ReSizingTime = 0.4f;
	public bool EnableAutoVanish = false;
	public bool Vanished { get; private set; }
	public TransitionUI NextUI = null;
	public bool WhenAppearedDestroyThisScript = false;
	public bool WhenVanishedDestroyGameObject = false;
	public SPEEDTYPE SpeedType = SPEEDTYPE.UseEditorValue;
	public bool AllowLerpMoveAroundAwakePosition = false;
	public Vector2 LerpOffset = new Vector2(10,0);
	public enum SPEEDTYPE {
		UseEditorValue, Lightning, SuperQuickly, Quickly, Speedy, Normal, Slowly
	}
	void ChangeSpeedBySPEEDTYPE() {
		switch (SpeedType) {
			case SPEEDTYPE.UseEditorValue: break;
			case SPEEDTYPE.Lightning:
				ReSizingTime = 0.08f; break;
			case SPEEDTYPE.SuperQuickly:
				ReSizingTime = 0.13f; break;
			case SPEEDTYPE.Quickly:
				ReSizingTime = 0.2f; break;
			case SPEEDTYPE.Speedy:
				ReSizingTime = 0.3f; break;
			case SPEEDTYPE.Normal:
				ReSizingTime = 0.4f; break;
			case SPEEDTYPE.Slowly:
				ReSizingTime = 0.72f; break;
		}
	}

	private bool isDisabledNotByMySelf = false;
	public bool IsDisabledNotByMySelf {
		get { return isDisabledNotByMySelf; }
		set { isDisabledNotByMySelf = value; }
	}

	private float StartTime = 0f;
	public Vector3 AwakePosition { get;  set; }
	private Vector3 VanishPosition { get { return AwakePosition - (Vector3)LerpOffset; } }
	private bool isAppearing = true;
	public bool isVanishing {get;private set;}
	public enum CurveType { Linear, Square, Pop }
	public CurveType curvetype = CurveType.Linear;
	Vector3 Lerp(Vector3 Base, Vector3 Dest, float Per) {
		return Base * (1 - Per) + Dest * Per;
	}

	//SetActive(true);の時に呼ばれる！
	void OnEnable() {
		if (IsDisabledNotByMySelf) {
			Start();
		}
	}
	void OnDisable() {
		if (!WhenAppearedDestroyThisScript) this.transform.localScale = new Vector3(1, 1, 1) * InitSize;
		IsDisabledNotByMySelf = true;
	}
	
	void Awake() {
		Vanished = false;
		ChangeSpeedBySPEEDTYPE();
		AwakePosition = transform.localPosition;
		if(AllowLerpMoveAroundAwakePosition)transform.localPosition += (Vector3)LerpOffset;
		this.transform.localScale = new Vector3(1, 1, 1) * InitSize;
		StartTime = Time.time;
		isAppearing = true;
	}
	public void Start() {
		Vanished = false;
		if (NextUI != null && NextUI.gameObject.activeSelf) {
			NextUI.Start();
			NextUI.gameObject.SetActive(false);
		}
		this.transform.localScale = new Vector3(1, 1, 1) * InitSize;
		StartTime = Time.time;
		isAppearing = true;
	}

	public void Vanish() {
		Vanished = false;
		isVanishing = true;
		isAppearing = false;
		StartTime = Time.time;
	}
	public void ReStart() {
		if (AllowLerpMoveAroundAwakePosition) 
			transform.localPosition = AwakePosition + (Vector3)LerpOffset;
		isVanishing = false;
		Start();
	}

	float FSize(float LerpingTime,float Diff) {
		float Size = 1f;
		if (LerpingTime > 0) {
			float DiffSize = 1 - InitSize;
			float RDiff = 1 - Diff;//RDiff in [1 → 0] as Linear
			float F = Diff;
			switch (curvetype) {
				case CurveType.Linear:
					F = Diff; break;
				case CurveType.Square:
					F = -Diff * (Diff - 2); break;
				case CurveType.Pop:
					F = (-25f / 16f) * (Diff * Diff) + 2.5f * Diff; break;
			}
			Size = InitSize + DiffSize * F;
		}
		return Size;
	}

	void Appearing(float LerpingTime) {
		this.transform.localScale = new Vector3(1, 1, 1) * FSize(LerpingTime, LerpingTime / ReSizingTime);
		if (AllowLerpMoveAroundAwakePosition) {
			float per = LerpingTime / ReSizingTime;
			transform.localPosition = Lerp(transform.localPosition, AwakePosition, per);
		}	
	}
	void Vanishing(float LerpingTime) {
		this.transform.localScale = new Vector3(1, 1, 1) * FSize(LerpingTime,1- LerpingTime / ReSizingTime);
		if (AllowLerpMoveAroundAwakePosition) {
			float per = LerpingTime / ReSizingTime;
			transform.localPosition = Lerp(transform.localPosition,VanishPosition , per);
		}
	}

	void Update() {
		float LerpingTime = Time.time - StartTime;
		if (isAppearing) {
			if (LerpingTime < ReSizingTime) {
				Appearing(LerpingTime);
			} else {
				this.transform.localScale = new Vector3(1, 1, 1);
				if (AllowLerpMoveAroundAwakePosition) { transform.localPosition = AwakePosition; }
				if (NextUI != null) NextUI.gameObject.SetActive(true);
				isAppearing = false;
				if (WhenAppearedDestroyThisScript) Destroy(this);
			}
		}else if(isVanishing){
			if (LerpingTime < ReSizingTime) {
				Vanishing(LerpingTime);
			} else {
				this.transform.localScale = new Vector3(1, 1, 1) * InitSize;
				if (AllowLerpMoveAroundAwakePosition) { transform.localPosition = VanishPosition; }
				isVanishing = false;
				Vanished = true;
				if (WhenVanishedDestroyGameObject) Destroy(this.gameObject);
			}
		}
	}

}
