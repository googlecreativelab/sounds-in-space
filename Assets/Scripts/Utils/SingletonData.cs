//-----------------------------------------------------------------------
// <copyright file="SingletonData.cs" company="Google">
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
using UnityEngine;

namespace SIS {

    public class SingletonData : MonoBehaviour {
        public static SingletonData Instance;

        public static float OnDemandDefaultDistFromUser = 3f;
        public static float OnDemandMinDistFromUser = 1f;
        public static float OnDemandMaxDistFromUser = 8f;

        public Sprite[] sprites;
        public Sprite[] soundShapeSprites;

        public float InnerRadiusMinDiameter = 0.25f;
        public float InnerRadiusMaxDiameter = 14f;

        public float OuterRadiusMinDiameter = 0.5f;
        public float OuterRadiusMaxDiameter = 20.1f;

        void Awake() {
            if (Instance != null) {
                GameObject.Destroy(Instance);
            } else {
                Instance = this;
                DontDestroyOnLoad(this);
            }
        }
    }
}