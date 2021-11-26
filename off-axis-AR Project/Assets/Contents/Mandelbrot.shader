// reference: https://www.youtube.com/watch?v=kY7liQVPQSc

Shader "Explorer/Mandelbrot"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Area("Area", vector) = (0,0,4,4)
        _Angle("Angle", range(-3.1415, 3.1415)) = 0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 _Area;
            float _Angle;
            sampler2D _MainTex;

            float2 rot(float2 p, float2 pivot, float a) { // point rotation
                float s = sin(a);
                float c = cos(a);

                p -= pivot;
                p = float2(p.x * c - p.y * s, p.x * s + p.y * c);
                p += pivot;

                return p;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 c = _Area.xy + (i.uv - .5)*_Area.zw; // translate and zoom
                c = rot(c, _Area.xy, _Angle); // rotate
                float2 z; // position after iteration
                float iter; // brightness value depending on steps until z is out of radius
                for (iter = 0; iter < 255; iter++) {
                    z = float2(z.x * z.x - z.y * z.y, 2 * z.x * z.y) + c; // old: r = x | i = y  new: r = x^2 - y^2 + c.x | i= 2*x*y + c.y
                    if (length(z) > 2) break; // escaped
                }
                return iter/255; // sqrt to broaden gradient
            }
            ENDCG
        }
    }
}
