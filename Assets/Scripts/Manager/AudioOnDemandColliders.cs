using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIS {
    public class AudioOnDemandColliders : MonoBehaviour {
        
        public SphereCollider loadAudioCollider;
        public SphereCollider unloadAudioCollider;

        [Range(0.1f, 2.0f)] public float ThresholdDistance = 0.5f;
        [Range(0.2f, 10.0f)] public float DistanceFromUser = 4f;
        // [Range(1.0f, 4.0f)] public float MaxRadiusFactor = 1f;

        private void Awake() {
            // float loadDistance = MaxRadiusFactor * SingletonData.Instance.MaxDiameterForSoundMarkers * 0.5f;
            // float unloadDistance = loadDistance - ThresholdDistance;

            float loadDistance = DistanceFromUser;
            float unloadDistance = loadDistance + ThresholdDistance;

            loadAudioCollider.radius = loadDistance;
            unloadAudioCollider.radius = unloadDistance;
        }

        // Start is called before the first frame update
        // void Start() {
            
        // }

        // Update is called once per frame
        // void Update() {
            
        // }
    }
}