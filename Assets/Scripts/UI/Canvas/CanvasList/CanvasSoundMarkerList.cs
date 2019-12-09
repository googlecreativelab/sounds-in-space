//-----------------------------------------------------------------------
// <copyright file="CanvasSoundMarkerList.cs" company="Google">
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
using UnityEngine.UI;

namespace SIS {
    
    public class CanvasSoundMarkerList : CanvasListBase<SoundMarker> {

        // ----------------------------------------------
        public enum Mode { FromMainMenu, SyncronisedMarkers }
        private Mode _mode = Mode.FromMainMenu;
        public Mode listMode { get { return _mode; } }

        private SoundMarker _selectedMarker = null;
        private string _selectedMarkerID { get { return _selectedMarker?.hotspot.id; } }
        private HashSet<string> _preselectedMarkerIDs = null;

        // ----------------------------------------------

        public override CanvasController.CanvasUIScreen canvasID { get { return CanvasController.CanvasUIScreen.SoundMarkerList; } }
        public GameObject createSoundPrompt = null;
        public Button deleteButton = null;

        public Button placeMarkersBTN = null;

        public UnityEngine.UI.Text subtitleText = null;
        public UnityEngine.UI.Text centerSubtitleText = null;

        [SerializeField] private Sprite _backSpriteLeft;
        [SerializeField] private Sprite _backSpriteBottom;

        public void setMode(Mode newMode, 
            SoundMarker selectedMarker = null, 
            HashSet<string> syncedMarkerIDs = null) {

            // if (selectedMarker != null) { Debug.Log ("setMode selectedMarker: " + selectedMarker); }

            _mode = newMode;
            canSelectMultipleCells = (newMode == Mode.SyncronisedMarkers);
            _selectedMarker = selectedMarker;

            if (syncedMarkerIDs != null) {
                _preselectedMarkerIDs = syncedMarkerIDs;
            }
        }

        private void updateTitleState() {
            if (_mode == Mode.FromMainMenu) {
                subtitleText.text = "Select & Tap 'Edit Marker'";
                centerSubtitleText.text = "Tap 'Place Markers'";
                SetCanvasTitle(string.Format("{0} Sound Marker{1}", data.Count, data.Count == 1 ? "" : "s"));

            } else if (_mode == Mode.SyncronisedMarkers) {
                subtitleText.text = "Select Multiple Markers";
                centerSubtitleText.text = "Create Other Markers";
                // SetCanvasTitle(string.Format("{0} Synced Marker{1}", data.Count, data.Count == 1 ? "" : "s"));
                SetCanvasTitle(string.Format("{1} Synced to '{0}'", _selectedMarker.hotspot.name, _selectedCells.Count));
            }
        }

        override public void CanvasWillAppear() {
            data = new List<SoundMarker>(MainController.soundMarkers);
            
            string curMarkerID = _selectedMarkerID;
            if (curMarkerID != null) { data.RemoveAll(s => { return s.hotspot.id == curMarkerID; }); }
            data.Sort((a, b) => { return string.Compare(a.name, b.name); });
            
            base.CanvasWillAppear(); // Creates the cells
            
            // Preselect cells
            if (_preselectedMarkerIDs != null) {
                foreach (CanvasListCell<SoundMarker> cell in currentCells) {
                    if (_preselectedMarkerIDs.Contains(cell.datum.hotspot.id)) {
                        base.CellClicked(cell, cell.datum);
                    }
                }
                _preselectedMarkerIDs = null;
            }

            createSoundPrompt.SetActive(data.Count <= 0);

            updateTitleState();
            UpdateBTNStates();
        }

        public override void CellClicked(CanvasListCell<SoundMarker> listCell, SoundMarker datum) {
            base.CellClicked(listCell, datum); // Changes the selection state

            // Select other cells that are also synchronised with this new cell
            if (_mode == Mode.SyncronisedMarkers && listCell.isSelected && canvasDelegate != null) {
                HashSet<string> syncedMarkerIDs = canvasDelegate.SynchronisedMarkerIDsWithMarkerID(datum.hotspot.id);
                if (syncedMarkerIDs != null) {
                    foreach (CanvasListCell<SoundMarker> cell in currentCells) {
                        if (cell.isSelected) { continue; } // Cell is already selected regardless
                        if (cell.datum.hotspot.id == datum.hotspot.id) { continue; } // This is the cell that was just clicked

                        if (syncedMarkerIDs.Contains(cell.datum.hotspot.id)) {
                            base.CellClicked(cell, cell.datum);
                        }
                    }
                }
            }

            updateTitleState();
            UpdateBTNStates();
        }

        public void DeleteButtonClicked() {
            if (_selectedCells.Count < 1) { return; }

            foreach (CanvasListCell<SoundMarker> cell in _selectedCells) {
                data.Remove(cell.datum);
                DeleteCell(cell);
                canvasDelegate?.DeleteSoundMarker(cell.datum);
            }

            // Update the title
            updateTitleState();

            // bring back the prompt if there are no data elements remaining
            createSoundPrompt.SetActive(data.Count <= 0);

            // selectedCell = null;
            deselectAllCells();
            UpdateBTNStates();
        }

        private void UpdateBTNStates() {
            bool atLeast1CellSelected = _selectedCells.Count > 0;
            confirmButton.interactable = atLeast1CellSelected;
            deleteButton.interactable = atLeast1CellSelected;

            deleteButton.gameObject.SetActive(_mode == Mode.FromMainMenu && atLeast1CellSelected);
            confirmButton.gameObject.SetActive(_mode == Mode.FromMainMenu);
            placeMarkersBTN.gameObject.SetActive(_mode == Mode.FromMainMenu);

            if (_mode == Mode.FromMainMenu) {
                backButton.GetComponent<Image>().sprite = _backSpriteLeft;
                backButton.GetComponentInChildren<Text>().text = "Back";
                
                RectTransform rect = backButton.GetComponent<RectTransform>();
                rect.anchorMax = new Vector2(0.33f, 0);
                rect.SetRight(0);

            } else if (_mode == Mode.SyncronisedMarkers) {
                backButton.GetComponent<Image>().sprite = _backSpriteBottom;
                backButton.GetComponentInChildren<Text>().text = "Done";

                RectTransform rect = backButton.GetComponent<RectTransform>();
                rect.anchorMax = new Vector2(1f, 0);
                rect.SetRight(0);
            }
        }

        public void PlusButtonClicked() {
            BackButtonClicked();
            canvasDelegate?.NewSoundMarkerButtonClickedInSoundMarkerList();
        }

    }
}