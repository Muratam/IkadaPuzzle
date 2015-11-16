using UnityEngine;
using System.Collections;

public class ProcedualTexture : MonoBehaviour {

	public int textureSize = 512;
	public Texture2D generatedTexture;
	Material currentMaterial;
	Vector2 centerPosition;

	Texture2D GenerateParabola(){
		var proceduralTexture = new Texture2D (textureSize,textureSize);
		var centerPixelPosition = centerPosition * textureSize;
		for (int x = 0; x < textureSize; x++) {
			for (int y = 0; y < textureSize; y++) {
				var currentPosition = new Vector2(x,y);
				float pixelDistance = Vector2.Distance(currentPosition,centerPixelPosition)/(textureSize * 0.5f);
				pixelDistance = Mathf.Abs (1-Mathf.Clamp(pixelDistance,0f,1f));
				Color pixelColor = new Color(pixelDistance,pixelDistance,pixelDistance,1f);
				proceduralTexture.SetPixel(x,y,pixelColor);
			}
		}
		proceduralTexture.Apply ();
		return proceduralTexture;
	}

	// Use this for initialization
	void Start () {
		if (!currentMaterial) {
			currentMaterial = transform.GetComponent<Renderer>().sharedMaterial;
		}
		if (!currentMaterial) {Debug.Log("Exist");return;}
		centerPosition = new Vector2 (0.5f, 0.5f);
		generatedTexture = GenerateParabola ();

		currentMaterial.SetTexture ("_MainTex",generatedTexture);
	}


}
