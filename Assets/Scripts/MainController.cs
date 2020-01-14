//-----------------------------------------------------------------------
// <copyright file="MainController.cs" company="Google">
//
// Copyright 2019 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;

namespace SIS {

    [RequireComponent(typeof(SoundMarkerSelection))]
    public class MainController : MonoBehaviour, IObjectSelectionDelegate, ICanvasControllerDelegate, ILayoutManagerDelegate, ISoundMarkerDelegate {

        // Used to reset the ARCore device.
        private SessionStatus arCoreSessionStatus = SessionStatus.None;
        private GameObject arCoreDevice;

        public GameObject soundMarkerPrefab;
        public GameObject originMarkerPrefab;

        // General objects we need to access
        public CanvasController canvasControl;
        public Camera firstPersonCamera;
        public static List<SoundMarker> soundMarkers;
        public static IEnumerable<SoundMarker> soundMarkersForSoundFileIDs(HashSet<string> soundFileIDs) {
            return soundMarkers.Where( sm => soundFileIDs.Contains(sm.hotspot.soundID) );
        }
        public static IEnumerable<SoundMarker> soundMarkersNotUsingSoundFileIDs(HashSet<SoundFile> soundFileSetToKeepLoaded) {
            return soundMarkersNotUsingSoundFileIDs(new HashSet<string> (soundFileSetToKeepLoaded.Select(sf => sf.filename)));
        }
        public static IEnumerable<SoundMarker> soundMarkersNotUsingSoundFileIDs(HashSet<string> soundFileIDsToAvoid) {
            return soundMarkers.Where(sm => !soundFileIDsToAvoid.Contains(sm.hotspot.soundID));
        }

        public SoundPlacement soundPlacementRef;
        public SoundPlacement soundPlacement { get { return soundPlacementRef; } }
        public Transform cursorTransform { get { return soundPlacementRef.cursorTransform; } }

        private SoundMarkerSelection myObjectSelection;
        public SoundMarkerSelection objectSelection { get { return myObjectSelection; } }

        // Sounds will be wrapped in the anchorWrapperTransform object
        public Transform anchorWrapperTransform;
        OriginMarker originMarker = null;

        public LayoutManager layoutManager;
        public bool playbackIsStopped { get { return canvasControl.mainScreen.playbackIsStopped; } }

        public static AudioOnDemandColliders OnDemandColliders { get { return Camera.main.GetComponent<AudioOnDemandColliders>(); } }
        public bool onDemandIsActive { get { return GetCurrentLayout().onDemandActive; } }
        public bool IsOnDemandActive() { return onDemandIsActive; }

        // Serialize so that it can be set in the editor without being public
        [SerializeField] float defaultMinDistance = 0.25f;
        [SerializeField] float defaultMaxDistance = 0.5f;

        // True if the app is in the process of quitting due to an ARCore
        bool isQuitting = false;

        void Awake() {
            soundMarkers = new List<SoundMarker>();

            layoutManager = new LayoutManager();
            layoutManager.layoutManagerDelegate = this;
            layoutManager.LoadSoundMetaFiles(() => { });
            
            layoutManager.LoadCurrentLayout(); // Calls the delegate method LayoutManagerLoadedNewLayout
            // The below is called in LayoutManagerLoadedNewLayout
            // layoutManager.LoadSoundClipsForCurrentLayout(() => { });
        }

        void Start() {
            // #if UNITY_ANDROID
            // Screen.fullScreen = false;
            // #endif

            myObjectSelection = GetComponent<SoundMarkerSelection>();
            myObjectSelection.selectionDelegate = this;

            canvasControl.canvasDelegate = this;
            originMarker = anchorWrapperTransform.GetComponentInChildren<OriginMarker>();

            arCoreDevice = GameObject.Find("ARCore Device");
            firstPersonCamera = arCoreDevice.gameObject.transform.GetChild(0).GetComponent<Camera>();
            soundPlacement.SetCursorModelHidden(true);
        }

        public void Update() {
            UpdateApplicationLifecycle();
        }


        /// <summary>
        /// </summary>
        public void SetSoundMarkerRadiusUIParentToCursor() {
            objectSelection.SetSelectionRadiusParent(cursorTransform);
        }

        /// <summary>
        /// Call when reset or starting scene
        /// </summary>
        private void CreateOriginMarkerAtCameraPosition() {
            if (originMarker != null) {
                Destroy(originMarker.transform.parent.gameObject);
            }
            originMarker = OriginMarker.CreatePrefab(firstPersonCamera.transform, originMarkerPrefab, anchorWrapperTransform);
        }

        /// <summary>
        /// </summary>
        /// <param name="soundObj"></param>
        /// <param name="removeFromList"></param>
        /// <param name="eraseHotspotData"></param>
        public void DeleteAndDestroySoundMarker(SoundMarker soundObj, bool removeFromList = true, bool eraseHotspotData = true) {
            if (soundObj == null) { return; }

            // TODO: Implement object pooling
            if (removeFromList) {
                soundObj.markerDelegate = null;
                soundMarkers.Remove(soundObj);
            }
            if (eraseHotspotData) layoutManager.EraseHotspot(soundObj.hotspot);

            if (soundObj.transform.parent != null) {
                Destroy(soundObj.transform.parent.gameObject);
            } else {
                Destroy(soundObj);
            }
        }

        /// <summary>
        /// Remove all the sound markers from the scene
        /// </summary>
        private void UnloadCurrentSoundMarkers() {
            foreach (SoundMarker s in soundMarkers) {
                DeleteAndDestroySoundMarker(s, removeFromList: false, eraseHotspotData: false);
            }
            soundMarkers.Clear();
        }

        /// <summary>
        /// Place the next sound marker
        /// </summary>
        private void PlaceSoundTapped() {
            // Place a new sound with default config
            SoundMarker soundIconObj = null;
            // Create and position the prefab.
            if (canvasControl.placeSoundsOverlay.placeSoundsOnCursor && cursorTransform != null) {
                soundIconObj = SoundMarker.CreatePrefab(cursorTransform, soundMarkerPrefab, anchorWrapperTransform);
            } else {
                soundIconObj = SoundMarker.CreatePrefab(firstPersonCamera.transform, soundMarkerPrefab, anchorWrapperTransform);
            }
            soundIconObj.markerDelegate = this;
            Anchor anchorParent = soundIconObj.transform.parent.GetComponent<Anchor>();
            Vector3 anchorPos = anchorParent.transform.localPosition;

            // Create a new hotspot for the json file and save it.
            soundMarkers.Add(soundIconObj);

            Hotspot h = layoutManager.AddNewHotspot(
              localPos: anchorPos,
              rotation: Vector3.zero,
              minDist: defaultMinDistance * 0.5f,
              maxDist: canvasControl.placeSoundsOverlay.maxRadiusSlider.radiusValue);
            layoutManager.Bind(soundIconObj, h, !playbackIsStopped, reloadSoundClips: false);

            soundIconObj.SetIconAndRangeToRandomValue();

        }

        /// <summary>
        /// Load all the sounds after the camera has been moved
        /// </summary>
        /// <returns></returns>
        void ReloadSoundsRelativeToCamera() {

            anchorWrapperTransform.position = firstPersonCamera.transform.position;
            float camYRot = firstPersonCamera.transform.rotation.eulerAngles.y;
            anchorWrapperTransform.rotation = Quaternion.Euler(0, camYRot, 0);

            CreateOriginMarkerAtCameraPosition();

            LoadLayoutData();
        }

        /// <summary>
        /// Update the scene data and place the corresponding sources
        /// </summary>
        public void LoadLayoutData() {
            // remove the previous
            UnloadCurrentSoundMarkers();
            // set up all the hotspots
            layoutManager.LoadCurrentLayout();
            // Bind all the sounds to their game objects
            foreach (Hotspot h in layoutManager.layout.hotspots) {
                var pf = SoundMarker.CreatePrefab(
                  anchorWrapperTransform.TransformPoint(h.positon),
                  h.rotation, soundMarkerPrefab, anchorWrapperTransform);
                SoundMarker soundObj = pf.GetComponent<SoundMarker>();
                layoutManager.Bind(soundObj, h, !playbackIsStopped, reloadSoundClips: false);

                pf.markerDelegate = this;
                soundMarkers.Add(pf);
            }

            OnDemandActiveWasChanged(GetCurrentLayout().onDemandActive);
        }

        /// <summary>
        /// Update the scene data and place the corresponding sources
        /// </summary>
        private void UpdateApplicationLifecycle() {
            // Make sure screen doesn't timeout.
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            if (isQuitting) { return; } // Arleady triggered exit

            if (arCoreSessionStatus != Session.Status) {
                // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
                if (Session.Status == SessionStatus.ErrorPermissionNotGranted) {
                    Messages.ShowMessage("Camera permission is needed to run this application.");
                    isQuitting = true;
                    Invoke("DoQuit", 1.0f);
                } else if (Session.Status.IsError()) {
                    Messages.ShowMessage("ARCore encountered a problem connecting.  Please start the app again.");
                    isQuitting = true;
                    Invoke("DoQuit", 1.0f);
                }
            }

            // Store the last returned Session.Status so we know if the status when the status changes
            arCoreSessionStatus = Session.Status;
        }

        /// <summary>
        /// Quit the application.
        /// </summary>
        private void DoQuit() {
            Application.Quit();
        }

        // ------------------------------------------------------
        // ISoundMarkerDelegate functions...
        // ------------------------------------------------------

        private bool atLeastOneMarkerIsInTriggerRange(IEnumerable<SoundMarker> syncedMarkers) {
            foreach (SoundMarker tmpMarker in syncedMarkers) {
                if (tmpMarker.userIsInsideTriggerRange) {
                    return true;
                }
            }
            return false;
        }

        private bool atLeastOneSyncedMarkerClipShouldBeLoaded(IEnumerable<SoundMarker> syncedMarkers) {
            if (syncedMarkers == null) { return false; }

            foreach (SoundMarker syncedMarker in syncedMarkers) {
                if (syncedMarker.onDemandAudioShouldBeLoaded) { return true; }
            }

            return false;
        }

        #region ISoundMarkerDelegate
        // - - - - -

        public void soundMarkerDebugLog(string debugStr) {
            if (canvasControl.activeScreen == CanvasController.CanvasUIScreen.EditSound) {
                canvasControl.editSoundOverlay.debugText.text = debugStr;
            }
        }

        // --------------------------------------------------------------

        public void OnDemandActiveWasChanged(bool onDemandIsActive) {
            Debug.Log("OnDemandActiveWasChanged active: " + onDemandIsActive);
            if (onDemandIsActive) {
                RefreshLoadStateForSoundMarkers(() => {
                    canvasControl.mainScreen.LayoutChanged(
                        layoutManager.layout,
                        layoutManager.loadedAudioClipCount,
                        layoutManager.soundDictionary.Count - 1);
                });
            } else {
                // TODO: Load all AudioClips into memory
                layoutManager.LoadAllAudioClipsIntoMemory(MainController.soundMarkers, 
                completion: () => {
                    canvasControl.mainScreen.LayoutChanged(
                        layoutManager.layout,
                        layoutManager.loadedAudioClipCount,
                        layoutManager.soundDictionary.Count - 1);
                });
            }
        }

        public void loadOnDemandAudioForSoundMarker(SoundMarker marker, SoundFile soundFile) {
            if (!onDemandIsActive) { return; }

            Debug.Log("loadOnDemandAudioForSoundMarker: " + soundFile.filename);
            layoutManager.LoadSoundMarkerAndSyncedClips(marker, completion:
                (HashSet<SoundMarker> loadedMarkers) => {
                    Debug.LogWarning(   "loadOnDemandAudioForSoundMarker COMPLETE!");
                    
                    canvasControl.editSoundOverlay.refreshDebugText();
                    // foreach (var loadedMarker in loadedMarkers) {
                        
                    // }
                });
        }
        public bool unloadOnDemandAudioForSoundMarkerIfAllowed(SoundMarker marker, SoundFile soundFile) {
            if (!onDemandIsActive) { return false; }

            // FIRST - make sure no other SoundMarkers are using the SoundFile
            IEnumerable<SoundMarker> otherMarkersUsingSoundFile = layoutManager.SoundMarkersUsingSoundFileID(
                soundFile.filename, 
                hotspotIDToExclude: marker.hotspot.id);
            foreach (SoundMarker otherMarker in otherMarkersUsingSoundFile) {
                if (otherMarker.onDemandAudioShouldBeLoaded) { return false; }
            }

            // UNLOAD Synced Marker friends if none of them need to be loaded
            IEnumerable<SoundMarker> syncedMarkers = layoutManager.layout.getSynchronisedMarkers(marker.hotspot.id);
            Debug.LogError("unloadOnDemandAudioForSoundMarkerIsPossible: " + marker.gameObject.name);

            if (syncedMarkers == null || !atLeastOneSyncedMarkerClipShouldBeLoaded(syncedMarkers)) {
                Debug.LogError ("   UNLOAD ON DEMAND!");
                // We should UNLOAD this AudioClip...
                layoutManager.UnloadSoundMarkerAndSyncedClips(marker, syncedMarkers);
                canvasControl.editSoundOverlay.refreshDebugText();
                
                return true;
            }

            return false;
        }

        // ^ OnDemandAudio
        // --------------------------------------------------------------

        public bool shouldSoundMarkerTriggerPlayback(SoundMarker marker) {
            IEnumerable<SoundMarker> syncedMarkers = layoutManager.layout.getSynchronisedMarkers(marker.hotspot.id);
            if (syncedMarkers == null) { return true; }

            /*
                There is at least 1 Synchronised SoundMarker
                 - Check if ANY of synced markers are already playing (in user range)
                 - If NONE are in range, let's trigger their playback!
                 - Notify the calling SoundMarker if it should start it's own playback
             */

            bool atLeastOneSyncedMarkerIsInTriggerRange = atLeastOneMarkerIsInTriggerRange(syncedMarkers);
            if (!atLeastOneSyncedMarkerIsInTriggerRange) {
                // The FIRST SoundMarker that is in the synced collection, let's TRIGGER the others!
                foreach (SoundMarker tmpMarker in syncedMarkers) { tmpMarker.PlayAudioFromBeginning(ignoreTrigger: true); }
            }

            // Debug.Log(string.Format("START syncedMarkers.Count: {0} - atLeastOneSyncedMarkerIsInTriggerRange: {1}", 
            //     syncedMarkers.Count(), atLeastOneSyncedMarkerIsInTriggerRange ? "true" : "false"));

            // Start playing if 'atLeastOne...' is NOT in range
            return !atLeastOneSyncedMarkerIsInTriggerRange;
        }

        public bool shouldSoundMarkerStopPlaybackAfterUserLeftTriggerRange(SoundMarker marker) {
            IEnumerable<SoundMarker> syncedMarkers = layoutManager.layout.getSynchronisedMarkers(marker.hotspot.id);
            if (syncedMarkers == null) { return true; }

            /*
                There is at least 1 Synchronised SoundMarker
                 - Check if ANY of synced markers are already playing (in user range)
                 - If NONE are in range, let's STOP their playback!
                 - Notify the calling SoundMarker if it should stop it's own playback
             */

            bool atLeastOneSyncedMarkerIsInTriggerRange = atLeastOneMarkerIsInTriggerRange(syncedMarkers);
            if (!atLeastOneSyncedMarkerIsInTriggerRange) {
                // The LAST SoundMarker that is in the synced collection, let's STOP the others!
                foreach (SoundMarker tmpMarker in syncedMarkers) { tmpMarker.StopAudioPlayback(); }
            }

            // Debug.Log(string.Format("STOP syncedMarkers.Count: {0} - atLeastOneSyncedMarkerIsInTriggerRange: {1}",
            //     syncedMarkers.Count(), atLeastOneSyncedMarkerIsInTriggerRange ? "true" : "false"));

            // Stop playing if 'atLeastOne...' is NOT in range
            return !atLeastOneSyncedMarkerIsInTriggerRange;
        }

        // ------------------------------------------------------
        #endregion
        #region ICanvasControllerDelegate
        // ------------------------------------------------------

        /// <summary>
        /// Resets the camera, which basically restarts AR Core.s
        /// </summary>
        public void ResetCameraTapped() {
            // Debug.Log("DONE");
            ReloadSoundsRelativeToCamera();
        }

        public void PlaybackStateChanged(bool playbackIsStopped) {
            foreach (SoundMarker marker in soundMarkers) {
                marker.userHasHeardSound = false;
                if (playbackIsStopped) {
                    marker.StopAudioPlayback();
                } else {
                    marker.PlayAudioFromBeginning(ignoreTrigger: true);
                }
            }
        }

        /// <summary>
        /// handles switching some of the GameObject states after a canvas change
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="oldCanvas"></param>
        public void CanvasBecameActive(CanvasController.CanvasUIScreen canvas, CanvasController.CanvasUIScreen oldCanvas) {
            if (canvas == CanvasController.CanvasUIScreen.AddSounds) {
                soundPlacement.SetCursorModelHidden(false);
                SetSoundMarkerRadiusUIParentToCursor();
                objectSelection.setShape(SoundShape.Sphere);
                objectSelection.SetSelectionMinRadiusVisible(false);
                objectSelection.SetSelectionMaxRadiusVisible(true);
                objectSelection.SetSelectionRadiusColor(Color.white);
            } else if (canvas == CanvasController.CanvasUIScreen.EditSound) {
                soundPlacement.SetCursorModelHidden(true);
            } else if (canvas == CanvasController.CanvasUIScreen.Main) {
                soundPlacement.SetCursorModelHidden(true);
                objectSelection.SetSelectionMinRadiusVisible(false);
                objectSelection.SetSelectionMaxRadiusVisible(false);
                
                canvasControl.mainScreen.UpdateMarkerCountLabel(
                    MainController.soundMarkers.Count, 
                    layoutManager.loadedAudioClipCount, 
                    layoutManager.soundDictionary.Count - 1);
            }
        }

        public void DeleteSoundMarker(SoundMarker soundObj) {
            // Make sure the SelectionRadius won't be deleted
            SetSoundMarkerRadiusUIParentToCursor();
            DeleteAndDestroySoundMarker(soundObj);
        }

        // newPosition is the world position
        public void ChangePositionOfSoundMarker(SoundMarker soundObj, Vector3 newPosition) {
            soundObj.ChangePosition(newPosition, anchorWrapperTransform);
        }

        public void CurrentLayoutWasRenamed(string layoutName) {
            layoutManager.layout.layoutName = layoutName;
            layoutManager.layout.Save();
        }

        public void NewLayout() {
            layoutManager.currentLayoutId = layoutManager.NextLayoutId();
            LoadLayoutData();
        }


        public void LoadLayout(Layout layout) {
            layoutManager.currentLayoutId = layout.id;
            LoadLayoutData();

        }

        public void DuplicateLayout(Layout layout) {
            layoutManager.DuplicateLayout(layout);
            LoadLayoutData();
        }

        public void DeleteLayout(Layout layout) {
            layoutManager.DeleteLayout(layout);
            LoadLayoutData();
        }

        public Layout GetCurrentLayout() {
            return layoutManager.layout;
        }

        public void BindSoundFile(SoundFile sf) {
            if (SoundMarkerIsSelected()) {
                layoutManager.Bind(objectSelection.selectedMarker, sf, reloadSoundClips: true);
            } else {
                // Another use case?
                Debug.Log("NO SSO SELECTD");
            }
        }

        public void SelectSoundMarker(SoundMarker sso) {
            objectSelection.SetSelectedSoundMarker(sso);
        }

        public bool SoundMarkerIsSelected() {
            return myObjectSelection.selectedMarker != null;
        }

        public void ReloadSoundFiles(System.Action completion) {
            layoutManager.ReloadSoundFiles(completion);
        }

        public List<SoundFile> AllSoundFiles() {
            return layoutManager.AllSoundFiles();
        }

        public void LoadClipInSoundFile(SoundFile soundFile, System.Action<SoundFile> completion) {
            layoutManager.LoadClipInSoundFile(soundFile, completion);
        }

        public void RefreshLoadStateForSoundMarkers(System.Action completion) {
            if (!onDemandIsActive) {
                completion();
                return;
            }

            layoutManager.RefreshLoadStateForSoundMarkers(MainController.soundMarkers,
                () => {
                    canvasControl.editSoundOverlay.refreshDebugText();
                    completion();
                });
        }

        // public void LoadSoundClipsExclusivelyForCurrentLayout(System.Action completion) {
        //     layoutManager.LoadSoundClipsExclusivelyForCurrentLayout(completion);
        // }

        // ------------------------------------------------------
        #endregion
        #region ICanvasCreateSoundsDelegate
        // ------------------------------------------------------

        public void SoundPlacementModeChanged(bool isOnCursorOtherwiseDevice) {
            soundPlacement.SetCursorModelHidden(!isOnCursorOtherwiseDevice);

            // objectSelection.setSelectionMinRadiusVisible(isOnCursorOtherwiseDevice);
            objectSelection.SetSelectionMinRadiusVisible(false);
            objectSelection.SetSelectionMaxRadiusVisible(isOnCursorOtherwiseDevice);
        }

        public void CreateSoundButtonClicked() {
            PlaceSoundTapped();
        }

        public void CreateSoundsMaxRadiusSliderValueChanged(float radiusVal, float adjustedRadius) {
            objectSelection.SetSelectionMaxRadius(adjustedRadius);

            if (objectSelection.selectedMarker == null) return;
            objectSelection.selectedMarker.SetSoundMaxDistance(adjustedRadius);
        }

        // ------------------------------------------------------
        #endregion
        // ------------------------------------------------------
        #region ICanvasEditSoundDelegate
        // ------------------------------------------------------

        public void EditSoundMinRadiusSliderValueChanged(float radiusVal, float adjustedRadius) {
            objectSelection.SetSelectionMinRadius(adjustedRadius);

            if (objectSelection.selectedMarker == null) return;
            if (objectSelection.selectedMarker.SetSoundMinDistance(adjustedRadius)) {
                // Update the editSound MaxRadiusSlider
                canvasControl.editSoundOverlay.SetMaxRadiusSliderDistanceValue(objectSelection.selectedMarker.soundMaxDist);
            }
        }

        public void EditSoundMaxRadiusSliderValueChanged(float radiusVal, float adjustedRadius) {
            objectSelection.SetSelectionMaxRadius(adjustedRadius);

            if (objectSelection.selectedMarker == null) return;
            if (objectSelection.selectedMarker.SetSoundMaxDistance(adjustedRadius)) {
                // Update the editSound MinRadiusSlider
                canvasControl.editSoundOverlay.SetMinRadiusSliderDistanceValue(objectSelection.selectedMarker.soundMinDist);
            }

        }

        // ------------------------------------------------------
        #endregion
        // ------------------------------------------------------
        #region IObjectSelection
        // ------------------------------------------------------

        public void ObjectSelectionSoundSourceIconSelected(SoundMarker sso) {
            if (sso != null) {
                objectSelection.setShape(sso.soundShape);
                objectSelection.SetSelectionRadiusColor(sso.color);
            }
            canvasControl.ObjectSelectionSoundSourceIconSelected(sso);
            canvasControl.SetCanvasScreenActive(CanvasController.CanvasUIScreen.EditSound);
        }

        public void ObjectSelectionEmptySpaceTapped(bool shouldDeselect) {
            if (shouldDeselect) {
                SetSoundMarkerRadiusUIParentToCursor();
                canvasControl.SetCanvasScreenActive(CanvasController.CanvasUIScreen.Main);
            } else {
                if (canvasControl.activeScreen == CanvasController.CanvasUIScreen.AddSounds) {
                    CreateSoundButtonClicked();
                }
            }
        }

        public bool ObjectShouldDeselectAllSounds() {
            // if (canvasControl.activeScreen != CanvasController.CanvasUIScreen.AddSounds &&
            //     canvasControl.activeScreen != CanvasController.CanvasUIScreen.EditSound) {
            //     return true;
            // }
            return canvasControl.activeScreen == CanvasController.CanvasUIScreen.EditSound;
        }
        // ------------------------------------------------------
        #endregion
        #region ILayoutManagerDelegate
        // ------------------------------------------------------

        public void LayoutManagerLoadedNewLayout(LayoutManager manager, Layout newLayout) {
            // LoadSoundClipsExclusivelyForCurrentLayout(() => { });
            canvasControl.mainScreen.LayoutChanged(newLayout, manager.loadedAudioClipCount, manager.soundDictionary.Count);
        }
        public void LayoutManagerHotspotListChanged(LayoutManager manager, Layout layout) {
            canvasControl.mainScreen.LayoutChanged(layout, manager.loadedAudioClipCount, manager.soundDictionary.Count);
        }

        public void LayoutManagerLoadedAudioClipsChanged(LayoutManager manager, int hotspotCount) {
            Debug.Log("MainController::LoadedAudioClipsChanged. loading: " + manager.loadingAudioClipCount 
                    + " loaded: " + manager.loadedAudioClipCount 
                    + ", AllClipCount: " + manager.soundDictionary.Count);
            canvasControl.mainScreen.UpdateMarkerCountLabel(hotspotCount, manager.loadedAudioClipCount, manager.soundDictionary.Count);
        }

        public void StartCoroutineOn(IEnumerator e) {
            StartCoroutine(e);
        }
        
        #endregion
        // ------------------------------------------------------
    }
}