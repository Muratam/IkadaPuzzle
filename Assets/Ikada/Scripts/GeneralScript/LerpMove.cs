using UnityEngine;
using System;
using System.Collections.Generic;
public class LerpMove : MonoBehaviour {

	// Use this for initialization
	
	public Vector3 LocalPosition {
		set {SetStatus(value,DestLocalRotation);}		
	}
	public Quaternion LocalRotation {
		set {SetStatus(DestLocalPosition,value);}
	}
	public Vector3 Position {
		set { SetStatus(transform.parent?transform.parent.InverseTransformPoint(value) :value, DestLocalRotation); }
	}
	public Vector3 Rotate {
		set { SetStatus(DestLocalPosition,this.transform.localRotation * Quaternion.Euler(value));}
	}

	public void SetParent(Transform parent) {
		transform.SetParent(parent);
	}
	public void AddAction(Action action) {
		actions.Enqueue(action);
	}
	public void AddSelfAction(Action<LerpMove> action) {
		selfActions.Enqueue(action);
	}
	public void ClearSelfActions() {
		selfActions.Clear();
	}
	public void ClearActions() {
		actions.Clear();
	}
	Queue<Action> actions = new Queue<Action>();
	Queue<Action<LerpMove>> selfActions = new Queue<Action<LerpMove>>();

	void SetStatus(Vector3 _DestLocalPosition,Quaternion _DestLocalRotation) {
		LerpingTime = 0f;
		LerpFinished = false;
		DestLocalPosition = _DestLocalPosition;
		DestLocalRotation = _DestLocalRotation;
	}

	Vector3 DestLocalPosition;
	Quaternion DestLocalRotation;
	public float LerpTime = 3f;
	float LerpingTime = 0;
	public bool DestroyWhenFinished = false;
	public bool LerpFinished { get; private set ; }
	Vector3 Lerp(Vector3 Base, Vector3 Dest, float Per) {
		return Base * (1 - Per) + Dest * Per;
	}
	Quaternion Lerp(Quaternion Base, Quaternion Dest, float Per) {
		return new Quaternion( 
			Base.x * (1 - Per) + Dest.x * Per,
			Base.y * (1 - Per) + Dest.y * Per,
			Base.z * (1 - Per) + Dest.z * Per,
			Base.w * (1 - Per) + Dest.w * Per);
	}

	void Awake() { SetInit(); }
	public void SetInit(bool _lerpFinished = false) {
		LerpingTime = LerpTime;
		LerpFinished = _lerpFinished;
		DestLocalPosition = transform.localPosition;
		DestLocalRotation = transform.localRotation;
	}
	void Update() {
		LerpingTime += Time.deltaTime;
		if (LerpingTime < LerpTime) {
			float per = LerpingTime / LerpTime;
			transform.localPosition = Lerp(transform.localPosition, DestLocalPosition, per);
			transform.localRotation = Lerp(transform.localRotation, DestLocalRotation, per);
		}else if(!LerpFinished){
			LerpFinished = true;
			transform.localPosition = DestLocalPosition;
			transform.localRotation =  DestLocalRotation;
			foreach (var ac in actions) { ac(); }
			actions.Clear();
			foreach (var ac in selfActions) { ac(this); }
			selfActions.Clear();
			if (DestroyWhenFinished) Destroy(this);
		}
	}
}
