using System.Collections;
using CubeShift.Player;
using CubeShift.Core;
using CubeShift.LevelDesign;
using UnityEngine;
using UnityEngine.Events;

namespace CubeShift.Tiles
{
    /// <summary>
    /// Button tile foundation. It can later open doors or power objects when the yellow face is down.
    /// </summary>
    public sealed class ButtonTile : TileBase
    {
        [System.Serializable]
        public sealed class PlayerCubeEvent : UnityEvent<PlayerCubeController>
        {
        }

        [Header("Activation Rules")]
        [SerializeField] private bool requireYellowBottomFace;
        [SerializeField] private bool triggerOnce = true;

        [Header("Visual Feedback")]
        [SerializeField] private Color activatedColor = new Color(0.2f, 1f, 0.35f);
        [SerializeField] private Color rejectedColor = new Color(1f, 0.2f, 0.15f);
        [SerializeField, Min(0.05f)] private float rejectedFlashDuration = 0.35f;

        [Header("Events")]
        [SerializeField] private DoorTile[] controlledDoors;
        [SerializeField] private PlayerCubeEvent onActivated;
        [SerializeField] private UnityEvent onRejected;

        private bool hasActivated;
        private Renderer[] tileRenderers;
        private MaterialPropertyBlock propertyBlock;
        private Coroutine feedbackRoutine;

        public bool HasActivated => hasActivated;
        public override TileEffectType EffectType => TileEffectType.Button;

        private void Awake()
        {
            tileRenderers = GetComponentsInChildren<Renderer>();
            propertyBlock = new MaterialPropertyBlock();
        }

        protected override void HandlePlayerLanded(PlayerCubeController player)
        {
            if (triggerOnce && hasActivated)
            {
                return;
            }

            if (!requireYellowBottomFace || player.BottomFace == CubeFace.Yellow)
            {
                hasActivated = true;
                LevelManager.Instance?.RegisterObjective(LevelObjective.YellowButton);
                ShowColor(activatedColor);
                OpenControlledDoors();
                onActivated?.Invoke(player);
                return;
            }

            Debug.Log($"Yellow button rejected. Current bottom face: {player.BottomFace}.", this);
            if (feedbackRoutine != null)
            {
                StopCoroutine(feedbackRoutine);
            }
            feedbackRoutine = StartCoroutine(ShowRejectedFeedback());
            onRejected?.Invoke();
        }

        public void ResetButton()
        {
            hasActivated = false;
            ClearColorOverride();
        }

        public void SetControlledDoors(DoorTile[] doors)
        {
            controlledDoors = doors ?? System.Array.Empty<DoorTile>();
        }

        private void OpenControlledDoors()
        {
            foreach (DoorTile door in controlledDoors)
            {
                if (door != null)
                {
                    door.Open();
                }
            }
        }

        private IEnumerator ShowRejectedFeedback()
        {
            ShowColor(rejectedColor);
            yield return new WaitForSeconds(rejectedFlashDuration);
            ClearColorOverride();
            feedbackRoutine = null;
        }

        private void ShowColor(Color color)
        {
            if (tileRenderers == null || propertyBlock == null)
            {
                return;
            }

            foreach (Renderer tileRenderer in tileRenderers)
            {
                if (tileRenderer == null)
                {
                    continue;
                }

                tileRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_BaseColor", color);
                propertyBlock.SetColor("_Color", color);
                tileRenderer.SetPropertyBlock(propertyBlock);
            }
        }

        private void ClearColorOverride()
        {
            if (tileRenderers == null)
            {
                return;
            }

            foreach (Renderer tileRenderer in tileRenderers)
            {
                if (tileRenderer != null)
                {
                    tileRenderer.SetPropertyBlock(null);
                }
            }
        }
    }
}
