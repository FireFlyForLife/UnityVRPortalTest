﻿Shader "Portals/PortalShader"
{
    Properties
    {
		_Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
		_StencilReferenceID("Stencil ID Reference", Int) = 1
		_StillDrawToColorBuffer("Enable color output (For debugging purposes)", Float) = 255
    }
    SubShader
    {
        Tags { 
			"RenderType"="Portal"
			//Most optimal would be Transparent-1 but I want that early Z discard
			"Queue"="Geometry+1"
			"IgnoreProjector" = "True"
		}
        LOD 100

        Pass
        {
			ZWrite On
			ZTest LEqual
			ColorMask[_StillDrawToColorBuffer]

			Stencil
			{
				Ref[_StencilReferenceID]
				Comp always
				Pass replace
			}

            CGPROGRAM
			#pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _Color;
			int _StencilReferenceID;
			RWStructuredBuffer<int> perPortalPixelCount : register(u1);

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

			[earlydepthstencil]
            fixed4 frag (v2f i) : SV_Target
            {
				//TODO: Test if there is a performance increase by just doing `= true`
				perPortalPixelCount[_StencilReferenceID] = 123321;

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
