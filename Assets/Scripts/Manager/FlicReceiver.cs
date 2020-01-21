// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

namespace SIS {
    public interface IFlicReceiverDelegate {
        void flicReceiverButtonClicked();
        void flicReceiverButtonClickedTwice();
        void flicReceiverButtonClickAndHold();
    }
    
    public class FlicReceiver : MonoBehaviour {
        IFlicReceiverDelegate _delegate;
        public void setDelegate(IFlicReceiverDelegate del) { _delegate = del; }

        AndroidJavaClass javaIntentReceiver; // Java class for Flic integration

        // Start is called before the first frame update
        void Start() {
            javaIntentReceiver = new AndroidJavaClass("com.google.cl.syd.soundsinspace.flic.FlicReceiver");
            javaIntentReceiver.CallStatic("createInstance");
            
            // string javaReceiverString = javaIntentReceiver.GetStatic<string>("text");
            // if (javaReceiverString.Length > 0) { Debug.Log(javaReceiverString); } else { Debug.Log("FlicReceiver text string = " + javaReceiverString); }
        }

        // Update is called once per frame
        void Update() {
            // Flic button resets camera on android
            try {

                string javaReceiverString = javaIntentReceiver.GetStatic<string>("text");
                if (javaReceiverString != null) {
                    if (javaReceiverString.Length > 0) { Debug.Log("FLIC Received: " + javaReceiverString); }
                    
                    if (javaReceiverString == "clicked") {
                        _delegate?.flicReceiverButtonClicked();
                    } else if (javaReceiverString == "double-clicked") {
                        _delegate?.flicReceiverButtonClickedTwice();
                    } else if (javaReceiverString == "click-hold") {
                        _delegate?.flicReceiverButtonClickAndHold();
                    }

                    javaIntentReceiver.CallStatic("clearText");
                }
            } catch (System.NullReferenceException e) {
                Debug.Log(e); // Not sure why we're getting a null exception here. Pass over it for now.
            }
        }
    }
}