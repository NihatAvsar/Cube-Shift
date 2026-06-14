using CubeShift.Player;
using CubeShift.Core;
using CubeShift.LevelDesign;
using UnityEngine;

namespace CubeShift.Tiles
{
    /// <summary>
    /// Moves the player two cells forward when the green face is down.
    /// Intermediate tile landing effects are skipped during the jump.
    /// </summary>
    public sealed class JumpTile : TileBase
    {
        [SerializeField, Min(2)] private int jumpDistance = 2;
        [SerializeField] private bool requireGreenBottomFace;
        [SerializeField] private bool logDebugMessages;

        public override TileEffectType EffectType => TileEffectType.Jump;

        protected override void HandlePlayerLanded(PlayerCubeController player)
        {
            if ((requireGreenBottomFace && player.BottomFace != CubeFace.Green)
                || player.LastMoveDirection == Vector2Int.zero)
            {
                return;
            }

            LevelManager.Instance?.RegisterObjective(LevelObjective.GreenJump);
            bool startedJump = player.TryForcedMove(player.LastMoveDirection, jumpDistance, false);
            if (!startedJump && logDebugMessages)
            {
                Debug.LogWarning($"Jump tile could not start a forced move. Direction={player.LastMoveDirection}, Distance={jumpDistance}", this);
            }
        }
    }
}
