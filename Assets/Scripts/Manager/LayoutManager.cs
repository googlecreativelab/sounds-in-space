//-----------------------------------------------------------------------
// <copyright file="LayoutManager.cs" company="Google">
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
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using UnityEngine;

namespace SIS {

    public interface ILayoutManagerDelegate {
        void LayoutManagerLoadedNewLayout(Layout newLayout);
        void LayoutHotspotListChanged(Layout layout);
        void StartCoroutineOn(IEnumerator e);
    }

    public class LayoutManager : ILayoutDelegate {

        public ILayoutManagerDelegate layoutManagerDelegate = null;

        public Layout layout; // Current scene layout in memory
        public Dictionary<string, SoundFile> soundDictionary;

        // Top level save state int
        public int currentLayoutId {
            get {
                return PlayerPrefs.GetInt("layout", 0);
            }
            set {
                PlayerPrefs.SetInt("layout", value);
            }
        }

        // ===========
        // HOTSPOT METHODS
        public Hotspot AddNewHotspot(Vector3 localPos, Vector3 rotation, float minDist, float maxDist) {
            Hotspot h = new Hotspot(localPos, rotation, minDist, maxDist);
            layout.AddHotspot(h);
            layoutManagerDelegate?.LayoutHotspotListChanged(layout);
            return h;
        }

        public void EraseHotspot(Hotspot hotspot) {
            if (layout == null) return;

            layout?.EraseHotspot(hotspot);
            layoutManagerDelegate?.LayoutHotspotListChanged(layout);
        }

        public void EraseAllHotspots() {
            layout.EraseHotspots();
            layoutManagerDelegate?.LayoutHotspotListChanged(layout);
        }


        // ============
        // SOUND FILE METHODS
        public void Bind(SoundMarker obj, Hotspot hotspot, bool startPlayback) {
            AudioClip clip = soundDictionary.TryGetValue(hotspot.soundID, out SoundFile sf)
                ? sf.clip  // If the sound is found, use it
                : SoundFile.defaultSoundFile.clip; // Fallback to default

            // bind these together
            obj.SetHotspot(hotspot, true); // with override color etc
            obj.LaunchNewClip(clip, playAudio: startPlayback);
        }

        public void Bind(SoundMarker obj, SoundFile sf) {
            // bind these
            obj.hotspot.Set(sf.filename);
            obj.LaunchNewClip(sf.clip);
        }

        // =================
        // LAYOUT METHODS

        // Directory for save files
        public int NextLayoutId() {
            var lIds = AllLayouts().Select((l) => { return l.id; });
            if (lIds.Count() == 0) return 0;
            return lIds.Max() + 1;
        }

        public int NearestLayoutId(int to) {
            (int, int) closest = (0, int.MaxValue);
            foreach (var l in AllLayouts()) {
                int diff = Math.Abs(l.id - to);
                if (diff < int.MaxValue) {
                    closest = (l.id, diff);
                }
            }
            return closest.Item1;
        }

        public void DuplicateLayout(Layout layout) {
            // to duplicate, take all data of the current layout, but overwrite the id
            // to the next available
            var newLayout = layout;
            newLayout.id = NextLayoutId();
            newLayout.Save();
            currentLayoutId = newLayout.id;
        }

        public void DeleteLayout(Layout layout) {
            // Remove the layout from hdd, then select the closest layout to load
            DeleteLayoutsWith(id: layout.id);
            // Order dependent, the nearest layout is derived from disk
            currentLayoutId = NearestLayoutId(to: layout.id);
        }

        // ILayoutDelegate
        public SoundFile GetSoundFileFromSoundID(string soundID) {
            if (soundID != null && soundDictionary.TryGetValue(soundID, out SoundFile sf)) { return sf; }
            return SoundFile.defaultSoundFile;
        }

        // ============
        // IO METHODS]
        private static Regex jsonRegex = new Regex("json$"); // Ends in json
        private static string[] jsonFiles {
            get {
                return Directory.GetFiles(Layout.saveDirectory).Where(f => jsonRegex.IsMatch(f)).ToArray();
            }
        }
        private static Layout LayoutFromFile(string filename) {
            // IO is risky business. Let's not make assumptions
            try {
                string dataAsJson = File.ReadAllText(filename);
                var l = JsonUtility.FromJson<Layout>(dataAsJson);
                l.filename = filename; // filename is not guaranteed same, so we store it on load
                return l;
            } catch (Exception e) {
                Debug.Log("failed to load layout: " + e);
                return null;
            }
        }

        public static Layout LayoutWithId(int LayoutID) {
            var layouts = AllLayouts();
            var withID = layouts.Where( l => { return l.id == LayoutID; } );
            if (withID.Count() < 1) {
                throw new Exception(string.Format("There is no file with the id {}.", LayoutID));
            }
            if (withID.Count() > 1) {
                throw new Exception(string.Format("There are multiple files with id: {}.", LayoutID));
            }
            // Nice, exactly one
            return withID.First();
        }

        public static List<Layout> AllLayouts() {
            List<Layout> lays = new List<Layout>();
            foreach (var filename in jsonFiles) {
                Layout l = LayoutFromFile(filename);
                if (l != null) lays.Add(l);
            }
            return lays;
        }

        public void LoadCurrentLoudout() {
            // Try to load a single layout with the current id
            // If anything goes wrong, make a new layout with the next valid id
            try {
                this.layout = LayoutWithId(currentLayoutId);
            } catch {
                // Create a new layout instead
                this.currentLayoutId = NextLayoutId();
                this.layout = new Layout(currentLayoutId);
                this.layout.Save();
            }

            // Set appropriate references
            layout.layoutDelegate = this;
            layout.SetHotspotDelegates();
            layoutManagerDelegate?.LayoutManagerLoadedNewLayout(this.layout);
        }

        private static void DeleteLayoutsWith(int id) {
            // observe every file for any with the correct id
            foreach (var filename in jsonFiles) {
                Layout l = LayoutFromFile(filename);
                if (l.id != id) continue;
                // Must have the same id now
                File.Delete(filename);
            }
        }

        public void LoadSoundFiles(Action completion) {
            var into = this.soundDictionary; // A nice little alias for it
                                             // Make sure we have data for every wav
            SoundFile.CreateNewMetas();

            // load all sound files from their metas
            foreach (var filename in SoundFile.metaFiles) {
                SoundFile sf = SoundFile.ReadFromMeta(filename);
                string wavName = sf.filename;

                // If the SoundFile has already been loaded, don't reload it
                if (soundDictionary.ContainsKey(wavName)) { continue; }

                layoutManagerDelegate?.StartCoroutineOn(SoundFile.LoadSoundFileFromMeta(filename, into));
            }
            layoutManagerDelegate?.StartCoroutineOn(SoundFile.AwaitLoading(into, completion));

        }


        public void ReloadSoundFiles(Action completion) {
            LoadSoundFiles(completion);
        }

        public List<SoundFile> AllSoundFiles() {
            return soundDictionary.Values.ToList();
        }

        public LayoutManager() {
            // load the latest scene from memory or create new save
            soundDictionary = new Dictionary<string, SoundFile>();
            soundDictionary[SoundFile.DEFAULT_CLIP] = SoundFile.defaultSoundFile;
        }
    }


}