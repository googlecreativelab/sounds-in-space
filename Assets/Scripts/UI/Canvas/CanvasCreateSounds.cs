//-----------------------------------------------------------------------
// <copyright file="CanvasCreateSounds.cs" company="Google">
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
using DG.Tweening;

namespace SIS {
    public interface ICanvasCreateSoundsDelegate {
        void CreateSoundButtonClicked();
        void SoundPlacementModeChanged(bool isOnCursorOtherwiseDevice);
        void CreateSoundsMaxRadiusSliderValueChanged(float radiusVal, float adjustedRadius);
        void BackButtonClicked(CanvasController.CanvasUIScreen fromScreen);
    }

    public class CanvasCreateSounds : CanvasBase, ISoundRadiusSliderDelegate {
        public ICanvasCreateSoundsDelegate canvasDelegate = null;
        public override CanvasController.CanvasUIScreen canvasID { get { return CanvasController.CanvasUIScreen.AddSounds; } }

        public SoundRadiusSlider maxRadiusSlider = null;

        public bool placeSoundsOnCursor { get { return !placeModeToggle.isOn; } }
        // ------------------
        // Placement Mode UI
        public UnityEngine.UI.Toggle placeModeToggle = null;
        UnityEngine.UI.Text placeModeText = null;
        Transform toggleKnob = null;
        float toggleKnobStartX = 0;
        // ------------------

        // Start is called before the first frame update
        void Start() {
            toggleKnob = placeModeToggle.targetGraphic.transform.GetChild(0);
            placeModeText = placeModeToggle.transform.GetChild(1).GetComponent<UnityEngine.UI.Text>();
            toggleKnobStartX = toggleKnob.GetComponent<RectTransform>().anchoredPosition3D.x;

            maxRadiusSlider.sliderDelegate = this;

            placeModeText.text = "Placing at\nCursor";
            SetCanvasTitle("Place Sound at Cursor");
        }

        public override void CanvasWillAppear() {
            maxRadiusSlider.SetSliderDiameter(maxRadiusSlider.minDiameter);
            placeModeToggle.isOn = false;
        }

        #region Button Callbacks

        public void PlaceSoundModeToggled(bool isOn) {

            placeModeText.text = isOn ? "Placing at\nDevice" : "Placing at\nCursor";
            SetCanvasTitle(isOn ? "Place Sound at Device" : "Place Sound at Cursor");

            // -----------------------
            // Animation
            float animDuration = 0.4f;
            toggleKnob.DOLocalMoveX(endValue: isOn ? -toggleKnobStartX : toggleKnobStartX, duration: animDuration)
            .SetEase(Ease.InOutExpo);

            Color bgCol = isOn ? ColorThemeData.Instance.interactionColor : new Color(1, 1, 1, 0.4f);
            placeModeToggle.targetGraphic.DOColor(bgCol, animDuration);

            if (canvasDelegate == null) { return; }

            canvasDelegate.CreateSoundsMaxRadiusSliderValueChanged(maxRadiusSlider.sliderValue, maxRadiusSlider.radiusValue);
            canvasDelegate.SoundPlacementModeChanged(isOnCursorOtherwiseDevice: !isOn);
        }

        public void CreateSoundButtonClicked() {
            if (canvasDelegate == null) { return; }
            canvasDelegate.CreateSoundButtonClicked();
        }

        public override void BackButtonClicked() {
            base.BackButtonClicked();

            if (canvasDelegate == null) { return; }
            canvasDelegate.BackButtonClicked(this.canvasID);
        }

        #endregion

        #region ISoundRadiusSliderDelegate

        public void SoundRadiusSliderValueChanged(SoundRadiusSlider slider, float sliderPercentage, float adjustedRadius) {

            if (placeModeToggle.isOn) { return; }
            canvasDelegate?.CreateSoundsMaxRadiusSliderValueChanged(sliderPercentage, adjustedRadius);
        }

        #endregion

    }
}