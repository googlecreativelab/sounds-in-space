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
using UnityEngine;
using UnityEngine.UI;

namespace SIS {
    public class CanvasSoundMarkerList : CanvasListBase<SoundMarker> {
        public override CanvasController.CanvasUIScreen canvasID { get { return CanvasController.CanvasUIScreen.SoundMarkerList; } }
        public GameObject createSoundPrompt = null;
        public Button deleteButton = null;

        override public void CanvasWillAppear() {
            data = MainController.soundMarkers;
            data.Sort((a, b) => { return string.Compare(a.name, b.name); });
            base.CanvasWillAppear();

            createSoundPrompt.SetActive(data.Count <= 0);
            SetCanvasTitle(string.Format("{0} Sound Markers", data.Count));

            UpdateBTNStates();
        }

        public override void CellClicked(CanvasListCell<SoundMarker> listCell, SoundMarker datum) {
            base.CellClicked(listCell, datum);

            UpdateBTNStates();
        }

        public void DeleteButtonClicked() {
            if (selectedCell == null) { return; }

            data.Remove(selectedCell.datum);
            DeleteCell(selectedCell);
            canvasDelegate?.DeleteSoundMarker(selectedCell.datum);

            // Update the title
            SetCanvasTitle(string.Format("{0} Sound Markers", data.Count));

            // bring back the prompt if there are no data elements remaining
            createSoundPrompt.SetActive(data.Count <= 0);

            selectedCell = null;
            UpdateBTNStates();
        }

        private void UpdateBTNStates() {
            confirmButton.interactable = (selectedCell != null);
            deleteButton.interactable = (selectedCell != null);
            deleteButton.gameObject.SetActive(selectedCell != null);
        }

        public void PlusButtonClicked() {
            BackButtonClicked();
            canvasDelegate?.NewSoundMarkerButtonClickedInSoundMarkerList();
        }

    }
}