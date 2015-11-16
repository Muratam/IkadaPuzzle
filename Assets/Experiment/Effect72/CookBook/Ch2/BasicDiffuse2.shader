Shader "Custom/BasicDiffuse2" {
	//Memo 
	//- Properties
	//  - http://docs.unity3d.com/Manual/SL-Properties.html
	//  - Range (min,max) : Color (f,f,f,f) : 2D (texture)
	//  - Rect (texture (not ^2)) : Cube (Cubemap)
	//  - Float : Vector(f,f,f,f) (for directions or colors)
	//- Built-in function
	//  - pow
	
	//EmissiveColor : ? 発行係数(ライトに影響されない光)
	//AmbientColor ： ? 環境光(光の直接当たらない部分の色？)http://contest.japias.jp/tqj2000/30317/head/ren/kan/kan.htm
	//Albedo : 　基本の色 >入射光に対する反射光の比(反射能)
	//Lambert : ランバート拡散反射
	//Half Lambert : http://www.project-asura.com/program/d3d11/d3d11_005.html
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
		inline float4 LightingBasicDiffuse(SurfaceOutput s,fixed3 lightDir, fixed atten){
			
			float difLight = max(0,dot(s.Normal,lightDir));
			float hLambert = difLight * 0.5 + 0.5;
			float4 col;
			col.rgb = s.Albedo * _LightColor0.rgb * (hLambert * atten * 2);			
			//col.rgb = s.Albedo * _LightColor0.rgb * (difLight * atten * 2);
			col.a = s.Alpha;
			return col;
		}
		float4 _EmissiveColor;
		float4 _AmbientColor;
		float _MySliderValue;
		#pragma surface surf BasicDiffuse
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
