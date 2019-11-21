//-----------------------------------------------------------------------
// <copyright file="CanvasBase.cs" company="Google">
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
    public class CanvasBase : MonoBehaviour {
        public UnityEngine.UI.Text titleText = null;
        public UnityEngine.UI.Text titleTextShadow = null;

        public virtual CanvasController.CanvasUIScreen canvasID { get { return CanvasController.CanvasUIScreen.None; } }

        public void SetCanvasTitle(string titleStr) {
            if (titleText != null) { titleText.text = titleStr; }
            if (titleTextShadow != null) { titleTextShadow.text = titleStr; }
        }

        // Override me...
        public virtual void CanvasWillAppear() {

        }

        public virtual void BackButtonClicked() {
        }

        protected void ShowNativeUnsupportedDialog(string featureName = "This feature") {
            // TODO: Add a native mobile dialogue to show a message to the user
            // TODO: DEAD CODE
        }
    }
}
