//-----------------------------------------------------------------------
// <copyright file="CanvasListCellSoundMarker.cs" company="Google">
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

namespace SIS {
    public class CanvasListCellSoundMarker : CanvasListCell<SoundMarker> {

        public override void SetDatum(SoundMarker newDatum) {
            base.SetDatum(newDatum);

            // Specifics
            string soundName = "\"" + datum.hotspot.name + "\"";
            if (soundName == null || soundName.Length < 1) {
                soundName = "<Untitled>";
            }

            soundName += string.Format(" ({0:0.0}m - {1:0.0}m)",
                datum.hotspot.minDistance * 2f,
                datum.hotspot.maxDistance * 2f);

            titleLabel.text = soundName;
            subtitleLabel.text = datum.hotspot.soundFile.filenameWithExtension;

            Color markerCol = datum.color;
            titleLabel.color = markerCol;
            subtitleLabel.color = markerCol;
            iconImage.color = markerCol;
            iconImage.sprite = datum.iconSprite;

            UnityEngine.UI.ColorBlock buttonCols = button.colors;
            buttonCols.pressedColor = markerCol.ColorWithBrightness(0.5f);
            button.colors = buttonCols;
        }

        public override void CellWasSelected(CanvasListBase<SoundMarker> parentList) {
            // Don't call base.cellWasSelected(parentList);
            _isSelected = true;
            SetContentUIColor(Color.white, this.datum.color);
        }

        public override void CellWasDeselected(CanvasListBase<SoundMarker> parentList) {
            _isSelected = false;
            SetContentUIColor(this.datum.color, Color.white);
        }
    }
}