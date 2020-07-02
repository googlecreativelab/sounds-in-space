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

    public interface IVoiceOverDelegate {
        void voiceOverWillStart();
        void voiceOverWillStop();
    }
    public class VoiceOver : MonoBehaviour {

        IVoiceOverDelegate _voiceOverDelegate;
        public void setDelegate(IVoiceOverDelegate del) { _voiceOverDelegate = del; }

        AudioSource audioSource;
        public AudioMixer mixer;

        static public VoiceOver main {
            get { return Camera.main.GetComponentInChildren<VoiceOver>(); }
        }
        

        void Awake() {
            audioSource = GetComponent<AudioSource>();
        }
        

        private void PlayThis(SoundFile sf, bool playOnLoop = false, float volume = 1.0f) {
            _voiceOverDelegate?.voiceOverWillStart();

            audioSource.clip = sf.clip;
            audioSource.loop = playOnLoop;
            audioSource.volume = volume;
            audioSource.Play();
        }
        private void StopVoiceOver() {
            _voiceOverDelegate?.voiceOverWillStop();
            
            audioSource.clip = null;
            audioSource.Stop();
        }

        public void PlayWarning() {
            // Don't start more than once
            if (audioSource.clip == SoundFile.warningSoundFile.clip && audioSource.isPlaying) return;
            // otherwise, launch the warning
            PlayThis(SoundFile.warningSoundFile, playOnLoop: true, volume: 0.7f);
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