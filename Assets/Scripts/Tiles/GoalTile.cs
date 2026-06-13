using CubeShift.Core;
using CubeShift.LevelDesign;
using CubeShift.Player;
using UnityEngine;

namespace CubeShift.Tiles
{
    /// <summary>
    /// Completes the current level when the cube lands on this tile.
    /// </summary>
    public sealed class GoalTile : TileBase
    {
        [SerializeField] private LevelManager levelManager;
        public override TileEffectType EffectType => TileEffectType.Goal;

        private void Awake()
        {
            if (levelManager == null && LevelManager.Instance != null)
            {
                levelManager = LevelManager.Instance;
            }
        }

        protected override void HandlePlayerLanded(PlayerCubeController player)
        {
            LevelManager manager = levelManager != null
                ? levelManager
                : LevelManager.Instance != null
                    ? LevelManager.Instance
                    : FindAnyObjectByType<LevelManager>();

            if (manager == null)
            {
                Debug.LogWarning("GoalTile could not find a LevelManager to complete the level.", this);
                return;
            }

            levelManager = manager;

            bool canComplete = manager.CanCompleteActiveLevel();
            if (canComplete)
            {
                Debug.Log("[GoalTile] CanCompleteLevel=True -> CompleteLevel()", this);
            }
            else
            {
                var activeLevel = manager.ActiveLevel;
                var completed   = manager.CompletedObjectives;
                var required    = activeLevel != null ? activeLevel.RequiredObjectives : LevelObjective.None;
                var missing     = required & ~completed;
                Debug.Log($"[GoalTile] CanCompleteLevel=False | Required={required} | Completed={completed} | Missing={missing}", this);
            }

            manager.CompleteLevel();
        }
    }
}
