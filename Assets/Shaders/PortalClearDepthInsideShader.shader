Shader "Unlit/PortalClearDepthInsideShader"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
		ZWrite On
		ZTest Always
		Stencil{
			Ref 0
			Comp notequal
			Pass keep
			Fail keep
			ZFail keep
		}
		ColorMask 0

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
#ifdef UNITY_REVERSED_Z
				float far = 0; // far projection distance at 0 on dx11, exists so this works in editor
#else
				float far = 1;
#endif

                o.vertex = UnityObjectToClipPos(v.vertex);
				o.vertex.z = far;
				o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(1,0,0,1);
            }
            ENDCG
        }
    }
}
