using System;
using System.Collections;
using Com.Krackhet.Runtime.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Com.Krackhet.Runtime.UI.Components
{
    public class UIButton : Selectable, IPointerClickHandler, ISubmitHandler
    {
        #region Events & Delegates
        public UnityEvent onClick = new UnityEvent();
        public UnityEvent onPressed = new UnityEvent();
        public UnityEvent onReleased = new UnityEvent();
        #endregion

        #region Serialized Fields
        [SerializeField]
        private Graphic _targetGraphic;

        [SerializeField]
        private ButtonInteractionFXType _interactionFXType;

        [SerializeField]
        private float _interactionFXDuration = 0.05f;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowIf("@HasInteractionFXType(ButtonInteractionFXType.Fade)")]
#endif
        [Range(0f, 1f)]
        [SerializeField]
        private float _fadeTargetAlpha = 0.5f;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowIf("@HasInteractionFXType(ButtonInteractionFXType.Scale)")]
#endif
        [SerializeField]
        private float _scaleDownFactor = 0.9f;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowIf("@HasInteractionFXType(ButtonInteractionFXType.Drop)")]
#endif
        [SerializeField]
        private float _dropDownDistance = 20f;

#if UNITY_EDITOR
        [SerializeField]
        [HideInInspector]
        private bool _migratedInteractable = true;
#endif
        #endregion

        #region Private Fields
        private Coroutine _submitCoroutine;
        private bool _isPressed;
        private const string ButtonSfxName = "UI-sfx";
        #endregion

        #region Unity Callbacks
        protected override void Awake()
        {
            base.Awake();

            transition = Transition.None;

            if (_targetGraphic != null)
                base.targetGraphic = _targetGraphic;

#if UNITY_EDITOR
            interactable = _migratedInteractable;
#endif
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (_targetGraphic != null)
                base.targetGraphic = _targetGraphic;
        }
#endif

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            if (_targetGraphic == null || !gameObject.activeInHierarchy)
                return;

            switch (state)
            {
                case SelectionState.Disabled:
                    _targetGraphic.raycastTarget = false;
                    _targetGraphic.CrossFadeColor(
                        Color.gray, colors.fadeDuration, true, true);
                    break;

                case SelectionState.Normal:
                case SelectionState.Highlighted:
                    _targetGraphic.raycastTarget = true;
                    _targetGraphic.CrossFadeColor(
                        Color.white, colors.fadeDuration, true, true);
                    break;
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);

            if (!IsInteractable() || !IsActive() || _targetGraphic == null)
                return;

            _isPressed = true;
            ApplyInteractionFX();
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);

            _isPressed = false;

            if (!IsInteractable() || !IsActive() || _targetGraphic == null)
                return;

            ResetInteractionFX();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            if (_isPressed)
                ApplyInteractionFX();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            if (_isPressed)
                ResetInteractionFX();
        }
        #endregion

        #region IPointerClickHandler
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
                return;

            if (_submitCoroutine != null)
                StopCoroutine(_submitCoroutine);

            _submitCoroutine = StartCoroutine(HandleSubmit());
        }
        #endregion

        #region ISubmitHandler
        public void OnSubmit(BaseEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
                return;

            Press();
        }
        #endregion

        #region Editor Helpers
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button("Contain target graphic")]
        private void ContainTargetGraphic()
        {
            if (_targetGraphic == null)
                return;

            RectTransform buttonRect = GetComponent<RectTransform>();
            if (buttonRect == null)
                return;

            buttonRect.sizeDelta = _targetGraphic.rectTransform.sizeDelta;
            buttonRect.anchoredPosition = _targetGraphic.rectTransform.anchoredPosition;
            _targetGraphic.rectTransform.anchoredPosition = Vector2.zero;
        }

        [Sirenix.OdinInspector.Button("Fit target graphic")]
        private void FitTargetGraphic()
        {
            if (_targetGraphic == null)
                return;

            RectTransform buttonRect = GetComponent<RectTransform>();
            if (buttonRect == null)
                return;

            _targetGraphic.rectTransform.sizeDelta = buttonRect.sizeDelta;
            _targetGraphic.rectTransform.anchoredPosition = Vector2.zero;
        }
#endif
        #endregion

        #region FX Parameters (for external readers)
        internal float InteractionFXDuration => _interactionFXDuration;
        internal float FadeTargetAlpha => _fadeTargetAlpha;
        internal float ScaleDownFactor => _scaleDownFactor;
        internal float DropDownDistance => _dropDownDistance;

        internal bool HasInteractionFXType(ButtonInteractionFXType type)
        {
            return (_interactionFXType & type) == type;
        }
        #endregion

        #region Private Methods
        private void ApplyInteractionFX()
        {
            if (_targetGraphic == null)
                return;

            if (HasInteractionFXType(ButtonInteractionFXType.Fade))
                StartCoroutine(FadeFX());

            if (HasInteractionFXType(ButtonInteractionFXType.Scale))
                StartCoroutine(ScaleDownFX());

            if (HasInteractionFXType(ButtonInteractionFXType.Drop))
                StartCoroutine(DropDownFX());

            onPressed?.Invoke();
        }

        private void ResetInteractionFX()
        {
            if (_targetGraphic == null)
                return;

            if (HasInteractionFXType(ButtonInteractionFXType.Scale))
                StartCoroutine(ResetScaleFX());

            if (HasInteractionFXType(ButtonInteractionFXType.Drop))
                StartCoroutine(ResetPositionFX());

            if (HasInteractionFXType(ButtonInteractionFXType.Fade))
                StartCoroutine(ResetFadeFX());

            onReleased?.Invoke();
        }

        private IEnumerator HandleSubmit()
        {
            yield return new WaitForSeconds(_interactionFXDuration);
            Press();
        }

        private void Press()
        {
            onClick?.Invoke();
            GameInternalManager.AudioManager?.PlayAudio(ButtonSfxName, 0);
        }
        #endregion

        #region Interaction FX Coroutines
        private IEnumerator FadeFX()
        {
            Color originalColor = _targetGraphic.color;
            Color targetColor = new Color(
                originalColor.r, originalColor.g, originalColor.b, _fadeTargetAlpha);
            float timer = 0f;

            while (timer <= _interactionFXDuration)
            {
                _targetGraphic.color = Color.Lerp(
                    originalColor, targetColor, timer / _interactionFXDuration);
                timer += Time.deltaTime;
                yield return null;
            }
            _targetGraphic.color = targetColor;
        }

        private IEnumerator ResetFadeFX()
        {
            Color currentColor = _targetGraphic.color;
            Color targetColor = new Color(
                currentColor.r, currentColor.g, currentColor.b, 1f);
            float timer = 0f;

            while (timer <= _interactionFXDuration)
            {
                _targetGraphic.color = Color.Lerp(
                    currentColor, targetColor, timer / _interactionFXDuration);
                timer += Time.deltaTime;
                yield return null;
            }
            _targetGraphic.color = targetColor;
        }

        private IEnumerator ScaleDownFX()
        {
            Vector3 originalScale = _targetGraphic.transform.localScale;
            Vector3 targetScale = originalScale * _scaleDownFactor;
            float timer = 0f;

            while (timer < _interactionFXDuration)
            {
                _targetGraphic.transform.localScale = Vector3.Lerp(
                    originalScale, targetScale, timer / _interactionFXDuration);
                timer += Time.deltaTime;
                yield return null;
            }
            _targetGraphic.transform.localScale = targetScale;
        }

        private IEnumerator ResetScaleFX()
        {
            Vector3 currentScale = _targetGraphic.transform.localScale;
            float timer = 0f;

            while (timer <= _interactionFXDuration)
            {
                _targetGraphic.transform.localScale = Vector3.Lerp(
                    currentScale, Vector3.one, timer / _interactionFXDuration);
                timer += Time.deltaTime;
                yield return null;
            }
            _targetGraphic.transform.localScale = Vector3.one;
        }

        private IEnumerator DropDownFX()
        {
            Vector3 originalPosition = _targetGraphic.transform.localPosition;
            Vector3 targetPosition = originalPosition
                + Vector3.up * -_dropDownDistance;
            float timer = 0f;

            while (timer <= _interactionFXDuration)
            {
                _targetGraphic.transform.localPosition = Vector3.Lerp(
                    originalPosition, targetPosition, timer / _interactionFXDuration);
                timer += Time.deltaTime;
                yield return null;
            }
            _targetGraphic.transform.localPosition = targetPosition;
        }

        private IEnumerator ResetPositionFX()
        {
            Vector3 currentPosition = _targetGraphic.transform.localPosition;
            float timer = 0f;

            while (timer <= _interactionFXDuration)
            {
                _targetGraphic.transform.localPosition = Vector3.Lerp(
                    currentPosition, Vector3.zero, timer / _interactionFXDuration);
                timer += Time.deltaTime;
                yield return null;
            }
            _targetGraphic.transform.localPosition = Vector3.zero;
        }
        #endregion
    }

    [Flags]
    public enum ButtonInteractionFXType
    {
        None = 0,
        Fade = 1 << 0,
        Scale = 1 << 1,
        Drop = 1 << 2,
    }
}
