//-----------------------------------------------------------------------
// <copyright file="CanvasListBase.cs" company="Google">
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
    public interface ICanvasListDelegate<D> {
        void ListCellClicked(CanvasListCell<D> listCell, D datum);
        void NewLayoutButtonClicked();
        void ConfirmButtonClicked(CanvasController.CanvasUIScreen fromScreen, CanvasListCell<D> currentSelectedCell);
        void NewSoundMarkerButtonClickedInSoundMarkerList();
        void BackButtonClicked(CanvasController.CanvasUIScreen fromScreen);
        List<SoundFile> AllSoundFiles();
        void ReloadSoundFiles(System.Action completion);
        void DeleteSoundMarker(SoundMarker soundObj);
        void DeleteLayout(Layout layout);
        void DuplicateLayout(Layout layout);
    }

    public class CanvasListBase<D> : CanvasBase {
        public ICanvasListDelegate<D> canvasDelegate = null;

        public GameObject cellPrefab;
        private List<CanvasListCell<D>> currentCells = new List<CanvasListCell<D>>();
        protected List<D> data;
        public VerticalLayoutGroup listView;

        private CanvasListCell<D> mySelectedCell = null;
        protected CanvasListCell<D> selectedCell {
            get { return mySelectedCell; }
            set {
                if (value != mySelectedCell) {
                    if (mySelectedCell != null) { mySelectedCell.CellWasDeselected(this); }
                    if (value != null) { value.CellWasSelected(this); }
                    mySelectedCell = value;
                }
            }
        }
        public Button confirmButton = null;

        override public void CanvasWillAppear() {
            RefreshCells();
        }

        public void RefreshCells() {
            ClearCells();

            if (data == null) return;
            // add a child cell for each item
            foreach (D datum in data) {
                CanvasListCell<D> listCell = Instantiate(cellPrefab, Vector3.zero, Quaternion.identity).GetComponent<CanvasListCell<D>>();
                listCell.SetDatum(datum);

                listCell.transform.SetParent(listView.transform);
                listCell.transform.localScale = Vector3.one;

                Button cellButton = listCell.GetComponentInChildren<Button>();
                cellButton.onClick.AddListener(() => { CellClicked(listCell, datum); });

                currentCells.Add(listCell);

                selectedCell = null;
            }
        }

        protected void DeleteSelectedCell() {
            DeleteCell(selectedCell);
        }

        protected void DeleteCell(CanvasListCell<D> cellToDelete) {
            currentCells.Remove(cellToDelete);
            Destroy(cellToDelete.gameObject);
        }

        protected void ClearCells() {
            // Remove all the old cells
            foreach (CanvasListCell<D> cell in currentCells) {
                Destroy(cell.gameObject);
            }
            currentCells.Clear();
        }

        #region List Callbacks

        public virtual void ConfirmButtonClicked() {

            if (canvasDelegate == null) { return; }
            canvasDelegate.ConfirmButtonClicked(this.canvasID, currentSelectedCell: selectedCell);
        }

        public virtual void CellClicked(CanvasListCell<D> listCell, D datum) {
            selectedCell = listCell;

            if (canvasDelegate == null || datum == null) { return; }

            // Get the clicked cell info
            canvasDelegate.ListCellClicked(listCell, datum);
        }

        public override void BackButtonClicked() {
            base.BackButtonClicked();
            selectedCell = null;

            if (canvasDelegate == null) { return; }
            canvasDelegate.BackButtonClicked(this.canvasID);
        }

        #endregion
    }
}