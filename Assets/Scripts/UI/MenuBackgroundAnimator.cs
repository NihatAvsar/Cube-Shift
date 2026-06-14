using UnityEngine;
using UnityEngine.UI;

namespace CubeShift.UI
{
    public sealed class MenuBackgroundAnimator : MonoBehaviour
    {
        [SerializeField] private RectTransform[] driftingShapes;
        [SerializeField] private Image[] pulseImages;
        [SerializeField] private float driftAmount = 18f;
        [SerializeField] private float driftSpeed = 0.35f;
        [SerializeField] private float rotationSpeed = 6f;

        private Vector2[] startPositions;
        private float[] phaseOffsets;

        private void Awake()
        {
            startPositions = new Vector2[driftingShapes != null ? driftingShapes.Length : 0];
            phaseOffsets = new float[startPositions.Length];
            for (int i = 0; i < startPositions.Length; i++)
            {
                if (driftingShapes[i] == null)
                {
                    continue;
                }

                startPositions[i] = driftingShapes[i].anchoredPosition;
                phaseOffsets[i] = i * 1.71f;
            }
        }

        private void Update()
        {
            float time = Time.unscaledTime;
            for (int i = 0; i < startPositions.Length; i++)
            {
                RectTransform shape = driftingShapes[i];
                if (shape == null)
                {
                    continue;
                }

                float phase = time * driftSpeed + phaseOffsets[i];
                shape.anchoredPosition = startPositions[i] + new Vector2(Mathf.Sin(phase), Mathf.Cos(phase * 0.8f)) * driftAmount;
                shape.Rotate(Vector3.forward, rotationSpeed * Time.unscaledDeltaTime * (i % 2 == 0 ? 1f : -1f));
            }

            if (pulseImages == null)
            {
                return;
            }

            for (int i = 0; i < pulseImages.Length; i++)
            {
                Image image = pulseImages[i];
                if (image == null)
                {
                    continue;
                }

                Color color = image.color;
                color.a = Mathf.Lerp(0.12f, 0.34f, (Mathf.Sin(time * 0.9f + i) + 1f) * 0.5f);
                image.color = color;
            }
        }

        public void Configure(RectTransform[] shapes, Image[] pulses)
        {
            driftingShapes = shapes;
            pulseImages = pulses;
            Awake();
        }
    }
}
