Shader "UI/Stencil/HoleWriter"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // Propriedades obrigatórias para a UI da Unity não quebrar o RectTransform
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        
        ColorMask 0 // Fica invisível aos olhos
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Stencil
        {
            Ref 1           // O ID secreto do nosso furo
            Comp Always     // Escreve sempre que for instanciado
            Pass Replace
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata_t { float4 vertex : POSITION; float4 color : COLOR; float2 texcoord : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; fixed4 color : COLOR; float2 texcoord : TEXCOORD0; };
            
            sampler2D _MainTex;
            fixed4 _Color;
            
            v2f vert(appdata_t v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target { 
                fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;
                
                // MAGIA DA TRANSPARÊNCIA: Se o pixel for quase invisível, cancela a escrita no Stencil.
                // Isso garante que o furo fique circular e esfumaçado, e não um bloco quadrado.
                clip(col.a - 0.05); 
                
                return col; 
            }
            ENDCG
        }
    }
}