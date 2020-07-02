//-----------------------------------------------------------------------
// <copyright file="SoundMarkerTrigger.cs" company="Google">
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
    public interface ISoundMarkerTriggerDelegate {
        void UserDidEnterMarkerTrigger(SoundMarkerTrigger.Type triggerType);
        void UserDidExitMarkerTrigger(SoundMarkerTrigger.Type triggerType);
    }

    public class SoundMarkerTrigger : MonoBehaviour {

        public enum Type { Playback, OnDemandLoad, OnDemandUnload }

        public ISoundMarkerTriggerDelegate triggerDelegate = null;
        public SphereCollider triggerCollider = null;

        private Type typeForTag(string tag) {
            if (tag.StartsWith("U")) { // "UNLOAD-OnDemand-Audio"
                return Type.OnDemandUnload;
            } else if (tag.StartsWith("L")) { // "LOAD-OnDemand-Audio"
                return Type.OnDemandLoad;
            } else {
                return Type.Playback; // DEFAULT
            }
        }

        private void OnTriggerEnter(Collider other) {
            // ÷Debug.Log ("SoundMarkerTrigger::OnTriggerEnter");
            triggerDelegate?.UserDidEnterMarkerTrigger( typeForTag(other.tag) );
        }

        private void OnTriggerExit(Collider other) {
            // Debug.Log("SoundMarkerTrigger::OnTriggerExit");
            triggerDelegate?.UserDidExitMarkerTrigger( typeForTag(other.tag) );
        }
    }
}