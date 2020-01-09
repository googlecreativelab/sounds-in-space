//-----------------------------------------------------------------------
// <copyright file="SoundRadiusSlider.cs" company="Google">
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
    public interface ISoundRadiusSliderDelegate {
        void SoundRadiusSliderValueChanged(SoundRadiusSlider slider, float sliderPercentage, float adjustedRadius);
    }

    public class SoundRadiusSlider : MonoBehaviour {
        public ISoundRadiusSliderDelegate sliderDelegate = null;

        public bool isInnerOtherwiseOuterRadius = false;
        // public float minDiameter { get; private set; } // Will get set on Awake
        // public float maxDiameter { get; private set; } // Will get set on Awake
        public float minDiameter { get { return isInnerOtherwiseOuterRadius 
                ? SingletonData.Instance.InnerRadiusMinDiameter 
                : SingletonData.Instance.OuterRadiusMinDiameter; } } // Will get set on Awake
        public float maxDiameter { get { return isInnerOtherwiseOuterRadius 
                ? SingletonData.Instance.InnerRadiusMaxDiameter 
                : SingletonData.Instance.OuterRadiusMaxDiameter; } } // Will get set on Awake

        public float minRadius { get { return minDiameter * 0.5f; } }
        public float maxRadius { get { return maxDiameter * 0.5f; } }
        protected float minDiameterDelta { get { return 1f - minDiameter; } }

        [SerializeField] bool maxGoesToInfinity = false;

        protected UnityEngine.UI.Slider radiusSlider;
        public float sliderValue { get { return radiusSlider.value; } }
        public float diameterValue { get { return DiameterValueFromPercentage(radiusSlider.value); } }
        public float radiusValue { get { return DiameterValueFromPercentage(radiusSlider.value) * 0.5f; } }

        [SerializeField] UnityEngine.UI.Text radiusText;
        [SerializeField] UnityEngine.UI.Text minRadiusText;
        [SerializeField] UnityEngine.UI.Text maxRadiusText;

        [SerializeField] UnityEngine.UI.Image sliderKnobImage = null;
        [SerializeField] UnityEngine.UI.Image sliderFillImage = null;
        [SerializeField] UnityEngine.UI.Image sliderBorderImage = null;

        bool notifySliderDelegate = true;

        private void Awake() {
            
        }

        // Start is called before the first frame update
        void Start() {
            radiusSlider = GetComponentInChildren<UnityEngine.UI.Slider>();

        }

        public void SetSliderDiameter(float diameter, bool notifyDelegate = true) {
            if (radiusSlider == null) { radiusSlider = GetComponentInChildren<UnityEngine.UI.Slider>(); }

            notifySliderDelegate = notifyDelegate;
            float sliderPercent = PercentageFromDiameterVal(diameter);
            radiusSlider.value = sliderPercent;
            notifySliderDelegate = true;
        }

        public void SetSliderRadius(float radius, bool notifyDelegate = true) {
            SetSliderDiameter(radius * 2f, notifyDelegate);
        }

        public void SetColorTint(Color newCol) {
            // sliderKnobImage.color = newCol;
            sliderFillImage.color = newCol;
            sliderBorderImage.color = newCol;
        }

        protected float PercentageFromDiameterVal(float diam) {
            if (maxGoesToInfinity && diam < 0) { return 1.0f; }

            float clampedDiameter = Mathf.Max(minDiameter, Mathf.Min(maxDiameter, diam));
            return Mathf.Log(clampedDiameter + minDiameterDelta) / Mathf.Log(maxDiameter + minDiameterDelta);
        }
        protected float DiameterValueFromPercentage(float percent) {
            if (maxGoesToInfinity && percent >= 1.0f) { return -1f; }

            return Mathf.Pow(maxDiameter + minDiameterDelta, percent) - minDiameterDelta;
        }
        // -----------------------------------------
        #region Slider Callback
        // -----------------------------------------
        public void SoundDiameterSliderValueChanged(float percentage) {
            float adjustedDiameter = DiameterValueFromPercentage(percentage);
            if (radiusText != null) {
                if (maxGoesToInfinity && adjustedDiameter < 0) {
                    radiusText.text = "∞m";
                } else {
                    radiusText.text = string.Format("{0:0.0}m", Mathf.Floor(adjustedDiameter * 10f) / 10f);
                }
            }

            // this.soundRadiusChanged(radiusVal, adjustedRadius);
            if (sliderDelegate == null || !notifySliderDelegate) {
                return;
            }
            sliderDelegate.SoundRadiusSliderValueChanged(this, percentage, adjustedDiameter * 0.5f);
        }
        #endregion

    }
}
