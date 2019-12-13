// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
using UnityEngine.EventSystems;

namespace SIS {
    public interface IEditSoundEventTriggerDelegate {
        void bottomPanelStartedDragging();
        void bottomPanelFinishedDragging(float yVelocity);
    }

    public class EditSoundEventTrigger : EventTrigger {
        private IEditSoundEventTriggerDelegate _delegate = null;
        public void setDelegate(IEditSoundEventTriggerDelegate del) { _delegate = del; }

        private bool _dragging;
        private float _centerYOffset;
        private float _maxYPos = 0;
        private float _minYPos = 0;

        private void Awake() {
            _maxYPos = UnityEngine.Screen.height;
            _minYPos = 0.09375f * _maxYPos;
            // Debug.Log("_maxYPos: " + _maxYPos);
        }

        public override void OnBeginDrag(PointerEventData eventData) {
            _dragging = true;

            _centerYOffset = transform.position.y - eventData.position.y;
            _delegate?.bottomPanelStartedDragging();
        }

        public override void OnDrag(PointerEventData eventData) {
            UnityEngine.Vector2 pos = transform.position;
            // pos.y = eventData.position.y + _centerYOffset;
            float newPos = eventData.position.y + _centerYOffset;
            // Debug.Log ("newPos: " + newPos);
            pos.y = UnityEngine.Mathf.Clamp(newPos, _minYPos, _maxYPos);
            transform.position = pos;
        }

        public override void OnEndDrag(PointerEventData eventData) {
            _dragging = false;
            _delegate?.bottomPanelFinishedDragging(yVelocity: 0);
        }

    }
}