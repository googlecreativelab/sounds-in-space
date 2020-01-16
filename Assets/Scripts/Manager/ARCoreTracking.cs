// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

namespace SIS {
    public interface IARCoreTrackingDelegate {
        void arCoreTrackingPausedTracking();
        void arCoreTrackingResumedTracking();
        void arCoreTrackingStoppedTracking();
    }

    public class ARCoreTracking : MonoBehaviour {
        private IARCoreTrackingDelegate _managerDelegate = null;
        public void setDelegate(IARCoreTrackingDelegate del) { _managerDelegate = del; }

        private Transform _anchorWrapper;
        private TrackingState _prevTrackingState = TrackingState.Tracking;

        private Anchor FirstAnchor {
            get { return _anchorWrapper.GetComponentInChildren<Anchor>(includeInactive: true); }
        }

        // Start is called before the first frame update
        void Start() {
            _anchorWrapper = GetComponent<MainController>().anchorWrapperTransform;
        }

        void Update() {
            DetectTrackingLoss();
        }

        private void DetectTrackingLoss() {

            if (FirstAnchor == null) {
                // No anchor? New scene, warning is not relevant
                VoiceOver.main.StopWarning();
                return;
            }
            // make sure the state has changed
            TrackingState currTrackingState = FirstAnchor.TrackingState;
            if (currTrackingState == _prevTrackingState) return;

            switch(currTrackingState) {
                case TrackingState.Tracking: _managerDelegate?.arCoreTrackingResumedTracking(); break;
                case TrackingState.Paused: _managerDelegate?.arCoreTrackingPausedTracking(); break;
                case TrackingState.Stopped: _managerDelegate?.arCoreTrackingStoppedTracking(); break;
            }

            _prevTrackingState = currTrackingState; // update prev state
        }
    }
}