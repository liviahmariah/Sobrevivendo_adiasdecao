
Shader "Custom/ShaderOndulacaoVisao"
{
    Properties
    {
        _MainTex ("Imagem da Camera", 2D) = "white" {}
        _Intensidade ("Intensidade", Range(0, 0.05)) = 0.015
        _Velocidade ("Velocidade", Range(0, 10)) = 3
        _Frequencia ("Frequencia", Range(1, 30)) = 12
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Intensidade;
            float _Velocidade;
            float _Frequencia;

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                float tempo = _Time.y * _Velocidade;

                float ondulacaoX =
                    sin(uv.y * _Frequencia + tempo)
                    * _Intensidade;

                float ondulacaoY =
                    cos(uv.x * _Frequencia + tempo * 0.8)
                    * _Intensidade;

                uv.x += ondulacaoX;
                uv.y += ondulacaoY;

                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}
