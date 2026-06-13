using CubeShift.Core;
using CubeShift.LevelDesign;
using CubeShift.Player;
using UnityEngine;
using UnityEngine.Events;

namespace CubeShift.Tiles
{
    [DisallowMultipleComponent]
    public sealed class PressurePlateTile : TileBase
    {
        [SerializeField] private string linkId;
        [SerializeField] private RequiredCubeFace requiredFace = RequiredCubeFace.Any;
        [SerializeField] private PlateActivationMode activationMode = PlateActivationMode.ActivateOnly;
        [SerializeField] private bool singleUse;
        [SerializeField] private bool hasBeenUsed;
        [SerializeField] private bool requirePlayerLandedFully = true;
        [SerializeField] private ToggleBridgeTile[] controlledBridges;
        [SerializeField] private UnityEvent onActivated;
        [SerializeField] private UnityEvent onRejected;

        public string LinkId => linkId;
        public RequiredCubeFace RequiredFace => requiredFace;
        public PlateActivationMode ActivationMode => activationMode;
        public bool SingleUse => singleUse;
        public bool HasBeenUsed => hasBeenUsed;
        public override TileEffectType EffectType => TileEffectType.Button;

        public void Configure(
            string newLinkId,
            RequiredCubeFace newRequiredFace,
            PlateActivationMode newActivationMode,
            bool newSingleUse,
            bool newRequirePlayerLandedFully = true)
        {
            linkId = newLinkId;
            requiredFace = newRequiredFace;
            activationMode = newActivationMode;
            singleUse = newSingleUse;
            requirePlayerLandedFully = newRequirePlayerLandedFully;
            hasBeenUsed = false;
        }

        public void SetControlledBridges(ToggleBridgeTile[] bridges)
        {
            controlledBridges = bridges ?? System.Array.Empty<ToggleBridgeTile>();
        }

        protected override void HandlePlayerLanded(PlayerCubeController player)
        {
            if (singleUse && hasBeenUsed)
            {
                return;
            }

            if (requirePlayerLandedFully && (player.IsMoving || player.IsFalling))
            {
                return;
            }

            if (!MatchesRequiredFace(player.BottomFace))
            {
                Debug.Log($"[PressurePlate] '{linkId}' rejected. Required={requiredFace}, PlayerFace={player.BottomFace}.", this);
                onRejected?.Invoke();
                return;
            }

            hasBeenUsed = true;
            Debug.Log($"[PressurePlate] '{linkId}' activated. PlayerFace={player.BottomFace}. Registering LevelObjective.PressurePlate.", this);
            LevelManager.Instance?.RegisterObjective(LevelObjective.PressurePlate);
            foreach (ToggleBridgeTile bridge in controlledBridges)
            {
                if (bridge == null)
                {
                    continue;
                }

                switch (activationMode)
                {
                    case PlateActivationMode.ActivateOnly:
                        bridge.ActivateBridge();
                        break;
                    case PlateActivationMode.DeactivateOnly:
                        bridge.DeactivateBridge();
                        break;
                    case PlateActivationMode.Toggle:
                        bridge.ToggleBridge();
                        break;
                }
            }

            onActivated?.Invoke();
        }

        private bool MatchesRequiredFace(CubeFace bottomFace)
        {
            return requiredFace == RequiredCubeFace.Any
                || requiredFace.ToString() == bottomFace.ToString();
        }
    }
}
