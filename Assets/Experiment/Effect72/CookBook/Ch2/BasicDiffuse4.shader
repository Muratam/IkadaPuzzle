Shader "Custom/BasicDiffuse4" {
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
		_MainTint("Diffuse Tint",Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_ScrollXSpeed ("X Scroll Speed",Range(0,10)) = 2
		_ScrollYSpeed ("Y Scroll Speed",Range(0,10)) = 2
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		fixed4 _MainTint;
		sampler2D _MainTex;
		fixed _ScrollXSpeed;
		fixed _ScrollYSpeed;
		inline float4 LightingBasicDiffuse(SurfaceOutput s,fixed3 lightDir, half3 viewDir, fixed atten){
			
			float difLight = max(0,dot(s.Normal,lightDir));
			float rimLight = dot (s.Normal, viewDir);
			float hLambert = difLight * 0.5 + 0.5;
			float4 col;
			col.rgb = s.Albedo * _LightColor0.rgb * (hLambert * atten * 2);			
			//col.rgb = s.Albedo * _LightColor0.rgb * (difLight * atten * 2);
			col.a = s.Alpha;
			return col;
		}
		#pragma surface surf BasicDiffuse
		struct Input {
			float2 uv_MainTex;
		};
		void surf (Input IN, inout SurfaceOutput o) {
			fixed2 scrolledUV = IN.uv_MainTex;
			fixed xScrollValue = _ScrollXSpeed * _Time;
			fixed yScrollValue = _ScrollYSpeed * _Time;
			scrolledUV += fixed2 (xScrollValue,yScrollValue);
			half4 c = tex2D(_MainTex,scrolledUV);
			o.Albedo = c.rgb * _MainTint;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
