using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIS {
    using UnityEngine;

    public class SafeArea : MonoBehaviour {

        private Rect m_ScreenSafeArea = new Rect(0, 0, 0, 0);

        public void Update() {
            Rect safeArea;
#if UNITY_2017_2_OR_NEWER
            safeArea = Screen.safeArea;
#else
            safeArea = new Rect(0, 0, Screen.width, Screen.height);
#endif

            if (m_ScreenSafeArea != safeArea) {
                m_ScreenSafeArea = safeArea;
                _MatchRectTransformToSafeArea();
            }
        }

        private void _MatchRectTransformToSafeArea() {
            RectTransform rectTransform = GetComponent<RectTransform>();

            // lower left corner offset
            Vector2 offsetMin = new Vector2(m_ScreenSafeArea.xMin,
                Screen.height - m_ScreenSafeArea.yMax);

            // upper right corner offset
            Vector2 offsetMax = new Vector2(m_ScreenSafeArea.xMax - Screen.width,
                -m_ScreenSafeArea.yMin);

            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }
    }
}