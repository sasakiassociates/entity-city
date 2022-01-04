Shader "Custom/HeightPosition"
{
   Properties 
   {
     _MainTex ("Base (RGB)", 2D) = "white" {}
     _HeightMin ("Height Min", Float) = -1
     _HeightMax ("Height Max", Float) = 1
     _ClampValue ("Clamp Value", Float) = 0.9
   }
  
   SubShader
   {
     Tags { "RenderType"="Opaque" }
  
     CGPROGRAM
     #pragma surface surf Lambert
  
     sampler2D _MainTex;
     float _HeightMin;
     float _HeightMax;
     float _ClampValue;
  
     struct Input
     {
       float2 uv_MainTex;
       float3 worldPos;
     };  
  

     
       
    void surf (Input IN, inout SurfaceOutput o)
        {
          float h = clamp((_HeightMax-IN.worldPos.y) / (_HeightMax-_HeightMin), 0.0, _ClampValue);
          half4 c = tex2D (_MainTex, float2(_ClampValue-h,0.5));
           o.Albedo = c.rgb;
        }
     
     ENDCG
   } 
   Fallback "Diffuse"
 }