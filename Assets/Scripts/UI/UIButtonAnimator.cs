using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CubeShift.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class UIButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private float hoverScale = 1.04f;
        [SerializeField] private float pressScale = 0.97f;
        [SerializeField] private float animationSpeed = 14f;
        [SerializeField] private Color normalColor = new(0.09f, 0.14f, 0.2f, 0.9f);
        [SerializeField] private Color highlightedColor = new(0.12f, 0.35f, 0.44f, 0.95f);
        [SerializeField] private Color selectedColor = new(0.14f, 0.58f, 0.72f, 1f);

        private RectTransform rectTransform;
        private Graphic targetGraphic;
        private Coroutine animationRoutine;
        private bool selected;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            targetGraphic = GetComponent<Graphic>();
            if (targetGraphic != null)
            {
                normalColor = targetGraphic.color;
            }
        }

        public void SetColors(Color normal, Color highlighted, Color selectedState)
        {
            normalColor = normal;
            highlightedColor = highlighted;
            selectedColor = selectedState;
            if (targetGraphic != null)
            {
                targetGraphic.color = normalColor;
            }
        }

        public void SetSelectedVisual(bool value)
        {
            selected = value;
            AnimateTo(value ? hoverScale : 1f, value ? selectedColor : normalColor);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            AnimateTo(hoverScale, selected ? selectedColor : highlightedColor);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            AnimateTo(selected ? hoverScale : 1f, selected ? selectedColor : normalColor);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            AnimateTo(pressScale, highlightedColor);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            AnimateTo(selected ? hoverScale : 1f, selected ? selectedColor : highlightedColor);
        }

        public void OnSelect(BaseEventData eventData)
        {
            AnimateTo(hoverScale, selected ? selectedColor : highlightedColor);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            AnimateTo(selected ? hoverScale : 1f, selected ? selectedColor : normalColor);
        }

        private void AnimateTo(float scale, Color color)
        {
            if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
            {
                ApplyInstant(Vector3.one * scale, color);
                return;
            }

            if (animationRoutine != null)
            {
                StopCoroutine(animationRoutine);
            }

            animationRoutine = StartCoroutine(AnimateRoutine(Vector3.one * scale, color));
        }

        private void ApplyInstant(Vector3 scale, Color color)
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            if (targetGraphic == null)
            {
                targetGraphic = GetComponent<Graphic>();
            }

            rectTransform.localScale = scale;
            if (targetGraphic != null)
            {
                targetGraphic.color = color;
            }
        }

        private IEnumerator AnimateRoutine(Vector3 targetScale, Color targetColor)
        {
            Vector3 startScale = rectTransform.localScale;
            Color startColor = targetGraphic != null ? targetGraphic.color : Color.white;
            float elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.unscaledDeltaTime * animationSpeed;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed));
                rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
                if (targetGraphic != null)
                {
                    targetGraphic.color = Color.Lerp(startColor, targetColor, t);
                }

                yield return null;
            }

            rectTransform.localScale = targetScale;
            if (targetGraphic != null)
            {
                targetGraphic.color = targetColor;
            }

            animationRoutine = null;
        }
    }
}
