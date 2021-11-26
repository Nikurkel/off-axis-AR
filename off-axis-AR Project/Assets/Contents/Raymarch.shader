Shader "Unlit/Raymarch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        _Power ("Mandelbulb Power", range(0.1, 16)) = 8
        _Iterations ("Iterations", range(1,100)) = 10

        _MaxSteps ("Max Marcher Steps", range(1,5000)) = 300
        _MaxDist ("Max Marcher Distance", range(1,5000)) = 100
        _SurfDist ("Surface Detail Exponent", range(1,10)) = 7

        _Figure ("Object Index", range(1,6)) = 1
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
                float3 ro : TEXCOORD1;
                float3 hitPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Power;
            int _Iterations;

            int _MaxSteps;
            float _MaxDist;
            float _SurfDist;

            int _Figure;

            // vertex shader
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex); // uv coords
                o.ro = _WorldSpaceCameraPos; // ray origin
                o.hitPos = mul(unity_ObjectToWorld, v.vertex); // material space = unity space
                return o;
            }

            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }

            float3 Repeating(float3 p) {
                return sqrt(p * p) % 4 - 2;
            }

            float MandelbulbDistance(float3 pos, bool voxel, bool repeat) {
                if (voxel) {
                    pos = round(pos * 640) / 640;
                }
                if (repeat) {
                    pos = Repeating(pos);
                }
                float Bailout = 2;
                int Iterations = _Iterations;
                float Power = _Power;
                float3 z = pos;
                float dr = 1.0;
                float r = 0.0;
                for (int i = 0; i < Iterations; i++) {
                    r = length(z) ;
                    if (r > Bailout) break;

                    // convert to polar coordinates
                    float theta = acos(z.z / r);
                    float phi = atan2(z.y, z.x);
                    dr = pow(r, Power - 1.0) * Power * dr + 1.0;

                    // scale and rotate the point
                    float zr = pow(r, Power);
                    theta = theta * Power;
                    phi = phi * Power;

                    // convert back to cartesian coordinates
                    z = zr * float3(sin(theta) * cos(phi), sin(phi) * sin(theta), cos(theta));
                    z += pos;
                }
                return 0.5 * log(r) * r / dr; // abs()
            }

            float SpongeDistance(float3 z0, bool repeat) {
                if (repeat) {
                    z0 = Repeating(z0);
                }
                float4 z = float4(z0, 2);
                float scale = 3;
                int iterations = _Iterations;
                for (int n = 0; n < iterations; n++) {
                    z = abs(z);
                    if (z.x < z.y) z.xy = z.yx;
                    if (z.x < z.z) z.xz = z.zx;
                    if (z.y < z.z) z.yz = z.zy;
                    z = z * scale;
                    z.xyz -= scale - 1;
                    if (z.z < -0.5 * (scale - 1.0)) z.z += (scale - 1.0);
                }
                return (length(max(abs(z.xyz) - 1 , 0.0))) / z.w;
            }

            // input sdf
            float GetDist(float3 p) {
                float d;

                if (_Figure == 1) {
                    d = MandelbulbDistance(p, false, false);
                }
                else if (_Figure == 2) {
                    d = MandelbulbDistance(p, true, false);
                }
                else if (_Figure == 3) {
                    d = SpongeDistance(p, false);
                }
                else if (_Figure == 4) {
                    d = MandelbulbDistance(p, false, true);
                }
                else if (_Figure == 5) {
                    d = SpongeDistance(p, true);
                }
                else if (_Figure == 6) {
                    d = length(Repeating(p)) - 1;
                }

                return d;
            }

            float3 GetNormal(float3 p) {
                float2 e = float2(1e-2, 0);
                float3 n = GetDist(p) - float3(
                    GetDist(p - e.xyy),
                    GetDist(p - e.yxy),
                    GetDist(p - e.yyx)
                    );
                return normalize(n);
            }

            float2 Raymarch(float3 ro, float3 rd) { // ray origin, ray direction
                float dO = 0; // distance origin
                float dS; // distance surface
                float i; // needed steps
                for (i = 0; i < _MaxSteps; i++) { // march until max steps reached
                    float3 p = ro + dO * rd; // position
                    dS = GetDist(p); // distance we can go without intersecting
                    if (dO + dS == dO) break;
                    dO += dS; // marching
                    if (dS < pow(10, -_SurfDist) || dO > _MaxDist) break; // hit surface or max distance reached
                }
                return float2(dO, i); // (distance, steps)
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv; // - 0.5; // move (0,0) to centre
                float3 ro = i.ro;
                float3 rd = normalize(i.hitPos - ro);

                float2 d = Raymarch(ro, rd);
                fixed4 col = 0;

                float3 p = ro + rd * d.x; // hit position
                float3 n = GetNormal(p);

                if (p.x < 0) {
                    p.x = 0;
                }
                if (p.y < 0) {
                    p.y = 0;
                }
                if (p.z < 0) {
                    p.z = 0;
                }

                if (d.x < _MaxDist) {

                    if (_Figure > 3) {
                        p = p / _MaxDist;
                    }
                    //col.rgb = d.x / _MaxDist;
                    if (d.y < _MaxSteps) {
                        col.rgb = (p + 0.1) *sqrt(d.y / _MaxSteps);
                    }
                    
                    
                }
                else {
                    //discard;
                    col.rgb = d.y / _MaxSteps; // * n * 10;
                }
                return col;
            }
            ENDCG
        }
    }
}
