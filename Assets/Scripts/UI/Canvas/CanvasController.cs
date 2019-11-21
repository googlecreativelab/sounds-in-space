//-----------------------------------------------------------------------
// <copyright file="CanvasController.cs" company="Google">
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
using System.Collections.Generic;
using UnityEngine;

namespace SIS {
    public interface ICanvasControllerDelegate {
        SoundMarkerSelection objectSelection { get; }
        SoundPlacement soundPlacement { get; }

        void PlaybackStateChanged(bool playbackIsStopped);

        void DeleteSoundMarker(SoundMarker soundObj);
        // Change the Anchored Hotspot location of a SoundMarker
        void ChangePositionOfSoundMarker(SoundMarker soundObj, Vector3 newPosition);
        void CreateSoundButtonClicked();
        void SoundPlacementModeChanged(bool isOnCursorOtherwiseDevice);
        void ResetCameraTapped();
        void CanvasBecameActive(CanvasController.CanvasUIScreen canvas, CanvasController.CanvasUIScreen oldCanvas);

        void CreateSoundsMaxRadiusSliderValueChanged(float radiusVal, float adjustedRadius);
        void EditSoundMinRadiusSliderValueChanged(float radiusVal, float adjustedRadius);
        void EditSoundMaxRadiusSliderValueChanged(float radiusVal, float adjustedRadius);
        bool SoundMarkerIsSelected();

        void NewLayout();
        void LoadLayout(Layout layout);
        Layout GetCurrentLayout();
        void DeleteLayout(Layout layout);
        void DuplicateLayout(Layout layout);

        void BindSoundFile(SoundFile sf);
        void SelectSoundMarker(SoundMarker sso);
        void CurrentLayoutWasRenamed(string layoutName);

        List<SoundFile> AllSoundFiles();
        void ReloadSoundFiles(System.Action completion);

    }

    public class CanvasController : MonoBehaviour,
    ICanvasCreateSoundsDelegate, ICanvasMainMenuDelegate, ICanvasEditSoundDelegate,
    ICanvasListDelegate<Layout>, ICanvasListDelegate<SoundFile>, ICanvasListDelegate<SoundMarker> {
        public ICanvasControllerDelegate canvasDelegate = null;
        public SoundMarkerSelection objectSelection { get { return canvasDelegate != null ? canvasDelegate?.objectSelection : null; } }
        public SoundPlacement soundPlacement { get { return canvasDelegate != null ? canvasDelegate?.soundPlacement : null; } }

        public CanvasMainMenu mainScreen;
        public CanvasCreateSounds placeSoundsOverlay;
        public CanvasEditSound editSoundOverlay;
        public CanvasLayoutList layoutList;
        public CanvasSoundFileList soundFileList;
        public CanvasSoundMarkerList soundMarkerList;

        public enum CanvasUIScreen { Main, AddSounds, EditSound, LayoutList, SoundFileList, SoundMarkerList, None }
        private CanvasUIScreen _activeScreen = CanvasUIScreen.Main;
        public CanvasUIScreen activeScreen { get { return _activeScreen; } }
        public int activeScreenIndex { get { return (int)_activeScreen; } }

        // Start is called before the first frame update
        void Start() {
            mainScreen.gameObject.SetActive(true);
            placeSoundsOverlay.gameObject.SetActive(false);
            editSoundOverlay.gameObject.SetActive(false);
            layoutList.gameObject.SetActive(false);
            soundFileList.gameObject.SetActive(false);
            soundMarkerList.gameObject.SetActive(false);

            mainScreen.canvasDelegate = this;
            placeSoundsOverlay.canvasDelegate = this;
            editSoundOverlay.canvasDelegate = this;
            layoutList.canvasDelegate = this;
            soundFileList.canvasDelegate = this;
            soundMarkerList.canvasDelegate = this;
        }

        void Update() {
            CheckBackButton();
        }

        private void CheckBackButton() {
            // Go back one canvas when tapped
            if (Input.GetKeyDown(KeyCode.Escape)) {
                // call top-most back action
                BackButtonClicked(_activeScreen); // will use the active canvas
            }
        }


        public void SetCanvasScreenActive(int index) {
            SetCanvasScreenActive((CanvasUIScreen)index);
        }

        public void SetCanvasScreenActive(CanvasUIScreen screen) {
            if (Application.isPlaying) {
                switch (screen) {
                    case CanvasUIScreen.Main: mainScreen.CanvasWillAppear(); break;
                    case CanvasUIScreen.AddSounds: placeSoundsOverlay.CanvasWillAppear(); break;
                    case CanvasUIScreen.EditSound: editSoundOverlay.CanvasWillAppear(); break;
                    case CanvasUIScreen.LayoutList: layoutList.CanvasWillAppear(); break;
                    case CanvasUIScreen.SoundFileList: soundFileList.CanvasWillAppear(); break;
                    case CanvasUIScreen.SoundMarkerList: soundMarkerList.CanvasWillAppear(); break;
                    default: break;
                }
            }

            mainScreen.gameObject.SetActive(screen == CanvasUIScreen.Main);
            placeSoundsOverlay.gameObject.SetActive(screen == CanvasUIScreen.AddSounds);
            editSoundOverlay.gameObject.SetActive(screen == CanvasUIScreen.EditSound);
            layoutList.gameObject.SetActive(screen == CanvasUIScreen.LayoutList);
            soundFileList.gameObject.SetActive(screen == CanvasUIScreen.SoundFileList);
            soundMarkerList.gameObject.SetActive(screen == CanvasUIScreen.SoundMarkerList);
            canvasDelegate?.CanvasBecameActive(screen, _activeScreen);

            _activeScreen = screen;
        }

        public void ObjectSelectionSoundSourceIconSelected(SoundMarker icon) {
            editSoundOverlay.SoundMarkerSelected(icon);
        }

        #region ICanvasMainMenuDelegate

        public void SoundPlaybackBTNClicked(bool playbackIsStopped) {
            canvasDelegate?.PlaybackStateChanged(playbackIsStopped);
        }

        public void ResetCameraBTNClicked() {
            canvasDelegate?.ResetCameraTapped();
        }

        public void PlaceSoundsBTNClicked() {
            float defaultRadius = placeSoundsOverlay.maxRadiusSlider.minRadius;
            placeSoundsOverlay.maxRadiusSlider.SetSliderDiameter(defaultRadius);
            canvasDelegate?.EditSoundMaxRadiusSliderValueChanged(0, defaultRadius);

            SetCanvasScreenActive(CanvasUIScreen.AddSounds);
        }
        public void LoadLayoutBTNClicked() {
            SetCanvasScreenActive(CanvasUIScreen.LayoutList);
        }
        public void LoadSoundBTNClicked() {
            SetCanvasScreenActive(CanvasUIScreen.SoundMarkerList);
        }
        public void LoadSoundFileBTNClicked() {
            SetCanvasScreenActive(CanvasUIScreen.SoundFileList);
        }

        public void CurrentLayoutWasRenamed(string layoutName) {
            canvasDelegate?.CurrentLayoutWasRenamed(layoutName);
        }

        public Layout GetCurrentLayout() {
            if (canvasDelegate == null) { return null; }
            return canvasDelegate.GetCurrentLayout();
        }

        #endregion
        #region ICanvasCreateSoundsDelegate

        public void SoundPlacementModeChanged(bool isOnCursorOtherwiseDevice) {
            canvasDelegate?.SoundPlacementModeChanged(isOnCursorOtherwiseDevice);
        }

        public void CreateSoundButtonClicked() {
            canvasDelegate?.CreateSoundButtonClicked();
        }

        public void CreateSoundsMaxRadiusSliderValueChanged(float radiusVal, float adjustedRadius) {
            canvasDelegate?.CreateSoundsMaxRadiusSliderValueChanged(radiusVal, adjustedRadius);
        }

        #endregion
        #region ICanvasEditSoundDelegate

        public void EditSoundMinRadiusSliderValueChanged(float radiusVal, float adjustedRadius) {
            canvasDelegate?.EditSoundMinRadiusSliderValueChanged(radiusVal, adjustedRadius);
        }
        public void EditSoundMaxRadiusSliderValueChanged(float radiusVal, float adjustedRadius) {
            canvasDelegate?.EditSoundMaxRadiusSliderValueChanged(radiusVal, adjustedRadius);
        }

        public void DeleteSoundMarker(SoundMarker soundObj) {
            canvasDelegate?.DeleteSoundMarker(soundObj);
        }

        public void ChangePositionOfSoundMarker(SoundMarker soundObj, Vector3 newPosition) {
            canvasDelegate?.ChangePositionOfSoundMarker(soundObj, newPosition);
        }

        public void SoundFileButtonClicked() {
            SetCanvasScreenActive(CanvasUIScreen.SoundFileList);
        }

    public void PlaceNewSoundsButtonClickedFromSoundEdit() {
        objectSelection.ReturnSelectedSoundIconFromCursor();
        objectSelection.DeselectSound();

        PlaceSoundsBTNClicked();
    }

    #endregion
    #region ICanvasListDelegate


        public void ListCellClicked(CanvasListCell<Layout> listCell, Layout layout) {

        }
        public void ListCellClicked(CanvasListCell<SoundFile> listCell, SoundFile sf) {
            // !!! OCCURS ON ConfirmButtonClicked()
        }
        public void ListCellClicked(CanvasListCell<SoundMarker> listCell, SoundMarker sso) {
        }

        // -----------------------

        public void NewSoundMarkerButtonClickedInSoundMarkerList() {
            PlaceSoundsBTNClicked();
        }

        public void ConfirmButtonClicked(CanvasUIScreen fromScreen, CanvasListCell<Layout> currentSelectedCell) {
            canvasDelegate?.LoadLayout(currentSelectedCell.datum);
            BackButtonClicked(CanvasUIScreen.LayoutList);
        }

        public void ConfirmButtonClicked(CanvasUIScreen fromScreen, CanvasListCell<SoundFile> currentSelectedCell) {
            canvasDelegate?.BindSoundFile(currentSelectedCell.datum);
            BackButtonClicked(CanvasUIScreen.SoundFileList);
        }

        public void ConfirmButtonClicked(CanvasUIScreen fromScreen, CanvasListCell<SoundMarker> currentSelectedCell) {
            canvasDelegate?.SelectSoundMarker(currentSelectedCell.datum);
        }

        // -----------------------
        public void ReloadSoundFiles(System.Action completion) {
            canvasDelegate?.ReloadSoundFiles(completion);
        }

        public List<SoundFile> AllSoundFiles() {
            return canvasDelegate?.AllSoundFiles();
        }

        public void DeleteLayout(Layout layout) {
            canvasDelegate?.DeleteLayout(layout);
        }

        public void DuplicateLayout(Layout layout) {
            canvasDelegate?.DuplicateLayout(layout);
        }

        #endregion
        #region ICommonDelegateCallbacks

        public void BackButtonClicked(CanvasUIScreen fromScreen) {

            objectSelection.ReturnSelectedSoundIconFromCursor();
            objectSelection.DeselectSound();

            if (canvasDelegate == null) return;
            // Different case: If we were selecting a sound while editing a sound object, go to edit
            if (fromScreen == CanvasUIScreen.SoundFileList && canvasDelegate.SoundMarkerIsSelected()) {
                editSoundOverlay.SoundMarkerSelected(canvasDelegate?.objectSelection.selectedSound);
                SetCanvasScreenActive(CanvasUIScreen.EditSound);
            } else {
                // BASE CASE:
                SetCanvasScreenActive(CanvasUIScreen.Main);
            }
        }


        #endregion

        #region ICanvasLayoutListDelegate

        public void NewLayoutButtonClicked() {
            canvasDelegate?.NewLayout();
            BackButtonClicked(CanvasUIScreen.LayoutList);
        }

        #endregion

    }
}