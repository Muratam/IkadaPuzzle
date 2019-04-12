Shader "Custom/SimulateWater" {
	Properties {
		_MainTint ("Diffuse Tint",Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Transparent ("Transparency",Range(0,1)) = 0.72
		_BumpSize("Bump Size",Range(0.5,16)) = 1
		_BumpTex ("Bump",2D) = "bump"{}
		_SpecPower("Specular Power",Range(0,1)) = 0.5
		_BumpPower("Bump Power",Range(0,1)) = 0.4
	}
	SubShader {
		Tags { "RenderType"="Opaque"  "Queue" = "Transparent" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Water alpha
		#pragma target 3.0
		sampler2D _MainTex;
		sampler2D _BumpTex;
		fixed _SpecPower;
		fixed _BumpSize;
		fixed _BumpPower;
		float4 _MainTint;
		fixed _Transparent;
		struct Input {
			fixed2 uv_MainTex;
			float2 uv_BumpTex;
		};
		struct SurfaceCustomOutput{
			fixed3 Albedo;
			fixed Alpha;
			fixed3 Normal;
			fixed3 Emission;
		};

		void surf (Input IN, inout SurfaceCustomOutput o) {
			half x = 3+(half)sin(_Time*1.21) + IN.uv_MainTex.x  *2+0.25* (half)sin(_Time*3.4);
			half y = 3+(half)cos(_Time*0.72) + IN.uv_MainTex.y  *2+0.25* (half)sin(_Time*4.1);
			fixed2 uv2 = fixed2(fmod(x,0.999),fmod(y,0.999));
			float4 c = tex2D (_MainTex,uv2) * _MainTint;
			o.Albedo = c.rgb;
			//o.Emission = o.Albedo * 0.1;
			o.Alpha = _Transparent;
			o.Normal =tex2D(_BumpTex,fixed2(0.05+fmod(x * _BumpSize,0.9),fmod(0.05+y*_BumpSize,0.9)));
		}
		inline fixed4 LightingWater(SurfaceCustomOutput s,fixed3 lightDir,half3 viewDir,fixed atten){
			float diff = dot(s.Normal,lightDir);
			float3 reflectionVec = normalize(2.0 * s.Normal * diff -lightDir);
			float spec = pow(max(0,dot(reflectionVec,viewDir)),_SpecPower) ;
			fixed4 c;
			//c.rgb = s.Albedo * _LightColor0.rgb ;
			c.rgb = (s.Albedo * _LightColor0.rgb * diff) + (_LightColor0.rgb * spec);
			c.a = s.Alpha + 0.1 *sin(_Time * 10);
			return c;
		}

		ENDCG
	}
	FallBack "Diffuse"
}
