Shader "Custom/Gradient"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        _Color0 ("Color0", Color) = (1, 1, 1, 1)
        _Color1 ("Color1", Color) = (1, 1, 1, 1)
        _Color2 ("Color2", Color) = (1, 1, 1, 1)
        _Color3 ("Color3", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
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
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float4 _Color0;
            float4 _Color1;
            float4 _Color2;
            float4 _Color3;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float noise(float2 pos)
            {
                return frac(sin(dot(pos, float2(12.9898, 78.233))) * 43758.5453);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // lerp colors
                float kx = i.uv.x;
                float ky = i.uv.y;
                
                fixed4 lerp1 = lerp(_Color0, _Color1, kx);
                fixed4 lerp2 = lerp(_Color2, _Color3, kx);
                fixed4 lerp3 = lerp(lerp2, lerp1, ky);

                // add dithering
                fixed4 col = lerp3 + 0.02 * noise(i.uv);
                
                return col;
            }

            ENDCG
        }
    }
}
