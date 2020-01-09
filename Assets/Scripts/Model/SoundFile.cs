//-----------------------------------------------------------------------
// <copyright file="SoundFile.cs" company="Google">
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
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.Networking;

namespace SIS {

    /*
    primarily used to define the json metafiles used to declare sounds
     */
    public enum LoadState {
        NotLoaded, Loading, Success, Fail
    }

    [Serializable]
    public class SoundFile {

        static SoundFile preloadInstance;

        public static string DEFAULT_CLIP = "__default";

        public static bool MP3_otherwise_WAV = true; // true=MP3, false=WAV
        public static string SoundFileExtension = MP3_otherwise_WAV ? ".mp3" : ".wav";
        private static Regex soundFileRegex = new Regex(MP3_otherwise_WAV ? @"\.mp3$" : @"\.wav$");

        public static string MetaFileExtension = ".json";
        public static string PlaceholderString = "Placeholder Sound";

        private static Regex metaRegex = new Regex(@"\.json$"); // Ends in .meta

        public string filename;
        public string filenameWithExtension { get { return isDefaultSoundFile ? PlaceholderString : filename + SoundFileExtension; } }
        public string soundName;
        public string averagePitch;
        public int duration;
        public int durationSafe { get { return clip == null ? duration : Mathf.RoundToInt(clip.length); } }

        [NonSerialized] public LoadState loadState;
        [NonSerialized] public AudioClip clip;

        public static SoundFile defaultSoundFile;
        public bool isDefaultSoundFile { get { return this == defaultSoundFile; } }
        public static SoundFile warningSoundFile;

        public static string saveDirectory {
            get { return DirectoryManager.soundSaveDirectory; }
        }

        private void Awake() {
            preloadInstance = this;
        }

        static SoundFile() {
            defaultSoundFile = new SoundFile(Resources.Load<AudioClip>("Default"));
            defaultSoundFile.soundName = "Placeholder Sound";
            warningSoundFile = new SoundFile(Resources.Load<AudioClip>("NegativeBeep1")); // Old Filename: 'VoiceOver'
            warningSoundFile.soundName = "Tracking warning";
        }

        public SoundFile(string filename) {
            this.filename = Path.GetFileNameWithoutExtension(filename);
            this.soundName = this.filename;
            this.loadState = LoadState.NotLoaded;
        }

        public SoundFile(AudioClip clip) {
            this.clip = clip;
            this.clip.name = soundName;
            this.loadState = LoadState.Success;
        }

        public string soundFilepath {
            get { return Path.Combine(saveDirectory, UnityWebRequest.EscapeURL(filename) + SoundFileExtension); }
        }
        public string jsonPath {
            get { return Path.Combine(saveDirectory, UnityWebRequest.EscapeURL(filename) + MetaFileExtension); }
        }

        public static string[] metaFiles {
            get { return Directory.GetFiles(saveDirectory).Where(f => metaRegex.IsMatch(f)).ToArray(); }
        }
        public static string[] soundFilenames {
            get { return Directory.GetFiles(saveDirectory).Where(f => soundFileRegex.IsMatch(f)).ToArray(); }
        }

        // ==================
        // IO
        public static List<SoundFile> AllSoundFilesFromDisk() {
            List<SoundFile> list = new List<SoundFile>();
            foreach (string filename in SoundFile.metaFiles) {
                try {
                    list.Add(ReadFromMeta(filename));
                } catch (Exception e) {
                    Debug.Log("failed to load sound file: " + e);
                }
            }
            return list;
        }

        static public SoundFile ReadFromMeta(string jsonFile) {
            string dataAsJson = File.ReadAllText(jsonFile);
            return JsonUtility.FromJson<SoundFile>(dataAsJson);
        }

        public static void CreateNewMetas() {
            // Get list of soundFiles listed in metafiles
            List<string> foundFilenamesWithData = new List<string>();
            foreach (string filename in SoundFile.metaFiles) {
                foundFilenamesWithData.Add(ReadFromMeta(filename).soundFilepath);
            }
            // look for wavs without metas and create them
            foreach (string filename in SoundFile.soundFilenames) {
                if (!foundFilenamesWithData.Contains(filename)) {
                    SoundFile sf = new SoundFile(filename);
                    sf.Save();
                }
            }
        }

        public static IEnumerator AwaitLoading(Dictionary<string, SoundFile> ofDict, Action completion) {
            yield return AwaitLoading(ofDict.Values, completion);
        }

        public static IEnumerator AwaitLoading(IEnumerable<SoundFile> soundFilesThatAreLoading, Action completion) {
            foreach (SoundFile sf in soundFilesThatAreLoading) {
                if (sf.loadState == LoadState.Fail) { continue; }
                if (sf.loadState != LoadState.Success) {
                    // Will either be loading or about to load, let's give it a chance!
                    yield return new WaitForSeconds(0.1f);
                }
            }

            completion();
        }

        private static IEnumerator loadSoundFileClip(SoundFile sf, System.Action<SoundFile> completion = null) {
            if (sf.isDefaultSoundFile || (sf.loadState == LoadState.Success && sf.clip != null)) {
                // ALREADY LOADED!
                if (completion != null) { completion(sf); }
                yield break;
            }

            sf.loadState = LoadState.Loading;
            // Get and bind the assoc soundfile
            string url = "file://" + sf.soundFilepath; // System.Text.Encoding.UTF8

            using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(url,
                        MP3_otherwise_WAV ? AudioType.MPEG : AudioType.WAV)) {

                DownloadHandlerAudioClip dHandler = req.downloadHandler as DownloadHandlerAudioClip;
                // dHandler.streamAudio = true;
                dHandler.compressed = true;

                yield return req.SendWebRequest();
                // if (dHandler.isDone) {
                //     Debug.Log("Get Audio clip IS DONE");
                // } else {
                //     Debug.Log("Get Audio clip is NOT done");
                // }
                if (req.error != null) {
                    Debug.Log(req.error + " for: " + url);
                    sf.loadState = LoadState.Fail;
                } else {
                    AudioClip ac = DownloadHandlerAudioClip.GetContent(req);
                    if (ac != null) {
                        Debug.Log("AudioClip loadState: " + ac.loadState + " - " + url);

                        sf.clip = ac;
                        sf.loadState = LoadState.Success;
                        sf.duration = Mathf.RoundToInt(ac.length);
                        sf.Save();
                    }
                }
            }
            Debug.LogWarning("SoundFile::loadSoundFileClip WILL call completion...");
            if (completion != null) { completion(sf); }
        }

        // Loads a soundfile object from a metafile, as well as preloading the audio
        // public static IEnumerator LoadSoundFileFromMeta(string filename, Dictionary<string, SoundFile> into, System.Action completion = null) {
        //     // Load the metadata
        //     SoundFile sf = ReadFromMeta(filename);

        //     yield return loadSoundFileClip(sf, completion);
        // }

        public static IEnumerator LoadClipInSoundFile(SoundFile sf, System.Action<SoundFile> completion = null) {
            yield return loadSoundFileClip(sf, completion);
        }

        public void Save() {

#if UNITY_EDITOR
            string jsonString = JsonUtility.ToJson(this, prettyPrint: true);
#else
            string jsonString = JsonUtility.ToJson(this);
#endif

            using (StreamWriter streamWriter = File.CreateText(jsonPath)) {
                streamWriter.Write(jsonString);
            }
        }
    }
}