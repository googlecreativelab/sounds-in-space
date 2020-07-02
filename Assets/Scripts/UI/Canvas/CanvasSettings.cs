// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SIS {
    public interface ICanvasSettingsDelegate {
        void BackButtonClicked(CanvasController.CanvasUIScreen fromScreen);
        Layout GetCurrentLayout();
    }

    public class CanvasSettings : CanvasBase {
        public ICanvasSettingsDelegate canvasDelegate = null;

        public override CanvasController.CanvasUIScreen canvasID { get { return CanvasController.CanvasUIScreen.Settings; } }

        [SerializeField] Toggle onDemandToggle = null;
        [SerializeField] Text onDemandSliderText = null;
        [SerializeField] Slider onDemandSlider = null;

        // Start is called before the first frame update
        void Start() {
            
        }

        // - - - - 
        private void updateOnDemandSliderTextWithValue(float val) {
            onDemandSliderText.text = string.Format("On-Demand Radius: {0:0.0}m", val);
        }
        private static float onDemandSliderPercentageFromValue(float val) {
            float range = SingletonData.OnDemandMaxDistFromUser - SingletonData.OnDemandMinDistFromUser;
            return (val - SingletonData.OnDemandMinDistFromUser) / range;
        }
        private static float onDemandSliderValueFromPercentage(float percent) {
            float range = SingletonData.OnDemandMaxDistFromUser - SingletonData.OnDemandMinDistFromUser;
            return SingletonData.OnDemandMinDistFromUser + (percent * range);
        }
        // - - - - 

        override public void CanvasWillAppear() {
            base.CanvasWillAppear();

            Layout curLayout = canvasDelegate?.GetCurrentLayout();
            if (curLayout == null) { return; }
            onDemandToggle.isOn = curLayout.onDemandActive;
            onDemandSlider.value = onDemandSliderPercentageFromValue(curLayout.onDemandRadius);
            updateOnDemandSliderTextWithValue(curLayout.onDemandRadius);
        }

        public void OnDemandToggled(bool isOn) {
            if (canvasDelegate == null) { return; }

            CanvasEditSound.AnimateToggle(onDemandToggle, isOn, animDuration: gameObject.activeInHierarchy ? 0.36f : 0); // Animate
        }

        public void OnDemandSliderChanged(float newVal) {
            updateOnDemandSliderTextWithValue(onDemandSliderValueFromPercentage(newVal));
        }

        public void SaveToCurrentLayout() {
            // Save the Data
            Layout curLayout = canvasDelegate?.GetCurrentLayout();
            curLayout.SetOnDemand(onDemandToggle.isOn, onDemandSliderValueFromPercentage(onDemandSlider.value));
        }

        public void saveButtonClicked() {
            base.BackButtonClicked();

            SaveToCurrentLayout();

            if (canvasDelegate == null) { return; }
            canvasDelegate.BackButtonClicked(this.canvasID);
        }

        // Update is called once per frame
        // void Update() {
            
        // }
    }
}
