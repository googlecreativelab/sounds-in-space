//-----------------------------------------------------------------------
// <copyright file="MobileUnlitColorDithered.shader" company="Google">
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
Shader "ARCore/CL/UnlitColorDithered"
{
    Properties
    {

    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 150

        CGPROGRAM
        #include "Dither Functions.cginc"
        #pragma surface surf Unlit noforwardadd

        struct Input
        {
            float4 screenPos;
            float4 vertexColor : COLOR;
        };

        half4 LightingUnlit (SurfaceOutput s, half3 lightDir, half atten) {
           return half4(s.Albedo, s.Alpha);
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Albedo = IN.vertexColor.rgb;
            // o.Albedo = float4(IN.vertexColor.a,IN.vertexColor.a,IN.vertexColor.a,1);
            clip(
                isDithered(IN.screenPos.xy / IN.screenPos.w, // ScreenPos
                            IN.vertexColor.a));
        }
        ENDCG
    }

    Fallback "ARCore/CL/UnlitColor"
}
