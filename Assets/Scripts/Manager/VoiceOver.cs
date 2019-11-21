//-----------------------------------------------------------------------
// <copyright file="VoiceOver.cs" company="Google">
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
using UnityEngine.Audio;
using GoogleARCore;

namespace SIS {

    public class VoiceOver : MonoBehaviour {

        AudioSource audioSource;
        public AudioMixer mixer;
        public Transform anchorWrapper;
        TrackingState prevTrackingState;

        static public VoiceOver main {
            get {
                return Camera.main.GetComponentInChildren<VoiceOver>();
            }
        }

        private Anchor FirstAnchor {
            get {
                return anchorWrapper.GetComponentInChildren<Anchor>(includeInactive: true);
            }
        }

        private AudioSource[] AnchorSources {
            get {
                return anchorWrapper.GetComponentsInChildren<AudioSource>(includeInactive: true);
            }
        }

        void Awake() {
            audioSource = GetComponent<AudioSource>();
        }

        void Update() {
            DetectTrackingLoss();
        }
        private void DetectTrackingLoss() {

            if (FirstAnchor == null) {
                // No anchor? New scene, warning is not relevant
                VoiceOver.main.StopWarning();
                return;
            }
            // make sure the state has changed
            TrackingState currTrackingState = FirstAnchor.TrackingState;
            if (currTrackingState == prevTrackingState) return;

            if (currTrackingState == TrackingState.Paused) {
                VoiceOver.main.PlayWarning();
            } else if (currTrackingState == TrackingState.Tracking) {
                VoiceOver.main.StopWarning();
            }
            prevTrackingState = currTrackingState; // update prev state
        }

        private void AdjustVolume(bool voiceOver) {
            foreach (var source in AnchorSources) {
                // If we are going to voice over mode, mute all world sounds
                source.mute = voiceOver;
            }
        }

        private void PlayThis(SoundFile sf, bool playOnLoop = false) {
            AdjustVolume(voiceOver: true);
            audioSource.clip = sf.clip;
            audioSource.loop = playOnLoop;
            audioSource.Play();
        }
        private void StopVoiceOver() {
            AdjustVolume(voiceOver: false);
            audioSource.clip = null;
            audioSource.Stop();
        }

        public void PlayWarning() {
            // Don't start more than once
            if (audioSource.clip == SoundFile.warningSoundFile.clip && audioSource.isPlaying) return;
            // otherwise, launch the warning
            PlayThis(SoundFile.warningSoundFile, playOnLoop: true);
        }
        public void PlayPreview(SoundFile sf) {
            PlayThis(sf, playOnLoop: true);
        }

        public void StopWarning() {
            if (audioSource.clip == SoundFile.warningSoundFile.clip) {
                StopVoiceOver();
            }
        }

        public void StopPreview() {
            // Dont stop if this is the warning file
            if (audioSource.clip != SoundFile.warningSoundFile.clip) {
                StopVoiceOver();
            }
        }
    }
}