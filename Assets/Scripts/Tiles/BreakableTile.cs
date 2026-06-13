using System.Collections;
using CubeShift.Player;
using UnityEngine;
using UnityEngine.Events;

namespace CubeShift.Tiles
{
    /// <summary>
    /// Breakable tile foundation. It can be enabled later for red-face interactions.
    /// </summary>
    public sealed class BreakableTile : TileBase
    {
        [Header("Break Rules")]
        [SerializeField] private bool breakWhenRedFaceLands;
        [SerializeField] private bool requireRedBottomFace = true;
        [SerializeField, Min(0f)] private float breakDelay = 0.05f;

        [Header("Events")]
        [SerializeField] private UnityEvent onBroken;

        private bool isBroken;

        public bool IsBroken => isBroken;

        protected override void HandlePlayerLanded(PlayerCubeController player)
        {
            if (!breakWhenRedFaceLands || isBroken)
            {
                return;
            }

            if (CanBreak(player))
            {
                Break();
            }
        }

        public bool CanBreak(PlayerCubeController player)
        {
            if (player == null)
            {
                return false;
            }

            return !requireRedBottomFace || player.BottomFace == CubeFace.Red;
        }

        public void Break()
        {
            if (isBroken)
            {
                return;
            }

            isBroken = true;
            onBroken?.Invoke();

            if (breakDelay > 0f)
            {
                StartCoroutine(BreakRoutine());
            }
            else
            {
                ApplyBrokenState();
            }
        }

        private IEnumerator BreakRoutine()
        {
            yield return new WaitForSeconds(breakDelay);
            ApplyBrokenState();
        }

        private void ApplyBrokenState()
        {
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider tileCollider in colliders)
            {
                tileCollider.enabled = false;
            }

            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer tileRenderer in renderers)
            {
                tileRenderer.enabled = false;
            }
        }
    }
}
