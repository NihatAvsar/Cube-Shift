using System.Collections;
using CubeShift.Player;
using UnityEngine;
using UnityEngine.Events;

namespace CubeShift.Tiles
{
    /// <summary>
    /// A floor cell that blocks entry while closed and becomes walkable when opened.
    /// </summary>
    public sealed class DoorTile : TileBase
    {
        [SerializeField] private bool startsOpen;
        [SerializeField] private Transform doorVisual;
        [SerializeField, Min(0.01f)] private float animationDuration = 0.2f;
        [SerializeField] private UnityEvent onOpened;
        [SerializeField] private UnityEvent onClosed;

        private bool isOpen;
        private Coroutine animationRoutine;
        private Vector3 closedScale;

        public bool IsOpen => isOpen;
        public override TileEffectType EffectType => TileEffectType.Door;

        private void Awake()
        {
            closedScale = doorVisual != null ? doorVisual.localScale : Vector3.one;
            SetOpenImmediate(startsOpen);
        }

        public override bool CanPlayerEnter(PlayerCubeController player)
        {
            return base.CanPlayerEnter(player) && isOpen;
        }

        public void Open()
        {
            SetOpen(true);
        }

        public void Close()
        {
            SetOpen(false);
        }

        public void Toggle()
        {
            SetOpen(!isOpen);
        }

        private void SetOpen(bool open)
        {
            if (isOpen == open)
            {
                return;
            }

            isOpen = open;
            if (doorVisual != null && animationRoutine != null)
            {
                StopCoroutine(animationRoutine);
            }

            if (doorVisual != null)
            {
                animationRoutine = StartCoroutine(AnimateDoorRoutine(open));
            }

            if (open)
            {
                onOpened?.Invoke();
            }
            else
            {
                onClosed?.Invoke();
            }
        }

        private void SetOpenImmediate(bool open)
        {
            isOpen = open;
            if (doorVisual != null)
            {
                doorVisual.localScale = open ? Vector3.zero : closedScale;
            }
        }

        private IEnumerator AnimateDoorRoutine(bool opening)
        {
            Vector3 startScale = doorVisual.localScale;
            Vector3 targetScale = opening ? Vector3.zero : closedScale;
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / animationDuration));
                doorVisual.localScale = Vector3.Lerp(startScale, targetScale, progress);
                yield return null;
            }

            doorVisual.localScale = targetScale;
            animationRoutine = null;
        }
    }
}
