//-----------------------------------------------------------------------
// <copyright file="SoundMarker.cs" company="Google">
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

    public interface ISoundMarkerDelegate {
        bool shouldSoundMarkerTriggerPlayback(SoundMarker marker);
        bool shouldSoundMarkerStopPlaybackAfterUserLeftTriggerRange(SoundMarker marker);
    }

    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(ResonanceAudioSource))]
    [RequireComponent(typeof(SphereCollider))]
    public class SoundMarker : MonoBehaviour, ISoundMarkerTriggerDelegate {

        public ISoundMarkerDelegate markerDelegate = null;

        private AudioLowPassFilter filterLowPass;
        private AudioHighPassFilter filterHighPass;
        private AudioDistortionFilter filterDistortion;
        private AudioPhaser filterPhaser;

        [HideInInspector] private AudioSource _audioSrc;
        [HideInInspector] public Hotspot hotspot { get; private set; }

        public float lowestPitch = 0.3f;
        public float highestPitch = 3f;
        static float LPFMultiplier = 10000f;
        static float HPFMultiplier = 4000f;

        public SoundMarkerTrigger markerTrigger;
        public bool userIsInsideTriggerRange { get; private set; } = false;

        public static float SoundDistBuffer = 0.2f; // The min distance between min. and max.
        public static float SoundMultiplierBuffer = 2f;
        public float soundMinDist {
            get { return hotspot != null ? hotspot.minDistance : _audioSrc.minDistance; }
            private set {
                if (_audioSrc != null) { _audioSrc.minDistance = value; }
                hotspot.SetMinRadius(value);
            }
        }
        public float soundMaxDist {
            get { return hotspot != null ? hotspot.maxDistance : _audioSrc.maxDistance; }
            private set {
                if (_audioSrc != null) {
                    if (value < 0) {
                        _audioSrc.spatialBlend = 0;
                        _audioSrc.pitch = 1.0f;
                    } else {
                        _audioSrc.spatialBlend = 1;
                        _audioSrc.maxDistance = value;
                    }
                }
                if (markerTrigger != null && markerTrigger.triggerCollider != null) { markerTrigger.triggerCollider.radius = value; }

                hotspot.SetMaxRadius(value);
            }
        }

        SoundIcons soundIcons = null;
        public SoundIcons SoundIcons { get { return soundIcons; } }
        public int colorIndex {
            get { return hotspot == null ? 0 : hotspot.colorIndex; }
            set {
                soundIcons.SetSoundIconColour(ColorThemeData.Instance.soundColors[value]);
                if (hotspot == null) return;
                hotspot.SetColorIndex(value);
            }
        }
        public int iconIndex {
            get { return hotspot == null ? 0 : hotspot.iconIndex; }
            set {
                soundIcons.SetSoundIconWithIndex(value);
                if (hotspot == null) return;
                hotspot.SetIconIndex(value);
            }
        }

        public Color color { get { return ColorThemeData.Instance.soundColors[colorIndex]; } }
        public Sprite iconSprite { get { return SoundIconData.Instance.sprites[iconIndex]; } }

        public void PlayAudioFromBeginning(bool ignoreTrigger = false) {
            if (_audioSrc == null) return;
            if (_audioSrc.isPlaying) { 
                _audioSrc.Stop();
                soundIcons.rotateFast = false;
            }
            if (ignoreTrigger || !hotspot.triggerPlayback || userIsInsideTriggerRange) { 
                _audioSrc.Play();
                soundIcons.rotateFast = true;
            }
        }
        public void StopAudioPlayback() {
            if (_audioSrc == null) return;
            if (_audioSrc.isPlaying) { 
                _audioSrc.Stop();
                soundIcons.rotateFast = false;
            }
        }

        public void SetAudioShouldLoop(bool shouldLoop) {
            _audioSrc.loop = shouldLoop;
            hotspot.SetLoopAudio(shouldLoop);
        }

        public void SetTriggerPlayback(bool shouldTrigger) {
            hotspot.SetTriggerPlayback(shouldTrigger);
        }

        public void SetPitchBend(float pitchBend) {
            _audioSrc.pitch = pitchBend;
            hotspot.SetPitchBend(pitchBend);
        }

        public void SetSoundVolume(float vol) {
            _audioSrc.volume = vol;
            hotspot.SetSoundVolume(vol);
        }

        private void SetFreqCutoffComponents(float freqCutoff) {
            if (freqCutoff > 0.1f) {
                filterLowPass.enabled = false;
                filterHighPass.cutoffFrequency = 22000f;
                filterHighPass.enabled = true;
            } else if (freqCutoff < -0.1f) {
                filterHighPass.enabled = false;
                filterLowPass.cutoffFrequency = 10f;
                filterLowPass.enabled = true;
            } else {
                filterLowPass.enabled = false;
                filterHighPass.enabled = false;
            }
        }

        public void SetFrequencyCutoff(float freqCutoff) {
            SetFreqCutoffComponents(freqCutoff);
            hotspot.SetFreqCutoff(freqCutoff);
        }
        public void SetPhaserLevel(float newLevel) {
            // Debug.Log ("New Phaser Level: " + newLevel);
            
            // filterLowPass.lowpassResonanceQ = 1f + (newLevel * 9f);
            // filterHighPass.highpassResonanceQ = 1f + (newLevel * 9f);
            filterPhaser.setMaxSpeedWithPercentage(newLevel);
            filterPhaser.setEnabled(newLevel > 0.1f);
            hotspot.SetPhaserLevel(newLevel);
        }
        public void SetDistortion(float distortion) {
            filterDistortion.enabled = distortion > 0.1f;
            hotspot.SetDistortion(distortion);
        }

        void Awake() {
            _audioSrc = GetComponent<AudioSource>();
            soundIcons = GetComponentInChildren<SoundIcons>();
            
            filterLowPass = GetComponent<AudioLowPassFilter>();
            filterHighPass = GetComponent<AudioHighPassFilter>();
            filterDistortion = GetComponent<AudioDistortionFilter>();
            filterPhaser = GetComponent<AudioPhaser>();

            filterLowPass.enabled = false;
            filterHighPass.enabled = false;
            filterDistortion.enabled = false;
            filterPhaser.setEnabled(false);

            markerTrigger.triggerDelegate = this;
        }

        // Start is called before the first frame update
        void Start() {
            if (soundIcons == null) { soundIcons = GetComponentInChildren<SoundIcons>(); }
            
            if (hotspot == null) return;
            markerTrigger.triggerDelegate = this;
        }

        private void Update() {
            if (userIsInsideTriggerRange && _audioSrc.spatialBlend > 0) {
                bool atLeast1DistBasedEffect = hotspot.distortion > 0.1f 
                                            || Mathf.Abs(hotspot.pitchBend) > 0.1f 
                                            || Mathf.Abs(hotspot.freqCutoff) > 0.1f 
                                            || Mathf.Abs(hotspot.phaserLevel) > 0.1f || true;

                if (Mathf.Abs(hotspot.pitchBend) < 0.1f) { _audioSrc.pitch = 1f; }
                if (!atLeast1DistBasedEffect) { return; }

                // TODO: CHECK IF SPATIAL BLEND is 2D
                float adjMax = soundMaxDist;
                float adjMin = soundMinDist * 2f;

                float distFromCam = Vector3.Distance(transform.position, Camera.main.transform.position);
                float preClampedPercent = (distFromCam - adjMin) / (adjMax - adjMin);
                float percentageToEdge = Mathf.Clamp(preClampedPercent, 0, 1);
                // Debug.Log ("percentageToEdge: " + percentageToEdge);

                // =============================
                // Optimise these checks so they don't occur on each Update()
                if (hotspot.distortion > 0.1f) {
                    filterDistortion.distortionLevel = Mathf.Pow(percentageToEdge, 0.5f) * hotspot.distortion;
                }
                if (Mathf.Abs(hotspot.pitchBend) > 0.1f) {
                    float pitchBendcoefficient = hotspot.pitchBend > 0
                                    ? Mathf.Lerp(0, highestPitch - 1.0f, hotspot.pitchBend)
                                    : Mathf.Lerp(0, lowestPitch - 1.0f, Mathf.Abs(hotspot.pitchBend));
                    _audioSrc.pitch = percentageToEdge * pitchBendcoefficient + 1.0f;
                }

                if (hotspot.phaserLevel > 0.1f) {
                    filterPhaser.setPhaserPercent(percentageToEdge);
                }
                
                // =============================
                // FREQUENCY CUTOFF
                // TODO: Should think about making this a logarithmic relationship
                if (hotspot.freqCutoff > 0.1f) {
                    // float coefficient = Mathf.Pow(percentageToEdge * 1.1f, 2f);
                    //   10 + ([0-1] x 1 * 4000)
                    // = 
                    filterHighPass.cutoffFrequency = 80f + (percentageToEdge * hotspot.freqCutoff * HPFMultiplier);
                } else if (hotspot.freqCutoff < -0.1f) {
                    // Minus sign because the freqCutoff value will be negative
                    // Mathf.Log10(10f); == 1
                    // float coefficient = (1f - percentageToEdge);
                    // float coefficient = Mathf.Log10((1f - percentageToEdge) * 10f);
                    // float coefficient = (1f - Mathf.Pow(percentageToEdge, 3));
                    float coefficient = Mathf.Pow((1f - (percentageToEdge * 1.1f)), 3f);
                    filterLowPass.cutoffFrequency = 150f - (coefficient * hotspot.freqCutoff * LPFMultiplier); // 21990f
                }
                
            }
        }

        public void SetIconAndRangeToRandomValue() {
            colorIndex = Random.Range(0, ColorThemeData.Instance.soundColors.Length - 1);
            iconIndex = Random.Range(0, SoundIconData.Instance.sprites.Length - 1);
        }

        public void SetHotspot(Hotspot newHotspot, bool overrideInteralData = true) {
            hotspot = newHotspot;

            if (overrideInteralData) {
                iconIndex = newHotspot.iconIndex;
                colorIndex = newHotspot.colorIndex;
                _audioSrc.volume = newHotspot.soundVolume;

                filterDistortion.enabled = newHotspot.distortion > 0.1f;
                filterDistortion.distortionLevel = 0.9f;
                SetFreqCutoffComponents(newHotspot.freqCutoff);

                filterPhaser.setEnabled(Mathf.Abs(newHotspot.phaserLevel) > 0.1f);

                // Set the audioSource min/max distance in the correct order
                if (newHotspot.minDistance < _audioSrc.maxDistance) {
                    soundMinDist = newHotspot.minDistance;
                    soundMaxDist = newHotspot.maxDistance;
                } else if (newHotspot.maxDistance > _audioSrc.minDistance) {
                    soundMaxDist = newHotspot.maxDistance;
                    soundMinDist = newHotspot.minDistance;
                } else {
                    soundMinDist = newHotspot.minDistance;
                    soundMaxDist = newHotspot.maxDistance;
                    soundMinDist = newHotspot.minDistance;
                }
            }
        }

        public void SetToNextColor() {
            colorIndex = (colorIndex + 1) % ColorThemeData.Instance.soundColors.Length;
        }

        public void SetToNextIcon() {
            iconIndex = (iconIndex + 1) % SoundIconData.Instance.sprites.Length;
        }

        // -     -     -     -     -     -     -

        /// <summary>Be aware that this function may also modify the MaxDistance based on SoundMarker.SoundDistBuffer</summary>
        /// <param name="minDist">Parameter value to pass.</param>
        /// <returns>Returns a bool if the maxDist was modified</returns>
        public bool SetSoundMinDistance(float minDist) {
            if (_audioSrc == null) { _audioSrc = GetComponent<AudioSource>(); }

            bool maxDistModified = false;
            if (minDist > 0) {
                if (minDist * SoundMultiplierBuffer > _audioSrc.maxDistance) {
                    soundMaxDist = minDist * SoundMultiplierBuffer;

                    maxDistModified = true;
                }
                soundMinDist = minDist;
            }
            return maxDistModified;
        }

        /// <summary>Be aware that this function may also modify the MinDistance based on SoundMarker.SoundDistBuffer</summary>
        /// <param name="maxDist">Parameter value to pass.</param>
        /// <returns>Returns a bool if the minDist was modified</returns>
        public bool SetSoundMaxDistance(float maxDist) {
            if (_audioSrc == null) { _audioSrc = GetComponent<AudioSource>(); }

            bool minDistModified = false;
            if (maxDist > -2) {
                if (maxDist > 0 && maxDist / SoundMultiplierBuffer < _audioSrc.minDistance) {
                    soundMinDist = maxDist / SoundMultiplierBuffer;

                    minDistModified = true;
                }
                soundMaxDist = maxDist;
            }
            return minDistModified;
        }

        // -     -     -     -     -     -     -

        public void LaunchNewClip(AudioClip clip, bool playAudio = true) {
            _audioSrc.clip = clip;
            if (playAudio) {
                if (!hotspot.triggerPlayback || userIsInsideTriggerRange) {
                    _audioSrc.Play();
                    soundIcons.rotateFast = true;
                }
            } else {
                _audioSrc.Stop();
                soundIcons.rotateFast = false;
            }
        }
        #region ISoundMarkerTriggerDelegate

        public void UserDidEnterMarkerTrigger() {
            userIsInsideTriggerRange = true;
            if (!hotspot.triggerPlayback) { return; }
            
            if (markerDelegate == null) {
                PlayAudioFromBeginning(); // audioSrc.Play();
                return;
            }
            if (markerDelegate.shouldSoundMarkerTriggerPlayback(this)) {
                PlayAudioFromBeginning(); // audioSrc.Play();
            }
        }

        public void UserDidExitMarkerTrigger() {
            userIsInsideTriggerRange = false;
            if (!hotspot.triggerPlayback) { return; }

            if (markerDelegate == null) {
                _audioSrc.Stop();
                soundIcons.rotateFast = false;
                return;
            }

            if (markerDelegate.shouldSoundMarkerStopPlaybackAfterUserLeftTriggerRange(this)) {
                _audioSrc.Stop();
                soundIcons.rotateFast = false;
            }
        }


        #endregion

        public void ChangePosition(Vector3 newPosition, Transform anchorWrapperT) {
            GameObject oldAnchorGameObject = this.transform.parent.gameObject;

            Pose p = new Pose() {
                position = newPosition,
                rotation = Quaternion.identity
            };

            Anchor newAnchor = Session.CreateAnchor(p);

            this.transform.parent = newAnchor.transform;
            this.transform.localPosition = Vector3.zero;
            newAnchor.transform.parent = anchorWrapperT;

            // Change the AudioObject's Hotspot Data
            Vector3 anchorSpacePos = anchorWrapperT.transform.InverseTransformPoint(newPosition);
            this.hotspot.Set(anchorSpacePos);

            // Destroy the old Anchor
            Destroy(oldAnchorGameObject);
        }

        // ----------------------------
        // Construct from prefab convenience methods
        // ----------------------------

        /// <summary>
        /// Construct from transform
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static SoundMarker CreatePrefab(Transform t, GameObject prefab, Transform anchorWrapperT) { return CreatePrefab(t.position, t.rotation, prefab, anchorWrapperT); }

        /// <summary>
        /// Creates a hotspot prefab and positions it in 3D space relative to the current camera position.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        /// <returns></returns>
        public static SoundMarker CreatePrefab(Vector3 pos, Quaternion rot, GameObject prefab, Transform anchorWrapperT) {
            Pose p = new Pose() // {position = pos, rotation = rot }
            {
                position = pos,
                rotation = Quaternion.identity // anchorWrapperTransform.rotation * rot
            };
            Anchor anchor = Session.CreateAnchor(p);

            SoundMarker sso = Instantiate(prefab, parent: anchor.transform).GetComponent<SoundMarker>();
            sso.transform.localPosition = Vector3.zero;
            anchor.transform.parent = anchorWrapperT;

            return sso;
        }
    }
}