
//-----------------------------------------------------------------------
// <copyright file="UnlitRimOutline.shader" company="Google">
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

Shader "ARCore/CL/UnlitRimOutline"
{
    Properties
    {
        _Color ("Color", Color) = (1.0, 1.0, 0.0, 1.0)
        [MaterialToggle] _DitheringEnabled("Use Dithering", Float) = 0
        _RimScale ("Rim Scale", Range(0.0,1.0)) = 0.63
        _RimSize ("Rim Size", Range(0.1,1.0)) = 0.63
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest on
        ZWrite off

        LOD 150

        CGPROGRAM
        #include "Dither Functions.cginc"
        #pragma surface surf Unlit noforwardadd alpha

        struct Input
        {
            float4 screenPos;
            float3 viewDir;
        };

        float4 _Color;
        float _RimSize;
        float _RimScale;
        float _DitheringEnabled;

        half4 LightingUnlit (SurfaceOutput s, half3 lightDir, half atten) {
           return half4(s.Albedo, s.Alpha);
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Albedo = _Color.rgb;

            half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));

            o.Alpha = (_DitheringEnabled ?
                        (isDithered(IN.screenPos.xy / IN.screenPos.w, // ScreenPos
                         2*lerp(-(4*_RimScale),4*_RimSize,rim)))
                            : 2*lerp(-(4*_RimScale),4*_RimSize,rim))
                       * _Color.a;
        }
        ENDCG
    }

    Fallback "ARCore/DiffuseWithLightEstimation"
}
