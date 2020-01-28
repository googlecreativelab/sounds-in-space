// using System.Collections;
using System.Collections.Generic;
using System;
// using UnityEngine;

namespace SIS {
    public class LambdaWaiter<T> {
        HashSet<Action<T>> actions;
        int numActionsComplete = 0;

        Action allCompleteCallback = null;

        public LambdaWaiter() {
            actions = new HashSet<Action<T>>();
        }
        ~LambdaWaiter() => Console.WriteLine("The LambdaWaiter destructor is executing.");

        private void callCompleteIfPossible() {
            // UnityEngine.Debug.Log ("LambdaWaiter... " + actions.Count + " actions, numActionsComplete: " + numActionsComplete);
            if (numActionsComplete >= actions.Count && allCompleteCallback != null) {
                UnityEngine.Debug.Log("LambdaWaiter ALL COMPLETE! " + actions.Count + " Actions, " + numActionsComplete + " Actions Complete.");
                allCompleteCallback();
            }
        }
        public Action<T> AddCallback(Action<T> callback) {
            Action<T> newAction = (T returnValue) => {
                if (callback != null) { callback(returnValue); }
                ++numActionsComplete;
                callCompleteIfPossible();
            };
            actions.Add(newAction);
            return newAction;
        }

        public void WaitForAllCallbacks(Action callback) {
            allCompleteCallback = callback;
            callCompleteIfPossible();
        }
    }
}