using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SIS {
    
    public interface ICanvasMarkerAppearanceDelegate {
        void BackButtonClicked(CanvasController.CanvasUIScreen fromScreen);
        Layout GetCurrentLayout();
    }

    public class CanvasMarkerAppearance : CanvasBase {
        public ICanvasMarkerAppearanceDelegate canvasDelegate = null;
        public override CanvasController.CanvasUIScreen canvasID { get { return CanvasController.CanvasUIScreen.MarkerAppearance; } }

        public RectTransform colourSelectTransform;
        public RectTransform iconSelectTransform;

        public List<Button> colourButtons;
        public List<Button> iconsButtons;

        private int _selectedColour = 0;
        private int _selectedIcon = 0;
        public int SelectedColourIndex { get { return _selectedColour; } }
        public int SelectedIconIndex { get { return _selectedIcon; } }

        public void UpdateSelectionPositions() {
            colourSelectTransform.position = colourButtons[_selectedColour].transform.position;
            iconSelectTransform.position = iconsButtons[_selectedIcon].transform.position;
            
            Vector2 size = colourButtons[_selectedColour].GetComponent<RectTransform>().sizeDelta;
            size.x += 32;
            size.y += 32;
            colourSelectTransform.sizeDelta = size;

            size = iconsButtons[_selectedIcon].GetComponent<RectTransform>().sizeDelta;
            size.x += 24;
            size.y += 4;
            iconSelectTransform.sizeDelta = size;

            foreach (Button btn in iconsButtons) {
                btn.colors = colourButtons[_selectedColour].colors;
            }
        }

        private void Awake() {
            UpdateSelectionPositions();
        }

        // Start is called before the first frame update
        void Start() {
            UpdateSelectionPositions();
        }

        public void setSelectedProperties(int colourIndex, int iconIndex) {
            _selectedColour = colourIndex;
            _selectedIcon = iconIndex;
        }

        override public void CanvasWillAppear() {
            base.CanvasWillAppear();

            UpdateSelectionPositions();
        }

        public void colourButtonClicked(int index) {
            _selectedColour = index;
            colourSelectTransform.position = colourButtons[_selectedColour].transform.position;

            Vector2 size = colourButtons[_selectedColour].GetComponent<RectTransform>().sizeDelta;
            size.x += 32;
            size.y += 32;
            colourSelectTransform.sizeDelta = size;

            foreach (Button btn in iconsButtons) {
                btn.colors = colourButtons[_selectedColour].colors;
            }
        }

        public void icon3DButtonClicked(int index) {
            _selectedIcon = index;
            iconSelectTransform.position = iconsButtons[_selectedIcon].transform.position;

            Vector2 size = iconsButtons[_selectedIcon].GetComponent<RectTransform>().sizeDelta;
            size.x += 24;
            size.y += 4;
            iconSelectTransform.sizeDelta = size;
        }


        public void saveButtonClicked() {
            base.BackButtonClicked();


            if (canvasDelegate == null) { return; }
            canvasDelegate.BackButtonClicked(this.canvasID);
        }

        // Update is called once per frame
        // void Update() {
            
        // }
    }
}