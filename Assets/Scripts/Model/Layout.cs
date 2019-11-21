//-----------------------------------------------------------------------
// <copyright file="Layout.cs" company="Google">
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
using UnityEngine;
using System.Collections.Generic;

namespace SIS {

    public interface ILayoutDelegate {
        SoundFile GetSoundFileFromSoundID(string soundID);
    }

    [Serializable]
    public class Layout : IHotspotDelegate {

        [NonSerialized] public ILayoutDelegate layoutDelegate;

        public int id;
        public string layoutName;
        public List<Hotspot> hotspots;
        public long lastSaveDate;
        [NonSerialized] public string filename;

        public DateTime LastSaveDate() {
            try {
                return new DateTime(lastSaveDate);
            } catch {
                Save(); // touches the file, updating time to now
                return new DateTime(lastSaveDate);
            }
        }
        public void SetLastSaveDate(DateTime dateTime) {
            this.lastSaveDate = dateTime.Ticks;
            Save();
        }

        public static string saveDirectory {
            get {
                return DirectoryManager.layoutSaveDirectory;
            }
        }

        public Layout(int id) {
            this.id = id;
            this.layoutName = string.Format("New Layout {0}", id);
            this.hotspots = new List<Hotspot>();
            this.lastSaveDate = DateTime.Now.Ticks;
        }

        // =========
        // HOTSPOT METHODS
        public void SetHotspotDelegates() {
            foreach (var h in hotspots) {
                h.hotspotDelegate = this;
            }
        }

        public void EraseHotspot(Hotspot hotspot) {
            if (hotspot == null) return;
            // Save the Layout of the Hotspot was successfully removed
            if (hotspots.Remove(hotspot)) Save();
        }

        public void EraseHotspots() {
            if (hotspots == null) { this.hotspots = new List<Hotspot>(); }
            hotspots.Clear();
            Save();
        }

        public void AddHotspot(Hotspot newHotspot) {
            if (hotspots == null) { this.hotspots = new List<Hotspot>(); }
            hotspots.Add(newHotspot);
            newHotspot.hotspotDelegate = this;
            Save();
        }

        // ==========
        // IO METHODS
        public string jsonDataPath {
            get {
                // first check if this id leads to a single filename
                try {
                    return LayoutManager.LayoutWithId(id).filename;
                } catch {
                    // fallback to the default nomenclature
                    return Path.Combine(saveDirectory, string.Format("new_layout_{0}.json", id));
                }

            }
        }
        public void Save() {
            this.lastSaveDate = DateTime.Now.Ticks;

            string jsonString = JsonUtility.ToJson(this);
            using (StreamWriter streamWriter = File.CreateText(jsonDataPath)) {
                streamWriter.Write(jsonString);
            }
        }

        public SoundFile GetSoundFileFromSoundID(string soundID) {
            return layoutDelegate?.GetSoundFileFromSoundID(soundID);
        }
    }
}