//-----------------------------------------------------------------------
// <copyright file="OriginMarker.cs" company="Google">
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
using GoogleARCore;
namespace SIS {

    public class OriginMarker : MonoBehaviour {

        GoogleARCore.Anchor anchor = null;
        public GoogleARCore.Anchor Anchor {
            get {
                if (anchor == null) { anchor = GetComponent<GoogleARCore.Anchor>(); }
                return anchor;
            }
        }

        public void SetAnchorWorldPosition(Vector3 worldPos) {
            //TODO: DEAD CODE

        }

        // Start is called before the first frame update
        void Start() {
            anchor = GetComponent<GoogleARCore.Anchor>();
        }

        /// <summary>
        /// </summary>
        public static OriginMarker CreatePrefab(Transform t, GameObject prefab, Transform anchorWrapperT) {
            Pose p = new Pose() {
                position = new Vector3(t.position.x, -1.85f, t.position.z),
                rotation = Quaternion.Euler(0, t.transform.rotation.eulerAngles.y, 0)
            };
            Anchor anchor = Session.CreateAnchor(p);
            GameObject originMarkerObject = Instantiate(prefab, parent: anchor.transform);
            originMarkerObject.transform.localPosition = Vector3.zero;
            anchor.transform.parent = anchorWrapperT;
            return originMarkerObject.GetComponent<OriginMarker>();
        }
    }
}