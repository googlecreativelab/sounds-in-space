//-----------------------------------------------------------------------
// <copyright file="SoundPlacement.cs" company="Google">
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

    public class SoundPlacement : MonoBehaviour {
        public Transform cursorTransform;
        Transform cursorChildObj;
        public Transform cameraTransform;

        public float followRate = 0.2f;

        // Start is called before the first frame update
        void Awake() {
            cursorChildObj = cursorTransform.GetChild(0);
        }

        void Start() {

        }

        public void SetCursorModelHidden(bool isHidden) {
            cursorChildObj.gameObject.SetActive(!isHidden);
        }

        // Update is called once per frame
        void Update() {
            if (cameraTransform == null || cursorTransform == null) { return; }

            float distAway = 1f;
            Vector3 newPos = cameraTransform.position + Quaternion.AngleAxis(0, cameraTransform.up) * cameraTransform.forward * distAway;
            newPos.y += 0.3f;
            // cursorTransform.position = newPos;
            cursorChildObj.localScale = Vector3.one + (0.1f * Vector3.one * Mathf.Sin(5f * Time.time));

            // Smooth the cursor position to the target
            // Compute our exponential smoothing factor.
            float blend = 1f - Mathf.Pow(1f - followRate, Time.deltaTime * 30f);
            cursorTransform.position = Vector3.Lerp(
                    cursorTransform.position,
                    newPos, blend);
            // ----------------------------------------

        }
    }
}