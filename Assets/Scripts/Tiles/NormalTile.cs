using CubeShift.Player;

namespace CubeShift.Tiles
{
    /// <summary>
    /// A standard tile that the cube can stand on without triggering special behavior.
    /// </summary>
    public sealed class NormalTile : TileBase
    {
        protected override void HandlePlayerLanded(PlayerCubeController player)
        {
            // Intentionally empty. Future tile-wide effects can still hook through TileBase.
        }
    }
}
