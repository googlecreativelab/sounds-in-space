using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIS {
    public interface INumPadDelegate {
        void NumPadClicked(int number);
    }

    public class NumPad : MonoBehaviour {
        private INumPadDelegate _numPadDelegate;
        public void setNumPadDelegate(INumPadDelegate del) { _numPadDelegate = del; }

        public List<UnityEngine.UI.Button> numButtons;

        private void Awake() {
            // for (int i = 0; i < numButtons.Count; ++i) {
            //     numButtons[i].onClick.AddListener(() => NumPadBTNClicked(i) );
            // }
        }

        // Start is called before the first frame update
        void Start() {
            
        }

        public void NumPadBTNClicked(int val) {
            _numPadDelegate?.NumPadClicked(val);
        }

        // Update is called once per frame
        // void Update() {
            
        // }
    }
}