// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace SIS {
    
    [RequireComponent(typeof(HighAndLowFilter))]
    public class AudioPhaser : MonoBehaviour {

        static float MIN_FREQ = 150;
        static float MAX_FREQ = 1200;
        static float FREQ_RANGE = MAX_FREQ - MIN_FREQ;

        static float MIN_SPEED = 2;
        static float MAX_SPEED = 48;
        static float SPEED_RANGE = MAX_SPEED - MIN_SPEED;

        private HighAndLowFilter filter;
        
        private Vector2 _minMaxFrequencies = new Vector2(MIN_FREQ, MAX_FREQ);
        private float _minMaxFreqRange = 0;

        private float _phaserSpeed = 12f;
        private float _minMaxSpeedRange = SPEED_RANGE;
        private float _time = 0f;

        public void setEnabled(bool isEnabled) {
            if (filter == null) { filter = GetComponent<HighAndLowFilter>(); }
            if (isEnabled) { this.enabled = true; }

            filter.filterOn = isEnabled;
            filter.enabled = isEnabled;

            if (!isEnabled) { this.enabled = false; }
        }

        private void Awake() {
            filter = GetComponent<HighAndLowFilter>();
            _minMaxFreqRange = _minMaxFrequencies.y - _minMaxFrequencies.x;
        }

        // Start is called before the first frame update
        // void Start() {
            
        // }

        public void setMaxSpeedWithPercentage(float percent) {
            // setMaxMinFrequences(MIN_FREQ, MIN_FREQ + (percent * FREQ_RANGE));
            _minMaxSpeedRange = MIN_SPEED + (percent * SPEED_RANGE);
        }

        private void setMaxMinFrequences(float min, float max) {
            _minMaxFrequencies.x = min;
            _minMaxFrequencies.y = max;
            _minMaxFreqRange = max - min;
        }

        public void setPhaserPercent(float newSpeedPercent) {
            _phaserSpeed = MIN_SPEED + (newSpeedPercent * _minMaxSpeedRange);
        }

        // Update is called once per frame
        void Update() {
            _time += Time.deltaTime * _phaserSpeed;

            float phasePercent = (Mathf.Sin(_time) + 1f) * 0.5f;
            filter.cutoffFrequency = _minMaxFrequencies.x + (_minMaxFreqRange * phasePercent);
        }
    }
}