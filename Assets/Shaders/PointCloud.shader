//-----------------------------------------------------------------------
// <copyright file="PointCloud.shader" company="Google">
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

// Don't remove the following line. It is used to bypass Unity
// upgrader change. This is necessary to make sure the shader
// continues to compile on Unity 5.2
// UNITY_SHADER_NO_UPGRADE
Shader "ARCore/CL/#PointCloud-Boilerplate" {
Properties{
        _PointSize("Point Size", Float) = 5.0
        _Color ("PointCloud Color", Color) = (0.121, 0.737, 0.823, 1.0)
}
  SubShader {
     Pass {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag

        #include "UnityCG.cginc"

        struct appdata
        {
           float4 vertex : POSITION;
        };

        struct v2f
        {
           float4 vertex : SV_POSITION;
           float size : PSIZE;
        };

        float _PointSize;
        fixed4 _Color;

        v2f vert (appdata v)
        {
           v2f o;
           o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
           o.size = _PointSize;

           return o;
        }

        fixed4 frag (v2f i) : SV_Target
        {
           return _Color;
        }
        ENDCG
     }
  }
}
