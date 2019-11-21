//-----------------------------------------------------------------------
// <copyright file="MobileDiffuseColorDitherLightEstimation.shader" company="Google">
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
// This is a duplicate from one of the GoogleARCore example Shaders
// Duplicate this to create a custom shader...
Shader "ARCore/CL/DiffuseColorDitherLightEstimation"
{
    Properties
    {
        // _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1.0, 1.0, 0.0, 1.0)
        _Dither("Dither", Range(0.0,1.0)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 150

        CGPROGRAM
        #include "Dither Functions.cginc"
        #pragma surface surf Lambert noforwardadd finalcolor:lightEstimation

        float4 _Color;
        float _Dither;
        fixed3 _GlobalColorCorrection;

        struct Input
        {
            float4 screenPos;
        };

        void lightEstimation(Input IN, SurfaceOutput o, inout fixed4 color)
        {
            color.rgb *= _GlobalColorCorrection;
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Albedo = _Color.rgb;
            clip(
                isDithered(IN.screenPos.xy / IN.screenPos.w, // ScreenPos
                            1-_Dither));
        }
        ENDCG
    }

    Fallback "ARCore/CL/DiffuseColorWithLightEstimation"
}
