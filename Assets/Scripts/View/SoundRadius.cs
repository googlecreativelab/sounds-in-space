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

    [RequireComponent(typeof(ParticleSystem))]
    public class SoundRadius : MonoBehaviour {
        [SerializeField] GameObject minRadiusSphere = null;
        [SerializeField] GameObject maxRadiusSphere = null;

        MeshRenderer minRadMeshRend = null;
        MaterialPropertyBlock minRadPropBlock = null;

        ParticleSystem ps = null;
        float initialStartSpeed = 4f;

        void Awake() {
            minRadPropBlock = new MaterialPropertyBlock();
            minRadMeshRend = minRadiusSphere.GetComponent<MeshRenderer>();
            minRadMeshRend.GetPropertyBlock(minRadPropBlock);
        }

        // Start is called before the first frame update
        void Start() {

            ps = GetComponentInChildren<ParticleSystem>();
            initialStartSpeed = ps.main.startSpeedMultiplier;

            if (minRadiusSphere != null) { minRadiusSphere.SetActive(!_isMinHidden); }
            if (maxRadiusSphere != null) { maxRadiusSphere.SetActive(!_isMaxHidden); }
        }

        private bool _isMinHidden = false;
        private bool _isMaxHidden = false;

        public bool isMinHidden {
            get => _isMinHidden;
            set {
                _isMinHidden = value;
                if (minRadiusSphere != null) { minRadiusSphere.SetActive(!_isMinHidden); }
            }
        }
        public bool isMaxHidden {
            get => _isMaxHidden;
            set {
                _isMaxHidden = value;
                if (maxRadiusSphere != null) { maxRadiusSphere.SetActive(!_isMaxHidden); }

                if (ps != null) {
                    if (_isMaxHidden) {
                        ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
                    } else {
                        ps.Play();
                    }
                }
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
                if (ps != null) { ps.transform.localScale = scale; }
            }
        }

        private float _minRadius = 10;
        public float minRadius {
            get => _minRadius;
            set {
                _minRadius = value;
                if (maxRadiusSphere == null) { return; }

                Vector3 scale = Vector3.one * _minRadius * 2f * 1.8f;
                minRadiusSphere.transform.localScale = scale;

                float delta = (Mathf.Clamp(_minRadius, 0.1f, 4) / 4f);
                minRadPropBlock.SetFloat("_RimScale", (delta * 3f) + 1f);
                minRadMeshRend.SetPropertyBlock(minRadPropBlock);
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

            if (ps != null) {
                ps.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);

                ParticleSystem.MainModule main = ps.main;
                main.startColor = col;

                ps.Play();
            }
        }
    }
}