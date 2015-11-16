using UnityEngine;
using System.Collections;

public class AutoDestroy : MonoBehaviour {
	public float LifeTime = 0.3f;
	float time = 0;
	
	// Update is called once per frame
	void Update () {
		time += Time.deltaTime;
		if (time > LifeTime) Destroy(this.gameObject);
	}
}
