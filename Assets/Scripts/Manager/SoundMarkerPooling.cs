using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

namespace SIS {

    public interface ISoundMarkerPoolingDelegate {
        LayoutManager getLayoutManager();
        Transform getPoolingTransform();
        GameObject getSoundMarkerPrefab();
    }

    public class SoundMarkerPooling {
        ISoundMarkerPoolingDelegate _poolingDelegate = null;

        private Stack<SoundMarker> _unusedMarkers = new Stack<SoundMarker>();

        // - - - - - - - - - - - - - - - - - - -

        public SoundMarkerPooling(int numToPreallocate, ISoundMarkerPoolingDelegate del) {
            _poolingDelegate = del;
            preallocateMarkers(numToPreallocate);
        }

        // Assume this will only be called once during initialisation
        private void preallocateMarkers(int numMarkers = 64) {
            Debug.Log("preallocateMarkers numMarkers: " + numMarkers);
            Transform transform = _poolingDelegate.getPoolingTransform();

            for (int i = 0; i < numMarkers; i++) {
                SoundMarker marker = initNewSoundMarker(transform);
                marker.gameObject.SetActive(false);
                _unusedMarkers.Push(marker);
            }
        }

        // - - - - - - - - - - - - - - - - - - -

        private SoundMarker initNewSoundMarker(Transform atTransform) {

            // Place a new sound with default config
            SoundMarker newMarker = Object.Instantiate(_poolingDelegate.getSoundMarkerPrefab(),
                                                parent: atTransform).GetComponent<SoundMarker>();
            newMarker.transform.localPosition = Vector3.zero;

            return newMarker;
        }

        // - - - - - - - - - - - - - - - - - - -

        public SoundMarker GetSoundMarker() {
            SoundMarker marker = null;
            if (_unusedMarkers.Count > 0) {
                // Debug.Log("MarkerPooling::returning UNUSED marker");
                marker = _unusedMarkers.Pop();
                marker.gameObject.SetActive(true);
            } else {
                // Debug.Log("MarkerPooling::returning NEW marker");
                marker = initNewSoundMarker(_poolingDelegate.getPoolingTransform());
            }
            return marker;
        }

        // - - - - - - - - - - - - - - - - - - -

        public void RecycleAllMarkers(bool eraseHotspotData) {
            Debug.Log ("RecycleAllMarkers");
            foreach (SoundMarker marker in MainController.soundMarkers) {
                RecycleSoundMarker(marker, removeFromSoundMarkerList: false, eraseHotspotData);
            }
            MainController.soundMarkers.Clear();
        }

        public void RecycleSoundMarker(SoundMarker marker, bool removeFromSoundMarkerList, bool eraseHotspotData) {
            _unusedMarkers.Push(marker);
            if (removeFromSoundMarkerList) {
                MainController.soundMarkers.Remove(marker);
            }

            if (eraseHotspotData) {
                _poolingDelegate.getLayoutManager().EraseHotspot(marker.hotspot);
            }

            marker.markerDelegate = null;
            marker.NullifyHotspot();
            marker.gameObject.SetActive(false);

            Anchor anchorToDestroy = marker.GetComponentInParent<Anchor>();
            marker.transform.parent = _poolingDelegate.getPoolingTransform();
            marker.transform.localPosition = Vector3.zero;

            if (anchorToDestroy != null && anchorToDestroy is Anchor) {
                Object.Destroy(anchorToDestroy.gameObject);
            }
        }

        // public void RecycleSoundMarker(SoundMarker soundMarker, bool removeFromList = true, bool eraseHotspotData = true) {
        //     if (soundMarker == null) { return; }

        //     // TODO: Implement object pooling
        //     if (removeFromList) {
        //         soundMarker.markerDelegate = null;
        //         soundMarkers.Remove(soundMarker);
        //     }
        //     if (eraseHotspotData) layoutManager.EraseHotspot(soundMarker.hotspot);

        //     if (soundMarker.transform.parent != null) {
        //         Destroy(soundMarker.transform.parent.gameObject);
        //     } else {
        //         Destroy(soundMarker);
        //     }
        // }

    }
}