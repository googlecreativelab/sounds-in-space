using System.Collections;
using System.Collections.Generic;
using System;
// using UnityEngine;

namespace SIS {

    public interface IOnDemandLoadingDelegate {
        Dictionary<string, SoundFile> getSoundDictionary();

        // void LayoutManagerLoadedNewLayout(LayoutManager manager, Layout newLayout);
        // void LayoutManagerHotspotListChanged(LayoutManager manager, Layout layout);
        void OnDemandLoadingLoadedAudioClipsChanged(OnDemandLoadingManager manager);
        void StartCoroutineOn(IEnumerator e);
    }

    public class OnDemandLoadingManager {
        private IOnDemandLoadingDelegate _managerDelegate = null;
        public void setDelegate(IOnDemandLoadingDelegate newDel) { _managerDelegate = newDel; }

        private Queue<AudioClipOperation> _operationQueue = new Queue<AudioClipOperation>();

        // --------------------------

        void performNextOperationInQueue() {
            if (_operationQueue.Count < 1) { return; }
            UnityEngine.Debug.LogWarning("DEQUEUING performNextOperationInQueue");

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
                UnityEngine.Debug.LogWarning("ENQUEUE LoadClipInSoundFile");
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
            _managerDelegate?.StartCoroutineOn(SoundFile.LoadClipInSoundFile(
                soundFile,
                completion: (SoundFile returnedSoundFile) => {
                    _managerDelegate?.OnDemandLoadingLoadedAudioClipsChanged(this);

                    if (completion != null) { completion(returnedSoundFile); }
                }));
        }

        void loadSoundMarkerAndSyncedClips(SoundMarker marker, Layout layout, Action<HashSet<SoundMarker>> completion) {
            HashSet<SoundFile> loadingOrLoadedSoundFiles = new HashSet<SoundFile>();
            HashSet<SoundMarker> loadingOrLoadedMarkers = new HashSet<SoundMarker>();

            // Load the SoundFile for the marker passed in
            SoundFile markerSF = marker.hotspot.soundFile;
            loadingOrLoadedSoundFiles.Add(markerSF);
            loadingOrLoadedMarkers.Add(marker);
            if (!markerSF.isDefaultSoundFile && (markerSF.loadState != LoadState.Success || markerSF.clip == null)) {
                _managerDelegate?.StartCoroutineOn(SoundFile.LoadClipInSoundFile(markerSF,
                completion: (SoundFile returnedSoundFile) => {
                    marker.OnDemandSoundFileClipWasLoaded(returnedSoundFile);
                }));
            }

            // - - - - - - - - - - - - - - - - - - -
            // Load the Synced Markers
            IEnumerable<SoundMarker> syncedMarkers = layout.getSynchronisedMarkers(marker.hotspot.id);
            if (syncedMarkers != null) {

                foreach (SoundMarker syncedMarker in syncedMarkers) {
                    SoundFile syncedSF = marker.hotspot.soundFile;
                    loadingOrLoadedSoundFiles.Add(syncedSF);
                    loadingOrLoadedMarkers.Add(syncedMarker);
                    if (!markerSF.isDefaultSoundFile && (syncedSF.loadState != LoadState.Success || syncedSF.clip == null)) {
                        _managerDelegate?.StartCoroutineOn(SoundFile.LoadClipInSoundFile(syncedSF,
                        completion: (SoundFile returnedSoundFile) => {
                            syncedMarker.OnDemandSoundFileClipWasLoaded(returnedSoundFile);
                        }));
                    }
                }
            }
            // - - - - - - - - - - - - - - - - - - -

            // Wait for loading to complete
            _managerDelegate?.StartCoroutineOn(SoundFile.AwaitLoading(
                loadingOrLoadedSoundFiles,
                completion: () => {
                    _managerDelegate?.OnDemandLoadingLoadedAudioClipsChanged(this);

                    if (completion != null) { completion(loadingOrLoadedMarkers); }
                }));
        }

        void unloadSoundMarkerAndSyncedClips(SoundMarker marker, IEnumerable<SoundMarker> syncedMarkers) {

            SoundFile markerSF = marker.hotspot.soundFile;
            if (!markerSF.isDefaultSoundFile && (markerSF.loadState == LoadState.Success || markerSF.clip != null)) {
                // Unload the first marker
                marker.SetAudioPauseState(true);
                UnityEngine.GameObject.DestroyImmediate(markerSF.clip, allowDestroyingAssets: false);
                markerSF.clip = null;
                markerSF.loadState = LoadState.NotLoaded;
            }

            // - - - - - - - - - - - - - - - - - - -
            // Unload Synced Markers
            if (syncedMarkers != null) {
                foreach (SoundMarker sm in syncedMarkers) {
                    SoundFile syncedSF = marker.hotspot.soundFile;

                    if (!syncedSF.isDefaultSoundFile && (syncedSF.loadState == LoadState.Success || syncedSF.clip != null)) {
                        // GameObject.DestroyImmediate(syncedSF.clip, allowDestroyingAssets: false);
                        sm.SetAudioPauseState(true);
                        UnityEngine.GameObject.Destroy(syncedSF.clip);
                        syncedSF.clip = null;
                        syncedSF.loadState = LoadState.NotLoaded;
                    }
                }
            }
            // - - - - - - - - - - - - - - - - - - -

            _managerDelegate?.OnDemandLoadingLoadedAudioClipsChanged(this);
        }

        void refreshLoadStateForSoundMarkers(List<SoundMarker> markers, Layout layout, Action completion) {
            if (_managerDelegate == null) { return; }
            Dictionary<string, SoundFile> sfDict = _managerDelegate.getSoundDictionary();

            HashSet<string> loadingOrLoadedMarkerIDs = new HashSet<string>();
            HashSet<SoundFile> loadingOrLoadedSoundFiles = new HashSet<SoundFile>();
            foreach (SoundMarker marker in markers) {
                if (loadingOrLoadedMarkerIDs.Contains(marker.hotspot.id)) { continue; } // Already covered by a SyncedMarker

                SoundFile sf;
                if (!sfDict.TryGetValue(marker.hotspot.id, out sf)) { continue; }

                if (marker.onDemandAudioShouldBeLoaded) {
                    loadingOrLoadedSoundFiles.Add(sf);
                    loadingOrLoadedMarkerIDs.Add(marker.hotspot.id);

                    if (sf.loadState != LoadState.Success || sf.clip == null) {
                        _managerDelegate?.StartCoroutineOn(SoundFile.LoadClipInSoundFile(sf,
                        completion: (SoundFile returnedSoundFile) => {
                            marker.OnDemandSoundFileClipWasLoaded(returnedSoundFile);
                        }));
                    }

                    // Also make sure any synced markers are loaded or loading...
                    // IEnumerable<string> syncedMarkerIDs = layout.getSynchronisedMarkerIDs(marker.hotspot.id);
                    IEnumerable<SoundMarker> syncedMarkers = layout.getSynchronisedMarkers(marker.hotspot.id);
                    foreach (SoundMarker syncedMarker in syncedMarkers) {
                        if (loadingOrLoadedMarkerIDs.Contains(syncedMarker.hotspot.id)) { continue; } // Marker already loaded or loading...

                        SoundFile syncedSoundFile;
                        if (!sfDict.TryGetValue(syncedMarker.hotspot.id, out syncedSoundFile)) { continue; }

                        loadingOrLoadedSoundFiles.Add(syncedSoundFile);
                        loadingOrLoadedMarkerIDs.Add(syncedMarker.hotspot.id);

                        if (syncedSoundFile.loadState != LoadState.Success || syncedSoundFile.clip == null) {
                            _managerDelegate?.StartCoroutineOn(SoundFile.LoadClipInSoundFile(syncedSoundFile,
                            completion: (SoundFile returnedSoundFile) => {
                                syncedMarker.OnDemandSoundFileClipWasLoaded(returnedSoundFile);
                            }));
                        }
                    }
                }
            }

            // Unload SoundClips that aren't being used in the current layout
            int numDestroyed = unloadSoundFilesExceptThoseInSet(loadingOrLoadedSoundFiles, sfDict);

            UnityEngine.Debug.Log("RefreshLoadStateForSoundMarkers... " + loadingOrLoadedSoundFiles.Count + " SoundClip(s) are loading... "
                                       + (sfDict.Values.Count - loadingOrLoadedSoundFiles.Count + " NOT loaded. "
                                       + numDestroyed + " DESTROYED!"));
            _managerDelegate?.StartCoroutineOn(SoundFile.AwaitLoading(
                loadingOrLoadedSoundFiles,
                completion: () => {
                    // audioClipLoadingOrUnloadingComplete(completion);
                    _managerDelegate?.OnDemandLoadingLoadedAudioClipsChanged(this);

                    if (completion != null) { completion(); }
                }));
        }

        // --------------------------

        private int unloadSoundFilesExceptThoseInSet(HashSet<SoundFile> soundFileSetToKeepLoaded, Dictionary<string, SoundFile> sfDict) {
            int numDestroyed = 0;
            foreach (SoundFile sf in sfDict.Values) {
                if (sf.clip == null
                 || soundFileSetToKeepLoaded.Contains(sf)
                 || sf.isDefaultSoundFile) { continue; }

                UnityEngine.GameObject.DestroyImmediate(sf.clip, allowDestroyingAssets: false);
                ++numDestroyed;
                sf.clip = null;
                sf.loadState = LoadState.NotLoaded;
            }
            return numDestroyed;
        }

    }
}