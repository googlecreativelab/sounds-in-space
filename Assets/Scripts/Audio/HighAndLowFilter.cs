using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIS {
    public class HighAndLowFilter : MonoBehaviour {
        struct Data {
            public float a1;
            public float a2;
            public float a3;
            public float b1;
            public float b2;
            public float out_1;
            public float out_2;
            public float in_1;
            public float in_2;
            public Data(float outVal) {
                a1 = 0;
                a2 = 0;
                a3 = 0;
                b1 = 0;
                b2 = 0;
                out_1 = outVal;
                out_2 = outVal;
                in_1 = outVal;
                in_2 = outVal;
            }
        }

        public bool filterOn = true;

        [Range(5f, 1500f)]
        public float cutoffFrequency = 50;

        [Range(0.1f, 1.41421f)]
        public float resonance = 0.2f;

        [Range(1f, 200f)]
        public float bandwidth = 20;

        [Range(0f, 1f)]
        public float maxAmplitude = 0.9f;

        [Range(0f, 1f)]
        public float multiplier = 0.9f;

        public float highPassCuttoff { get { return cutoffFrequency + bandwidth; } }
        public float lowPassCuttoff { get { return Mathf.Max(5, cutoffFrequency - bandwidth); } }

        Data lowPassData;
        Data highPassData;
        // float a1, a2, a3, b1, b2;

        // float in_1 = 0f, in_2 = 0f;

        void Start() {
            lowPassData = new Data(0);
            highPassData = new Data(0);

            lowPass(ref lowPassData);
            highPass(ref highPassData);
        }

        void Update() {
            lowPass(ref lowPassData);
            highPass(ref highPassData);
        }

        void OnAudioFilterRead(float[] data, int channels) {
            if (!filterOn) return;
            for (int i = 0; i < data.Length; i++) {
                
                float aux = data[i];

                // LOW PASS
                float lowPassVal = lowPassData.a1 * data[i] + lowPassData.a2 * lowPassData.in_1 + lowPassData.a3 * lowPassData.in_2 - lowPassData.b1 * lowPassData.out_1 - lowPassData.b2 * lowPassData.out_2;
                lowPassData.in_2 = lowPassData.in_1;
                lowPassData.in_1 = aux;
                lowPassData.out_2 = lowPassData.out_1;
                lowPassData.out_1 = lowPassVal;

                // HIGH PASS
                float highPassVal = highPassData.a1 * data[i] + highPassData.a2 * highPassData.in_1 + highPassData.a3 * highPassData.in_2 - highPassData.b1 * highPassData.out_1 - highPassData.b2 * highPassData.out_2;
                highPassData.in_2 = highPassData.in_1;
                highPassData.in_1 = aux;
                highPassData.out_2 = highPassData.out_1;
                highPassData.out_1 = data[i];

                float newVal = (lowPassVal + highPassVal);
                if (newVal > maxAmplitude) {
                    data[i] = maxAmplitude + ((newVal - maxAmplitude) * multiplier);
                    // data[i] = Mathf.Min(maxAmplitude, newVal);
                } else if (newVal < -maxAmplitude) {
                    data[i] = -maxAmplitude - ((maxAmplitude - newVal) * multiplier);
                    // data[i] = Mathf.Max(-maxAmplitude, newVal);
                } else {
                    data[i] = newVal;
                }
            }
        }

        void lowPass(ref Data dat) {
            float c = 1.0f / Mathf.Tan(Mathf.PI * lowPassCuttoff / AudioSettings.outputSampleRate);
            dat.a1 = 1.0f / (1.0f + resonance * c + c * c);
            dat.a2 = 2f * dat.a1;
            dat.a3 = dat.a1;
            dat.b1 = 2.0f * (1.0f - c * c) * dat.a1;
            dat.b2 = (1.0f - resonance * c + c * c) * dat.a1;
        }

        void highPass(ref Data dat) {
            float c = Mathf.Tan(Mathf.PI * highPassCuttoff / AudioSettings.outputSampleRate);
            dat.a1 = 1.0f / (1.0f + resonance * c + c * c);
            dat.a2 = -2f * dat.a1;
            dat.a3 = dat.a1;
            dat.b1 = 2.0f * (c * c - 1.0f) * dat.a1;
            dat.b2 = (1.0f - resonance * c + c * c) * dat.a1;
        }
    }
}