using System;
using System.Collections;
using Com.Krackhet.Runtime.Managers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Com.Krackhet.Runtime.UI.Components
{
    public class UIButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Graphic targetGraphic;
        [SerializeField] private ButtonInteractionFXType interactionFXType;
        [SerializeField] private float interactionFXDuration = 0.05f;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowIf("@HasInteractionFXType(ButtonInteractionFXType.Fade)")]
#endif
        [Range(0f, 1f)]
        [SerializeField] private float fadeTargetAlpha = 0.5f;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowIf("@HasInteractionFXType(ButtonInteractionFXType.Scale)")]
#endif
        [SerializeField] private float scaleDownFactor = 0.9f;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowIf("@HasInteractionFXType(ButtonInteractionFXType.Drop)")]
#endif
        [SerializeField] private float dropDownDistance = 20f;
        [SerializeField] private bool m_interactable = true;

        public bool interactable
        {
            get => m_interactable;
            set
            {
                m_interactable = value;
                if (targetGraphic != null)
                {
                    targetGraphic.raycastTarget = value;
                    targetGraphic.color = value ? Color.white : Color.gray; // Simple visual feedback for interactability
                }
            }
        }
        public UnityEvent onClick = new UnityEvent();

        private Coroutine submitCoroutine;
        private CanvasGroup canvasGroup;
        private bool _isPointerDown;
        private bool _isPointerInside;
        private const string buttonSFXName = "UI-sfx";

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button("Contain target graphic")]
        private void ContainTargetGraphic()
        {
            if (targetGraphic == null) return;

            RectTransform buttonRect = GetComponent<RectTransform>();

            if (buttonRect == null) return;

            // Adjust the size of the button to match the target graphic
            buttonRect.sizeDelta = targetGraphic.rectTransform.sizeDelta;

            // Jump to the target graphic's position
            buttonRect.anchoredPosition = targetGraphic.rectTransform.anchoredPosition;
            targetGraphic.rectTransform.anchoredPosition = Vector2.zero; // Reset target graphic's position to be relative to the button
        }
        [Sirenix.OdinInspector.Button("Fit target graphic")]
        private void FitTargetGraphic()
        {
            if (targetGraphic == null) return;

            RectTransform buttonRect = GetComponent<RectTransform>();

            if (buttonRect == null) return;

            // Adjust the size of the target graphic to match the button
            targetGraphic.rectTransform.sizeDelta = buttonRect.sizeDelta;

            // Reset target graphic's position to be centered within the button
            targetGraphic.rectTransform.anchoredPosition = Vector2.zero;
        }
#endif

        private void ApplyInteractionFX()
        {
            if (targetGraphic == null) return;

            if (HasInteractionFXType(ButtonInteractionFXType.Fade))
            {
                StartCoroutine(FadeFX());
            }

            if (HasInteractionFXType(ButtonInteractionFXType.Scale))
            {
                StartCoroutine(ScaleDownFX());
            }

            if (HasInteractionFXType(ButtonInteractionFXType.Drop))
            {
                StartCoroutine(DropDownFX());
            }
        }

        private void ResetInteractionFX()
        {
            if (targetGraphic == null) return;

            // Reset all effects to default state
            if (HasInteractionFXType(ButtonInteractionFXType.Scale))
            {
                StartCoroutine(ResetScaleFX());
            }

            if (HasInteractionFXType(ButtonInteractionFXType.Drop))
            {
                StartCoroutine(ResetPos());
            }

            if (HasInteractionFXType(ButtonInteractionFXType.Fade))
            {
                StartCoroutine(ResetFadeFX());
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!targetGraphic.IsActive() || !interactable) return;
            _isPointerDown = true;
            _isPointerInside = true;
            ApplyInteractionFX();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isPointerInside = true;
            if (_isPointerDown)
            {
                ApplyInteractionFX();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isPointerInside = false;
            if (_isPointerDown)
            {
                ResetInteractionFX();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!targetGraphic.IsActive() || !interactable) return;
            _isPointerDown = false;
            ResetInteractionFX();
            if (!_isPointerInside) return;
            if (submitCoroutine != null)
            {
                StopCoroutine(submitCoroutine);
            }
            submitCoroutine = StartCoroutine(OnSubmit());
        }

        public IEnumerator OnSubmit()
        {
            yield return new WaitForSeconds(interactionFXDuration);
            Press();
        }

        private void Press()
        {
            onClick?.Invoke();
            GameInternalManager.AudioManager?.PlaySound(buttonSFXName); // Play button click sound effect if assigned
        }

        private bool HasInteractionFXType(ButtonInteractionFXType type)
        {
            return (interactionFXType & type) == type;
        }

        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }


        #region --------------- Interaction FX Coroutines ---------------
        private IEnumerator FadeFX()
        {
            float timer = 0f;

            while (timer <= interactionFXDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, fadeTargetAlpha, timer / interactionFXDuration);
                timer += Time.deltaTime;
                yield return null;
            }

            canvasGroup.alpha = fadeTargetAlpha;
        }

        private IEnumerator ResetFadeFX()
        {
            float timer = 0f;

            while (timer <= interactionFXDuration)
            {
                canvasGroup.alpha = Mathf.Lerp(fadeTargetAlpha, 1f, timer / interactionFXDuration);
                timer += Time.deltaTime;
                yield return null;
            }

            canvasGroup.alpha = 1f;
        }

        private IEnumerator ScaleDownFX()
        {
            Vector3 originalScale = targetGraphic.transform.localScale;
            Vector3 targetScale = originalScale * scaleDownFactor;
            float timer = 0f;

            while (timer < interactionFXDuration)
            {
                targetGraphic.transform.localScale = Vector3.Lerp(originalScale, targetScale, timer / interactionFXDuration);
                timer += Time.deltaTime;
                yield return null;
            }

            targetGraphic.transform.localScale = targetScale;
        }

        private IEnumerator ResetScaleFX()
        {
            Vector3 targetScale = Vector3.one;
            Vector3 currentScale = targetGraphic.transform.localScale;
            float timer = 0f;

            while (timer <= interactionFXDuration)
            {
                targetGraphic.transform.localScale = Vector3.Lerp(currentScale, targetScale, timer / interactionFXDuration);
                timer += Time.deltaTime;
                yield return null;
            }

            targetGraphic.transform.localScale = targetScale;
        }

        private IEnumerator DropDownFX()
        {
            Vector3 originalPosition = targetGraphic.transform.localPosition;
            Vector3 targetPosition = originalPosition + Vector3.up * -dropDownDistance; // Drop down by dropDownDistance units
            float timer = 0f;

            while (timer <= interactionFXDuration)
            {
                targetGraphic.transform.localPosition = Vector3.Lerp(originalPosition, targetPosition, timer / interactionFXDuration);
                timer += Time.deltaTime;
                yield return null;
            }

            targetGraphic.transform.localPosition = targetPosition;
        }

        private IEnumerator ResetPos()
        {
            Vector3 targetPosition = Vector3.zero;
            Vector3 currentPosition = targetGraphic.transform.localPosition;
            float timer = 0f;

            while (timer <= interactionFXDuration)
            {
                targetGraphic.transform.localPosition = Vector3.Lerp(currentPosition, targetPosition, timer / interactionFXDuration);
                timer += Time.deltaTime;
                yield return null;
            }

            targetGraphic.transform.localPosition = targetPosition;
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