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
        [SerializeField] private Vector2Int gridPosition;
        [SerializeField, Min(0)] private int heightLevel;

        public bool Walkable => walkable;
        public bool IsWalkable => walkable;
        public Vector2Int GridPosition => gridPosition;
        public int HeightLevel => heightLevel;
        public virtual TileEffectType EffectType => TileEffectType.None;

        public virtual bool CanPlayerEnter(PlayerCubeController player)
        {
            return walkable;
        }

        public virtual bool CanEnter(PlayerCubeController player)
        {
            return CanPlayerEnter(player);
        }

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

        public virtual void OnPlayerLeft(PlayerCubeController player)
        {
        }

        public void ConfigureGrid(Vector2Int position, int level)
        {
            gridPosition = position;
            heightLevel = Mathf.Max(0, level);
        }

        protected void SetWalkable(bool value)
        {
            walkable = value;
        }
    }
}
