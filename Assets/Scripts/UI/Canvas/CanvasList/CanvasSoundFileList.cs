//-----------------------------------------------------------------------
// <copyright file="CanvasSoundFileList.cs" company="Google">
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
using UnityEngine.UI;
using System.Linq;

namespace SIS {
    public class CanvasSoundFileList : CanvasListBase<SoundFile> {
        public override CanvasController.CanvasUIScreen canvasID { get { return CanvasController.CanvasUIScreen.SoundFileList; } }

        public ScrollRect scrollRect;
        public Button refreshButton;
        [UnityEngine.SerializeField] InputField searchTextfield;
        [UnityEngine.SerializeField] public Button clearSearchButton;

        private string _searchString = null;

        private void Awake() {

        }


        override public void CanvasWillAppear() {
            base.CanvasWillAppear();

            confirmButton.interactable = false;
            searchTextfield.text = "";
            _searchString = null;
            clearSearchButton.gameObject.SetActive(false);

            ReloadSoundFiles();
        }

        private void ReloadSoundFiles() {
            confirmButton.interactable = false;
            refreshButton.enabled = false;
            scrollRect.enabled = false;

            canvasDelegate?.ReloadSoundFiles(() => {
                refreshCellsUsingData();

                refreshButton.enabled = true;
                scrollRect.enabled = true;
            });
        }

        private void refreshCellsUsingData() {
            data = canvasDelegate?.AllSoundFiles();
            if (data != null) {
                if (_searchString != null && _searchString.Length > 0) {
                    data = data.Where(sf => sf.filenameWithExtension.ToLower().Contains(_searchString)).ToList();
                }
                data.Sort((a, b) => { return string.Compare(a.filenameWithExtension, b.filenameWithExtension); });
            }
            RefreshCells();
        }

        // ----------------------------------

        public void onSearchTextChanged(string searchString) {
            if (searchString == null) {
                clearSearchButton.gameObject.SetActive(false);
                _searchString = null;
            } else {
                _searchString = searchString.ToLower();
                clearSearchButton.gameObject.SetActive(_searchString.Length > 0);
            }

            // ReloadSoundFiles();
            refreshCellsUsingData();
        }

        public void onSearchEndEdit(string searchString) {

        }

        public void clearSearchFieldButtonClicked() {
            clearSearchButton.gameObject.SetActive(false);
            searchTextfield.text = "";
            _searchString = null;
            // ReloadSoundFiles();
            refreshCellsUsingData();
        }
        
        // ----------------------------------

        public void RefreshSoundFileListBtnClicked() {
            ClearCells();
            ReloadSoundFiles();
            VoiceOver.main.StopPreview();
        }


        public override void CellClicked(CanvasListCell<SoundFile> listCell, SoundFile datum) {
            base.CellClicked(listCell, datum);

            confirmButton.interactable = (_selectedCells.Count > 0);
            
            // This will now be handled by CanvasController
            // VoiceOver.main.PlayPreview(datum);
        }

        public override void ConfirmButtonClicked() {
            VoiceOver.main.StopPreview();
            base.ConfirmButtonClicked();
        }


        public override void BackButtonClicked() {
            VoiceOver.main.StopPreview();
            base.BackButtonClicked();
        }


    }
}