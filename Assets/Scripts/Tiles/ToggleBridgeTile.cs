using CubeShift.Player;
using UnityEngine;

namespace CubeShift.Tiles
{
    [DisallowMultipleComponent]
    public sealed class ToggleBridgeTile : TileBase
    {
        [SerializeField] private string linkId;
        [SerializeField] private bool startsActive = true;
        [SerializeField] private bool isActive = true;
        [SerializeField] private GameObject visual;
        [SerializeField] private Collider tileCollider;
        [SerializeField, Range(0f, 1f)] private float inactiveVisualScale = 0.2f;

        private Renderer[] renderers;
        private Vector3 visualOpenScale = Vector3.one;

        public string LinkId => linkId;
        public bool IsActive => isActive;
        public override TileEffectType EffectType => TileEffectType.None;

        private void Awake()
        {
            CacheReferences();
            SetActiveState(startsActive);
        }

        public override bool CanPlayerEnter(PlayerCubeController player)
        {
            return base.CanPlayerEnter(player) && isActive;
        }

        public void Configure(string newLinkId, bool newStartsActive)
        {
            linkId = newLinkId;
            startsActive = newStartsActive;
            CacheReferences();
            SetActiveState(startsActive);
        }

        public void ActivateBridge()
        {
            SetActiveState(true);
        }

        public void DeactivateBridge()
        {
            SetActiveState(false);
        }

        public void ToggleBridge()
        {
            SetActiveState(!isActive);
        }

        public void SetActiveState(bool state)
        {
            isActive = state;
            SetWalkable(state);

            if (visual != null)
            {
                visual.SetActive(true);
                visual.transform.localScale = state ? visualOpenScale : visualOpenScale * inactiveVisualScale;
                return;
            }

            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<Renderer>();
            }

            foreach (Renderer tileRenderer in renderers)
            {
                if (tileRenderer != null)
                {
                    tileRenderer.enabled = true;
                }
            }
        }

        private void CacheReferences()
        {
            if (visual == null)
            {
                visual = gameObject;
            }

            visualOpenScale = visual.transform.localScale;

            if (tileCollider == null)
            {
                tileCollider = GetComponentInChildren<Collider>();
            }

            renderers = GetComponentsInChildren<Renderer>();
        }
    }
}
