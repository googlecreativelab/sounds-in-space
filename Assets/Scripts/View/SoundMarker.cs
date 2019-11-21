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

    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(ResonanceAudioSource))]
    [RequireComponent(typeof(SphereCollider))]
    public class SoundMarker : MonoBehaviour, ISoundMarkerTriggerDelegate {

        [HideInInspector] public AudioSource audioSrc;
        [HideInInspector] public Hotspot hotspot { get; private set; }

        public float lowestPitch = 0.3f;
        public float highestPitch = 3f;

        public SoundMarkerTrigger markerTrigger;
        public bool userIsInsideTriggerRange { get; private set; } = false;

        public static float SoundDistBuffer = 0.2f; // The min distance between min. and max.
        public static float SoundMultiplierBuffer = 2f;
        public float soundMinDist {
            get { return hotspot != null ? hotspot.minDistance : audioSrc.minDistance; }
            private set {
                if (audioSrc != null) { audioSrc.minDistance = value; }
                hotspot.SetMinRadius(value);
            }
        }
        public float soundMaxDist {
            get { return hotspot != null ? hotspot.maxDistance : audioSrc.maxDistance; }
            private set {
                if (audioSrc != null) {
                    if (value < 0) {
                        audioSrc.spatialBlend = 0;
                        audioSrc.pitch = 1.0f;
                    } else {
                        audioSrc.spatialBlend = 1;
                        audioSrc.maxDistance = value;
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

        public void PlayAudioFromBeginning() {
            if (audioSrc == null) return;
            if (audioSrc.isPlaying) { audioSrc.Stop(); }
            if (!hotspot.triggerPlayback || userIsInsideTriggerRange) { audioSrc.Play(); }
        }
        public void StopAudioPlayback() {
            if (audioSrc == null) return;
            if (audioSrc.isPlaying) { audioSrc.Stop(); }
        }

        public void SetAudioShouldLoop(bool shouldLoop) {
            audioSrc.loop = shouldLoop;
            hotspot.SetLoopAudio(shouldLoop);
        }

        public void SetTriggerPlayback(bool shouldTrigger) {
            hotspot.SetTriggerPlayback(shouldTrigger);
        }

        public void SetPitchBend(float pitchBend) {
            audioSrc.pitch = pitchBend;
            hotspot.SetPitchBend(pitchBend);
        }

        public void SetSoundVolume(float vol) {
            audioSrc.volume = vol;
            hotspot.SetSoundVolume(vol);
        }

        void Awake() {
            audioSrc = GetComponent<AudioSource>();
            soundIcons = GetComponentInChildren<SoundIcons>();
            markerTrigger.triggerDelegate = this;
        }

        // Start is called before the first frame update
        void Start() {
            if (soundIcons == null) { soundIcons = GetComponentInChildren<SoundIcons>(); }
            if (hotspot == null) return;
            markerTrigger.triggerDelegate = this;
        }

        private void Update() {
            if (userIsInsideTriggerRange && audioSrc.spatialBlend > 0) {
                if (Mathf.Abs(hotspot.pitchBend) < 0.1f) {
                    audioSrc.pitch = 1f;
                    return;
                }

                // TODO: CHECK IF SPATIAL BLEND is 2D
                float adjMax = soundMaxDist;
                float adjMin = soundMinDist * 2f;

                float distFromCam = Vector3.Distance(transform.position, Camera.main.transform.position);
                float preClampedPercent = (distFromCam - adjMin) / (adjMax - adjMin);
                float percentageToEdge = Mathf.Clamp(preClampedPercent, 0, 1);
                float coefficient = hotspot.pitchBend > 0
                                    ? Mathf.Lerp(0, highestPitch - 1.0f, hotspot.pitchBend)
                                    : Mathf.Lerp(0, lowestPitch - 1.0f, Mathf.Abs(hotspot.pitchBend));

                audioSrc.pitch = percentageToEdge * coefficient + 1.0f;
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

                // Set the audioSource min/max distance in the correct order
                if (newHotspot.minDistance < audioSrc.maxDistance) {
                    soundMinDist = newHotspot.minDistance;
                    soundMaxDist = newHotspot.maxDistance;
                } else if (newHotspot.maxDistance > audioSrc.minDistance) {
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
            if (audioSrc == null) { audioSrc = GetComponent<AudioSource>(); }

            bool maxDistModified = false;
            if (minDist > 0) {
                if (minDist * SoundMultiplierBuffer > audioSrc.maxDistance) {
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
            if (audioSrc == null) { audioSrc = GetComponent<AudioSource>(); }

            bool minDistModified = false;
            if (maxDist > -2) {
                if (maxDist > 0 && maxDist / SoundMultiplierBuffer < audioSrc.minDistance) {
                    soundMinDist = maxDist / SoundMultiplierBuffer;

                    minDistModified = true;
                }
                soundMaxDist = maxDist;
            }
            return minDistModified;
        }

        // -     -     -     -     -     -     -

        public void LaunchNewClip(AudioClip clip, bool playAudio = true) {
            audioSrc.clip = clip;
            if (playAudio) {
                if (!hotspot.triggerPlayback || userIsInsideTriggerRange) {
                    audioSrc.Play();
                }
            } else {
                audioSrc.Stop();
            }
        }
        #region ISoundMarkerTriggerDelegate

        public void UserDidEnterMarkerTrigger() {
            userIsInsideTriggerRange = true;
            if (!hotspot.triggerPlayback) { return; }
            audioSrc.Play();
        }

        public void UserDidExitMarkerTrigger() {
            userIsInsideTriggerRange = false;
            if (!hotspot.triggerPlayback) { return; }

            audioSrc.Stop();
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