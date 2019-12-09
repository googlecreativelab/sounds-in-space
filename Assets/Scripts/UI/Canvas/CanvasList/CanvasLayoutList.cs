//-----------------------------------------------------------------------
// <copyright file="CanvasLayoutList.cs" company="Google">
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
    public class CanvasLayoutList : CanvasListBase<Layout> {
        public override CanvasController.CanvasUIScreen canvasID { get { return CanvasController.CanvasUIScreen.LayoutList; } }

        public Button openLayoutButton = null;
        public Button deleteButton = null;
        public Button duplicateButton = null;

        private void Awake() {

        }

        override public void CanvasWillAppear() {
            ReloadLayouts();
            base.CanvasWillAppear();

            UpdateBTNStates();
        }

        private void ReloadLayouts() {
            data = LayoutManager.AllLayouts();
            data.Sort((a, b) => { return string.Compare(a.layoutName, b.layoutName); });
        }

        private void UpdateBTNStates() {

            bool atLeast1CellSelected = _selectedCells.Count > 0;
            openLayoutButton.interactable = atLeast1CellSelected;
            duplicateButton.interactable = atLeast1CellSelected;
            duplicateButton.gameObject.SetActive(atLeast1CellSelected);

            bool deletion = atLeast1CellSelected && data.Count > 1;
            deleteButton.gameObject.SetActive(deletion);
            deleteButton.interactable = (deletion);
        }

        public override void CellClicked(CanvasListCell<Layout> listCell, Layout datum) {
            base.CellClicked(listCell, datum);

            UpdateBTNStates();
        }


        public void DeleteButtonClicked() {
            if (_selectedCells.Count < 1) { return; }

            foreach (CanvasListCell<Layout> cell in _selectedCells) {
                data.Remove(cell.datum);
                DeleteCell(cell);
                canvasDelegate?.DeleteLayout(cell.datum);
            }

            ReloadLayouts();
            RefreshCells();

            deselectAllCells();
            UpdateBTNStates();
        }

        public void DuplicateButtonClicked() {
            if (_selectedCells.Count != 1) { return; }

            canvasDelegate?.DuplicateLayout(_selectedCells.First().datum);
            ReloadLayouts();
            RefreshCells();

            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }


        public void NewLayoutButtonClicked() {
            canvasDelegate?.NewLayoutButtonClicked();
        }
    }
}