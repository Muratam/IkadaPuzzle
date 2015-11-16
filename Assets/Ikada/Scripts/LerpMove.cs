using UnityEngine;
using System.Collections;

public class LerpMove : MonoBehaviour {

	// Use this for initialization
	
	public Vector3 LocalPosition {
		set {SetStatus(value,DestLocalRotation);}		
	}
	public Quaternion LocalRotation {
		set {SetStatus(DestLocalPosition,value);}
	}
	public Vector3 Position {
		set { SetStatus(value,DestLocalRotation); }
	}
	public Quaternion Rotation {
		set { SetStatus(DestLocalPosition, value); }
	}
	public void SetParent(Transform parent) {
		transform.SetParent(parent);
	}

	void SetStatus(Vector3 _DestLocalPosition,Quaternion _DestLocalRotation) {
		LerpingTime = 0f;
		LerpFixedOnce = false;
		DestLocalPosition = _DestLocalPosition;
		DestLocalRotation = _DestLocalRotation;
	}

	Vector3 DestLocalPosition;
	Quaternion DestLocalRotation;
	const float LerpTime = 3f;
	float LerpingTime = LerpTime;
	bool LerpFixedOnce = true;
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

	void Start() {
		DestLocalPosition = transform.localPosition;
		DestLocalRotation = transform.localRotation;
	}
	void Update() {
		LerpingTime += Time.deltaTime;
		if (LerpingTime < LerpTime) {
			float per = LerpingTime / LerpTime;
			transform.localPosition = Lerp(transform.localPosition, DestLocalPosition, per);
			transform.localRotation = Lerp(transform.localRotation, DestLocalRotation, per);
		}else if(!LerpFixedOnce){
			LerpFixedOnce = true;
			transform.localPosition = DestLocalPosition;
			transform.localRotation =  DestLocalRotation;
		}
	}
}
