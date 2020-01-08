using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIS {

    [RequireComponent(typeof(MainController))]
    public class AudioClipHotLoading : MonoBehaviour {
        
        private LayoutManager _layoutManager;
        [Range(0, 1.0f)] public float checkFrequency = 0.25f;
        [Range(1.0f, 4.0f)] public float maxRadiusFactor = 1f;
        [Range(0.1f, 2.0f)] public float thresholdDistance = 0.5f;

        // Start is called before the first frame update
        void Start() {
            _layoutManager = GetComponent<MainController>().layoutManager;

            // InvokeRepeating("PerformMarkerProximityChecks", time: checkFrequency, repeatRate: checkFrequency);
        }

        void PerformMarkerProximityChecks() {
            // TODO: 
        }

        // Update is called once per frame
        // void Update() {
            
        // }
    }
}