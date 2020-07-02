//-----------------------------------------------------------------------
// <copyright file="SoundRadius.cs" company="Google">
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

    // [RequireComponent(typeof(ParticleSystem))]
    public class SoundRadius : MonoBehaviour {
        private SoundShape _activeShape = SoundShape.Sphere;
        public SoundShape activeShape {
            get { return _activeShape; }
            set {
                if (_activeShape != value) {
                    switch (value) {
                        case SoundShape.Sphere:
                            // if (ps != null) { ps.Play(); }
                            if (minRadiusSphere != null) { minRadiusSphere.SetActive(!_isMinHidden); }
                            if (maxRadiusSphere != null) { maxRadiusSphere.SetActive(!_isMaxHidden); }
                            if (minRadiusColumn != null) { minRadiusColumn.SetActive(false); }
                            if (maxRadiusColumn != null) { maxRadiusColumn.SetActive(false); }
                            break;
                        case SoundShape.Column:
                            // if (ps != null) { ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear); }
                            // if (minRadiusColumn != null) { minRadiusColumn.SetActive(!_isMinHidden); }
                            if (minRadiusColumn != null) { minRadiusColumn.SetActive(false); }
                            if (maxRadiusColumn != null) { maxRadiusColumn.SetActive(!_isMaxHidden); }
                            if (minRadiusSphere != null) { minRadiusSphere.SetActive(false); }
                            if (maxRadiusSphere != null) { maxRadiusSphere.SetActive(false); }
                            break;
                    }
                }
                _activeShape = value;
            }
        }

        [SerializeField] GameObject minRadiusSphere = null;
        [SerializeField] GameObject maxRadiusSphere = null;
        [SerializeField] GameObject minRadiusColumn = null;
        [SerializeField] GameObject maxRadiusColumn = null;

        MeshRenderer minRadMeshRend = null;
        MaterialPropertyBlock minRadPropBlock = null;

        // ParticleSystem ps = null;
        // float initialStartSpeed = 4f;

        void Awake() {
            minRadPropBlock = new MaterialPropertyBlock();
            minRadMeshRend = minRadiusSphere.GetComponent<MeshRenderer>();
            minRadMeshRend.GetPropertyBlock(minRadPropBlock);
        }

        // Start is called before the first frame update
        void Start() {

            // ps = GetComponentInChildren<ParticleSystem>();
            // if (ps != null) { initialStartSpeed = ps.main.startSpeedMultiplier; }

            if (minRadiusSphere != null) { minRadiusSphere.SetActive(!_isMinHidden); }
            if (maxRadiusSphere != null) { maxRadiusSphere.SetActive(!_isMaxHidden); }
            if (minRadiusColumn != null) { minRadiusColumn.SetActive(false); }
            if (maxRadiusColumn != null) { maxRadiusColumn.SetActive(false); }
        }

        private bool _isMinHidden = false;
        private bool _isMaxHidden = false;

        public bool isMinHidden {
            get => _isMinHidden;
            set {
                _isMinHidden = value;
                if (_activeShape == SoundShape.Sphere && minRadiusSphere != null) { minRadiusSphere.SetActive(!_isMinHidden); }
                // if (_activeShape == SoundShape.Column && minRadiusColumn != null) { minRadiusColumn.SetActive(!_isMinHidden); }
                if (_activeShape == SoundShape.Column && minRadiusColumn != null) { minRadiusColumn.SetActive(false); }
            }
        }
        public bool isMaxHidden {
            get => _isMaxHidden;
            set {
                _isMaxHidden = value;
                if (_activeShape == SoundShape.Sphere && maxRadiusSphere != null) { maxRadiusSphere.SetActive(!_isMaxHidden); }
                if (_activeShape == SoundShape.Column && maxRadiusColumn != null) { maxRadiusColumn.SetActive(!_isMaxHidden); }

                // if (ps != null) {
                //     if (_isMaxHidden) {
                //         ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                //     } else if (_activeShape == SoundShape.Sphere) {
                //         ps.Play();
                //     }
                // }
            }
        }

        private float _maxRadius = 50;
        public float maxRadius {
            get => _maxRadius;
            set {
                _maxRadius = value;
                if (maxRadiusSphere == null) { return; }

                if (isMaxHidden != (_maxRadius < 0)) {
                    isMaxHidden = (_maxRadius < 0);
                }
                if (_maxRadius < 0) { return; }

                Vector3 scale = Vector3.one * _maxRadius * 2f;
                maxRadiusSphere.transform.localScale = scale;
                // if (ps != null) { ps.transform.localScale = scale; }

                if (maxRadiusColumn == null) { return; }
                // ---------------------------------------
                // COLUMN
                scale = Vector3.one * _maxRadius * 100f;
                scale.z *= 2f;
                maxRadiusColumn.transform.localScale = scale;
            }
        }

        private float _minRadius = 10;
        public float minRadius {
            get => _minRadius;
            set {
                _minRadius = value;
                if (minRadiusSphere == null) { return; }

                Vector3 scale = Vector3.one * _minRadius * 2f * 1.8f;
                minRadiusSphere.transform.localScale = scale;

                float delta = (Mathf.Clamp(_minRadius, 0.1f, 4) / 4f);
                minRadPropBlock.SetFloat("_RimScale", (delta * 3f) + 1f);
                minRadMeshRend.SetPropertyBlock(minRadPropBlock);

                if (minRadiusColumn == null) { return; }
                // ---------------------------------------
                // COLUMN
                scale = Vector3.one * _minRadius * 100f;
                minRadiusColumn.transform.localScale = scale;
            }
        }

        public void SetColor(Color col) {
            if (maxRadiusSphere != null) {
                MeshRenderer rend = maxRadiusSphere.GetComponent<MeshRenderer>();
                rend.material.color = col;
            } else {
                Debug.LogError("SoundRadius... maxRadiusSphere is null?");
            }
            if (minRadiusSphere != null) {
                MeshRenderer rend = minRadiusSphere.GetComponent<MeshRenderer>();
                rend.material.color = col;
            } else {
                Debug.LogError("SoundRadius... minRadiusSphere is null?");
            }
            // ---------------------
            if (maxRadiusColumn != null) {
                MeshRenderer rend = maxRadiusColumn.GetComponent<MeshRenderer>();
                rend.material.color = col;
            }
            if (minRadiusColumn != null) {
                MeshRenderer rend = minRadiusColumn.GetComponent<MeshRenderer>();
                rend.material.color = col;
            }
            // ---------------------

            // if (ps != null) {
            //     ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);

            //     ParticleSystem.MainModule main = ps.main;
            //     main.startColor = col;

            //     if (_activeShape == SoundShape.Sphere) { ps.Play(); }
            // }
        }
    }
}