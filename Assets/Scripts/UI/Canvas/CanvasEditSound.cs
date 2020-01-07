//-----------------------------------------------------------------------
// <copyright file="CanvasEditSound.cs" company="Google">
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
using UnityEngine;
using DG.Tweening;

namespace SIS {
    public interface ICanvasEditSoundDelegate {
        SoundMarkerSelection objectSelection { get; }
        // Transform cursorTransform { get; }
        SoundPlacement soundPlacement { get; }

        // Change the Anchored Hotspot location of a SoundMarker
        void ChangePositionOfSoundMarker(SoundMarker soundObj, Vector3 newPosition);
        void DeleteSoundMarker(SoundMarker soundObj);
        void EditSoundMinRadiusSliderValueChanged(float radiusVal, float adjustedRadius);
        void EditSoundMaxRadiusSliderValueChanged(float radiusVal, float adjustedRadius);
        void BackButtonClicked(CanvasController.CanvasUIScreen fromScreen);
        void SoundFileButtonClicked();
        void SyncPlaybackButtonClicked();
        void ResetCameraBTNClicked();
        void PlaceNewSoundsButtonClickedFromSoundEdit();
        
        // Synced Markers
        System.Collections.Generic.HashSet<string> SynchronisedMarkerIDsWithMarkerID(string markerID);
        void RemoveAnySynchronisationWithOtherMarkers(string markerID);
    }

    public class CanvasEditSound : CanvasBase, ISoundRadiusSliderDelegate, IInputFieldExtensionDelegate {
        public ICanvasEditSoundDelegate canvasDelegate = null;
        public override CanvasController.CanvasUIScreen canvasID { get { return CanvasController.CanvasUIScreen.EditSound; } }

        [SerializeField] EditSoundPanel _bottomPanel;

        [SerializeField] UnityEngine.UI.Button topBackButton = null;
        [SerializeField] UnityEngine.UI.Button placeNewSoundButton = null;

        
        [SerializeField] UnityEngine.UI.Button soundSrcButton = null;
        [SerializeField] UnityEngine.UI.Text soundLabelResizeText = null;
        [SerializeField] InputFieldExtension soundNameInputField = null;

        [SerializeField] UnityEngine.UI.Image topGradientImage = null;
        bool topGradientActive = true;

        [SerializeField] UnityEngine.UI.Image repositionImage = null;
        [SerializeField] UnityEngine.UI.Image soundShapeImage = null;
        [SerializeField] UnityEngine.UI.Image soundAppearanceImage = null;

        [SerializeField] SoundRadiusSlider minRadiusSlider = null;
        [SerializeField] SoundRadiusSlider maxRadiusSlider = null;

        [SerializeField] UnityEngine.UI.Button confirmRepositionButton = null;

        // [SerializeField] UnityEngine.UI.ScrollRect settingsScrollview = null;
        [SerializeField] UnityEngine.UI.Toggle triggerPlaybackToggle = null;
        [SerializeField] UnityEngine.UI.Toggle loopAudioToggle = null;
        [SerializeField] UnityEngine.UI.Toggle playOnceToggle = null;
        [SerializeField] UnityEngine.UI.Text soundFilenameText = null;

        [SerializeField] UnityEngine.UI.Button syncPlaybackButton = null;
        [SerializeField] UnityEngine.UI.Text syncSubtitleText = null;

        [SerializeField] UnityEngine.UI.Slider pitchSlider = null;
        [SerializeField] UnityEngine.UI.Slider volumeSlider = null;
        [SerializeField] UnityEngine.UI.Slider freqCutoffSlider = null;
        [SerializeField] UnityEngine.UI.Slider phaserSlider = null;
        [SerializeField] UnityEngine.UI.Slider distortionSlider = null;
        [SerializeField] UnityEngine.UI.Slider echoSlider = null;

        public UnityEngine.UI.Text debugText = null;

        // -----------------------------------------------

        void Awake() {
            soundNameInputField.inputDelegate = this;
        }

        // Start is called before the first frame update
        void Start() {
            minRadiusSlider.sliderDelegate = this;
            maxRadiusSlider.sliderDelegate = this;
        }

        public override void CanvasWillAppear() {
            confirmRepositionButton.gameObject.SetActive(false);
            SetCanvasTitle("Edit Sound");

            setTopGradientState(active: false, animated: false);

            _bottomPanel.panelWillAppear();
        }

        // ------------------------------------------------

        private void updateSyncedMarkersUI(SoundMarker soundMarker) {
            // Syncronisation button subtitle
            syncSubtitleText.text = "Edit synchronised Sound Markers";
            if (canvasDelegate != null) {
                System.Collections.Generic.HashSet<string> syncedMarkers = canvasDelegate.SynchronisedMarkerIDsWithMarkerID(soundMarker.hotspot.id);
                if (syncedMarkers != null && syncedMarkers.Count > 1) {
                    syncSubtitleText.text = string.Format("Synced with {0} Sound Marker{1}",
                                            syncedMarkers.Count - 1, (syncedMarkers.Count == 2 ? "" : "s"));
                }
            }
        }

        public void SoundMarkerSelected(SoundMarker selectedSound) {
            // Change the InputField text
            soundNameInputField.text = selectedSound.hotspot.name;
            soundLabelResizeText.text = selectedSound.hotspot.name;

            // Change the 2D UI representation
            soundAppearanceImage.sprite = selectedSound.iconSprite;
            soundShapeImage.sprite = selectedSound.soundShapeSprite;

            // Set the trigger and loop toggles
            triggerPlaybackToggle.isOn = selectedSound.hotspot.triggerPlayback;
            playOnceToggle.isOn = selectedSound.hotspot.playOnce;
            loopAudioToggle.isOn = selectedSound.hotspot.loopAudio;
            
            bool loopAudioInteractable = selectedSound.hotspot.triggerPlayback;
            if (selectedSound.hotspot.playOnce) { loopAudioInteractable = false; }
            loopAudioToggle.interactable = loopAudioInteractable;

            SetTriggerVisualInteractiveState(loopAudioToggle, loopAudioInteractable, selectedSound.hotspot.loopAudio);

            pitchSlider.value = selectedSound.hotspot.pitchBend;
            volumeSlider.value = selectedSound.hotspot.soundVolume;

            // Filter values
            freqCutoffSlider.value = selectedSound.hotspot.freqCutoff;
            phaserSlider.value = selectedSound.hotspot.phaserLevel;
            distortionSlider.value = selectedSound.hotspot.distortion;
            echoSlider.value = selectedSound.hotspot.echoMagnitude;

            updateSyncedMarkersUI(selectedSound);

            // Change the colour of the UI
            updateUIColor(selectedSound.color, notifyDelegate: false);
            // Color newCol = selectedSound.color;
            // repositionImage.color = newCol;
            // soundAppearanceImage.color = newCol;
            // soundColorImage.color = newCol;

            // minRadiusSlider.SetColorTint(newCol);
            // maxRadiusSlider.SetColorTint(newCol);

            // UnityEngine.UI.ColorBlock cols = soundSrcButton.colors;
            // cols.normalColor = newCol;
            // cols.highlightedColor = newCol.ColorWithBrightness(-0.15f);
            // cols.pressedColor = newCol.ColorWithBrightness(-0.3f);
            // soundSrcButton.colors = cols;

            minRadiusSlider.SetSliderRadius(selectedSound.soundMinDist, notifyDelegate: false);
            maxRadiusSlider.SetSliderRadius(selectedSound.soundMaxDist, notifyDelegate: false);

            if (selectedSound.hotspot.soundFile.isDefaultSoundFile) {
                soundFilenameText.text = "Tap to change sound";
            } else {
                soundFilenameText.text = "\"" + selectedSound.hotspot.soundFile.filenameWithExtension + "\"";
            }

            int charLimit = 21;
            int charsOver = soundFilenameText.text.Length - charLimit;
            float percentOverCharLimit = (charsOver > 0) ? (charsOver / 8f) : 0;
            soundFilenameText.fontSize = 36 - (int)(8 * percentOverCharLimit);

            debugText.text = selectedSound.userHasHeardSound ? "User HAS heard" : "NOT heard";
        }

        public void SetMinRadiusSliderDistanceValue(float dist) {
            minRadiusSlider.SetSliderRadius(dist);
        }

        public void SetMaxRadiusSliderDistanceValue(float dist) {
            maxRadiusSlider.SetSliderRadius(dist);
        }

        // ------------------------------------------------

        public void OnSoundNameTextfieldSelect(UnityEngine.EventSystems.BaseEventData eventData) {

        }

        #region IInputFieldExtensionDelegate

        public void InputFieldWasSelected(InputFieldExtension inputField) {
            if (inputField == soundNameInputField) {
                // Animate the bottom panel up based on the height of the keyboard...
                Debug.Log("InputFieldExtension::OnSelect KB height: " + TouchScreenKeyboard.area.height);

                _bottomPanel.setIsVisibleAboveKeyboard();
            }
        }

        #endregion

        public void setTopGradientState(bool active, bool animated = true) {
            if (topGradientActive == active) { return; }

            Color col = new Color(1, 1, 1, active ? 1 : 0);
            if (animated) {
                topGradientImage.DOColor(col, 0.35f);
            } else {
                topGradientImage.color = col;
            }
            topGradientActive = active;
        }

        // --------------------------------------
        #region Settings ScrollView Functions

        public void OnBeginDrag(UnityEngine.EventSystems.BaseEventData eventData) {
            UnityEngine.EventSystems.PointerEventData pointerData = eventData as UnityEngine.EventSystems.PointerEventData;
            if (pointerData != null) {
                _bottomPanel.bottomScrollViewStartedDragging(pointerData.position.y);
            }
            
        }

        public void ScrollViewMoved(Vector2 offset) {
            setTopGradientState(offset.y < 0.9f);
            _bottomPanel.bottomScrollViewYMoved(offset.y);
        }

        public void OnEndDrag(UnityEngine.EventSystems.BaseEventData eventData) {
            UnityEngine.EventSystems.PointerEventData pointerData = eventData as UnityEngine.EventSystems.PointerEventData;
            if (pointerData != null) {
                _bottomPanel.bottomScrollViewEndedDragging(pointerData.position.y);
            }
        }

        #endregion
        // --------------------------------------
        #region Textfield Callbacks

        public void SoundNameTextfieldChanged(string str) {
            soundLabelResizeText.text = str;
        }

        public void SoundNameTextfieldFinishedEditing(string str) {
            _bottomPanel.SetBottomPanelState(EditSoundPanel.Visibility.Mini, animated: true, delay: 0, 
                                                Ease.InOutExpo, bottomMargin: 0, animDuration: 0.1f);
            // UpdateBottomPanel(animated: true, delay: 0, DG.Tweening.Ease.InOutExpo, bottomMargin: 0, animDuration: 0.1f);

            SoundMarker selectedSound = canvasDelegate.objectSelection.selectedMarker;
            if (selectedSound == null) { return; }
            selectedSound.hotspot.SetName(str);
        }

        #endregion

        // ------------------------------------------------

        private Color bgColorForToggle(bool interactable, bool isOn) {
            Color interactiveCol = ColorThemeData.Instance.interactionColor;
            Color onButNotInteractiveColor = new Color(interactiveCol.r, interactiveCol.g, interactiveCol.b, 0.25f);

            return isOn
                ? (interactable ? interactiveCol : onButNotInteractiveColor)
                : (interactable ? new Color(0.632f, 0.632f, 0.632f) : new Color(0.87f, 0.87f, 0.87f));
        }

        void AnimateToggle(UnityEngine.UI.Toggle toggle, bool isOn, float animDuration = 0.36f) {
            

            Color bgCol = bgColorForToggle(toggle.interactable, isOn);
            toggle.targetGraphic.DOColor(bgCol, animDuration);

            // Transform toggleKnob = toggle.targetGraphic.transform.GetChild(0);
            Transform toggleKnob = toggle.transform.GetChild(0).transform.GetChild(0);
            RectTransform rectTransform = toggleKnob.GetComponent<RectTransform>();
            rectTransform.DOAnchorPos3DX(endValue: isOn ? 32 : -32, duration: animDuration).SetEase(DG.Tweening.Ease.InOutExpo);

            // Debug.Log(rectTransform);
            // float toggleKnobX = toggleKnob.GetComponent<RectTransform>().anchoredPosition3D.x;
            // off=-32, on=32
            // toggleKnob.DOLocalMoveX(endValue: isOn ? 32 : -32, duration: animDuration)
            // .SetEase(Ease.InOutExpo);
            

            // rectTransform.anchoredPosition3D = new Vector3(isOn ? 32 : -32, 0, 0);
            // rectTransform.anchoredPosition = new Vector2(isOn ? 32 : -32, 0);
        }

        void SetTriggerVisualInteractiveState(UnityEngine.UI.Toggle toggle, bool interactable, bool isOn) {

            Color bgCol = bgColorForToggle(toggle.interactable, isOn);
            toggle.targetGraphic.color = bgCol;

            // UnityEngine.UI.Image knobImage = toggle.transform.GetChild(0).transform.GetChild(0).GetComponent<UnityEngine.UI.Image>();
            // knobImage.color = toggle.interactable ? Color.white : new Color(0.8f, 0.8f, 0.8f);
        }

        #region Trigger Callbacks

        public void TriggerPlaybackToggled(bool isOn) {
            if (canvasDelegate == null) { return; }
            SoundMarker selectedMarker = canvasDelegate.objectSelection.selectedMarker;
            if (selectedMarker == null && selectedMarker.hotspot != null) { return; }

            AnimateToggle(triggerPlaybackToggle, isOn); // Animate

            // - - - - - - - - - - -
            // Only allow loop to be turned off if trigger is on
            bool loopInteractable = (isOn == true);
            if (playOnceToggle.isOn) { loopInteractable = false; }

            bool loopIsOn = loopAudioToggle.isOn;
            loopAudioToggle.interactable = loopInteractable;
            if (loopAudioToggle.isOn == false && isOn == false && playOnceToggle.isOn == false) {
                loopIsOn = true;
            }
            loopAudioToggle.isOn = loopIsOn;
            SetTriggerVisualInteractiveState(loopAudioToggle, loopInteractable, loopIsOn);
            // - - - - - - - - - - -

            if (!isOn) {
                // Remove any involvement with synced markers
                canvasDelegate?.RemoveAnySynchronisationWithOtherMarkers(selectedMarker.hotspot.id);
                updateSyncedMarkersUI(selectedMarker);
            }

            // Save the data to the Hotspot
            selectedMarker.SetTriggerPlayback(isOn);
        }

        public void LoopAudioToggled(bool isOn) {
            // Debug.Log("LoopAudioToggled");
            if (canvasDelegate == null) { return; }
            SoundMarker selectedMarker = canvasDelegate.objectSelection.selectedMarker;
            if (selectedMarker == null && selectedMarker.hotspot != null) { return; }

            AnimateToggle(loopAudioToggle, isOn); // Animate

            // Save the data to the Hotspot
            selectedMarker.SetAudioShouldLoop(isOn);
        }

        public void PlayOnceToggled(bool isOn) {
            if (canvasDelegate == null) { return; }
            SoundMarker selectedMarker = canvasDelegate.objectSelection.selectedMarker;
            if (selectedMarker == null && selectedMarker.hotspot != null) { return; }

            AnimateToggle(playOnceToggle, isOn); // Animate

            // - - - - - - - - - - -
            // Only allow loop to be turned on if PlayOnce is off
            bool loopInteractable = (isOn == false);
            bool loopIsOn = loopAudioToggle.isOn;
            loopAudioToggle.interactable = loopInteractable;
            if (loopAudioToggle.isOn == true && isOn == true) {
                loopIsOn = false;
            } else if (loopAudioToggle.isOn == false && isOn == false && triggerPlaybackToggle.isOn == false) {
                loopIsOn = true;
            }

            loopAudioToggle.isOn = loopIsOn;
            SetTriggerVisualInteractiveState(loopAudioToggle, loopInteractable, loopIsOn);
            // - - - - - - - - - - -

            // Save the data to the Hotspot
            selectedMarker.SetPlayOnce(isOn);
        }

        #endregion
        #region Volume Slider Callback

        public void SoundVolumeSliderChanged(float newVal) {
            if (canvasDelegate == null) { return; }
            SoundMarker selectedMarker = canvasDelegate.objectSelection.selectedMarker;
            if (selectedMarker == null && selectedMarker.hotspot != null) { return; }

            selectedMarker.SetSoundVolume(newVal);
        }

        #endregion
        #region Pitch Slider Callback

        public void PitchSliderChanged(float newVal) {
            if (canvasDelegate == null) { return; }
            SoundMarker selectedMarker = canvasDelegate.objectSelection.selectedMarker;
            if (selectedMarker == null && selectedMarker.hotspot != null) { return; }

            selectedMarker.SetPitchBend(newVal);
        }

        #endregion
        #region FreqCutoff Slider Callback

        public void FreqCutoffSliderChanged(float newVal) {
            if (canvasDelegate == null) { return; }
            SoundMarker selectedMarker = canvasDelegate.objectSelection.selectedMarker;
            if (selectedMarker == null && selectedMarker.hotspot != null) { return; }

            selectedMarker.SetFrequencyCutoff(newVal);
        }

        #endregion
        #region Phaser Slider Callback
        
        public void PhaserSliderChanged(float newVal) {
            if (canvasDelegate == null) { return; }
            SoundMarker selectedMarker = canvasDelegate.objectSelection.selectedMarker;
            if (selectedMarker == null && selectedMarker.hotspot != null) { return; }

            selectedMarker.SetPhaserLevel(newVal);
        }

        #endregion
        #region Distortion Slider Callback

        public void DistortionSliderChanged(float newVal) {
            if (canvasDelegate == null) { return; }
            SoundMarker selectedMarker = canvasDelegate.objectSelection.selectedMarker;
            if (selectedMarker == null && selectedMarker.hotspot != null) { return; }

            selectedMarker.SetDistortion(newVal);
        }

        #endregion
        #region Echo Slider Callback

        public void EchoSliderChanged(float newVal) {
            if (canvasDelegate == null) { return; }
            SoundMarker selectedMarker = canvasDelegate.objectSelection.selectedMarker;
            if (selectedMarker == null && selectedMarker.hotspot != null) { return; }

            selectedMarker.SetEchoMagnitude(newVal);
        }

        #endregion

        private void updateUIColor(Color newCol, bool notifyDelegate = true) {
            
            repositionImage.color = newCol;
            soundShapeImage.color = newCol;
            soundAppearanceImage.color = newCol;
            minRadiusSlider.SetColorTint(newCol);
            maxRadiusSlider.SetColorTint(newCol);

            UnityEngine.UI.ColorBlock cols = soundSrcButton.colors;
            cols.normalColor = newCol;
            cols.highlightedColor = newCol.ColorWithBrightness(-0.15f);
            cols.pressedColor = newCol.ColorWithBrightness(-0.3f);
            soundSrcButton.colors = cols;

            if (!notifyDelegate) { return; }
            canvasDelegate.objectSelection.SetSelectionRadiusColor(newCol);
        }

        #region Button Callbacks

        public void SyncPlaybackButtonClicked() {
            // Debug.Log ("SyncPlaybackButtonClicked");
            if (canvasDelegate == null) { return; }
            canvasDelegate.SyncPlaybackButtonClicked();
        }

        public override void BackButtonClicked() {
            if (_bottomPanel.isFullscreen) {
                _bottomPanel.SetBottomPanelState(EditSoundPanel.Visibility.Mini, animated: true);
            } else {
                base.BackButtonClicked();
                debugText.text = "";

                if (canvasDelegate == null) { return; }
                canvasDelegate.BackButtonClicked(this.canvasID);
            }
        }

        public void MoreButtonClicked() {
            _bottomPanel.toggleBetweenFullscreenAndMini();
            // SetBottomPanelState((botPanelState == EditSoundPanel.Visibility.Fullscreen) 
            //                             ? EditSoundPanel.Visibility.Mini 
            //                             : EditSoundPanel.Visibility.Fullscreen, 
            //                                 animated: true);
        }

        public void SoundNameButtonClicked() {
            // TODO: This...
        }

        public void PlaceNewSoundsButtonClicked() {
            // TODO: ME!
            Debug.Log("placeNewSoundsButtonClicked");
            if (canvasDelegate == null) { return; }
            canvasDelegate.PlaceNewSoundsButtonClickedFromSoundEdit();
        }

        public void SoundSourceFileButtonClicked() {
            if (canvasDelegate == null) { return; }
            canvasDelegate.SoundFileButtonClicked();
        }

        public void RepositionSoundButtonClicked() {
            _bottomPanel.SetBottomPanelState(EditSoundPanel.Visibility.Hidden, animated: true);

            topBackButton.gameObject.SetActive(false);
            placeNewSoundButton.gameObject.SetActive(false);

            _bottomPanel.sliderWrapperRect.gameObject.SetActive(false);
            confirmRepositionButton.gameObject.SetActive(true);
            SetCanvasTitle("Reposition Sound");

            if (canvasDelegate == null) { return; }
            canvasDelegate.objectSelection.selectionEnabled = false;

            // Hide the CursorTransform's model
            canvasDelegate.soundPlacement.SetCursorModelHidden(true);

            // Add the currently selected SoundMarker visuals to the cursor
            canvasDelegate.objectSelection.ParentSelectedSoundIconToCursor(canvasDelegate.soundPlacement.cursorTransform);
            // canvasDelegate.soundPlacement.cursorTransform.gameObject.SetActive(true);
        }

        public void ConfirmRepositionButtonClicked() {
            _bottomPanel.SetBottomPanelState(EditSoundPanel.Visibility.Mini, animated: true);

            topBackButton.gameObject.SetActive(true);
            placeNewSoundButton.gameObject.SetActive(true);

            _bottomPanel.sliderWrapperRect.gameObject.SetActive(true);
            confirmRepositionButton.gameObject.SetActive(false);
            SetCanvasTitle("Edit Sound");

            if (canvasDelegate == null) { return; }
            canvasDelegate.objectSelection.selectionEnabled = true;

            // Change the Anchored Hotspot location of a SoundMarker
            canvasDelegate.ChangePositionOfSoundMarker(canvasDelegate.objectSelection.selectedMarker,
                                                       canvasDelegate.soundPlacement.cursorTransform.position);

            canvasDelegate.objectSelection.ReturnSelectedSoundIconFromCursor();
        }

        public void SoundShapeButtonClicked() {
            if (canvasDelegate == null) { return; }
            SoundMarker selectedSound = canvasDelegate.objectSelection.selectedMarker;
            if (selectedSound == null) { return; }

            selectedSound.SetToNextSoundShape();

            soundShapeImage.sprite = selectedSound.soundShapeSprite;

            canvasDelegate?.objectSelection.setShape(selectedSound.soundShape);
        }

        public void SoundAppearanceButtonClicked() {
            // Cycle through colors AND icon
            if (canvasDelegate == null) { return; }
            SoundMarker selectedSound = canvasDelegate.objectSelection.selectedMarker;
            if (selectedSound == null) { return; }

            // TODO: selectedSound.SetToRandomDifferentColor();
            // TODO: selectedSound.SetToRandomDifferentIcon();
            selectedSound.SetToNextColor();
            selectedSound.SetToNextIcon();

            soundAppearanceImage.sprite = selectedSound.iconSprite;
            updateUIColor(selectedSound.color);
        }

        void DeleteSelectedSound() {
            canvasDelegate?.DeleteSoundMarker(canvasDelegate.objectSelection.selectedMarker);
            canvasDelegate?.BackButtonClicked(fromScreen: this.canvasID);
        }

        static string DeleteString = "Delete";

        public void SoundDeleteButtonClicked() {

#if !UNITY_EDITOR && UNITY_ANDROID
            DeleteSelectedSound();
#else
            DeleteSelectedSound();
#endif
        }

        #endregion

        #region ISoundRadiusSliderDelegate

        public void SoundRadiusSliderValueChanged(SoundRadiusSlider slider, float sliderPercentage, float adjustedRadius) {

            if (canvasDelegate == null) { return; }

            if (slider == minRadiusSlider) {
                canvasDelegate.EditSoundMinRadiusSliderValueChanged(sliderPercentage, adjustedRadius);
            } else if (slider == maxRadiusSlider) {
                canvasDelegate.EditSoundMaxRadiusSliderValueChanged(sliderPercentage, adjustedRadius);
            }
        }
        #endregion
    }
}