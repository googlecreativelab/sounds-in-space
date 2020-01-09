//-----------------------------------------------------------------------
// <copyright file="Hotspot.cs" company="Google">
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
using System;
using UnityEngine;

// using System.Runtime.Serialization;

namespace SIS {

    public interface IHotspotDelegate {
        void Save();
        SoundFile GetSoundFileFromSoundID(string soundID);
    }

    /*
    Hotspot encapsulates data about Sound Markers
    things like tranform info, radius, soundfile name, colour, are all saved in the json data files.
    */
    [Serializable]
    public class Hotspot : ISerializationCallbackReceiver {
        [NonSerialized] public IHotspotDelegate hotspotDelegate = null;

        [SerializeField] private string _id;
        // [SerializeField] private string _syncedSoundsJSON;

        [SerializeField] private float _positionX;
        [SerializeField] private float _positionY;
        [SerializeField] private float _positionZ;

        [SerializeField] private float _rotationX;
        [SerializeField] private float _rotationY;
        [SerializeField] private float _rotationZ;

        [SerializeField] private float _minDistance;
        [SerializeField] private float _maxDistance;

        [SerializeField] private int _iconIndex;
        [SerializeField] private int _colorIndex;
        [SerializeField] private int _soundShapeIndex;

        [SerializeField] private bool _triggerPlayback;
        [SerializeField] private bool _loopAudio;
        [SerializeField] private bool _playOnce;
        [SerializeField] private float _pitchBend;
        [SerializeField] private float _srcVolume = 1f;
        [SerializeField] private float _freqCutoff;
        [SerializeField] private float _distortion;
        [SerializeField] private float _phaserLevel;
        [SerializeField] private float _echoMagnitude;

        [SerializeField] private string _objectName;

        [SerializeField] private string _soundID; // See DEFAULT_CLIP
                                                  // -----------
                                                  // GETTERS
        public string id { get { return _id; } }
        public int iconIndex { get { return _iconIndex; } }
        public int colorIndex { get { return _colorIndex; } }
        public int soundShapeIndex { get { return _soundShapeIndex; } }
        public bool triggerPlayback { get { return _triggerPlayback; } }
        public bool loopAudio { get { return _loopAudio; } }
        public bool playOnce { get { return _playOnce; } }
        public float pitchBend { get { return _pitchBend; } }
        public float freqCutoff { get { return _freqCutoff; } }
        public float distortion { get { return _distortion; } }
        public float echoMagnitude { get { return _echoMagnitude; } }
        public float phaserLevel { get { return _phaserLevel; } }
        public float soundVolume { get { return _srcVolume < 0 ? 0 : _srcVolume; } }
        public Vector3 positon { get { return new Vector3(_positionX, _positionY, _positionZ); } }
        public Quaternion rotation { get { return Quaternion.Euler(_rotationX, _rotationY, _rotationZ); } }
        public float minDistance { get { return _minDistance; } }
        public float maxDistance { get { return _maxDistance; } }
        public bool hasInfiniteMaxDistance { get { return _maxDistance < 0; } }
        public string name { get { return _objectName; } }
        public string soundID { get { return _soundID; } }
        public SoundFile soundFile { get { return hotspotDelegate?.GetSoundFileFromSoundID(_soundID); } }

        public void OnBeforeSerialize() {
          // Debug.Log ("OnBeforeDeserialize _srcVolume: " + _srcVolume);
        }

        public void OnAfterDeserialize() {
            if (_srcVolume == 0) { _srcVolume = 1f; } // If _srcVolume==0, it has not been set.
            if (_id == null || _id.Length < 1) { _id = System.Guid.NewGuid().ToString(); }
        }

        // =============
        // SETTERS

        public void SetIconIndex(int newIconIndex) {
            _iconIndex = newIconIndex;
            hotspotDelegate?.Save();
        }
        public void SetColorIndex(int newColorIndex) {
            _colorIndex = newColorIndex;
            hotspotDelegate?.Save();
        }
        public void SetSoundShapeIndex(int newSoundShapeIndex) {
            _soundShapeIndex = newSoundShapeIndex;
            hotspotDelegate?.Save();
        }

        public void SetTriggerPlayback(bool newValue) {
            _triggerPlayback = newValue;
            hotspotDelegate?.Save();
        }

        public void SetLoopAudio(bool newValue) {
            _loopAudio = newValue;
            hotspotDelegate?.Save();
        }

        public void SetPlayOnce(bool newValue) {
            _playOnce = newValue;
            hotspotDelegate?.Save();
        }

        public void SetPitchBend(float newPitchBend) {
            _pitchBend = newPitchBend;
            hotspotDelegate?.Save();
        }

        public void SetSoundVolume(float newVolume) {
            _srcVolume = newVolume == 0 ? -1 : newVolume;
            hotspotDelegate?.Save();
        }

        public void SetFreqCutoff(float newFreqCutoff) {
            _freqCutoff = newFreqCutoff;
            hotspotDelegate?.Save();
        }

        public void SetDistortion(float newDistortion) {
            _distortion = newDistortion;
            hotspotDelegate?.Save();
        }
        
        public void SetEchoMagnitude(float newEcho) {
            _echoMagnitude = newEcho;
            hotspotDelegate?.Save();
        }

        public void SetPhaserLevel(float newLevel) {
            _phaserLevel = newLevel;
            hotspotDelegate?.Save();
        }

        public void Set(Vector3 position) {
            _positionX = position.x;
            _positionY = position.y;
            _positionZ = position.z;
            hotspotDelegate?.Save();
        }

        public void Set(Vector3 localPos, Vector3 rotation) {
            _positionX = localPos.x;
            _positionY = localPos.y;
            _positionZ = localPos.z;

            _rotationX = rotation.x;
            _rotationY = rotation.y;
            _rotationZ = rotation.z;

            hotspotDelegate?.Save();
        }

        public void SetMinRadius(float minRadius = -1) {
            if (minRadius > 0) _minDistance = minRadius;
            hotspotDelegate?.Save();
        }

        public void SetMaxRadius(float maxRadius = -2) {
            // _maxRadius can be negative to signify infinity
            if (maxRadius > -2) _maxDistance = maxRadius;
            hotspotDelegate?.Save();
        }

        public void Set(float minRadius = -1, float maxRadius = -2) {
            if (minRadius > 0) _minDistance = minRadius;
            if (maxRadius > -2) _maxDistance = maxRadius;
            hotspotDelegate?.Save();
        }

        public void SetName(string name) {
            this._objectName = name;
            hotspotDelegate?.Save();
        }

        public void Set(string soundId) {
            this._soundID = soundId;
            hotspotDelegate?.Save();
        }


        public Hotspot(Vector3 localPos, Vector3 rotation, float newMinDistance, float newMaxDistance) {
            _id = System.Guid.NewGuid().ToString();
            // _syncedSoundsJSON = "[]";
            
            _positionX = localPos.x;
            _positionY = localPos.y;
            _positionZ = localPos.z;

            _rotationX = rotation.x;
            _rotationY = rotation.y;
            _rotationZ = rotation.z;

            _minDistance = newMinDistance;
            _maxDistance = newMaxDistance;

            _colorIndex = 0;
            _iconIndex = 0;

            _objectName = "Untitled";

            _triggerPlayback = false;
            _loopAudio = true;
            _playOnce = false;
            _pitchBend = 0;
            _srcVolume = 1f;
            _freqCutoff = 0;
            _distortion = 0;
            _echoMagnitude = 0;

            _soundID = SoundFile.DEFAULT_CLIP;
        }

    }
}