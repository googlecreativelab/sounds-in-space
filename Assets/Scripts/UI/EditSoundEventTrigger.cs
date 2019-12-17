// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
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
        private float _lastOnDragTime = 0;

        private float _centerYOffset;
        private float _maxYPos = 0;
        private float _minYPos = 0;

        public bool isDragging { get { return _dragging; } }

        private void Awake() {
            _maxYPos = UnityEngine.Screen.height;
            _minYPos = 0.09375f * _maxYPos;
            // Debug.Log("_maxYPos: " + _maxYPos);
        }

        // ----------------------------------------------------------

        // public void subScrollViewStartedDragging(float startYPos) {
        //     startDragging(startYPos);
        // }

        // Returns the difference since last time
        public float subScrollViewMoved(float heightDelta) {
            if (!_dragging) { startDragging(0); }
            return moveByHeight(heightDelta);
        }

        public void subScrollViewFinishedDragging(float endYPos) {
            endDragging(endYPos);
        }
        
        public void subScrollViewEndedDragging() {
            _dragging = false;
        }

        // ----------------------------------------------------------

        private void startDragging(float startYPos) {
            _dragging = true;
            _lastOnDragTime = Time.time;

            _centerYOffset = startYPos;
            _delegate?.bottomPanelStartedDragging();
        }

        private float moveByHeight(float heightDelta) {

            UnityEngine.Vector2 pos = transform.position;
            pos.y = UnityEngine.Mathf.Clamp(pos.y + heightDelta, _minYPos, _maxYPos);
            float difference = transform.position.y - pos.y;
            transform.position = pos;

            _lastOnDragTime = Time.time;

            return difference;
        }

        private void endDragging(float endYPos) {
            _dragging = false;

            float time = Time.time - _lastOnDragTime;
            if (time > 0.05f) { // This is not a flick gesture!
                _delegate?.bottomPanelFinishedDragging(yVelocity: 0);
            } else {
                float posDiff = endYPos - (transform.position.y - _centerYOffset);
                float max = Screen.height * 0.5f;
                float speed = Mathf.Min(max, Mathf.Max(-max, 0.01f * posDiff / time));
                // Debug.Log("POSDIFF: " + posDiff + ", TIME: " + time + ", SPEED: " + speed);

                _delegate?.bottomPanelFinishedDragging(yVelocity: speed);
            }
        }

        // ----------------------------------------------------------

        public override void OnBeginDrag(PointerEventData eventData) {
            startDragging(transform.position.y - eventData.position.y);
        }

        public override void OnDrag(PointerEventData eventData) {
            // float newPos = eventData.position.y + _centerYOffset;
            // float delta = newPos - transform.position.y;
            moveByHeight(eventData.position.y + _centerYOffset - transform.position.y);
        }

        // public override void OnDrag(PointerEventData eventData) {
        //     UnityEngine.Vector2 pos = transform.position;
        //     float newPos = eventData.position.y + _centerYOffset;

        //     pos.y = UnityEngine.Mathf.Clamp(newPos, _minYPos, _maxYPos);
        //     transform.position = pos;

        //     _lastOnDragTime = Time.time;
        // }

        public override void OnEndDrag(PointerEventData eventData) {
            endDragging(eventData.position.y);
        }

    }
}