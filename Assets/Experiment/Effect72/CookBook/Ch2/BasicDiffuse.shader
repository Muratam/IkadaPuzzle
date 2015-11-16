Shader "Custom/BasicDiffuse" {
	//Memo 
	//- Properties
	//  - http://docs.unity3d.com/Manual/SL-Properties.html
	//  - Range (min,max) : Color (f,f,f,f) : 2D (texture)
	//  - Rect (texture (not ^2)) : Cube (Cubemap)
	//  - Float : Vector(f,f,f,f) (for directions or colors)
	//- Built-in function
	//  - pow
	Properties {
		//_MainTex ("Base (RGB)", 2D) = "white" {}
		_EmissiveColor("Emissive Color", Color) = (1,1,1,1)
		_AmbientColor ("Ambient Color", Color) = (1,1,1,1)
		_MySliderValue ("This is a Slider",Range(0,10)) = 2.5
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		float4 _EmissiveColor;
		float4 _AmbientColor;
		float _MySliderValue;
    	#pragma surface surf Lambert
		//sampler2D _MainTex;
		struct Input {
			float2 uv_MainTex;
		};
		void surf (Input IN, inout SurfaceOutput o) {
			//half4 c = tex2D (_MainTex, IN.uv_MainTex);
			float4 c;
			c = pow ((_EmissiveColor + _AmbientColor), _MySliderValue);
			
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
