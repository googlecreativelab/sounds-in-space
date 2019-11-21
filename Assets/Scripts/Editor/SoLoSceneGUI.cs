//-----------------------------------------------------------------------
// <copyright file="PLACEHOLDER.cs" company="Google">
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
 using UnityEditor;
using UnityEngine;

 public class SoLoSceneGUI : EditorWindow
 {

    static void Init() {
        SceneView.onSceneGUIDelegate += OnScene;
    }

     [MenuItem("Window/SoLo Kit/Enable Editor UI")]
     public static void Enable()
     {
         SceneView.onSceneGUIDelegate += OnScene;
         Debug.Log("Scene GUI : Enabled");
     }

     [MenuItem("Window/SoLo Kit/Disable Editor UI")]
     public static void Disable()
     {
         SceneView.onSceneGUIDelegate -= OnScene;
         Debug.Log("Scene GUI : Disabled");
     }

     private static void OnScene(SceneView sceneview) {
         Handles.BeginGUI();
         if (GUILayout.Button("Press Me")) Debug.Log("Got it to work.");

         Handles.EndGUI();
     }

     void OnGUI() {
        if (GUILayout.Button("Press Me")) Debug.Log("Got it to work.");
     }
 }

