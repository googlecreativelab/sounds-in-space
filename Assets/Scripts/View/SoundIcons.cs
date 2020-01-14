//-----------------------------------------------------------------------
// <copyright file="SoundIcons.cs" company="Google">
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

    public class SoundIcons : MonoBehaviour {
        MeshRenderer[] meshRends = null;
        public int Count { get { return meshRends == null ? 0 : meshRends.Length; } }
        int curIndex = 0;

        private Color _mainColor = Color.black;

        private float _flashTimer;
        private bool _flashBlack = false;
        private bool _isBlack = false;
        public bool FlashBlack {
            get { return _flashBlack; }
            set {
                if (_flashBlack == true && value == true) { return; }
                _flashBlack = value;
                if (value) {
                    _flashTimer = 0;
                    _isBlack = true;
                    setMaterialColor(Color.gray);
                } else {
                    setMaterialColor(_mainColor);
                }
            }
        }

        private bool _rotateFast = false;
        private float _rotMultiplier = 0.5f;
        public bool rotateFast {
            get { return _rotateFast; }
            set { 
                if (_rotateFast == value) { return; }
                _rotateFast = value;
                _rotMultiplier = _rotateFast ? 4f : 0.5f;
            }
        }

        MaterialPropertyBlock propBlock;

        void Awake() {
            meshRends = GetComponentsInChildren<MeshRenderer>();
            propBlock = new MaterialPropertyBlock();
        }

        // Start is called before the first frame update
        void Start() {
            SetSoundIconWithIndex(curIndex);
        }

        public void SetSoundIconColour(Color col) {
            _mainColor = col;
            setMaterialColor(col);
        }

        private void setMaterialColor(Color col) {
            for (int i = 0; i < meshRends.Length; i++) {
                meshRends[i].GetPropertyBlock(propBlock);
                propBlock.SetColor("_Color", col);
                meshRends[i].SetPropertyBlock(propBlock);
            }
        }

        public void SetSoundIconWithIndex(int index) {
            curIndex = index;
            if (meshRends == null) { return; }
            for (int i = 0; i < meshRends.Length; i++) {
                meshRends[i].transform.parent.gameObject.SetActive(index == i);
            }
        }

        // Update is called once per frame
        void Update() {
            if (_flashBlack) {
                _flashTimer += Time.deltaTime;
                if (_flashTimer > 0.6f) {
                    setMaterialColor(_isBlack ? _mainColor : Color.gray);
                    _isBlack = !_isBlack;
                    _flashTimer = 0;
                }
            } else if (_isBlack) {
                setMaterialColor(_mainColor);
                _isBlack = false;
            }

            transform.Rotate(
                _rotMultiplier * Mathf.Sin(0.618f * Time.time),
                _rotMultiplier * Mathf.Sin(Time.time),
                _rotMultiplier * Mathf.Sin(0.309f * Time.time)
            );
        }
    }
}