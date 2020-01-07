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
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SIS {
    public interface ICanvasListDelegate<D> {
        void ListCellClicked(CanvasListCell<D> listCell, D datum);
        void NewLayoutButtonClicked();
        void ConfirmButtonClicked(CanvasController.CanvasUIScreen fromScreen, HashSet<CanvasListCell<D>> currentSelectedCells);
        void NewSoundMarkerButtonClickedInSoundMarkerList();
        void CanvasListWillReturn(CanvasController.CanvasUIScreen fromScreen, HashSet<CanvasListCell<D>> currentSelectedCells);
        void BackButtonClicked(CanvasController.CanvasUIScreen fromScreen);
        List<SoundFile> AllSoundFiles();
        void ReloadSoundFiles(System.Action completion);
        void DeleteSoundMarker(SoundMarker soundObj);
        void DeleteLayout(Layout layout);
        void DuplicateLayout(Layout layout);
        HashSet<string> SynchronisedMarkerIDsWithMarkerID(string markerID);
    }

    public class CanvasListBase<D> : CanvasBase {
        public ICanvasListDelegate<D> canvasDelegate = null;

        public GameObject cellPrefab;
        protected List<CanvasListCell<D>> currentCells = new List<CanvasListCell<D>>();
        protected List<D> data;
        public VerticalLayoutGroup listView;

        public Button backButton = null;
        public Button confirmButton = null;

        protected HashSet<CanvasListCell<D>> _selectedCells = new HashSet<CanvasListCell<D>>();
        // public HashSet<CanvasListCell<D>> selectedCells { get { return _selectedCells; } }

        // ==========================================
        // private CanvasListCell<D> mySelectedCell = null;
        // protected CanvasListCell<D> selectedCell {
        //     get { return mySelectedCell; }
        //     set {
        //         if (value != mySelectedCell) {
        //             if (mySelectedCell != null) { mySelectedCell.CellWasDeselected(this); }
        //             if (value != null) { value.CellWasSelected(this); }
        //             mySelectedCell = value;
        //         }
        //     }
        // }
        // ------------------------------------------
        private bool _canSelectMultipleCells = false;
        public bool canSelectMultipleCells {
            get { return _canSelectMultipleCells; }
            set {
                if (_canSelectMultipleCells && !value) {
                    // TODO: modify the set of selectedCells so there is only 1 left
                }
                _canSelectMultipleCells = value;
            }
        }

        protected void cellWasSelected(CanvasListCell<D> newCell) {
            if (newCell == null || _selectedCells.Contains(newCell)) { return; } // Already selected

            newCell.CellWasSelected(this);
            
            if (!_canSelectMultipleCells) {
                deselectAllCells();
            }
            _selectedCells.Add(newCell);
        }
        protected void cellWasDeselected(CanvasListCell<D> oldCell) {
            if (oldCell == null || !_selectedCells.Contains(oldCell)) { return; } // Already NOT selected

            oldCell.CellWasDeselected(this);

            _selectedCells.Remove(oldCell);
        }
        protected void deselectAllCells() {
            foreach (CanvasListCell<D> cell in _selectedCells) { cell.CellWasDeselected(this); }
            _selectedCells.Clear();
        }
        // ==========================================

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
            }
            // selectedCell = null;
            deselectAllCells();
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
            canvasDelegate.ConfirmButtonClicked(this.canvasID, currentSelectedCells: _selectedCells);
        }

        public virtual void CellClicked(CanvasListCell<D> listCell, D datum) {
            // selectedCell = listCell;

            if (!_canSelectMultipleCells) {
                cellWasSelected(listCell);
            } else {
                if (listCell.isSelected) {
                    cellWasDeselected(listCell);
                } else {
                    cellWasSelected(listCell);
                }
            }

            if (canvasDelegate == null || datum == null) { return; }

            // Get the clicked cell info
            canvasDelegate.ListCellClicked(listCell, datum);
        }

        public override void BackButtonClicked() {
            base.BackButtonClicked();

            if (canvasDelegate != null) {
                canvasDelegate.CanvasListWillReturn(this.canvasID, _selectedCells);
            }

            // selectedCell = null;
            deselectAllCells();

            if (canvasDelegate == null) { return; }
            canvasDelegate.BackButtonClicked(this.canvasID);
        }

        #endregion
    }
}