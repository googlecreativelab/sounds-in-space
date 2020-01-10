// using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIS {

    [RequireComponent(typeof(MainController))]
    public class AudioClipOnDemandLoading : MonoBehaviour {
        
        private LayoutManager _layoutManager;
        private Transform _camTransform;

        [Range(0, 2.0f)] public float CheckFrequency = 1.5f;
        [Range(1.0f, 4.0f)] public float MaxRadiusFactor = 1f;
        [Range(0.1f, 2.0f)] public float ThresholdDistance = 0.5f;

        // TODO: Comment these out ()
        private float _loadDistance = 0;
        private float _unloadDistance = 0;

        // Start is called before the first frame update
        void Start() {
            _layoutManager = GetComponent<MainController>().layoutManager;
            _camTransform = Camera.main.transform;

            // This should all be in the AudioOnDemandColliders class
            // ......

            // InvokeRepeating("performMarkerProximityChecks", time: CheckFrequency, repeatRate: CheckFrequency);
            // float loadDistance = MaxRadiusFactor * SingletonData.Instance.MaxDiameterForSoundMarkers * 0.5f;
            // float unloadDistance = loadDistance - ThresholdDistance;

            // AudioOnDemandColliders cameraColliders = Camera.main.GetComponent<AudioOnDemandColliders>();
            // cameraColliders.loadAudioCollider.radius = loadDistance;
            // cameraColliders.unloadAudioCollider.radius = unloadDistance;
        }

        private bool atLeastOneSyncedMarkerShouldBeLoaded(IEnumerable<SoundMarker> syncedMarkers) {
            if (syncedMarkers == null) { return false; }

            foreach (SoundMarker sm in syncedMarkers) {
                float distToMaxRadius = Vector2.Distance(
                    new Vector2(_camTransform.position.x, _camTransform.position.z),
                    new Vector2(sm.transform.position.x, sm.transform.position.z)
                ) - sm.soundMaxDist;

                if (distToMaxRadius <= _unloadDistance) { return true; }
            }

            return false;
        }

        private void performMarkerProximityChecks() {
            // TODO: 
            foreach (SoundMarker sm in MainController.soundMarkers) {
                // Sounds that have an infinte range (indicated by a negative maxDistance) can be ignored
                if (sm.hotspot.hasInfiniteMaxDistance) { continue; }
                
                SoundFile sf;
                if (!_layoutManager.soundDictionary.TryGetValue(sm.hotspot.soundID, out sf)) { continue; }
                if (sf.isDefaultSoundFile) { continue; } // Skip the default soundFile

                float distToMaxRadius = Vector2.Distance(
                    new Vector2(_camTransform.position.x, _camTransform.position.z), 
                    new Vector2(sm.transform.position.x, sm.transform.position.z)
                ) - sm.soundMaxDist;

                if (sf.loadState == LoadState.Success && distToMaxRadius > _unloadDistance) {
                    IEnumerable<SoundMarker> syncedMarkers = _layoutManager.layout.getSynchronisedMarkers(sm.hotspot.id);
                    if (!atLeastOneSyncedMarkerShouldBeLoaded(syncedMarkers)) {
                        // We should UNLOAD this AudioClip...
                        _layoutManager.UnloadSoundMarkerAndSyncedClips(sm, syncedMarkers);
                    }
                }
                
                if (sf.loadState != LoadState.Success && distToMaxRadius < _loadDistance) {
                    // We should LOAD this AudioClip...
                    _layoutManager.LoadSoundMarkerAndSyncedClips(sm, completion: 
                    (HashSet<SoundMarker> loadedMarkers) => {
                        // TODO: Should we stop playing and start playing all clips?
                        foreach (var loadedMarker in loadedMarkers) { loadedMarker.PlayAudioFromBeginning(ignoreTrigger: true); }
                    });
                }
            }
        }

        // Update is called once per frame
        // void Update() {
            
        // }
    }
}