//-----------------------------------------------------------------------
// <copyright file="UnlitRimOutlineFade.shader" company="Google">
//
// Copyright 2019 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------
Shader "ARCore/CL/UnlitRimOutlineFadeMasked"
{
    Properties
    {
        _Color ("Color", Color) = (1.0, 1.0, 0.0, 1.0)
        _RimScale ("Rim Scale", Range(0.2,2.5)) = 0.63
        _RimSize ("Rim Size", Range(0.01,2.5)) = 0.63
        _MainTex ("Base (RGB)", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        LOD 150
        Cull Back

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        // #include "Dither Functions.cginc"
        #pragma surface surf Unlit noforwardadd alpha:fade
        // #pragma surface surf Lambert

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;
            float3 viewDir;
        };

        float4 _Color;
        float _RimSize;
        float _RimScale;
        sampler2D _MainTex;

        half4 LightingUnlit (SurfaceOutput s, half3 lightDir, half atten) {
           return half4(s.Albedo, s.Alpha);
         }

        void surf (Input IN, inout SurfaceOutput o)
        {
            half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal * _RimSize));
            fixed4 maskTex = tex2D(_MainTex, IN.uv_MainTex);

            // clip(
            //     isDithered(IN.screenPos.xy / IN.screenPos.w, // ScreenPos
            //                 rim * _RimScale * _Color.a)
            //                 < _RimSize ? -1:1
            //     );

            // float alphaVal = rim * _RimScale * _Color.a;
            // o.Alpha = alphaVal < _RimSize ? 0 : alphaVal;

            // o.Alpha = rim * _RimScale * _Color.a;
            o.Alpha = 0.5 * maskTex.a * rim * _RimScale * _Color.a;
            // o.Alpha = min(maskTex.a, rim * _RimScale * _Color.a);
            // o.Alpha = maskTex.a;
            o.Albedo = _Color.rgb;
        }
        ENDCG
    }

    Fallback "ARCore/DiffuseWithLightEstimation"
}
