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
			o.Normal =lerp(fixed3(0,0,1), UnpackNormal (tex2D(_BumpTex,fixed2(fmod(uv2.x * _BumpSize,0.999),fmod(uv2.y*_BumpSize,0.999)))),_BumpPower);
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

/*

Shader "Custom/SimulateWater" {
	Properties {
		[NoScaleOffset] _WaveTex ("WaveTexture ", 2D) = "white" {}
		WaveSpeed ("Wave speed (map1 x,y; map2 x,y)", Vector) = (19,9,-16,-7)
	}
	Subshader {
		Tags { "RenderType"="Opaque" }		
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			sampler2D _WaveTex;
			float4 _WaveTex_ST;

			struct appdata {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				UNITY_FOG_COORDS(0)
				fixed4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata v){
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);	
				o.uv = TRANSFORM_TEX (v.texcoord, _WaveTex);
				return o;
			}

			fixed4 frag( v2f i ) : SV_Target{
				fixed4 c = tex2D(_WaveTex,i.uv);					
				return c;
			}
			ENDCG
		}
	}
}
*/
/*
Shader "Custom/SimulateWater" {
	Properties {
		_WaveScale ("Wave scale", Range (0.02,0.15)) = 0.063
		_ReflDistort ("Reflection distort", Range (0,1.5)) = 0.44
		_RefrDistort ("Refraction distort", Range (0,1.5)) = 0.40
		_RefrColor ("Refraction color", COLOR)  = ( .34, .85, .92, 1)
		[NoScaleOffset] _Fresnel ("Fresnel (A) ", 2D) = "gray" {}
		[NoScaleOffset] _BumpMap ("Normalmap ", 2D) = "bump" {}
		WaveSpeed ("Wave speed (map1 x,y; map2 x,y)", Vector) = (19,9,-16,-7)
		[NoScaleOffset] _ReflectiveColor ("Reflective color (RGB) fresnel (A) ", 2D) = "" {}
		_HorizonColor ("Simple water horizon color", COLOR)  = ( .172, .463, .435, 1)
		[HideInInspector] _ReflectionTex ("Internal Reflection", 2D) = "" {}
		[HideInInspector] _RefractionTex ("Internal Refraction", 2D) = "" {}
	}

	Subshader {
		Tags { "WaterMode"="Refractive" "RenderType"="Opaque" }
		
		Pass {
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		#pragma multi_compile_fog
		#pragma multi_compile WATER_REFRACTIVE WATER_REFLECTIVE WATER_SIMPLE

		#if defined (WATER_REFLECTIVE) || defined (WATER_REFRACTIVE)
			#define HAS_REFLECTION 1
		#endif
		#if defined (WATER_REFRACTIVE)
			#define HAS_REFRACTION 1
		#endif

		#include "UnityCG.cginc"

		uniform float4 _WaveScale4;
		uniform float4 _WaveOffset;

		#if HAS_REFLECTION
			uniform float _ReflDistort;
		#endif
		#if HAS_REFRACTION
			uniform float _RefrDistort;
		#endif

		struct appdata {
			float4 vertex : POSITION;
			float3 normal : NORMAL;
		};

		struct v2f {
			float4 pos : SV_POSITION;
			#if defined(HAS_REFLECTION) || defined(HAS_REFRACTION)
				float4 ref : TEXCOORD0;
				float2 bumpuv0 : TEXCOORD1;
				float2 bumpuv1 : TEXCOORD2;
				float3 viewDir : TEXCOORD3;
			#else
				float2 bumpuv0 : TEXCOORD0;
				float2 bumpuv1 : TEXCOORD1;
				float3 viewDir : TEXCOORD2;
			#endif
			UNITY_FOG_COORDS(4)
		};

		v2f vert(appdata v){
			v2f o;
			o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			

			// scroll bump waves
			float4 temp;
			float4 wpos = mul (_Object2World, v.vertex);
			temp.xyzw = wpos.xzxz * _WaveScale4 + _WaveOffset;
			o.bumpuv0 = temp.xy;
			o.bumpuv1 = temp.wz;
			
			// object space view direction (will normalize per pixel)
			o.viewDir.xzy = WorldSpaceViewDir(v.vertex);
			
			#if defined(HAS_REFLECTION) || defined(HAS_REFRACTION)
			o.ref = ComputeScreenPos(o.pos);
			#endif

			UNITY_TRANSFER_FOG(o,o.pos);
			return o;
		}

		#if defined (WATER_REFLECTIVE) || defined (WATER_REFRACTIVE)
			sampler2D _ReflectionTex;
		#endif
		#if defined (WATER_REFLECTIVE) || defined (WATER_SIMPLE)
			sampler2D _ReflectiveColor;
		#endif
		#if defined (WATER_REFRACTIVE)
			sampler2D _Fresnel;
			sampler2D _RefractionTex;
			uniform float4 _RefrColor;
		#endif
		#if defined (WATER_SIMPLE)
			uniform float4 _HorizonColor;
		#endif
			sampler2D _BumpMap;

		half4 frag( v2f i ) : SV_Target{
			i.viewDir = normalize(i.viewDir);
			
			// combine two scrolling bumpmaps into one
			half3 bump1 = UnpackNormal(tex2D( _BumpMap, i.bumpuv0 )).rgb;
			half3 bump2 = UnpackNormal(tex2D( _BumpMap, i.bumpuv1 )).rgb;
			half3 bump = (bump1 + bump2) * 0.5;
			
			// fresnel factor
			half fresnelFac = dot( i.viewDir, bump );
			
			// perturb reflection/refraction UVs by bumpmap, and lookup colors
			
			#if HAS_REFLECTION
			float4 uv1 = i.ref; uv1.xy += bump * _ReflDistort;
			half4 refl = tex2Dproj( _ReflectionTex, UNITY_PROJ_COORD(uv1) );
			#endif
			#if HAS_REFRACTION
			float4 uv2 = i.ref; uv2.xy -= bump * _RefrDistort;
			half4 refr = tex2Dproj( _RefractionTex, UNITY_PROJ_COORD(uv2) ) * _RefrColor;
			#endif
			
			// final color is between refracted and reflected based on fresnel
			half4 color;
			
			#if defined(WATER_REFRACTIVE)
			half fresnel = UNITY_SAMPLE_1CHANNEL( _Fresnel, float2(fresnelFac,fresnelFac) );
			color = lerp( refr, refl, fresnel );
			#endif
			
			#if defined(WATER_REFLECTIVE)
			half4 water = tex2D( _ReflectiveColor, float2(fresnelFac,fresnelFac) );
			color.rgb = lerp( water.rgb, refl.rgb, water.a );
			color.a = refl.a * water.a;
			#endif
			
			#if defined(WATER_SIMPLE)
			half4 water = tex2D( _ReflectiveColor, float2(fresnelFac,fresnelFac) );
			color.rgb = lerp( water.rgb, _HorizonColor.rgb, water.a );
			color.a = _HorizonColor.a;
			#endif

			UNITY_APPLY_FOG(i.fogCoord, color);
			return color;
		}
		ENDCG

		}
	}

}
*/