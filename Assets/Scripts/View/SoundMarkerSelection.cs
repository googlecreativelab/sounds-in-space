//-----------------------------------------------------------------------
// <copyright file="SoundMarkerSelection.cs" company="Google">
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

// ---------------------------------------
// In Instant Preview, touch input on your app's AR scene does not automatically
// propagate input events to your Unity implementation.

// To set up touch input propagation, use the InstantPreviewInput class in any
// controller script that references the Unity Input class. Add the following
// code to the top of the controller script:
// https://developers.google.com/ar/develop/unity/instant-preview
#if UNITY_EDITOR
using Input = GoogleARCore.InstantPreviewInput;
#endif
// ---------------------------------------

namespace SIS {

    public interface IObjectSelectionDelegate {
        void ObjectSelectionSoundSourceIconSelected(SoundMarker icon);
        void ObjectSelectionEmptySpaceTapped(bool shouldDeselect);
        bool ObjectShouldDeselectAllSounds();
    }

    public class SoundMarkerSelection : MonoBehaviour {
        public LayerMask raycastLayerMask;

        public IObjectSelectionDelegate selectionDelegate = null;
        SoundRadius objSelectionRadius;
        public GameObject SoundRadiusPrefab;
        private SoundMarker selSound = null;
        public SoundMarker selectedSound { get { return selSound; } }

        [HideInInspector] public bool selectionEnabled = true;

        // Start is called before the first frame update
        void Start() {
            objSelectionRadius = GetComponentInChildren<SoundRadius>();
            objSelectionRadius.isMinHidden = true;
            objSelectionRadius.isMaxHidden = true;
        }

        private void CreateNewObjSelectionRadius() {
            objSelectionRadius = Instantiate(SoundRadiusPrefab, parent: transform).GetComponent<SoundRadius>();
            objSelectionRadius.transform.localScale = Vector3.one;
            objSelectionRadius.isMinHidden = true;
            objSelectionRadius.isMaxHidden = true;
        }

        #region Selection Radius

        public void SetSelectionMinRadiusVisible(bool isVisible) {
            if (objSelectionRadius == null) { CreateNewObjSelectionRadius(); }
            objSelectionRadius.isMinHidden = !isVisible;
        }
        public void SetSelectionMaxRadiusVisible(bool isVisible) {
            if (objSelectionRadius == null) { CreateNewObjSelectionRadius(); }
            objSelectionRadius.isMaxHidden = !isVisible;
        }

        public void SetSelectionRadiusParent(Transform transform) {
            if (objSelectionRadius == null) { CreateNewObjSelectionRadius(); }

            objSelectionRadius.transform.parent = transform;
            objSelectionRadius.transform.localPosition = Vector3.zero;
        }

        public void SetSelectionMaxRadius(float radius) {
            if (objSelectionRadius == null) { CreateNewObjSelectionRadius(); }
            objSelectionRadius.maxRadius = radius;
        }
        public void SetSelectionMinRadius(float radius) {
            if (objSelectionRadius == null) { CreateNewObjSelectionRadius(); }
            objSelectionRadius.minRadius = radius;
        }

        public void SetSelectionRadiusColor(Color col) {
            if (objSelectionRadius == null) { CreateNewObjSelectionRadius(); }
            objSelectionRadius.SetColor(col);
        }

        #endregion
        #region Reposition the selected sound

        public void ParentSelectedSoundIconToCursor(Transform cursorTransform) {
            if (selectedSound == null) { return; }

            selectedSound.SoundIcons.transform.parent = cursorTransform;
            selectedSound.SoundIcons.transform.localPosition = Vector3.zero;
            SetSelectionRadiusParent(cursorTransform);
        }

        public void ReturnSelectedSoundIconFromCursor() {
            if (selectedSound == null) { return; }

            selectedSound.SoundIcons.transform.parent = selectedSound.gameObject.transform;
            selectedSound.SoundIcons.transform.localPosition = Vector3.zero;
            SetSelectionRadiusParent(selectedSound.transform);
        }

        #endregion

        public void SetSelectedSoundMarker(SoundMarker sso) {
            if (objSelectionRadius == null) { CreateNewObjSelectionRadius(); }

            SetSelectionRadiusParent(sso.transform);
            objSelectionRadius.minRadius = sso.soundMinDist;
            objSelectionRadius.maxRadius = sso.soundMaxDist;
            objSelectionRadius.isMinHidden = false;
            objSelectionRadius.isMaxHidden = false;

            selSound = sso;
            if (selectionDelegate != null) { selectionDelegate.ObjectSelectionSoundSourceIconSelected(sso); }
        }

        public void SetSelectedSoundToNextColor() {
            if (selSound == null) { return; }
            selSound.SetToNextColor();
        }

        public void SetSelectedSoundToNextIcon() {
            if (selSound == null) { return; }
            selSound.SetToNextIcon();
        }

        public void DeselectSound() {
            objSelectionRadius.isMinHidden = true;
            objSelectionRadius.isMaxHidden = true;
            selSound = null;
        }

        // Update is called once per frame
        void Update() {

            if (Input.GetMouseButtonDown(0)) {
                if (!selectionEnabled) return;

                int pointerID = -1;
                if (Input.touchCount > 0) { pointerID = Input.GetTouch(0).fingerId; }

                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(pointerID)) {
                    return;
                }

                Ray raycast = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit raycastHit;

                bool soundIconSelected = false;
                // LayerMask layerMask = 1 << 9; // SoundIcons
                if (Physics.Raycast(raycast, out raycastHit, maxDistance: float.MaxValue, layerMask: raycastLayerMask)) {
                // if (Physics.Raycast(raycast, out raycastHit, layerMask)) {
                    SoundMarker soundMarker = raycastHit.collider.GetComponent<SoundMarker>();
                    if (soundMarker != null) {
                        // Debug.Log("Sound Marker: " + soundMarker);
                        SetSelectedSoundMarker(soundMarker);
                        soundIconSelected = true;
                    }
                }

                if (!soundIconSelected && selectionDelegate != null) {
                    bool shouldDeselect = selectionDelegate.ObjectShouldDeselectAllSounds();

                    if (shouldDeselect) {
                        if (objSelectionRadius == null) { CreateNewObjSelectionRadius(); }

                        DeselectSound();
                        Debug.Log("DESELECTING ALL SOUNDS");
                    }
                    selectionDelegate.ObjectSelectionEmptySpaceTapped(shouldDeselect);
                }
            }
        }
    }
}