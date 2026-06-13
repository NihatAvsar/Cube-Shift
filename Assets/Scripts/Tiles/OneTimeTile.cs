using System.Collections;
using CubeShift.Core;
using CubeShift.LevelDesign;
using CubeShift.Player;
using UnityEngine;
using UnityEngine.Events;

namespace CubeShift.Tiles
{
    [DisallowMultipleComponent]
    public sealed class OneTimeTile : TileBase
    {
        [SerializeField] private bool breakAfterLeaving = true;
        [SerializeField] private bool hasBeenSteppedOn;
        [SerializeField] private bool isBroken;
        [SerializeField, Min(0f)] private float breakDelay = 0.1f;
        [SerializeField] private UnityEvent onBroken;

        private Renderer[] renderers;
        private Collider[] colliders;
        private Coroutine breakRoutine;

        public bool HasBeenSteppedOn => hasBeenSteppedOn;
        public bool IsBroken => isBroken;
        public override TileEffectType EffectType => TileEffectType.Breakable;

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();
            colliders = GetComponentsInChildren<Collider>();
        }

        public override bool CanPlayerEnter(PlayerCubeController player)
        {
            return base.CanPlayerEnter(player) && !isBroken;
        }

        protected override void HandlePlayerLanded(PlayerCubeController player)
        {
            if (!isBroken)
            {
                hasBeenSteppedOn = true;
            }
        }

        public override void OnPlayerLeft(PlayerCubeController player)
        {
            if (!breakAfterLeaving || !hasBeenSteppedOn || isBroken || breakRoutine != null)
            {
                return;
            }

            breakRoutine = StartCoroutine(BreakAfterDelayRoutine());
        }

        public void BreakTile()
        {
            if (isBroken)
            {
                return;
            }

            isBroken = true;
            SetWalkable(false);
            Debug.Log($"[OneTimeTile] '{name}' broken. Registering LevelObjective.OneTime.", this);
            LevelManager.Instance?.RegisterObjective(LevelObjective.OneTime);
            SetCollidersEnabled(false);
            SetRenderersEnabled(false);
            onBroken?.Invoke();
        }

        private IEnumerator BreakAfterDelayRoutine()
        {
            if (breakDelay > 0f)
            {
                yield return new WaitForSeconds(breakDelay);
            }

            BreakTile();
            breakRoutine = null;
        }

        private void SetRenderersEnabled(bool enabled)
        {
            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<Renderer>();
            }

            foreach (Renderer tileRenderer in renderers)
            {
                if (tileRenderer != null)
                {
                    tileRenderer.enabled = enabled;
                }
            }
        }

        private void SetCollidersEnabled(bool enabled)
        {
            if (colliders == null || colliders.Length == 0)
            {
                colliders = GetComponentsInChildren<Collider>();
            }

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
