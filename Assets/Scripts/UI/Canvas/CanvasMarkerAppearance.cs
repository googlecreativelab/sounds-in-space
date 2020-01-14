using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public List<UnityEngine.UI.Button> colourButtons;
        public List<UnityEngine.UI.Button> iconsButtons;

        private int _selectedColour = 0;
        private int _selectedIcon = 0;
        public int SelectedColourIndex { get { return _selectedColour; } }
        public int SelectedIconIndex { get { return _selectedIcon; } }

        // Start is called before the first frame update
        void Start() {
            
        }

        public void setSelectedProperties(int colourIndex, int iconIndex) {
            _selectedColour = colourIndex;
            _selectedIcon = iconIndex;
        }

        override public void CanvasWillAppear() {
            base.CanvasWillAppear();

            colourSelectTransform.position = colourButtons[_selectedColour].transform.position;
            iconSelectTransform.position = iconsButtons[_selectedIcon].transform.position;
        }

        public void colourButtonClicked(int index) {
            _selectedColour = index;
            colourSelectTransform.position = colourButtons[_selectedColour].transform.position;
        }

        public void icon3DButtonClicked(int index) {
            _selectedIcon = index;
            iconSelectTransform.position = iconsButtons[_selectedIcon].transform.position;
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