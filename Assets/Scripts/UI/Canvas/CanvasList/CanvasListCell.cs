//-----------------------------------------------------------------------
// <copyright file="CanvasListCell.cs" company="Google">
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
    public class CanvasListCell<D> : MonoBehaviour {
        [HideInInspector] public D datum { get; private set; }

        protected bool _isSelected = false;
        public bool isSelected { get { return _isSelected; } }

        public Text titleLabel;
        public Text subtitleLabel;
        public Image iconImage;
        public Button button;

        public virtual void ReloadUI() {
            
        }

        public virtual void SetDatum(D newDatum) {
            datum = newDatum;

            if (titleLabel != null) { titleLabel.color = ColorThemeData.Instance.interactionColor; }
            if (subtitleLabel != null) { subtitleLabel.color = ColorThemeData.Instance.interactionColor; }
            if (iconImage != null) { iconImage.color = ColorThemeData.Instance.interactionColor; }

            UnityEngine.UI.ColorBlock buttonCols = button.colors;
            buttonCols.pressedColor = ColorThemeData.Instance.interactionColor.ColorWithBrightness(0.8f);
            button.colors = buttonCols;
        }

        protected void SetContentUIColor(Color contentCol, Color bgCol) {
            if (titleLabel != null) { titleLabel.color = contentCol; }
            if (subtitleLabel != null) { subtitleLabel.color = contentCol; }
            if (iconImage != null) { iconImage.color = contentCol; }

            ColorBlock colors = button.colors;
            colors.normalColor = bgCol;
            colors.highlightedColor = bgCol;
            button.colors = colors;
        }

        public virtual void CellWasSelected(CanvasListBase<D> parentList) {
            _isSelected = true;
            SetContentUIColor(Color.white, ColorThemeData.Instance.interactionColor);
        }

        public virtual void CellWasDeselected(CanvasListBase<D> parentList) {
            _isSelected = false;
            SetContentUIColor(ColorThemeData.Instance.interactionColor, Color.white);
        }
    }
}