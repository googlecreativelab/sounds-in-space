using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIS {
    public class AudioOnDemandColliders : MonoBehaviour {
        
        public SphereCollider loadAudioCollider;
        public SphereCollider unloadAudioCollider;

        [Range(0.1f, 2.0f)] public float ThresholdDistance = 0.5f;
        private float _distanceFromUser = 4f;
        public float DistanceFromUser {
            set {
                updateCollidersWithDistFromUser(value);
                _distanceFromUser = value;
            }
        }

        private void updateCollidersWithDistFromUser(float dist) {
            // float loadDistance = _distanceFromUser;
            float unloadDistance = dist + ThresholdDistance;

            loadAudioCollider.radius = dist;
            unloadAudioCollider.radius = unloadDistance;
        }

        private void Awake() {
            // float loadDistance = MaxRadiusFactor * SingletonData.Instance.MaxDiameterForSoundMarkers * 0.5f;
            // float unloadDistance = loadDistance - ThresholdDistance;

            updateCollidersWithDistFromUser(_distanceFromUser);
        }

        // Start is called before the first frame update
        // void Start() {
            
        // }

        // Update is called once per frame
        // void Update() {
            
        // }
    }
}