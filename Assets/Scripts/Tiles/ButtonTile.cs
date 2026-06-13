using CubeShift.Player;
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
        [SerializeField] private bool requireYellowBottomFace = true;
        [SerializeField] private bool triggerOnce = true;

        [Header("Events")]
        [SerializeField] private PlayerCubeEvent onActivated;
        [SerializeField] private UnityEvent onRejected;

        private bool hasActivated;

        public bool HasActivated => hasActivated;

        protected override void HandlePlayerLanded(PlayerCubeController player)
        {
            if (triggerOnce && hasActivated)
            {
                return;
            }

            if (!requireYellowBottomFace || player.BottomFace == CubeFace.Yellow)
            {
                hasActivated = true;
                onActivated?.Invoke(player);
                return;
            }

            onRejected?.Invoke();
        }

        public void ResetButton()
        {
            hasActivated = false;
        }
    }
}
