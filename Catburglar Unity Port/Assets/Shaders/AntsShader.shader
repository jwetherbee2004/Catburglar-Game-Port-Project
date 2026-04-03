Shader "Unlit/AntsShader"
{
    Properties
    {
        _MainTex ("Mask", 2D) = "white" {}
        _DashTex ("Dash Pattern", 2D) = "white" {}
        _Speed ("Speed", Float) = 2
        _Scale ("Scale", Float) = 8
        _Cutoff ("Cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _DashTex;
            float4 _MainTex_ST;
            float4 _DashTex_ST;
            float _Speed;
            float _Scale;
            float _Cutoff;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float mask = tex2D(_MainTex, i.uv).a;

                float2 duv = i.uv * _Scale;
                duv.x += _Time.y * _Speed;

                fixed4 dash = tex2D(_DashTex, duv);

                dash.a *= step(_Cutoff, mask);

                return dash;
            }
            ENDHLSL
        }
    }
}