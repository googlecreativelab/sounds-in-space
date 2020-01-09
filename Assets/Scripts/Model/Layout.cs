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
using System.Linq;

namespace SIS {

    public interface ILayoutDelegate {
        SoundFile GetSoundFileFromSoundID(string soundID);
    }

    [Serializable]
    public class SyncedMarkers {
        public List<string> list;
        public SyncedMarkers(HashSet<string> markers) {
            list = new List<string>(markers);
        }
        public SyncedMarkers() {
            list = new List<string>();
        }
        public bool containsMarkerID(string id) {
            return this.list.Contains(id);
        }
    }

    [Serializable]
    public class Layout : IHotspotDelegate, ISerializationCallbackReceiver {

        [NonSerialized] public ILayoutDelegate layoutDelegate;

        public int id;
        public string layoutName;
        public List<Hotspot> hotspots;
        
        public List<SyncedMarkers> syncedMarkerIDs;
        [NonSerialized] public HashSet<HashSet<string>> syncedMarkerIDSets;

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
            this.syncedMarkerIDs = new List<SyncedMarkers>();
            this.lastSaveDate = DateTime.Now.Ticks;
            this.syncedMarkerIDSets = new HashSet<HashSet<string>>();
        }

        public void OnBeforeSerialize() {

        }

        public void OnAfterDeserialize() {
            if (this.syncedMarkerIDSets == null) {
                this.syncedMarkerIDSets = new HashSet<HashSet<string>>();
            }
            if (this.syncedMarkerIDs == null) {
                this.syncedMarkerIDs = new List<SyncedMarkers>();
            } else {
                updateSyncedMarkerSets();
            }
        }

        // =========

        public void printSyncedMarkers() {
            for (int i = 0; i < this.syncedMarkerIDs.Count; ++i) {
                Debug.Log("   SynchronisedMarkerIDs(" + i + ") .count: " + this.syncedMarkerIDs[i].list.Count);
            }
        }

        private void updateSyncedMarkerSets() {
            this.syncedMarkerIDSets.Clear();
            foreach (SyncedMarkers markers in this.syncedMarkerIDs) {
                if (markers.list.Count < 2) { continue; }
                this.syncedMarkerIDSets.Add(new HashSet<string>(markers.list));
            }
        }

        public HashSet<string> getSynchronisedMarkerIDs(string forMarkerID) {
            foreach (HashSet<string> syncedMarkerIDs in this.syncedMarkerIDSets) {
                if (syncedMarkerIDs.Contains(forMarkerID)) {
                    return syncedMarkerIDs;
                }
            }

            return null;
        }

        // Returns Synced Markers - NOT INCLUDING the marker that was passed in
        public IEnumerable<SoundMarker> getSynchronisedMarkers(string forMarkerID) {
            HashSet<string> syncedMarkerIDs = getSynchronisedMarkerIDs(forMarkerID);
            if (syncedMarkerIDs == null || syncedMarkerIDs.Count < 1) { return null; }

            return MainController.soundMarkers.Where(
                (sm) => {
                    return sm.hotspot.id != forMarkerID // Ignore the caller marker
                        && syncedMarkerIDs.Contains(sm.hotspot.id);
                });
        }

        public void setSynchronisedMarkerIDs(HashSet<string> markers) {
            // Debug.Log ("setSynchronisedMarkerIDs markers.count: " + markers.Count);

            // Remove ANY duplicates
            for (int i = 0; i < this.syncedMarkerIDs.Count; i++) {
                for (int j = 0; j < this.syncedMarkerIDs[i].list.Count; j++) {
                    string markerID = this.syncedMarkerIDs[i].list[j];
                    if (markers.Contains(markerID)) {
                        this.syncedMarkerIDs[i].list.RemoveAt(j);
                        --j;
                    }
                }
                // If any lists have <2 markers, Delete the whole list
                if (this.syncedMarkerIDs[i].list.Count < 2) {
                    this.syncedMarkerIDs.RemoveAt(i);
                    --i;
                }
            }

            // Add the new markers
            this.syncedMarkerIDs.Add(new SyncedMarkers(markers));
            updateSyncedMarkerSets();
            
            Save();
        }

        public void removeMarkerIDFromSynchronisedMarkers(string markerID, bool save = true) {
            for (int i = 0; i < this.syncedMarkerIDs.Count; i++) {
                for (int j = 0; j < this.syncedMarkerIDs[i].list.Count; j++) {
                    string tmpMarkerID = this.syncedMarkerIDs[i].list[j];
                    if (markerID == tmpMarkerID) {
                        this.syncedMarkerIDs[i].list.RemoveAt(j);
                        --j;
                    }
                }
                // If any lists have <2 markers, Delete the whole list
                if (this.syncedMarkerIDs[i].list.Count < 2) {
                    this.syncedMarkerIDs.RemoveAt(i);
                    --i;
                }
            }

            updateSyncedMarkerSets();
            if (save) { Save(); }
        }

        private void clearSynchronisedMarkers(bool save = true) {
            this.syncedMarkerIDs.Clear();
            updateSyncedMarkerSets();
            if (save) { Save(); }
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
            removeMarkerIDFromSynchronisedMarkers(hotspot.id, save:false);
            
            hotspots.Remove(hotspot);
            Save();
        }

        public void EraseHotspots() {
            clearSynchronisedMarkers(save: false);
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
            // Debug.Log ("Saved to " + jsonDataPath);
        }

        public SoundFile GetSoundFileFromSoundID(string soundID) {
            return layoutDelegate?.GetSoundFileFromSoundID(soundID);
        }
    }
}