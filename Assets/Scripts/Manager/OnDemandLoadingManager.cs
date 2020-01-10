using System.Collections;
using System.Collections.Generic;
using System;
// using UnityEngine;

namespace SIS {

    public interface IOnDemandLoadingDelegate {
        Dictionary<string, SoundFile> getSoundDictionary();
        int getNumLoadedInSoundDictionary();

        // void LayoutManagerLoadedNewLayout(LayoutManager manager, Layout newLayout);
        // void LayoutManagerHotspotListChanged(LayoutManager manager, Layout layout);
        void OnDemandLoadingLoadedAudioClipsChanged(OnDemandLoadingManager manager);
        void StartCoroutineOn(IEnumerator e);
    }

    public class OnDemandLoadingManager {
        private IOnDemandLoadingDelegate _managerDelegate = null;
        public void setDelegate(IOnDemandLoadingDelegate newDel) { _managerDelegate = newDel; }

        private Queue<AudioClipOperation> _operationQueue = new Queue<AudioClipOperation>();
        private static bool destroyImmediate = false;

        // --------------------------

        void performNextOperationInQueue() {
            if (_operationQueue.Count < 1) { return; }
            UnityEngine.Debug.LogError("DEQUEUING performNextOperationInQueue");

            AudioClipOperation nextOperation = _operationQueue.Dequeue();
            if (nextOperation is LoadAudioClipOperation loadOp) {
                loadClipInSoundFile(loadOp.soundFile, completion: (SoundFile sf) => {
                    loadOp.completion(sf);
                    performNextOperationInQueue();
                });
            } else if (nextOperation is LoadSoundMarkerAndSyncedClipsOperation loadMarkerOp) {
                loadSoundMarkerAndSyncedClips(loadMarkerOp.marker, loadMarkerOp.layout, completion: (HashSet<SoundMarker> markerSet) => {
                    loadMarkerOp.completion(markerSet);
                    performNextOperationInQueue();
                });
            } else if (nextOperation is UnloadSoundMarkerAndSyncedClipsOperation unloadMarkerOp) {
                unloadSoundMarkerAndSyncedClips(unloadMarkerOp.marker, unloadMarkerOp.syncedMarkers);
                performNextOperationInQueue();
            } else if (nextOperation is RefreshMarkerClipLoadStatesOperation refreshOp) {
                refreshLoadStateForSoundMarkers(refreshOp.markers, refreshOp.layout, completion: () => {
                    refreshOp.completion();
                    performNextOperationInQueue();
                });
            }
        }

        // --------------------------

        public void LoadClipInSoundFile(SoundFile soundFile, Action<SoundFile> completion) {
            if (_operationQueue.Count < 1) {
                loadClipInSoundFile(soundFile, completion: (SoundFile sf) => {
                    completion(sf);
                    this.performNextOperationInQueue();
                });
            } else {
                UnityEngine.Debug.LogError("ENQUEUE LoadClipInSoundFile");
                _operationQueue.Enqueue(new LoadAudioClipOperation(soundFile, completion));
            }
        }

        public void LoadSoundMarkerAndSyncedClips(SoundMarker marker, Layout layout, Action<HashSet<SoundMarker>> completion) {
            if (_operationQueue.Count < 1) {
                loadSoundMarkerAndSyncedClips(marker, layout, completion: (HashSet<SoundMarker> markerSet) => {
                    completion(markerSet);
                    this.performNextOperationInQueue();
                });
            } else {
                UnityEngine.Debug.LogWarning("ENQUEUE LoadSoundMarkerAndSyncedClips");
                _operationQueue.Enqueue(new LoadSoundMarkerAndSyncedClipsOperation(marker, layout, completion));
            }
        }

        public void UnloadSoundMarkerAndSyncedClips(SoundMarker marker, IEnumerable<SoundMarker> syncedMarkers) {
            if (_operationQueue.Count < 1) {
                unloadSoundMarkerAndSyncedClips(marker, syncedMarkers);
                this.performNextOperationInQueue();
            } else {
                UnityEngine.Debug.LogWarning("ENQUEUE UnloadSoundMarkerAndSyncedClips");
                _operationQueue.Enqueue(new UnloadSoundMarkerAndSyncedClipsOperation(marker, syncedMarkers));
            }
        }

        public void RefreshLoadStateForSoundMarkers(List<SoundMarker> markers, Layout layout, Action completion) {
            if (_operationQueue.Count < 1) {
                refreshLoadStateForSoundMarkers(markers, layout, completion: () => {
                    completion();
                    this.performNextOperationInQueue();
                });
            } else {
                UnityEngine.Debug.LogWarning("ENQUEUE RefreshLoadStateForSoundMarkers");
                _operationQueue.Enqueue(new RefreshMarkerClipLoadStatesOperation(markers, layout, completion));
            }
        }

        // ===================================================

        class AudioClipOperation {

        }
        class LoadAudioClipOperation : AudioClipOperation {
            public SoundFile soundFile;
            public Action<SoundFile> completion;
            public LoadAudioClipOperation(SoundFile soundFile, Action<SoundFile> completion) {
                this.soundFile = soundFile;
                this.completion = completion;
            }
        }
        class LoadSoundMarkerAndSyncedClipsOperation : AudioClipOperation {
            public SoundMarker marker;
            public Layout layout;
            public Action<HashSet<SoundMarker>> completion;
            public LoadSoundMarkerAndSyncedClipsOperation(SoundMarker marker, Layout layout, Action<HashSet<SoundMarker>> completion) {
                this.marker = marker;
                this.layout = layout;
                this.completion = completion;
            }
        }
        class UnloadSoundMarkerAndSyncedClipsOperation : AudioClipOperation {
            public SoundMarker marker;
            public IEnumerable<SoundMarker> syncedMarkers;
            public UnloadSoundMarkerAndSyncedClipsOperation(SoundMarker marker, IEnumerable<SoundMarker> syncedMarkers) {
                this.marker = marker;
                this.syncedMarkers = syncedMarkers;
            }
        }
        class RefreshMarkerClipLoadStatesOperation : AudioClipOperation {
            public List<SoundMarker> markers;
            public Layout layout;
            public Action completion;
            public RefreshMarkerClipLoadStatesOperation(List<SoundMarker> markers, Layout layout, Action completion) {
                this.markers = markers;
                this.layout = layout;
                this.completion = completion;
            }
        }

        // ===================================================

        // --------------------------
        // ACTUAL IMPLEMENTATIONS BELOW

        void loadClipInSoundFile(SoundFile soundFile, Action<SoundFile> completion) {
            LoadClipInSoundFileOnCoroutine(soundFile, marker: null,
                completion: (SoundFile returnedSoundFile) => {
                    if (completion != null) { completion(returnedSoundFile); }
                });
        }

        void loadSoundMarkerAndSyncedClips(SoundMarker marker, Layout layout, Action<HashSet<SoundMarker>> completion) {
            HashSet<SoundFile> loadingOrLoadedSoundFiles = new HashSet<SoundFile>();
            HashSet<SoundMarker> loadingOrLoadedMarkers = new HashSet<SoundMarker>();

            // Load the SoundFile for the marker passed in
            SoundFile markerSF = marker.hotspot.soundFile;
            loadingOrLoadedSoundFiles.Add(markerSF);
            loadingOrLoadedMarkers.Add(marker);
            if (!markerSF.isDefaultSoundFile && (markerSF.loadState != LoadState.Success || markerSF.clip == null)) {
                LoadClipInSoundFileOnCoroutine(markerSF, marker,
                    completion: (SoundFile returnedSoundFile) => {

                    });
            }

            // - - - - - - - - - - - - - - - - - - -
            // Load the Synced Markers
            IEnumerable<SoundMarker> syncedMarkers = layout.getSynchronisedMarkers(marker.hotspot.id);
            if (syncedMarkers != null) {

                foreach (SoundMarker syncedMarker in syncedMarkers) {
                    SoundFile syncedSF = syncedMarker.hotspot.soundFile;
                    loadingOrLoadedSoundFiles.Add(syncedSF);
                    loadingOrLoadedMarkers.Add(syncedMarker);
                    if (!markerSF.isDefaultSoundFile && (syncedSF.loadState != LoadState.Success || syncedSF.clip == null)) {
                        LoadClipInSoundFileOnCoroutine(syncedSF, syncedMarker,
                            completion: (SoundFile returnedSoundFile) => {

                            });
                    }
                }
            }
            // - - - - - - - - - - - - - - - - - - -
            // Wait for loading to complete

            AwaitLoadinggOnCoroutine(loadingOrLoadedSoundFiles, notifyDelegate: false,
                completion: () => {
                    if (completion != null) { completion(loadingOrLoadedMarkers); }
                });
        }

        void unloadSoundMarkerAndSyncedClips(SoundMarker marker, IEnumerable<SoundMarker> syncedMarkers) {

            SoundFile markerSF = marker.hotspot.soundFile;
            if (!markerSF.isDefaultSoundFile && (markerSF.loadState == LoadState.Success || markerSF.clip != null)) {
                // Unload the first marker
                // marker.SetAudioPauseState(true);
                marker.OnDemandNullifyAudioClip();

                if (destroyImmediate) {
                    UnityEngine.GameObject.DestroyImmediate(markerSF.clip, allowDestroyingAssets: false);
                } else {
                    UnityEngine.GameObject.Destroy(markerSF.clip);
                }
                
                markerSF.clip = null;
                markerSF.loadState = LoadState.NotLoaded;
            }

            // - - - - - - - - - - - - - - - - - - -
            // Unload Synced Markers
            if (syncedMarkers != null) {
                foreach (SoundMarker syncedMarker in syncedMarkers) {
                    SoundFile syncedSF = syncedMarker.hotspot.soundFile;

                    if (!syncedSF.isDefaultSoundFile && (syncedSF.loadState == LoadState.Success || syncedSF.clip != null)) {
                        // syncedMarker.SetAudioPauseState(true);
                        syncedMarker.OnDemandNullifyAudioClip();

                        if (destroyImmediate) {
                            UnityEngine.GameObject.DestroyImmediate(syncedSF.clip, allowDestroyingAssets: false);
                        } else {
                            UnityEngine.GameObject.Destroy(syncedSF.clip);
                        }
                        syncedSF.clip = null;
                        syncedSF.loadState = LoadState.NotLoaded;
                    }
                }
            }
            // - - - - - - - - - - - - - - - - - - -

            _managerDelegate?.OnDemandLoadingLoadedAudioClipsChanged(this);
        }

        void refreshLoadStateForSoundMarkers(List<SoundMarker> markers, Layout layout, Action completion) {
            if (_managerDelegate == null || layout == null) { return; }
            Dictionary<string, SoundFile> sfDict = _managerDelegate.getSoundDictionary();
            int numLoaded = _managerDelegate.getNumLoadedInSoundDictionary();

            // UnityEngine.Debug.LogWarning("refreshLoadStateForSoundMarkers NumLoadedAudioClips: " + numLoaded);

            HashSet<string> loadingOrLoadedMarkerIDs = new HashSet<string>();
            HashSet<SoundFile> loadingOrLoadedSoundFiles = new HashSet<SoundFile>();
            foreach (SoundMarker marker in markers) {
                if (loadingOrLoadedMarkerIDs.Contains(marker.hotspot.id)) { continue; } // Already covered by a SyncedMarker

                SoundFile sf;
                if (!sfDict.TryGetValue(marker.hotspot.soundID, out sf)) { continue; }
                
                // if (marker.onDemandAudioShouldBeLoaded) {
                //     UnityEngine.Debug.LogWarning("   Marker " + sf.filename + " onDemandAudioShouldBeLoaded: " + marker.onDemandAudioShouldBeLoaded);
                // } else {
                //     UnityEngine.Debug.LogWarning("   Marker " + sf.filename + " onDemandAudioShouldBeLoaded: " + marker.onDemandAudioShouldBeLoaded);
                // }
                if (!marker.onDemandAudioShouldBeLoaded) { continue; }

                // The Marker SHOULD be loaded
                loadingOrLoadedSoundFiles.Add(sf);
                loadingOrLoadedMarkerIDs.Add(marker.hotspot.id);

                if (sf.loadState != LoadState.Success || sf.clip == null) {
                    LoadClipInSoundFileOnCoroutine(sf, marker,
                        completion: (SoundFile returnedSoundFile) => {

                        });
                }

                // Also make sure any synced markers are loaded or loading...
                // IEnumerable<string> syncedMarkerIDs = layout.getSynchronisedMarkerIDs(marker.hotspot.id);
                IEnumerable<SoundMarker> syncedMarkers = layout.getSynchronisedMarkers(marker.hotspot.id);
                if (syncedMarkers == null) { continue; }

                foreach (SoundMarker syncedMarker in syncedMarkers) {
                    if (loadingOrLoadedMarkerIDs.Contains(syncedMarker.hotspot.id)) { continue; } // Marker already loaded or loading...

                    SoundFile syncedSoundFile;
                    if (!sfDict.TryGetValue(syncedMarker.hotspot.soundID, out syncedSoundFile)) { continue; }

                    // UnityEngine.Debug.Log("   SyncedMarker " + sf.filename + " SHOULD be loaded");
                    loadingOrLoadedSoundFiles.Add(syncedSoundFile);
                    loadingOrLoadedMarkerIDs.Add(syncedMarker.hotspot.id);

                    if (!(syncedSoundFile.loadState != LoadState.Success || syncedSoundFile.clip == null)) { continue; }
                    // Execute the below if the AudioClip is not loaded
                    LoadClipInSoundFileOnCoroutine(syncedSoundFile, syncedMarker, 
                        completion: (SoundFile returnedSoundFile) => {
                            
                        });
                }
            }

            // Unload SoundClips that aren't being used in the current layout
            int numDestroyed = unloadSoundFilesExceptThoseInSet(loadingOrLoadedSoundFiles, sfDict);

            UnityEngine.Debug.Log("RefreshLoadStateForSoundMarkers... " + markers.Count + " Markers, " 
                                       + loadingOrLoadedSoundFiles.Count + " SoundClip(s) are loading or loaded... "
                                       + (sfDict.Values.Count - loadingOrLoadedSoundFiles.Count + " NOT loaded. "
                                       + numDestroyed + " DESTROYED!"));
            AwaitLoadinggOnCoroutine(loadingOrLoadedSoundFiles, notifyDelegate: true,
                completion: () => { 
                    if (completion != null) { completion(); }
                });
        }

        // --------------------------

        private void AwaitLoadinggOnCoroutine(HashSet<SoundFile> loadingOrLoadedSoundFiles, bool notifyDelegate, Action completion) {
            _managerDelegate?.StartCoroutineOn(SoundFile.AwaitLoading(
                loadingOrLoadedSoundFiles,
                completion: () => {
                    // audioClipLoadingOrUnloadingComplete(completion);
                    if (notifyDelegate) { _managerDelegate?.OnDemandLoadingLoadedAudioClipsChanged(this); }
                    if (completion != null) { completion(); }
                }));
        }

        private void LoadClipInSoundFileOnCoroutine(SoundFile sf, SoundMarker marker, System.Action<SoundFile> completion = null) {
            if (sf.isDefaultSoundFile || (sf.loadState == LoadState.Success && sf.clip != null)) {
                if (marker != null) { marker.OnDemandSoundFileClipWasLoaded(sf); }
                completion(sf);
                return;
            }

            sf.loadState = LoadState.Loading;
            _managerDelegate?.StartCoroutineOn(SoundFile.LoadClipInSoundFile(sf,
                completion: (SoundFile returnedSoundFile) => {
                    _managerDelegate?.OnDemandLoadingLoadedAudioClipsChanged(this);
                    if (marker != null) { marker.OnDemandSoundFileClipWasLoaded(returnedSoundFile); }
                    completion(returnedSoundFile);
                }));
        }

        private int unloadSoundFilesExceptThoseInSet(HashSet<SoundFile> soundFileSetToKeepLoaded, Dictionary<string, SoundFile> sfDict) {
            int numDestroyed = 0;
            IEnumerable<SoundMarker> soundMarkersToUnload = MainController.soundMarkersNotUsingSoundFileIDs(soundFileSetToKeepLoaded);

            foreach (SoundMarker marker in soundMarkersToUnload) {
                SoundFile sf;
                if (!sfDict.TryGetValue(marker.hotspot.soundID, out sf)) { continue; }
                if (sf.isDefaultSoundFile) { continue; }

                marker.OnDemandNullifyAudioClip();
                if (destroyImmediate) {
                    UnityEngine.GameObject.DestroyImmediate(sf.clip, allowDestroyingAssets: false);
                } else {
                    UnityEngine.GameObject.Destroy(sf.clip);
                }
                ++numDestroyed;
                sf.clip = null;
                sf.loadState = LoadState.NotLoaded;
            }
            return numDestroyed;
        }

    }
}