using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIS {
    
    [RequireComponent(typeof(SoundMarker))]
    public class FollowMainCameraYPosition : MonoBehaviour {

        private Transform camTransform;

        // Start is called before the first frame update
        void Start() {
            camTransform = Camera.main.transform;
        }

        private void OnEnable() {
            camTransform = Camera.main.transform;
        }

        private void OnDisable() {
            transform.localPosition = Vector3.zero;
        }

        // Update is called once per frame
        void Update() {
            Vector3 pos = transform.position;
            pos.y = camTransform.position.y;
            transform.position = pos;
        }
    }
}