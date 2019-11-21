//-----------------------------------------------------------------------
// <copyright file="RectTransformExtensions.cs" company="Google">
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

    public static class RectTransformExtensions {

        /// <summary>
        /// SetLeft moves a given transform based on a new left position
        /// </summary>
        /// <param name="rt">transform to move</param>
        /// <param name="left">new left position</param>
        public static void SetLeft(this RectTransform rt, float left) {
            rt.offsetMin = new Vector2(left, rt.offsetMin.y);
        }
        /// <summary>
        /// SetRight moves a given transform based on a new right position
        /// </summary>
        /// <param name="rt">transform to move</param>
        /// <param name="right">New position</param>
        public static void SetRight(this RectTransform rt, float right) {
            rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
        }
        /// <summary>
        /// SetTop moves a given transform based on a new top position
        /// </summary>
        /// <param name="rt">transform to move</param>
        /// <param name="top">New position</param>
        public static void SetTop(this RectTransform rt, float top) {
            rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
        }

        /// <summary>
        /// SetBottom moves a given transform based on a new bottom position
        /// </summary>
        /// <param name="rt">transform to move</param>
        /// <param name="bottom">New position</param>
        public static void SetBottom(this RectTransform rt, float bottom) {
            rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
        }
    }
}