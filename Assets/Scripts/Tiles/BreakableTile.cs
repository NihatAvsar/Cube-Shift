using System.Collections;
using CubeShift.Player;
using CubeShift.Core;
using CubeShift.LevelDesign;
using UnityEngine;
using UnityEngine.Events;

namespace CubeShift.Tiles
{
    /// <summary>
    /// Reacts to the red face by blinking temporarily, then restores itself.
    /// </summary>
    public sealed class BreakableTile : TileBase
    {
        [Header("Break Rules")]
        [SerializeField] private bool breakWhenRedFaceLands = true;
        [SerializeField] private bool requireRedBottomFace;
        [SerializeField, Min(0f)] private float safeExitDuration = 1f;
        [SerializeField, Min(0.1f)] private float blinkDuration = 3f;
        [SerializeField, Min(0.05f)] private float blinkInterval = 0.2f;

        [Header("Events")]
        [SerializeField] private UnityEvent onBroken;

        private bool isBroken;
        private Renderer[] tileRenderers;

        public bool IsBroken => isBroken;
        public override TileEffectType EffectType => TileEffectType.Breakable;

        private void Awake()
        {
            tileRenderers = GetComponentsInChildren<Renderer>();
        }

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
            LevelManager.Instance?.RegisterObjective(LevelObjective.RedBreakable);
            onBroken?.Invoke();
            StartCoroutine(BlinkAndRestoreRoutine());
        }

        private IEnumerator BlinkAndRestoreRoutine()
        {
            if (safeExitDuration > 0f)
            {
                yield return new WaitForSeconds(safeExitDuration);
            }

            PlayerCubeController player = FindAnyObjectByType<PlayerCubeController>();
            bool playerWasStandingOnTile = player != null && player.IsStandingOn(this);

            SetCollidersEnabled(false);

            if (playerWasStandingOnTile)
            {
                player.RecheckStandingTile();
            }

            float elapsed = 0f;
            bool renderersVisible = true;

            while (elapsed < blinkDuration)
            {
                renderersVisible = !renderersVisible;
                SetRenderersEnabled(renderersVisible);
                yield return new WaitForSeconds(blinkInterval);
                elapsed += blinkInterval;
            }

            SetRenderersEnabled(true);
            SetCollidersEnabled(true);
            isBroken = false;
        }

        private void SetRenderersEnabled(bool enabled)
        {
            if (tileRenderers == null || tileRenderers.Length == 0)
            {
                tileRenderers = GetComponentsInChildren<Renderer>();
            }

            foreach (Renderer tileRenderer in tileRenderers)
            {
                if (tileRenderer != null)
                {
                    tileRenderer.enabled = enabled;
                }
            }
        }

        private void SetCollidersEnabled(bool enabled)
        {
            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider tileCollider in colliders)
            {
                if (tileCollider != null)
                {
                    tileCollider.enabled = enabled;
                }
            }
        }
    }
}
