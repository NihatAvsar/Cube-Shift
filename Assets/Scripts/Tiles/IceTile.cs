using CubeShift.Player;
using CubeShift.Core;
using CubeShift.LevelDesign;

namespace CubeShift.Tiles
{
    /// <summary>
    /// Continues the player's previous movement direction until the ice lane ends.
    /// </summary>
    public sealed class IceTile : TileBase
    {
        public override TileEffectType EffectType => TileEffectType.Ice;

        protected override void HandlePlayerLanded(PlayerCubeController player)
        {
            LevelManager.Instance?.RegisterObjective(LevelObjective.IceSlide);
            if (player.LastMoveDirection == UnityEngine.Vector2Int.zero)
            {
                return;
            }

            player.TryForcedMove(player.LastMoveDirection);
        }
    }
}
