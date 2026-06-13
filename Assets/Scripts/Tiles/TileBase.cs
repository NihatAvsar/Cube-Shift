using CubeShift.Player;
using UnityEngine;

namespace CubeShift.Tiles
{
    /// <summary>
    /// Base class for every grid tile. PlayerCubeController calls OnPlayerLanded after each move.
    /// </summary>
    [DisallowMultipleComponent]
    public abstract class TileBase : MonoBehaviour
    {
        [SerializeField] private bool walkable = true;

        public bool Walkable => walkable;

        public virtual void OnPlayerLanded(PlayerCubeController player)
        {
            if (player == null)
            {
                Debug.LogWarning("Tile received a landing event without a player reference.", this);
                return;
            }

            HandlePlayerLanded(player);
        }

        protected virtual void HandlePlayerLanded(PlayerCubeController player)
        {
        }
    }
}
