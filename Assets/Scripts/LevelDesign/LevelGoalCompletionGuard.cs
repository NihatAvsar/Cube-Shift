using CubeShift.Core;
using CubeShift.Player;
using UnityEngine;

namespace CubeShift.LevelDesign
{
    /// <summary>
    /// Completes the level from LevelData goal coordinates, independent of tile landing events.
    /// </summary>
    public sealed class LevelGoalCompletionGuard : MonoBehaviour
    {
        [SerializeField] private LevelData levelData;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private LevelManager levelManager;
        [SerializeField, Min(0.1f)] private float goalRadius = 0.7f;
        [SerializeField, Min(0.1f)] private float heightTolerance = 1.6f;
        [SerializeField] private bool logDebugMessages;

        private PlayerCubeController player;

        public void Configure(LevelData data, Transform playerTarget, LevelManager manager)
        {
            levelData = data;
            playerTransform = playerTarget;
            levelManager = manager;
            player = playerTransform != null ? playerTransform.GetComponent<PlayerCubeController>() : null;
        }

        private void Update()
        {
            if (levelData == null)
            {
                return;
            }

            if (levelManager == null)
            {
                levelManager = LevelManager.Instance != null ? LevelManager.Instance : FindAnyObjectByType<LevelManager>();
            }

            if (levelManager == null || levelManager.IsLevelComplete || levelManager.IsTransitioning)
            {
                return;
            }

            if (playerTransform == null)
            {
                player = FindAnyObjectByType<PlayerCubeController>();
                playerTransform = player != null ? player.transform : null;
            }
            else if (player == null)
            {
                player = playerTransform.GetComponent<PlayerCubeController>();
            }

            if (playerTransform == null || (player != null && (player.IsMoving || player.IsFalling)))
            {
                return;
            }

            Vector3 playerPosition = playerTransform.position;
            foreach (LevelTileEntry entry in levelData.Tiles)
            {
                if (entry.TileType != LevelTileType.Goal)
                {
                    continue;
                }

                Vector3 goalCenter = new Vector3(
                    entry.Coordinate.x * levelData.TileSize,
                    entry.HeightLevel * levelData.TileSize + 0.5f,
                    entry.Coordinate.y * levelData.TileSize);

                Vector2 playerXZ = new Vector2(playerPosition.x, playerPosition.z);
                Vector2 goalXZ = new Vector2(goalCenter.x, goalCenter.z);
                bool sameColumn = Vector2.Distance(playerXZ, goalXZ) <= goalRadius;
                bool compatibleHeight = Mathf.Abs(playerPosition.y - goalCenter.y) <= heightTolerance;

                if (!sameColumn || !compatibleHeight)
                {
                    continue;
                }

                if (logDebugMessages)
                {
                    Debug.Log($"[LevelGoalCompletionGuard] Completed from LevelData goal at {entry.Coordinate}.", this);
                }

                levelManager.CompleteLevel();
                return;
            }
        }
    }
}
