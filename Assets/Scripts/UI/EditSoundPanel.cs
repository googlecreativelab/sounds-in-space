// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

// using UnityEngine.UI;
using DG.Tweening;

namespace SIS {
    public class EditSoundPanel : MonoBehaviour, IEditSoundEventTriggerDelegate {
        
        private EditSoundEventTrigger _eventTrigger;
        private RectTransform _rect;

        private bool _botScrollviewIsDragging = false;

        public RectTransform sliderWrapperRect = null;
        [SerializeField] UnityEngine.UI.Button moreButton = null;
        [SerializeField] RectTransform whiteBGRect = null;
        [SerializeField] UnityEngine.UI.ScrollRect settingsScrollview = null;

        // -----------------------------------------------

        public enum Visibility { Hidden, Mini, Fullscreen, VisibleAboveKeyboard }
        private Visibility _state = Visibility.Mini;
        public Visibility botPanelState { get { return _state; } }
        public bool isFullscreen { get { return _state == Visibility.Fullscreen; } }

        private static float MiniHeightPercent = 0.5243161094f;
        public float botPanelDefaultHeight { get { return _rect.sizeDelta.y * -MiniHeightPercent; } }

        public void toggleBetweenFullscreenAndMini() {
            SetBottomPanelState(isFullscreen ? Visibility.Mini : Visibility.Fullscreen,
                                            animated: true);
        }

        // -----------------------------------------------

        private void Awake() {
            if (_rect == null) { _rect = GetComponent<RectTransform>(); }
            
            _eventTrigger = GetComponent<EditSoundEventTrigger>();
            _eventTrigger.setDelegate(this);
        }

        public void panelWillAppear() {
            setSettingsScrollRectToTop();

            if (_rect == null) { _rect = GetComponent<RectTransform>(); }
            SetBottomPanelState(Visibility.Hidden, animated: false);
            SetBottomPanelState(Visibility.Mini, animated: true, easing: Ease.OutExpo);
        }

        // -----------------------------------------------------------
        // IEditSoundEventTriggerDelegate
        #region IEditSoundEventTriggerDelegate

        // public bool bottomPanelIsDragging() {
        //     return _eventTrigger.isDragging;
        // }

        public void bottomPanelStartedDragging() {
            if (_state == Visibility.Fullscreen) { whiteBGRect.DOScaleY(1, 0.3f).SetEase(DG.Tweening.Ease.OutExpo); }
        }

        public void bottomPanelFinishedDragging(float yVelocity) {

            float midPoint = Screen.height * 0.68265625f;
            Visibility vis = yVelocity > 12f ? Visibility.Fullscreen : (yVelocity < -12f ? Visibility.Mini : Visibility.Hidden);
            if (vis == Visibility.Hidden) {
                vis = transform.position.y + yVelocity > midPoint
                             ? Visibility.Fullscreen
                             : Visibility.Mini;
            }

            // Debug.Log ("transform.position.y: " + transform.position.y + ", midPoint: " + midPoint);
            // Debug.Log("EditSoundPanel::bottomPanelFinishedDragging yVelocity: " + yVelocity);

            SetBottomPanelState(vis, animated: true, delay: 0, easing: Ease.OutExpo, bottomMargin: 0, 
                                animDuration: 0.6f, forceUpdated: true);
        }

        #endregion
        // -----------------------------------------------------------

        public void bottomScrollViewStartedDragging(float startYPos) {
            _botScrollviewIsDragging = true;
            // _eventTrigger.subScrollViewStartedDragging(startYPos);
            // settingsScrollview.horizontalNormalizedPosition = 1f;
            
        }

        public void bottomScrollViewYMoved(float yPercent) {
            if (!_botScrollviewIsDragging) { return; }
            
            if (yPercent > 1) {
                float yDist = sliderWrapperRect.sizeDelta.y * (yPercent % 1);
                // Debug.Log ("yPercent: " + yPercent + ", yDist: " + yDist);
                _eventTrigger.subScrollViewMoved(yDist);
                
                settingsScrollview.verticalNormalizedPosition = 1f;
            } else {
                if (_eventTrigger.isDragging) {
                    float yDist = sliderWrapperRect.sizeDelta.y * (1f - yPercent);
                    float yDifference = _eventTrigger.subScrollViewMoved(-yDist);

                    if (yDifference < 0) {
                        settingsScrollview.verticalNormalizedPosition = 1f;
                    } else {
                        _eventTrigger.subScrollViewEndedDragging();

                        SetBottomPanelState(Visibility.Fullscreen, animated: false, delay: 0, easing: Ease.OutExpo, bottomMargin: 0,
                                animDuration: 0.6f, forceUpdated: true);
                    }
                }
            }
        }

        public void bottomScrollViewEndedDragging(float endYPos) {
            _botScrollviewIsDragging = false;

            // Debug.Log ("settingsScrollview.verticalNormalizedPosition: " + settingsScrollview.verticalNormalizedPosition);
            if (_eventTrigger.isDragging) {
                _eventTrigger.subScrollViewFinishedDragging(endYPos);
                // settingsScrollview.horizontalNormalizedPosition = 1f;
            }
        }

        // -----------------------------------------------------------

        private void setSettingsScrollRectToTop() {
            Vector3 pos = settingsScrollview.content.anchoredPosition3D;
            pos.y = 0;
            settingsScrollview.content.anchoredPosition3D = pos;
        }

        // -----------------------------------------------------------

        public void setIsVisibleAboveKeyboard() {
            _state = Visibility.VisibleAboveKeyboard;

            UpdateBottomPanel(animated: true, delay: 0, DG.Tweening.Ease.InOutExpo,
                bottomMargin: botPanelDefaultHeight * 0.37f);
        }

        public void SetBottomPanelState(Visibility vis, bool animated = false, float delay = 0, 
            Ease easing = Ease.InOutExpo, float bottomMargin = 0, float animDuration = 0.6f, bool forceUpdated = false) {

            if (_state == vis && !forceUpdated) { return; }

            _state = vis;
            UpdateBottomPanel(animated, delay, easing, bottomMargin, animDuration);
        }

        private void UpdateBottomPanel(bool animated, float delay, Ease easing, float bottomMargin = 0, float animDuration = 0.6f) {
            float botPanelYPos = 0;
            float bgYScale = 1.05f;
            float moreBTNAlpha = _state == Visibility.Fullscreen ? 0 : 1f;
            if (_state == Visibility.Mini || _state == Visibility.VisibleAboveKeyboard) {
                botPanelYPos = botPanelDefaultHeight - bottomMargin;
                bgYScale = 1f;
            } else if (_state == Visibility.Hidden) {
                botPanelYPos = -_rect.sizeDelta.y;
                bgYScale = 1f;
            }

            if (_state == Visibility.Fullscreen) { setSettingsScrollRectToTop(); }

            UnityEngine.CanvasGroup moreBTNCanvasGroup = moreButton.GetComponentInChildren<UnityEngine.CanvasGroup>();
            moreButton.interactable = _state == Visibility.Mini;
            if (!animated) {
                Vector3 pos = _rect.anchoredPosition3D;
                pos.y = botPanelYPos;
                _rect.anchoredPosition3D = pos;
                whiteBGRect.localScale = new Vector3(1f, bgYScale, 1f);
                moreBTNCanvasGroup.alpha = moreBTNAlpha;

            } else {
                if (delay > 0) {
                    moreBTNCanvasGroup.DOFade(moreBTNAlpha, animDuration).SetDelay(delay);
                    _rect.DOAnchorPos3DY(botPanelYPos, animDuration).SetEase(easing).SetDelay(delay);
                    whiteBGRect.DOScaleY(bgYScale, animDuration).SetEase(easing).SetDelay(delay);
                } else {
                    moreBTNCanvasGroup.DOFade(moreBTNAlpha, animDuration);
                    _rect.DOAnchorPos3DY(botPanelYPos, animDuration).SetEase(easing);
                    whiteBGRect.DOScaleY(bgYScale, animDuration).SetEase(easing);
                }
            }
        }
    }
}
