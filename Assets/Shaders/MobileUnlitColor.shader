//-----------------------------------------------------------------------
// <copyright file="MobileUnlitColor.shader" company="Google">
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

Shader "ARCore/CL/UnlitColor"
{
    Properties
    {
        _Color ("Color", Color) = (1.0, 1.0, 0.0, 1.0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 150

        CGPROGRAM
        #pragma surface surf Unlit noforwardadd

        float4 _Color;

        struct Input
        {
            float4 screenPos;
        };

        half4 LightingUnlit (SurfaceOutput s, half3 lightDir, half atten) {
           return half4(s.Albedo, s.Alpha);
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Albedo = _Color.rgb;
        }
        ENDCG
    }

    Fallback "Mobile/VertexLit"
}
