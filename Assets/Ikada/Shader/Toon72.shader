// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//X : No Lighting
Shader "Custom/Toon72" {

    Properties {
        _Color ("Main Color", Color) = (.5,.5,.5,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _Outline ("Outline width", Range (.002, 3)) = 1.06
        //_Outline ("Outline width", Range (.002, 0.06)) = .005
		_MainTex ("Base (RGB)", 2D) = "white" { }
        _Ramp ("Toon Ramp (RGB)", 2D) = "gray" {}
    }
    SubShader {
		Tags { "RenderType"="Opaque" }
		Pass {
			Lighting Off //Material の設定が無効になる→指定した色になる
            //ZTest Always //水面にLEqualが必要
			Name "OUTLINE" //UsePassに使用
            Tags { "LightMode" = "Always" } //Unityのライティングしない
            Cull Front // 裏のみ描画
            ZWrite On //深度バッファ
            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
				fixed3 normal : NORMAL;
			};
			struct v2f {
				float4 pos : SV_POSITION;
				UNITY_FOG_COORDS(0)
				fixed4 color : COLOR;
			};

			uniform float _Outline;
			uniform float4 _OutlineColor;

			v2f vert(appdata v) {
				v2f o;
				o.pos = v.vertex;
				o.pos.xyz =  v.vertex.xyz * _Outline ;
				o.pos = UnityObjectToClipPos(o.pos);
				o.color = _OutlineColor;
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}

	        #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            fixed4 frag(v2f i) : COLOR {
                UNITY_APPLY_FOG(i.fogCoord, i.color);
				//UNITY_OPAQUE_ALPHA(i.color.a);
                return i.color;
            }
            ENDCG
        }
    	UsePass "Toon/Lit/FORWARD"
    }

    Fallback "Toon/Basic"
}
